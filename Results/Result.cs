using System.Diagnostics.CodeAnalysis;

namespace Results;

public class Result<T>
{
    protected readonly bool Success;
    protected readonly string Message = null!;
    protected readonly T Value = default!;
    
    public Result(string message)
    {
        Success = false;
        Message = message;
    }
    
    public Result(T value)
    {
        Success = true;
        Value = value;
    }

    private Result(T value, string message, bool success)
    {
        Value = value;
        Message = message;
        Success = success;
    }

    public bool Deconstruct([MaybeNullWhen(false)] out T value, [MaybeNullWhen(true)] out string message)
    {
        value = Value;
        message = Message;
        return Success;
    }
    
    public bool IsError([MaybeNullWhen(false)] out string error)
    {
        error = Message;
        return !Success;
    }
    
    public bool IsSuccess([MaybeNullWhen(false)] out T value)
    {
        value = Value;
        return Success;
    }

    public bool IsError()
    {
        return !Success;
    }
    
    public bool IsSuccess()
    {
        return Success;
    }
    
    public static implicit operator Result<T>(string error) => new(error);
    public static implicit operator Result<T>(T value) => new(value);

    public static explicit operator T(Result<T> result)
    {
        if (result.Success)
            return result.Value;
        
        throw new ResultException(result.Message);
    }
    
    public static explicit operator string(Result<T> result)
    {
        if (!result.Success)
            return result.Message;
        
        throw new ResultException("Result is not an error.");
    }

    public static explicit operator Result(Result<T> result)
    {
        return result.Success ? Result.Success : result.Message;
    }

    public static Result<T> Cast<TFrom>(Result<TFrom> input) where TFrom : T
    {
        if (input.Success)
            return input.Value;
        
        return input.Message;
    }

    public static Result<string> FromStringResult(StringResult result)
    {
        if (result.Deconstruct(out var value))
            return new Result<string>(value, null!, true);
        else
            return new Result<string>(default!, value, false);
    }
    
    public static StringResult ToStringResult(Result<string> result)
    {
        if (result.Deconstruct(out var value, out var message))
            return new StringResult(true, value);

        return new StringResult(false, message);
    }

    public sealed override string ToString()
    {
        return Success ? $"Success: {Value}" : $"Failure: {Message}";
    }
    
    public Result<TOut> ErrorOr<TOut>(Func<T, TOut> func)
    {
        return Success ? func(Value) : Message;
    }
}

public class Result<T1, T2> : Result<(T1, T2)>
{
    public Result(string message) : base(message) { }
    public Result((T1, T2) value) : base(value) { }
    public Result(T1 value1, T2 value2) : base((value1, value2)) { }

    public bool Deconstruct([MaybeNullWhen(false)] out T1 value1, [MaybeNullWhen(false)] out T2 value2, [MaybeNullWhen(true)] out string message)
    {
        message = Message;
        (value1, value2) = Value;
        return Success;
    }
    
    public bool IsSuccess([MaybeNullWhen(false)] out T1 value1, [MaybeNullWhen(false)] out T2 value2)
    {
        (value1, value2) = Value;
        return Success;
    }
    
    public static implicit operator Result<T1, T2>(string error) => new(error);
    public static implicit operator Result<T1, T2>((T1, T2) value) => new(value);

    public static implicit operator Result<T1, T2>((Result<T1>, Result<T2>) value)
    {
        var (t1, t2) = value;
        
        if (!t1.Deconstruct(out var v1, out var m1))
            return new Result<T1, T2>(m1);

        if (!t2.Deconstruct(out var v2, out var m2))
            return new Result<T1, T2>(m2);
        
        return new Result<T1, T2>(v1, v2);
    }

    public Result<T1> Item1 => Success ? Value.Item1 : Message;
    public Result<T2> Item2 => Success ? Value.Item2 : Message;

    public static Result<T1, T2> Cast<T1From, T2From>(Result<T1From, T2From> input) where T1From : T1 where T2From : T2
    {
        if (input.Success)
            return input.Value;
        
        return input.Message;
    }
}

public class Result<T1, T2, T3> : Result<(T1, T2, T3)>
{
    public Result(string message) : base(message) { }
    public Result((T1, T2, T3) value) : base(value) { }
    public Result(T1 value1, T2 value2, T3 value3) : base((value1, value2, value3)) { }
    
    public bool Deconstruct([MaybeNullWhen(false)] out T1 value1, [MaybeNullWhen(false)] out T2 value2, [MaybeNullWhen(false)] out T3 value3, [MaybeNullWhen(true)] out string message)
    {
        message = Message;
        (value1, value2, value3) = Value;
        return Success;
    }
    
    public bool IsSuccess([MaybeNullWhen(false)] out T1 value1, [MaybeNullWhen(false)] out T2 value2, [MaybeNullWhen(false)] out T3 value3)
    {
        (value1, value2, value3) = Value;
        return Success;
    }
    
    public static implicit operator Result<T1, T2, T3>(string error) => new(error);
    
    public static implicit operator Result<T1, T2, T3>((T1, T2, T3) value) => new(value);

    public static implicit operator Result<T1, T2, T3>((Result<T1>, Result<T2>, Result<T3>) value)
    {
        var (t1, t2, t3) = value;
        
        if (!t1.Deconstruct(out var v1, out var m1))
            return new Result<T1, T2, T3>(m1);

        if (!t2.Deconstruct(out var v2, out var m2))
            return new Result<T1, T2, T3>(m2);

        if (!t3.Deconstruct(out var v3, out var m3))
            return new Result<T1, T2, T3>(m3);
        
        return new Result<T1, T2, T3>(v1, v2, v3);
    }

    public Result<T1> Item1 => Success ? Value.Item1 : Message;
    public Result<T2> Item2 => Success ? Value.Item2 : Message;
    public Result<T3> Item3 => Success ? Value.Item3 : Message;
    
    public  static Result<T1, T2, T3> Cast<T1From, T2From, T3From>(Result<T1From, T2From, T3From> input) where T1From : T1 where T2From : T2 where T3From : T3
    {
        if (input.Success)
            return input.Value;
        
        return input.Message;
    }
}

public class Result<T1, T2, T3, T4> : Result<(T1, T2, T3, T4)>
{
    public Result(string message) : base(message) { }
    public Result((T1, T2, T3, T4) value) : base(value) { }
    public Result(T1 value1, T2 value2, T3 value3, T4 value4) : base((value1, value2, value3, value4)) { }
    
    public bool Deconstruct([MaybeNullWhen(false)] out T1 value1, [MaybeNullWhen(false)] out T2 value2, [MaybeNullWhen(false)] out T3 value3, [MaybeNullWhen(false)] out T4 value4, [MaybeNullWhen(true)] out string message)
    {
        message = Message;
        (value1, value2, value3, value4) = Value;
        return Success;
    }
    
    public bool IsSuccess([MaybeNullWhen(false)] out T1 value1, [MaybeNullWhen(false)] out T2 value2, [MaybeNullWhen(false)] out T3 value3, [MaybeNullWhen(false)] out T4 value4)
    {
        (value1, value2, value3, value4) = Value;
        return Success;
    }
    
    public static implicit operator Result<T1, T2, T3, T4>(string error) => new(error);
    public static implicit operator Result<T1, T2, T3, T4>((T1, T2, T3, T4) value) => new(value);

    public static implicit operator Result<T1, T2, T3, T4>((Result<T1>, Result<T2>, Result<T3>, Result<T4>) value)
    {
        var (t1, t2, t3, t4) = value;
        
        if (!t1.Deconstruct(out var v1, out var m1))
            return new Result<T1, T2, T3, T4>(m1);

        if (!t2.Deconstruct(out var v2, out var m2))
            return new Result<T1, T2, T3, T4>(m2);

        if (!t3.Deconstruct(out var v3, out var m3))
            return new Result<T1, T2, T3, T4>(m3);

        if (!t4.Deconstruct(out var v4, out var m4))
            return new Result<T1, T2, T3, T4>(m4);
        
        return new Result<T1, T2, T3, T4>(v1, v2, v3, v4);
    }

    public Result<T1> Item1 => Success ? Value.Item1 : Message;
    public Result<T2> Item2 => Success ? Value.Item2 : Message;
    public Result<T3> Item3 => Success ? Value.Item3 : Message;
    public Result<T4> Item4 => Success ? Value.Item4 : Message;
    
    public static Result<T1, T2, T3, T4> Cast<T1From, T2From, T3From, T4From>(Result<T1From, T2From, T3From, T4From> input) where T1From : T1 where T2From : T2 where T3From : T3 where T4From : T4
    {
        if (input.Success)
            return input.Value;
        
        return input.Message;
    }
}

public class Result
{
    private bool _success;
    private string _message = null!;
    
    public Result(string message)
    {
        _success = false;
        _message = message;
    }
    
    public Result()
    {
        _success = true;
    }
    
    public bool Deconstruct([MaybeNullWhen(true)] out string message)
    {
        message = _message;
        return _success;
    }
    
    public bool IsError([MaybeNullWhen(false)] out string error)
    {
        error = _message;
        return !_success;
    }
    
    public bool IsSuccess()
    {
        return _success;
    }

    public bool IsError()
    {
        return !_success;
    }
    
    public static implicit operator Result(string error) => new(error);

    public static Result Success => new();
    
    public static bool operator true(Result result)
    {
        return result._success;
    }

    public static bool operator false(Result result)
    {
        return !result._success;
    }
    
    public static Result operator |(Result left, Result right)
    {
        return left._success ? left : right;
    }

    public static Result operator &(Result left, Result right)
    {
        return left._success ? right : left;
    }
    
    public void AssertSuccess()
    {
        if (_success)
            return;
        
        throw new ResultException(_message);
    }
    
    public static Result Cast<T>(Result<T> input)
    {
        if (input.IsError(out var error))
            return error;
        
        return Success;
    }
    
    public static Result Cast(StringResult input)
    {
        if (input.IsError(out var error))
            return error;
        
        return Success;
    }

    public override string ToString()
    {
        return _success ? $"Success" : $"Failure: {_message}";
    }
    
    public Result<T> ErrorOr<T>(T value)
    {
        return _success ? value : _message;
    }
}

public class StringResult
{
    private readonly bool _success;
    private readonly string _value;
    
    public StringResult(bool success, string value)
    {
        _success = success;
        _value = value;
    }

    public bool Deconstruct(out string message)
    {
        message = _value;
        return _success;
    }
    
    public bool Deconstruct([MaybeNullWhen(false)] out string value, [MaybeNullWhen(true)] out string message)
    {
        value = _value;
        message = _value;
        return _success;
    }
    
    public bool IsError([MaybeNullWhen(false)] out string error)
    {
        error = _value;
        return !_success;
    }
    
    public bool IsSuccess([MaybeNullWhen(false)] out string value)
    {
        value = _value;
        return _success;
    }

    public bool IsError()
    {
        return !_success;
    }
    
    public bool IsSuccess()
    {
        return _success;
    }
    
    public static implicit operator StringResult((bool success, string error) tuple) => new(tuple.success, tuple.error);
    public static implicit operator StringResult((string error, bool success) tuple) => new(tuple.success, tuple.error);
    
    public static explicit operator string(StringResult result)
    {
        if (result._success)
            return result._value;
        
        throw new ResultException(result._value);
    }
    
    public static explicit operator Result(StringResult result)
    {
        return result._success ? Result.Success : result._value;
    }

    public override string ToString()
    {
        return _success ? $"Success: {_value}" : $"Failure: {_value}";
    }
}