using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class RelicData
{
    public  string name;
    public  int sprite;
    public TriggerInfo trigger;
    public EffectInfo effect;

}

public class EffectInfo
{
    public  string description;
    public  string type;
    public  string amount;
    public  string? until;
}
public class TriggerInfo
{
    public  string description;
    public  string type;
    public  string? amount;

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
        effect.Activate(owner, GameManager.Instance.currentWave);
    }
}

public abstract class RelicEffect
{
    public EffectInfo data;

    public RelicEffect(EffectInfo data)
    {
        this.data = data;
    }

    public abstract void Activate(SpellCaster owner, int wave);
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

public class GainSpellpowerEffect : RelicEffect
{
    public GainSpellpowerEffect(EffectInfo data) : base(data)
    {
    }

    public override void Activate (SpellCaster owner, int wave)
    {
        var vars = new Dictionary<string, int>()
        {
             { "wave", wave},
            { "power", owner.spellpower}
        };
        
        int amount = RPNEvaluator.RPNEvaluator.Evaluate(data.amount, vars);
        owner.spellpower += amount;
    }
}
public abstract class RelicTrigger
{
    public TriggerInfo data;

    public RelicTrigger(TriggerInfo data)
    {
        this.data = data;
    }

    public abstract void Register(Relic relic);
    public abstract void Unregister(Relic relic);
}

public class TakeDamageTrigger : RelicTrigger
{
    private Relic relic;

    public TakeDamageTrigger(TriggerInfo data) : base(data)
    {
    }

    public override void Register(Relic relic)
    {
        this.relic = relic;

        EventBus.Instance.OnDamageTaken += HandleDamage;
    }

    public override void Unregister(Relic relic)
    {
        EventBus.Instance.OnDamageTaken -= HandleDamage;
    }

    void HandleDamage(Hittable target, Damage dmg)
    {
        if (target.team == Hittable.Team.PLAYER)
        {
            relic.Activate();
        }
    }
}



