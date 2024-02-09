using System.Diagnostics;
using System.Net;

namespace Backpacking.API.Utils;

public class Result
{
    public static implicit operator Result(BPError error) => new(error);

    protected readonly BPError _error;

    public bool Success
    {
        get => Equals(_error, BPError.Default);
    }

    public BPError Error
    {
        [DebuggerStepThrough]
        get => _error;
    }

    internal Result(BPError error)
    {
        _error = error;
    }

    public static Result Ok()
    {
        return new Result(BPError.Default);
    }

    public static Result Fail(BPError error)
    {
        return new(error);
    }

    public Result Then(Func<Result> next)
        => Success switch
        {
            true => next(),
            false => Fail(_error)
        };

    public Result<TResult> Then<TResult>(Func<Result<TResult>> next)
    => Success switch
    {
        true => next(),
        false => Result<TResult>.Fail(_error)
    };

    public async Task<Result> Then(Func<Task<Result>> next)
        => Success switch
        {
            true => await next(),
            false => Fail(_error)
        };

    public TResult Finally<TResult>(Func<TResult> some, Func<BPError, TResult> none)
        => Success switch
        {
            true => some(),
            false => none(_error)
        };

    public static Result Guard(Func<bool> func)
    {
        if (func() == true)
        {
            return Ok();
        }

        return Fail(new BPError(HttpStatusCode.BadRequest, ""));
    }

    public static Result Guard(Func<bool> func, HttpStatusCode rootCode = HttpStatusCode.BadRequest)
    {
        if (func() == true)
        {
            return Ok();
        }

        BPError rootError = new BPError(rootCode, "");

        return Fail(rootError);
    }
}

public class Result<TObject> : Result
{
    public static implicit operator Result<TObject>(BPError error) => new(error);
    public static implicit operator Result<TObject>(TObject value) => new(value);

    private readonly TObject _value;

    public TObject Value
    {
        get
        {
            if (Success)
            {
                return _value;
            }

            throw new Exception();
        }
    }

    internal Result(TObject value) : base(BPError.Default)
    {
        _value = value;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    internal Result(BPError error) : base(error)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        _value = default;
#pragma warning restore CS8601 // Possible null reference assignment.
    }

    public static Result<TObject> Ok(TObject value)
    {
        return new(value);
    }

    public new static Result<TObject> Fail(BPError error)
    {
        return new(error);
    }

    public static Result<TObject> Fail<TCurrent>(Result<TCurrent> result)
    {
        return new(result._error);
    }

    public Result<TOut> Then<TOut>(Func<TObject, Result<TOut>> next)
        => Success switch
        {
            true => next(_value),
            false => Result<TOut>.Fail(_error)
        };

    public async Task<Result<TOut>> Then<TOut>(Func<TObject, Task<Result<TOut>>> next)
        => Success switch
        {
            true => await next(_value),
            false => Result<TOut>.Fail(_error)
        };

    public TResult Finally<TResult>(Func<TObject, TResult> some, Func<BPError, TResult> none)
        => Success switch
        {
            true => some(_value),
            false => none(_error)
        };

    public void Finally(Action<TObject> some, Action<BPError> none)
    {
        if (Success)
        {
            some(_value);
        }
        else
        {
            none(_error);
        }
    }
}

public static class ResultExtensions
{
    public static async Task<Result<TNextData>> Then<TData, TNextData>(
        this Task<Result<TData>> option,
        Func<TData, Task<Result<TNextData>>> func
    )
    {
        Result<TData> result = await option;

        return await result.Then(func);
    }

    public static async Task<Result<TNextData>> Then<TData, TNextData>(
        this Task<Result<TData>> option,
        Func<TData, Result<TNextData>> func
    )
    {
        Result<TData> result = await option;

        return result.Then(func);
    }

    public static async Task<TResult> Finally<TData, TResult>(
        this Task<Result<TData>> option,
        Func<TData, TResult> some,
        Func<BPError, TResult> none
    )
    {
        Result<TData> result = await option;

        return result.Finally(some, none);
    }

    public static async Task<Result<TData>> Finally<TData>(
        this Task<Result<TData>> option,
        Action<TData> some,
        Action<BPError> none
    )
    {
        Result<TData> result = await option;

        result.Finally(some, none);

        return result;
    }

    public static async Task<Result<TOut>> Then<TOut>(this Result result, Func<Task<Result<TOut>>> func)
    {
        if (result.Success)
        {
            return await func();
        }

        return Result<TOut>.Fail(result.Error);
    }

    public static async Task<Result<TOut>> Then<TOut>(this Task<Result> result, Func<Result<TOut>> func)
    {
        Result awaited = await result;

        if (awaited.Success)
        {
            return func();
        }

        return Result<TOut>.Fail(awaited.Error);
    }
}

