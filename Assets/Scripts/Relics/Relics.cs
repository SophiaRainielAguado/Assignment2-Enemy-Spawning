using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class RelicData
{
    public virtual string name;
    public virtual int sprite;
    public TriggerInfo trigger;
    public EffectInfo effect;

}

public class EffectInfo
{
    public virtual string description;
    public virtual string type;
    public virtual string amount;
    public virtual string? until;
}
public class TriggerInfo
{
    public virtual string description;
    public virtual string type;
    public virtual string? amount;

}
public class Relic
{
    public RelicData data;
    public SpellCaster owner;

    public RelicTrigger trigger;
    public RelicEffect effect;

    public Relic(
        SpellCaster owner,
        RelicData data,
        RelicTrigger trigger,
        RelicEffect effect)
    {
        this.owner = owner;
        this.data = data;

        this.trigger = trigger;
        this.effect = effect;
    }

    public void Register()
    {
        trigger.Register(this);
    }

    public void Unregister()
    {
        trigger.Unregister(this);
    }

    public void Activate()
    {
        effect.Activate(owner);
    }
}

public abstract class RelicEffect
{
    public EffectInfo data;

    public RelicEffect(EffectInfo data)
    {
        this.data = data;
    }

    public abstract void Activate(SpellCaster owner);
}

public class GainManaEffect : RelicEffect
{
    public GainManaEffect(EffectInfo data) : base(data)
    {
    }
     public override void Activate(SpellCaster owner, int wave)
    {
        var vars = new Dictionary<string, int>()
        {
            { "wave", wave},
            { "power", owner.spellpower}
        };

        int amount = RPNEvaluator.RPNEvaluator.Evaluate(data.amount, vars);

        owner.mana += amount;
        owner.mana = Mathf.Min(owner.mana, owner.max_mana);
    }


}

public abstract class TriggerEffect
{
    public TriggerInfo data;

    public TriggerEffect(EffectInfo data)
    {
        this.data = data;
    }

    public abstract void Activate(SpellCaster owner);
}



