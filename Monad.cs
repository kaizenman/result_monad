using System;

/// <summary>
/// Enumeration for common error types.
/// </summary>
public enum ErrorType
{
    Error_Disposable1 = 0,
    Error1
}

/// <summary>
/// A class to hold the result of a method call.
/// </summary>
public class Result
{
    /// <summary>
    /// Flag indicating the success of the operation.
    /// </summary>
    public bool Success { get; private set; }

    /// <summary>
    /// Error type if the operation failed.
    /// </summary>
    public ErrorType? Error { get; private set; }

    /// <summary>
    /// Flag indicating if the failure has been handled.
    /// </summary>
    private bool IsFailureHandled { get; set; } = false;

    /// <summary>
    /// Flag indicating if the execution should be halted due to a failure.
    /// </summary>
    private static bool HaltExecution { get; set; } = false;

    /// <summary>
    /// Indicates if the operation was a failure.
    /// </summary>
    public bool Failure => !Success;

    /// <summary>
    /// Constructor of the Result class.
    /// </summary>
    protected Result(bool success, ErrorType? errorType = null)
    {
        Success = success;
        Error = errorType;

        if (!success)
        {
            HaltExecution = true;
        }
    }

    /// <summary>
    /// Creates a successful Result with a value.
    /// </summary>
    public static Result<T> Ok<T>(T value) => Result<T>.Ok(value);

    /// <summary>
    /// Creates a successful Result without a value.
    /// </summary>
    public static Result Ok() => new Result(true);

    /// <summary>
    /// Creates a failed Result with an error type.
    /// </summary>
    public static Result Fail(ErrorType errorType) => new Result(false, errorType);

    /// <summary>
    /// Executes a function on success.
    /// </summary>
    public Result OnSuccess(Func<Result> func) => Success && !HaltExecution ? func.Invoke() : this;

    /// <summary>
    /// Executes a function with a return type on success.
    /// </summary>
    public Result<T> OnSuccess<T>(Func<Result<T>> func) => Success && !HaltExecution ? func.Invoke() : Result<T>.Fail(Error.GetValueOrDefault());

    /// <summary>
    /// Executes a function on failure.
    /// </summary>
    public Result OnFailure(Func<Result> func)
    {
        if (!Success && !IsFailureHandled)
        {
            IsFailureHandled = true;
            return func.Invoke();
        }

        return this;
    }
}

/// <summary>
/// A class to hold the result of a method call that returns a value.
/// </summary>
public class Result<T> : Result
{
    /// <summary>
    /// Constructor of the Result class with a value.
    /// </summary>
    protected internal Result(T value, bool success, ErrorType? errorType = null)
        : base(success, errorType)
    {
    }

    /// <summary>
    /// Creates a successful Result with a value.
    /// </summary>
    public static Result<T> Ok(T value) => new Result<T>(value, true);

    /// <summary>
    /// Creates a failed Result with an error type.
    /// </summary>
    public new static Result<T> Fail(ErrorType errorType) => new Result<T>(default, false, errorType);
}

class DisposableClass1 : IDisposable { public void Dispose() => Console.WriteLine("Dispose 1 called"); }
class DisposableClass2 : IDisposable { public void Dispose() => Console.WriteLine("Dispose 2 called"); }
class DisposableClass3 : IDisposable { public void Dispose() => Console.WriteLine("Dispose 3 called"); }

class Test
{
    /// <summary>
    /// Executes a function with a disposable object.
    /// </summary>
    public static Result ExecuteWith<T>(Func<T, Result> func) where T : IDisposable, new()
    {
        using (T disposable = new T())
        {
            return func(disposable);
        }
    }

    /// <summary>
    /// A sample successful method.
    /// </summary>
    private Result Bar(DisposableClass1 d1)
    {
        Console.WriteLine("yay!");
        return Result.Ok();
    }

    /// <summary>
    /// A sample failing method.
    /// </summary>
    private Result BarFailed()
    {
        Console.WriteLine("Failed!");
        return Result.Fail(ErrorType.Error1);
    }

    /// <summary>
    /// Another sample successful method.
    /// </summary>
    private Result Buz()
    {
        Console.WriteLine("hello");
        return Result.Ok();
    }

    /// <summary>
    /// The main method that calls other methods with success and failure callbacks.
    /// </summary>
    public void Foo()
    {
        ExecuteWith<DisposableClass1>(d1 =>
            ExecuteWith<DisposableClass2>(d2 =>
                ExecuteWith<DisposableClass3>(d3 => 
                    Bar(d1).OnSuccess(Buz).OnSuccess(Buz)
                )
                .OnSuccess(() => Bar(d1))
                .OnSuccess(BarFailed)
            )
            .OnFailure(Buz)
            .OnSuccess(Buz)
        );
    }
}

class Program
{
    static void Main() 
    {
        Test test = new Test();
        test.Foo();
    }
}

