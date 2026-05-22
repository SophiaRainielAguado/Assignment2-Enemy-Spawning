using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

public class SpellBuilder
{
    Dictionary<string, SpellData> spells;

    public SpellBuilder()
    {
        LoadSpells();
    }

    void LoadSpells()
    {
        spells = new Dictionary<string, SpellData>();

        TextAsset spellText = Resources.Load<TextAsset>("spells");

        if (spellText == null)
        {
            Debug.LogError("spells.json not found in Resources!");
            return;
        }

        spells = JsonConvert.DeserializeObject<Dictionary<string, SpellData>>(spellText.text);
    }

    public Spell Build(SpellCaster owner, SpellData data)
    {
        return new Spell(owner, data);
    }
}