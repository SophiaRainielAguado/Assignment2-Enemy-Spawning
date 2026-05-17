using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpellCaster
{
    public int mana;
    public int max_mana;
    public int mana_reg;
    public Hittable.Team team;

    public int spellpower;
    public int wave;

    public Spell spell;
    private SpellBuilder spellBuilder;

    public SpellCaster(int mana, int max_mana, int mana_reg, Hittable.Team team, SpellBuilder builder)
    {
        this.mana = mana;
        this.max_mana = max_mana;
        this.mana_reg = mana_reg;
        this.team = team;

        this.spellBuilder = builder;

        spell = spellBuilder.Build("arcane_bolt", this);
    }

    public IEnumerator ManaRegeneration()
    {
        while (true)
        {
            mana = Mathf.Min(mana + mana_reg, max_mana);
            yield return new WaitForSeconds(1);
        }
    }

    public IEnumerator Cast(Vector3 where, Vector3 target)
    {
        if (mana >= spell.GetManaCost(spellpower) && spell.IsReady(spellpower, wave))
        {
            mana -= spell.GetManaCost(spellpower);
            yield return spell.Cast(where, target, team, spellpower, wave);
        }
    }

    public void SetSpell(string spellName)
    {
        spell = spellBuilder.Build(spellName, this);
    }
}