using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;


public class SpellData // can be used for all the base spells; some fields will be left null 
{
    public string name;
    public string description;
    public int icon;
    public DamageInfo damage;
    public string mana_cost;
    public string cooldown;
    public ProjectileInfo projectile;
    public SecondaryProjectileInfo secondary_projectile;
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

    public string GetName()
    {
        return data.name;
    }

    public int GetManaCost()
    {
        return data.mana_cost;
    }

    public int GetDamage()
    {
        return data.damage; //not sure if i have to run it thru the calculator first?
    }

    public float GetCooldown()
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

    public virtual IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        GameManager.Instance.projectileManager.CreateProjectile(data.projectile.sprite, data.projectile.trajectory, where, target - where, 15f, OnHit);
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
