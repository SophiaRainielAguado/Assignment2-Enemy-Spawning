using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellUI : MonoBehaviour
{
    public GameObject icon;
    public RectTransform cooldown;
    public TextMeshProUGUI manacost;
    public TextMeshProUGUI damage;
    public GameObject highlight;
    public Spell spell;
    float last_text_update;
    const float UPDATE_DELAY = 1;
    public GameObject dropbutton;
    public SpellCaster spellcaster;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        last_text_update = 0;
    }

    public void SetSpell(Spell spell)
    {
        this.spell = spell;
        GameManager.Instance.spellIconManager.PlaceSprite(spell.GetIcon(), icon.GetComponent<Image>());
    }

    public void SetSpellCaster(SpellCaster caster)
    {
        spellcaster = caster;
    }

    // Update is called once per frame
    void Update()
    {
        if (spell == null) return;
        if (Time.time > last_text_update + UPDATE_DELAY)
        {
            manacost.text = spell.GetManaCost(spellcaster.spellpower).ToString();
            damage.text = spell.GetDamage(spellcaster.spellpower, GameManager.Instance.currentWave).ToString();
            last_text_update = Time.time;
        }
        
        float since_last = Time.time - spell.last_cast;
        float perc;
        if (since_last > spell.GetCooldown(spellcaster.spellpower, GameManager.Instance.currentWave))
        {
            perc = 0;
        }
        else
        {
            perc = 1-since_last / spell.GetCooldown(spellcaster.spellpower, GameManager.Instance.currentWave);
        }
        cooldown.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 48 * perc);
    }
}
