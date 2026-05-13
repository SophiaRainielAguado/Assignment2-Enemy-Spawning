using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;


public class SpellBuilder 
{

    public Spell Build(SpellCaster owner)
    {
        Spell spell = new Spell(owner);
        spell.data = // load the spells.json file
        return spell;
    }

   
    public SpellBuilder()
    {        
    }

}
