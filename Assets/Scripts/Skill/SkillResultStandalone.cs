using System;

/// <summary>
/// 스킬 결과를 나타내는 클래스입니다.
/// 성공/실패와 함께 메시지를 포함합니다.
/// </summary>
public class SkillResult
{
    public bool IsSuccess { get; private set; }
    public string Message { get; private set; }
    public Exception Error { get; private set; }

    protected SkillResult(bool success, string message, Exception error = null)
    {
        IsSuccess = success;
        Message = message;
        Error = error;
    }

    public static SkillResult Success(string message = "") => new SkillResult(true, message);
    public static SkillResult Failure(string message) => new SkillResult(false, message);
    public static SkillResult Failure(string message, Exception error) => new SkillResult(false, message, error);
}

/// <summary>
/// 제네릭 스킬 결과 클래스입니다.
/// </summary>
public class SkillResult<T> : SkillResult
{
    public T Value { get; private set; }

    private SkillResult(bool success, T value, string message, Exception error = null)
        : base(success, message, error)
    {
        Value = value;
    }

    public static SkillResult<T> Success(T value, string message = "") => new SkillResult<T>(true, value, message);
    public static new SkillResult<T> Failure(string message) => new SkillResult<T>(false, default(T), message);
    public static new SkillResult<T> Failure(string message, Exception error) => new SkillResult<T>(false, default(T), message, error);
}