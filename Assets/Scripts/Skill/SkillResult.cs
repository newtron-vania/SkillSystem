using System;

/// <summary>
/// 스킬 시스템에서 작업 결과를 나타내는 클래스입니다.
/// 성공/실패 상태와 메시지, 오류 정보를 포함합니다.
/// </summary>
public class SkillResult
{
    public bool IsSuccess { get; private set; }
    public string Message { get; private set; }
    public Exception Error { get; private set; }

    private SkillResult(bool isSuccess, string message, Exception error = null)
    {
        IsSuccess = isSuccess;
        Message = message ?? string.Empty;
        Error = error;
    }

    /// <summary>
    /// 성공 결과를 생성합니다.
    /// </summary>
    public static SkillResult Success(string message = "")
    {
        return new SkillResult(true, message);
    }

    /// <summary>
    /// 실패 결과를 생성합니다.
    /// </summary>
    public static SkillResult Failure(string message)
    {
        return new SkillResult(false, message);
    }

    /// <summary>
    /// 예외와 함께 실패 결과를 생성합니다.
    /// </summary>
    public static SkillResult Failure(string message, Exception error)
    {
        return new SkillResult(false, message, error);
    }

    public override string ToString()
    {
        return $"SkillResult(Success: {IsSuccess}, Message: {Message})";
    }
}

/// <summary>
/// 값을 포함하는 스킬 결과 클래스입니다.
/// </summary>
public class SkillResult<T>
{
    public bool IsSuccess { get; private set; }
    public string Message { get; private set; }
    public T Value { get; private set; }
    public Exception Error { get; private set; }

    private SkillResult(bool isSuccess, string message, T value = default(T), Exception error = null)
    {
        IsSuccess = isSuccess;
        Message = message ?? string.Empty;
        Value = value;
        Error = error;
    }

    /// <summary>
    /// 성공 결과를 생성합니다.
    /// </summary>
    public static SkillResult<T> Success(T value, string message = "")
    {
        return new SkillResult<T>(true, message, value);
    }

    /// <summary>
    /// 실패 결과를 생성합니다.
    /// </summary>
    public static SkillResult<T> Failure(string message)
    {
        return new SkillResult<T>(false, message);
    }

    /// <summary>
    /// 예외와 함께 실패 결과를 생성합니다.
    /// </summary>
    public static SkillResult<T> Failure(string message, Exception error)
    {
        return new SkillResult<T>(false, message, default(T), error);
    }

    public override string ToString()
    {
        return $"SkillResult<{typeof(T).Name}>(Success: {IsSuccess}, Message: {Message}, Value: {Value})";
    }
}