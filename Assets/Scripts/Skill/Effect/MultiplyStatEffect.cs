using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplyStatEffect : Effect
{
    private Stat _originalStat;// 상태 효과가 끝난 후 되돌릴 수 있도록
    private List<float> _multiplyValues;  // 각 스탯에 대해 적용할 배수 리스트
    private float _duration;

    // 상태 변경을 적용하는 메서드

    public MultiplyStatEffect(Stat originalStat, List<float> multiplyValues, float duration)
    {
        _originalStat = originalStat;
        _multiplyValues = multiplyValues;
        _duration = duration;
    }

    public override void Initialize(params object[] objects)
    {
        if(objects[0] is Stat stat) _originalStat = stat.DeepCopy();
        if(objects[1] is List<float> values) _multiplyValues = values;
        if(objects[2] is float duration) _duration = duration;
    }

    public override bool Apply(Actor source, Actor target)
    {
        // 타겟 스탯에 영향을 미치는 효과 적용
        source.stat.ApplyStatModification(_multiplyValues);

        if (_duration <= 0)
        {
            source.stat = _originalStat;
            return true;
        }

        return false;
    }

    public override void Clear()
    {
        throw new System.NotImplementedException();
    }

}