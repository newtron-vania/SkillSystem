using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Skill
{
    private List<Effect> EffectList {get;} = new();

    public abstract bool ApplySkill(Actor source, Actor target);
}
