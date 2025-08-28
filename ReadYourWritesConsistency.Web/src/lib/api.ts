import { useAppState } from '@/state/AppState';

// Define the Result types using JavaScript naming conventions (camelCase)
export type Result<T> = {
  isSuccess: boolean;
  error: string | null;
  value: T | null;
  dbSource: string | null;
};

// Helper function to normalize the Result object from either PascalCase or camelCase
function normalizeResult<T>(result: any): Result<T> {
  return {
    isSuccess: result.IsSuccess ?? result.isSuccess ?? false,
    error: result.Error ?? result.error ?? null,
    value: result.Value ?? result.value ?? null,
    dbSource: result.DbSource ?? result.dbSource ?? null,
  };
}

export type HttpMethod = 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH';

type FetchOptions = {
  method?: HttpMethod;
  body?: unknown;
  headers?: Record<string, string>;
};

export function useApiBase() {
  const { userId, consistencyMode } = useAppState();

  console.log('API userId:', userId);

  const baseUrl = consistencyMode === 'ryw' ? '/api/v2' : '/api/v1';

  async function api<T>(path: string, options: FetchOptions = {}): Promise<T> {
    const method = options.method ?? 'GET';
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      'X-User-Id': String(userId),
      ...options.headers,
    };

    const res = await fetch(`${baseUrl}${path}`, {
      method,
      headers,
      body: options.body != null ? JSON.stringify(options.body) : undefined,
    });

    if (!res.ok) {
      const text = await res.text();
      throw new Error(text || `Request failed: ${res.status}`);
    }

    const contentType = res.headers.get('content-type') || '';
    if (contentType.includes('application/json')) {
      const rawResult = await res.json();
      const result = normalizeResult<T>(rawResult);
      if (!result.isSuccess) {
        throw new Error(result.error || 'Request failed');
      }
      return result.value as T;
    }

    // @ts-expect-error allow void
    return undefined;
  }

  return { api };
}
