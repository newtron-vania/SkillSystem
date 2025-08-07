using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MultiplyStatEffect는 일정 시간 동안 지정된 스탯 항목들을 배수로 증폭시키는 효과 클래스입니다.
/// 효과가 활성화되면 스탯을 수정하며, 시간이 종료되면 원래의 스탯으로 되돌립니다.
/// </summary>
public class MultiplyStatEffect : Effect
{
    private Stat _originalStat;// 상태 효과가 끝난 후 되돌릴 수 있도록
    private List<float> _multiplyValues;  // 각 스탯에 대해 적용할 배수 리스트
    private float _duration;

    private bool isActive = false;
    // 상태 변경을 적용하는 메서드

    public MultiplyStatEffect(Stat originalStat, List<float> multiplyValues, float duration)
    {
        _originalStat = originalStat.DeepCopy();
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
        if (!isActive)
        {
            // 타겟 스탯에 영향을 미치는 효과 적용
            source.stat.ApplyStatModification(_multiplyValues);
            isActive = true;
        }

        if (_duration <= 0)
        {
            source.stat = _originalStat;
            Clear();
            return true;
        }
        
        _duration -= Time.deltaTime;

        return false;
    }

    public override void Clear()
    {
        isActive = false;
    }

}