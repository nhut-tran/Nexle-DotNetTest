namespace AuthApi.Application.Core;


public class Result<T>
{
   
    
    private bool IsSuccess { get; init; }
    private string? Error { get; init; }
    private T? Value {get; init; }

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
    {
        if (IsSuccess && Value is not null)
            return onSuccess(Value);
        return onFailure(Error ?? "Unknown error");
    }
}

public class Result
{
    private bool IsSuccess { get; init; }
    private string? Error { get; init; }

    public static Result Success() => new() { IsSuccess = true };
    public static Result Failure(string error) => new() { IsSuccess = false, Error = error };

    public T Match<T>(Func<T> onSuccess, Func<string, T> onFailure)
    {
        return IsSuccess
            ? onSuccess()
            : onFailure(Error ?? "Unknown error");
    }
    
}
