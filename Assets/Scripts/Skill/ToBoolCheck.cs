/// <summary>
/// 불린 값을 체크하는 유틸리티 클래스입니다.
/// 스킬 효과에서 조건 확인용으로 사용됩니다.
/// </summary>
public class ToBoolCheck
{
    public bool _ischeck = false;

    public ToBoolCheck()
    {
        _ischeck = false;
    }

    public ToBoolCheck(bool initialValue)
    {
        _ischeck = initialValue;
    }

    public void SetCheck(bool value)
    {
        _ischeck = value;
    }

    public bool IsChecked()
    {
        return _ischeck;
    }

    public void Reset()
    {
        _ischeck = false;
    }
}