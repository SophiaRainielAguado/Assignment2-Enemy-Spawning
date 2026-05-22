using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Numerics;
using System.ComponentModel.DataAnnotations;

public class SpellData // can be used for all the base spells; some fields will be left null 
{
    public string name;
    public string description;
    public int icon;
    public DamageInfo damage;
    public string N;
    public string mana_cost;
    public string cooldown;
    public ProjectileInfo projectile;
    public SecondaryProjectileInfo? secondary_projectile;
    public string category;
}
// some fields for the spells require their own classes. these are them.
public class DamageInfo
{
    public string amount;
    public string type;
}

public class ProjectileInfo
{
    public string trajectory;
    public string speed;
    public int sprite;
}

public class SecondaryProjectileInfo
{
    public string trajectory;
    public string speed;
    public string lifetime;
    public int sprite;
}

// the base class for spells but not modifier spells (Except for the base modifier spell class, since it hands things down to those)
public class Spell
{
    public float last_cast;
    public SpellCaster owner;
    public Hittable.Team team;
    public SpellData data;

    public Spell(SpellCaster owner, SpellData spelldata)
    {
        this.owner = owner;
        this.data = spelldata;
    }

    public virtual string GetName()
    {
        return data.name;
    }

    public virtual int GetManaCost(int spellpower)
    {
        var vars = new Dictionary<string, int>()
        {
            { "power", spellpower}
        };

        return RPNEvaluator.RPNEvaluator.Evaluate(data.mana_cost, vars);
    }

    public virtual int GetDamage(int spellpower, int wave)
    {
        var vars = new Dictionary<string, int>()
        {
            { "power", spellpower },
            { "wave", wave }
        };

        return RPNEvaluator.RPNEvaluator.Evaluate(data.damage.amount, vars);
    }

    public virtual float GetCooldown(int spellpower, int wave)
    {
        var vars = new Dictionary<string, int>()
        {
            { "power", spellpower },
            { "wave", wave }
        };

        return RPNEvaluator.RPNEvaluator.Evaluate(data.cooldown, vars);
    }

    public virtual int GetIcon()
    {
        return data.icon;
    }

    public bool IsReady(int spellpower, int wave)
    {
        return last_cast + GetCooldown(spellpower, wave) < Time.time;
    }

    public virtual float GetProjectileSpeed(int spellpower, int wave)
    {
        var vars = new Dictionary<string, int>()
          {
              {"power", spellpower}
          };

        return RPNEvaluator.RPNEvaluator.Evaluate(data.projectile.speed, vars);
    }

    public virtual string GetTrajectory()
    {
        return data.projectile.trajectory;
    }

    public virtual IEnumerator Cast(UnityEngine.Vector3 where, UnityEngine.Vector3 target, Hittable.Team team, int power, int wave)
    {
        this.team = team;
        float speed = GetProjectileSpeed(power, wave);

        GameManager.Instance.projectileManager.CreateProjectile(
            data.projectile.sprite,
            data.projectile.trajectory,
            where,
            target - where,
            speed,
            OnHit
        );

        yield return new WaitForEndOfFrame();

        void OnHit(Hittable other, UnityEngine.Vector3 impact)
        {
            if (other.team != team)
            {
                other.Damage(
                    new Damage(
                        GetDamage(power, wave),
                        Damage.Type.ARCANE
                    )
                );
            }
        }
        last_cast = Time.time;
    }

}

//base spellz... dont have any additional fields so they can just inherit from the base spell class with no other additions
//but it's nice having them here for organizational purposes.

public class ArcaneBolt : Spell
{
    public ArcaneBolt(SpellCaster owner, SpellData data) : base(owner, data) //the data is already carried over from base class as theres nothing addtl
    {
    }
}

public class MagicMissile : Spell
{
    public MagicMissile(SpellCaster owner, SpellData data) : base(owner, data) //see above comment
    {
    }

}

// Class that modify spells (this base one is just a bunch of overrides that return the "pre" spell with no alterations)
public class SpellModifier : Spell
{
    protected Spell preSpell; //the spell that is being modified, protected because it's going to take from the higher class

    public SpellModifier(Spell inner) : base(inner.owner, inner.data)
    {
        preSpell = inner;
    }
    public override int GetManaCost(int spellpower)
    {
        return preSpell.GetManaCost(spellpower);
    }

    public override int GetDamage(int spellpower, int wave)
    {
        return preSpell.GetDamage(spellpower, wave);
    }

    public override float GetProjectileSpeed(int spellpower, int wave)
    {
        return preSpell.GetProjectileSpeed(spellpower, wave);
    }

    public override float GetCooldown(int spellpower, int wave)
    {
        return preSpell.GetCooldown(spellpower, wave);
    }
    public override string GetTrajectory()
    {
        return preSpell.GetTrajectory();
    }
}


//the class for spell mods themselves... lots of superflous fields except for name/desc
public class ModifierData
{
    public string name;
    public string description;
    public string damage;
    public float cooldown;

    public int? damage_multiplier; //just a buuuuunch of optional fields!!
    public int? mana_multiplier;
    public int? speed_multiplier;
    public int? cooldown_multiplier;
    public int? angle;
    public int? delay;
    public string? projectile_trajectory;
    public int? mana_adder;
    public string category;

}

// THESE are the classes for spell modifiers!! VV

public class DamageAmpModifier : SpellModifier //and spell modifier draws from the main spell class, so that should carry down?
{
    private ModifierData data;
    public DamageAmpModifier(Spell inner, ModifierData modInfo) : base(inner)
    {
        data = modInfo;
    }
    public override int GetDamage(int spellpower, int wave)
    {
        int mult = data.damage_multiplier ?? 1;
        return preSpell.GetDamage(spellpower, wave) * mult;
    }
    public override int GetManaCost(int spellpower)
    {
        int mult = data.mana_multiplier ?? 1;
        return preSpell.GetManaCost(spellpower) * mult;
    }
}

public class SpeedAmp : SpellModifier
{
    private ModifierData data;
    public SpeedAmp(Spell inner, ModifierData modInfo) : base(inner)
    {
        data = modInfo;
    }
    public override float GetProjectileSpeed(int spellpower, int wave)
    {
        int multiplier = data.speed_multiplier ?? 1;
        return preSpell.GetProjectileSpeed(spellpower, wave) * multiplier;
    }
}

public class Doubler : SpellModifier
{
    private ModifierData data;
    public Doubler(Spell inner, ModifierData modInfo) : base(inner)
    {
        data = modInfo;
    }
    public override int GetManaCost(int spellpower)
    {
        return preSpell.GetManaCost(spellpower) * (data.mana_multiplier ?? 1);
    }
    public override float GetCooldown(int spellpower, int wave)
    {
        var vars = new Dictionary<string, int>()
        {
            { "power", spellpower },
            { "wave", wave }
        };

        return data.cooldown;
    }
    public override IEnumerator Cast(UnityEngine.Vector3 where, UnityEngine.Vector3 target, Hittable.Team team, int power, int wave)
    {
        yield return preSpell.Cast(where, target, team, power, wave);
        yield return new WaitForSeconds(data.delay.GetValueOrDefault());
        yield return preSpell.Cast(where, target, team, power, wave);
    }
}

public class Cursed : SpellModifier
{
    private ModifierData data;
    public Cursed(Spell inner, ModifierData modInfo) : base(inner)
    {
        data = modInfo;
    }

    public override int GetDamage(int spellpower, int wave)
    {
        int mult = data.damage_multiplier ?? 1;
        return preSpell.GetDamage(spellpower, wave) * mult;
    }

    public override IEnumerator Cast(UnityEngine.Vector3 where, UnityEngine.Vector3 target, Hittable.Team team, int power, int wave)
    {
        yield return new WaitForSeconds(data.delay.GetValueOrDefault());
        yield return preSpell.Cast(where, target, team, power, wave);
    }

}
public class Reverse : SpellModifier
{
    private ModifierData data;
    public Reverse(Spell inner, ModifierData modInfo) : base(inner)
    {
        data = modInfo;
    }
    public override IEnumerator Cast(UnityEngine.Vector3 where, UnityEngine.Vector3 target, Hittable.Team team, int power, int wave)
    {

        Vector3 reversedTarget = where - (target - where);
        yield return preSpell.Cast(where, reversedTarget, team, power, wave);
    }
}
public class Slow : SpellModifier
{
    private ModifierData data;
    public Slow(Spell inner, ModifierData modInfo) : base(inner)
    {
        data = modInfo;
    }
    public override float GetProjectileSpeed(int spellpower, int wave)
    {
        int multiplier = data.speed_multiplier ?? 1;
        return preSpell.GetProjectileSpeed(spellpower, wave) * multiplier;
    }

}
public class Splitter : SpellModifier
{
    private ModifierData data;
    public Splitter(Spell inner, ModifierData modInfo) : base(inner)
    {
        data = modInfo;
    }
    public override int GetManaCost(int spellpower)
    {
        return preSpell.GetManaCost(spellpower);
    }
    public override IEnumerator Cast(UnityEngine.Vector3 where, UnityEngine.Vector3 target, Hittable.Team team, int power, int wave)
    {
        UnityEngine.Vector3 dir = (target - where).normalized;

        float angle = data.angle ?? 0;

        UnityEngine.Vector3 dir1 = Quaternion.Euler(0, 0, angle) * dir;
        UnityEngine.Vector3 dir2 = Quaternion.Euler(0, 0, -angle) * dir;

        yield return preSpell.Cast(where, where + dir1, team, power, wave);
        yield return preSpell.Cast(where, where + dir2, team, power, wave);
    }
}

public class Chaos : SpellModifier
{
    private ModifierData data;

    public Chaos(Spell inner, ModifierData modInfo) : base(inner)
    {
        data = modInfo;
    }

    public override int GetDamage(int spellpower, int wave)
    {
        int baseDamage = preSpell.GetDamage(spellpower, wave);

        var vars = new Dictionary<string, int>
        {
            { "power", spellpower },
            { "wave", wave },
            { "base", baseDamage }
        };

        return RPNEvaluator.RPNEvaluator.Evaluate(data.damage, vars);
    }

    public override string GetTrajectory()
    {
        return data.projectile_trajectory ?? preSpell.GetTrajectory();
    }
}

public class Homing : SpellModifier
{
    private ModifierData data;
    public Homing(Spell inner, ModifierData modInfo) : base(inner)
    {
        data = modInfo;
    }
    public override int GetDamage(int spellpower, int wave)
    {
        int mult = data.damage_multiplier ?? 1;
        return preSpell.GetDamage(spellpower, wave) * mult;
    }
    public override int GetManaCost(int spellpower)
    {
        int add = data.mana_adder ?? 0;
        return preSpell.GetManaCost(spellpower) + add;
    }
    public override string GetTrajectory()
    {
        return data.projectile_trajectory ?? preSpell.GetTrajectory();
    }
}

