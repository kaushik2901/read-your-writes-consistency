namespace ReadYourWritesConsistency.API.Models;

public sealed record Result
{
	public bool IsSuccess { get; }
	public string? Error { get; }
	public string? DbSource { get; init; } // Indicates which database was used (Master or Replica)

	private Result(bool isSuccess, string? error, string? dbSource = null)
	{
		IsSuccess = isSuccess;
		Error = error;
		DbSource = dbSource;
	}

	public static Result Success(string? dbSource = null) => new(true, null, dbSource);
	public static Result Failure(string error, string? dbSource = null) => new(false, error, dbSource);
}

public sealed record Result<T>
{
	public bool IsSuccess { get; }
	public string? Error { get; }
	public T? Value { get; }
	public string? DbSource { get; init; } // Indicates which database was used (Master or Replica)

	private Result(bool isSuccess, T? value, string? error, string? dbSource = null)
	{
		IsSuccess = isSuccess;
		Value = value;
		Error = error;
		DbSource = dbSource;
	}

	public static Result<T> Success(T value, string? dbSource = null) => new(true, value, null, dbSource);
	public static Result<T> Failure(string error, string? dbSource = null) => new(false, default, error, dbSource);
}


