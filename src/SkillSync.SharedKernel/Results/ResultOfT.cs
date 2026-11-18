using System.Diagnostics.CodeAnalysis;

namespace SkillSync.SharedKernel.Results;

public class Result<TValue> : Result , IValueResult
{
    private readonly TValue? _value;

    public Result(TValue? value, bool isSuccess, DomainError error) : base(isSuccess, error)
    {
        _value = value;
    }

    [NotNull]
    public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException("The value of a failure result can't be accessed");

    object? IValueResult.Value => Value;

    public static implicit operator Result<TValue>(TValue? value) => value is not null ? Success(value) : Failure<TValue>(DomainError.NullValue);

    public Result<TValue> ValidationFailure(DomainError error) => new(default, false, error);
}