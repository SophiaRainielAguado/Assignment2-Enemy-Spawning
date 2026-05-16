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
    public string mana_cost;
    public string cooldown;
    public ProjectileInfo projectile;
    public SecondaryProjectileInfo? secondary_projectile;
}

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

    public float GetProjectileSpeed()
    {
          //evaluate data.projectile.speed w the rpn later
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

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        yield return preSpell.Cast(where, target, team);
    }
}

public class ModifierData
{
    public string name;
    public string description;
    public int?  damage_multiplier; //just a buuuuunch of optional fields!!
    public int? mana_multiplier;
    public int? speed_multiplier;
    public int? cooldown_multiplier;
    public int? angle;
    public string? projectile_trajectory;
    public int? mana_adder;
}

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
    public SpeedAmpModifier(Spell inner, ModifierData modInfo) : base(inner)
    {
        data = modInfo;
    }
    protected override float GetProjectileSpeed()
    {
        float multiplier  = data.speed_multiplier;
        return preSpell.GetProjectileSpeed() * multiplier;
    }
}

