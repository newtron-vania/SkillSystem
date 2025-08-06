using System;
using System.Collections;
using UnityEngine;

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