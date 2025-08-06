using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// WaitForCheckEffect는 특정 조건(ToBoolCheck)이 충족될 때까지 기다리는 효과 클래스입니다.
/// 주로 연출이나 순차적 실행 제어를 위한 대기 상태 처리에 사용되며,
/// 조건이 만족되면 true를 반환하여 다음 로직으로 넘어갈 수 있게 합니다.
/// </summary>
public class WaitForCheckEffect : Effect
{
    private ToBoolCheck _checker; // 대기 시간

    public WaitForCheckEffect(ToBoolCheck checker) : base()
    {
        _checker = checker;
    }

    // 대기 시간과 완료 후 실행할 액션을 설정하는 메서드
    public override void Initialize(params object[] objects)
    {
        if (objects[0] is ToBoolCheck checker)
        {
            _checker = checker;  // 대기 시간
        }
    }

    // 대기 후에 실행할 액션을 처리하는 코루틴
    public override bool Apply(Actor source, Actor target)
    {
        return _checker._ischeck;
    }

    public override void Clear()
    {
        throw new NotImplementedException();
    }
}