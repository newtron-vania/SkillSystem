using System;
/// <summary>
/// 
/// DamageEffect는 스킬의 피해량을 계산하여 대상 Actor에게 피해를 가하는 효과 클래스입니다.
/// 기본 피해량과 AP/AD 계수를 기반으로 최종 피해를 산출하며, 대상의 방어력(stat.defense)을 고려하여 적용합니다.
/// </summary>
public class DamageEffect : Effect
{
    private float _defaultDamage;
    private float _mulAp;
    private float _mulAd;

    public DamageEffect(float defaultDamage, float mulAp, float mulAd)
    {
        _defaultDamage = defaultDamage;
        _mulAp = mulAp;
        _mulAd = mulAd;
    }

    public override void Initialize(params object[] objects)
    {
        if (objects[0] is float defaultDamage) _defaultDamage = defaultDamage;
        if (objects[1] is float mulAp) _mulAp = mulAp;
        if (objects[2] is float mulAd) _mulAd = mulAd;
    }

    public override bool Apply(Actor source, Actor target)
    {
        var finalDamage = _defaultDamage + _defaultDamage + source.stat.ap * _mulAp + source.stat.ad * _mulAd;
        target.stat.health -= finalDamage - target.stat.defense;
        return true;
    }

    public override void Clear()
    {
        throw new NotImplementedException();
    }
}