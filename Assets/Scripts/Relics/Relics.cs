using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class RelicData
{
    public virtual string name;
    public virtual int sprite;
    public TriggerInfo trigger;
    public EffectInfo effect;

}

public class EffectInfo
{
    public virtual string description;
    public virtual string type;
    public virtual string amount;
    public virtual string? until;
}
public class TriggerInfo
{
    public virtual string description;
    public virtual string type;
    public virtual string? amount;

}