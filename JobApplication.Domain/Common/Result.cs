using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Domain.Common;

public class Result
{
    public Error? Error { get; }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    protected Result(bool isSuccess, Error? error = null)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("A successful result cannot have an error.");

        if (!isSuccess && error is null)
            throw new InvalidOperationException("A failed result must have an error.");

        Error = error;
        IsSuccess = isSuccess;
    }

    public static Result Success()
        => new(true);

    public static Result Failure(Error error)
        => new(false, error);
}

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    private Result(
        TValue? value,
        bool isSuccess,
        Error? error = null)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException("A failed result does not have a value.");

    public static Result<TValue> Success(TValue value)
        => new(value, true);

    public new static Result<TValue> Failure(Error error)
        => new(default, false, error);

    public TResult Match<TResult>(
        Func<TValue, TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(_value!)
                         : onFailure(Error!);
    }

    //Implicit conversion
    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(Error error) => Failure(error);
}
