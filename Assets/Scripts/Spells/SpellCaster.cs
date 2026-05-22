using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpellCaster 
{
    public int mana;
    public int max_mana;
    public int mana_reg;
    public int spellpower;

    public Hittable.Team team;
    public Spell spell;
    public SpellData spellData;

    public SpellCaster(int mana, int mana_reg, int spellpower, Hittable.Team team, SpellData data)
    {
        this.mana = mana;
        this.max_mana = mana;
        this.mana_reg = mana_reg;
        this.spellpower = spellpower;
        this.team = team;

        this.spellData = data;
        spell = new SpellBuilder().Build(this, data);
    }

    public void SetSpell(SpellData data)
    {
        spellData = data;
        spell = new SpellBuilder().Build(this, data);
    }

    public IEnumerator Cast(Vector3 where, Vector3 target, int wave)
    {
        if (mana >= spell.GetManaCost(spellpower) &&
            spell.IsReady(spellpower, wave))
        {
            mana -= spell.GetManaCost(spellpower);

            yield return spell.Cast(where, target, team, spellpower, wave);
        }
    }

    public IEnumerator ManaRegeneration()
    {
        while (true)
        {
            mana += mana_reg;
            mana = Mathf.Min(mana, max_mana);
            yield return new WaitForSeconds(1);
        }
    }
}
