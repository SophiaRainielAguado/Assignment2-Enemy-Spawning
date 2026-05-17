using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Channels;



public class SpellBuilder 
{

    private Dictionary<string, SpellData> spellDictionary;
    public Spell Build(string spellName, SpellCaster owner)
    {
        SpellData spellInfo = spellDictionary[spellName];
        switch (spellName)
        {
            case "arcane_bolt":
                return new ArcaneBolt(owner, spellInfo);
            case "magic_missile":
                return new MagicMissile(owner, spellInfo);
        }
        return new Spell(owner, spellInfo);
    }

   
    public SpellBuilder(Dictionary<string, SpellData> spellDB)
    {
        spellDictionary = spellDB;
    }

}

public class SpellModifierBuilder
{
    private Dictionary<string, ModifierData> modifierDictionary;

    public SpellModifierBuilder(Dictionary<string, ModifierData> modifierDB)
    {
        modifierDictionary = modifierDB;
    }
    public Spell ApplyModifier(string modifierName, Spell spell)
    {
        if (!modifierDictionary.ContainsKey(modifierName))
            return spell;

        ModifierData modifierInfo = modifierDictionary[modifierName];

        switch (modifierName)
        {
            case "damage_amp":
                return new DamageAmpModifier(spell, modifierInfo);
            case "speed_amp":
                return new SpeedAmp(spell, modifierInfo);
            case "doubler":
                return new Doubler(spell, modifierInfo);
            case "splitter":
                return new Splitter(spell, modifierInfo);
            case "chaos":
                return new Chaos(spell,modifierInfo);
            case "homing":
                return new Homing(spell,modifierInfo);
        }
        return spell;
    }
}
