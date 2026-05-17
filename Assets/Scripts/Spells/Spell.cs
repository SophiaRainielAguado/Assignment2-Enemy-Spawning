using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Numerics;


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

    protected float Eval(string expression)
    {
        if (string.IsNullOrEmpty(expression))
            return 0;

        return (float)RPNEvaluator.RPNEvaluator.Evaluate(expression, GetRpnVars()); 
    }

    protected Dictionary<string, int> GetRpnVars()
    {
       // the part im confused on. fields like damage.amount for example rely on RPN calculations being made based on power or waves
    }

    public virtual string GetName()
    {
        return data.name;
    }

    public virtual int GetManaCost()
    {
        return data.mana_cost;
    }

    public virtual int GetDamage()
    {
        return data.damage; //not sure if i have to run it thru the calculator first?
    }

    public virtual float GetCooldown()
    {
        return data.cooldown;
    }

    public virtual int GetIcon()
    {
        return data.icon;
    }

    public bool IsReady()
    {
        return (last_cast + GetCooldown() < Time.time);
    }

    public virtual float GetProjectileSpeed()
    {
          //evaluate data.projectile.speed w the rpn later
    }

    public virtual string GetTrajectory()
    {
        return data.projectile.trajectory;
    }

    public virtual IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        float speed = GetProjectileSpeed();
        GameManager.Instance.projectileManager.CreateProjectile(data.projectile.sprite, data.projectile.trajectory, where, target - where, speed, OnHit);
        yield return new WaitForEndOfFrame(); //make speed into the rpn thing 
    }

    void OnHit(Hittable other, Vector3 impact)
    {
        if (other.team != team)
        {
            other.Damage(new Damage(GetDamage(), Damage.Type.ARCANE));
        }

    }

}

//base spellz... dont have any additional fields so they can just inherit from the base spell class with no other additions
//but it's nice having them here for organizational purposes.

public class ArcaneBolt : Spell
{
    public ArcaneBolt(SpellCaster owner, SpellData data): base(owner, data) //the data is already carried over from base class as theres nothing addtl
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
    public override int GetManaCost()
    {
        return preSpell.GetManaCost();
    }
    public override int GetDamage()
    {
       return preSpell.GetDamage(); 
    }
    public override float GetCooldown()
    {
        return preSpell.GetCooldown();
    }
     public override int GetIcon()
    {
        return preSpell.GetIcon();
    }

    protected override float GetProjectileSpeed()
    {
        return preSpell.GetProjectileSpeed();
    }
    public override string GetTrajectory()
    {
        return preSpell.GetTrajectory();
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        yield return preSpell.Cast(where, target, team);
    }
}


//the class for spell mods themselves... lots of superflous fields except for name/desc
public class ModifierData
{
    public string name;
    public string description;
    public int?  damage_multiplier; //just a buuuuunch of optional fields!!
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
    public override int GetDamage()
    {
        return preSpell.GetDamage() * data.damage_multiplier.Value;
    }
    public override int GetManaCost()
    {
        return preSpell.GetManaCost() * data.mana_multiplier.Value;
    }
}

public class SpeedAmp : SpellModifier
{
    private ModifierData data;
    public SpeedAmp(Spell inner, ModifierData modInfo) : base(inner)
    {
        data = modInfo;
    }
    protected override float GetProjectileSpeed()
    {
        int multiplier  = data.speed_multiplier;
        return preSpell.GetProjectileSpeed() * multiplier;
    }
}

public class Doubler : SpellModifier
{
    private ModifierData data; 
    public Doubler(Spell inner, ModifierData modInfo) :base(inner)
    {
        data = modInfo;
    }
    protected override int GetManaCost()
    {
        return preSpell.GetManaCost() * data.mana_multiplier;
    }
    protected override float GetCooldown()
    {
        return preSpell.GetCooldown() * data.cooldown_multiplier;
    }
    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        yield return preSpell.Cast(where, target, team);
        yield return new WaitForSeconds(data.delay);
        yield return preSpell.Cast(where, target, team);
    }
}

public class Splitter : SpellModifier
{
    private ModifierData data;
    public Splitter(Spell inner, ModifierData modInfo): base(inner)
    {
        data = modInfo;
    }
    protected override int GetManaCost()
    {
        return preSpell.GetManaCost() * data.mana_multiplier;
    }
    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        Vector3 dir = (target - where).normalized;

        float angle = data.angle;

        Vector3 dir1 = Quaternion.Euler(0, 0, angle) * dir;
        Vector3 dir2 = Quaternion.Euler(0, 0, -angle) * dir;

        yield return preSpell.Cast(where, where + dir1, team);
        yield return preSpell.Cast(where, where + dir2, team);
    }
}

public class Chaos : SpellModifier
{
    private ModifierData data;

    public Chaos(Spell inner, ModifierData modInfo): base(inner)
    {
        data = modInfo;
    }
    protected override int GetDamage()
    {
        return preSpell.GetDamage(); // multiply and calculate damage with RPN
    }
    protected override string GetTrajectory()
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
    protected override int GetDamage()
    {
        return preSpell.GetDamage() * data.damage_multiplier ?? 1;
    }
    protected override int GetManaCost()
    {
        return preSpell.GetManaCost() + data.mana_adder ?? 0;
    }
    protected override string GetTrajectory()
    {
        return data.projectile_trajectory ?? preSpell.GetTrajectory();
    }
}

