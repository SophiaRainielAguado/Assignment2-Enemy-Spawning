using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;


public class SpellBuilder 
{

    private Dictionary<string, SpellData> spellDictionary;
    public Spell Build(string spellName, SpellCaster owner)
    {
        SpellData spellInfo = spellDictionary[spellName];
        return new Spell(owner, spellInfo);
    }

   
    public SpellBuilder(Dictionary<string, SpellData> spellDB)
    {
        spellDictionary = spellDB;
    }

}
