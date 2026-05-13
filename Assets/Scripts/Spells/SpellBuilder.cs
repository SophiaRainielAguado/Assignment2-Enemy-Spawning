using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;


public class SpellBuilder
{
    Dictionary<string, spelldata> spells;

    public Spell Build(SpellCaster owner)
    {
        var data = spells["Bolt"];
        return new Spell(owner, data);
    }

    public SpellBuilder()
    {
        spells = new Dictionary<string, spelldata>();

        var spellText = Resources.Load<TextAsset>("spells");
        JToken jo = JToken.Parse(spellText.text);

        foreach (var token in jo)
        {
            spelldata s = token.ToObject<spelldata>();
            spells[s.name] = s;
        }
    }
}
