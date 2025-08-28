import { useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useAppState } from '@/state/AppState';

export function useUrlSync() {
  const navigate = useNavigate();
  const location = useLocation();
  const { userId, setUserId, consistencyMode, setConsistencyMode } = useAppState();

  // Sync state with URL parameters on mount
  useEffect(() => {
    const searchParams = new URLSearchParams(location.search);

    // Parse userId from URL
    const userIdParam = searchParams.get('userId');
    if (userIdParam) {
      const id = parseInt(userIdParam, 10);
      if (!isNaN(id) && id >= 1 && id <= 4) {
        setUserId(id);
      }
    }

    // Parse consistencyMode from URL
    const modeParam = searchParams.get('consistencyMode');
    if (modeParam === 'none' || modeParam === 'ryw') {
      setConsistencyMode(modeParam);
    }
  }, []); // Only run on mount

  // Update URL when state changes
  useEffect(() => {
    const searchParams = new URLSearchParams(location.search);

    // Update userId parameter
    searchParams.set('userId', userId.toString());

    // Update consistencyMode parameter
    searchParams.set('consistencyMode', consistencyMode);

    // Only update URL if parameters have changed
    const newSearch = searchParams.toString();
    if (newSearch !== location.search.substring(1)) {
      navigate(`${location.pathname}?${newSearch}`, { replace: true });
    }
  }, [
    userId,
    consistencyMode,
    location.search,
    location.pathname,
    navigate,
    setUserId,
    setConsistencyMode,
  ]);
}
