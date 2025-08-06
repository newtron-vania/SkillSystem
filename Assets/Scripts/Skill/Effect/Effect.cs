using System.Collections;
using System.Collections.Generic;

public abstract class Effect
{
    public abstract void Initialize(params object[] objects);
    
    public abstract bool Apply(Actor source, Actor target);

    public abstract void Clear();
}

