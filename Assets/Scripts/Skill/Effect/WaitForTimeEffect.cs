using System;
using UnityEngine;

public class WaitForTimeEffect : Effect
{
    private float _duration;      // 설정된 총 대기 시간
    private float _remainingTime; // 남은 대기 시간

    public WaitForTimeEffect(float duration)
    {
        _duration = duration;
        _remainingTime = duration;
    }
    public override void Initialize(params object[] objects)
    {
        if (objects[0] is float duration)
        {
            _duration = duration;
            _remainingTime = duration;
        }
        else
        {
            Debug.LogError("WaitForTimeEffect.Initialize: 잘못된 초기화 값입니다.");
        }
    }

    public override bool Apply(Actor source, Actor target)
    {
        // 실행 중이라면 시간 감소
        _remainingTime -= Time.deltaTime;

        // 시간이 다 되면 true 반환
        return _remainingTime <= 0f;
    }

    public override void Clear()
    {
        _remainingTime = _duration; // 다음 사용을 위해 리셋 가능
    }
}