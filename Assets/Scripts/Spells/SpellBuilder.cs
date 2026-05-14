using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;


public class SpellBuilder
{
    private Dictionary<string, spelldata> spells;

    public SpellBuilder(Dictionary<string, spelldata> spells)
    {
        this.spells = spells;
    }

    public Spell Build(SpellCaster owner, string spellName)
    {
        var data = spells[spellName];
        return new Spell(owner, data);
    }
}