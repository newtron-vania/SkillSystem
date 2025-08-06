using System;
using UnityEngine;

public class AirborneEffect : Effect
{
    private float _duration; // 효과 지속 시간

    public AirborneEffect(float duration) : base()
    {
        _duration = duration;
    }

    public override void Initialize(params object[] objects)
    {
        if (objects[0] is float duration) _duration = duration;
    }

    public override bool Apply(Actor source, Actor target)
    {
        try
        {
            if (target == null)
            {
                Debug.LogError("Target is null in AirborneEffect.");
                return false;
            }

            // Airborne 상태 적용
            target.ApplyAirborneEffect(_duration);
            target.rigid.velocity = Vector3.zero;
            target.rigid.AddForce(0, 100f, 0, ForceMode.Impulse);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in AirborneEffect: {e.Message}");
            return false;
        }
    }

    public override void Clear()
    {
        throw new NotImplementedException();
    }
}