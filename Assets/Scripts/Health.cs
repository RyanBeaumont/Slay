using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public enum Status {None, AttackBoost, BonusBoost, ArmorBoost, Poison}
public enum DamageType {Hot, Cute, Sassy}

[Serializable]
public class StatusEffect
{
    public Status name;
    public int amount;
    public int duration;
    public bool stack = false;
}

public class Health : MonoBehaviour
{
    public ClothingStats stats;
    public int maxHp = 100;
    public int hp = 100;
    public DamageType[] weakness;
    public DamageType[] resistance;
    public TMP_Text powerText;
    public GameObject heartPrefab;
    public RectTransform heartContainer;
    public TMP_Text nameText;
    public TMP_Text bonusText;
    public Image healthBar;
    public Image healthBarAnim;
    public Image healthBg;
    ClothingStats baseStats;
    public List<StatusEffect> statusEffects = new List<StatusEffect>();
    private void OnEnable()
    {
        if (GameManager.Instance != null)
        GameManager.Instance.OnTurnStart += NewTurn;
    }
    private void OnDisable()
    {
        if (GameManager.Instance != null)
        GameManager.Instance.OnTurnStart -= NewTurn;
    }
    public void SetName(string name){ nameText.text = name; }


    void Update()
    {
        healthBar.fillAmount = (float)hp / (float)maxHp;
        if (healthBarAnim.fillAmount > healthBar.fillAmount) healthBarAnim.fillAmount -= 0.2f * Time.deltaTime;
        if (healthBarAnim.fillAmount < healthBar.fillAmount) healthBarAnim.fillAmount = healthBar.fillAmount;
         healthBg.rectTransform.localScale = new Vector2((float)maxHp/100f, healthBg.rectTransform.localScale.y);
    }
    public void UpdateDisplay()
    {
        //Apply status effects
        stats = new ClothingStats() { hp = stats.hp, armor = baseStats.armor, bonus = baseStats.bonus, damage = baseStats.damage };
        Color armorColor = Color.white;
        bonusText.color = Color.cyan;
        Color powerColor = Color.red;
        foreach (StatusEffect effect in statusEffects)
        {
            if (effect.name == Status.AttackBoost) { stats.damage += effect.amount; powerText.color = Color.magenta; }
            else if (effect.name == Status.ArmorBoost) { stats.armor += effect.amount; armorColor = Color.magenta; }
            else if (effect.name == Status.BonusBoost) { stats.bonus += effect.amount; bonusText.color = Color.magenta; }
        }
        powerText.text = $"{stats.damage}";
        if (stats.bonus != 0) bonusText.text = $"+{stats.bonus}"; else bonusText.text = "";
        foreach (RectTransform child in heartContainer)
            Destroy(child.gameObject);
        /*
for (int i = 0; i < maxHp; i++)
{
GameObject h = Instantiate(heartPrefab, heartContainer);
if (stats.hp <= i)
{
    h.GetComponentInChildren<TMP_Text>().text = "";
    h.GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>("BrokenHeart");
    h.GetComponentInChildren<Image>().color = Color.black;
}
else
{
    h.GetComponentInChildren<TMP_Text>().text = $"{stats.armor}";
    h.GetComponentInChildren<Image>().color = armorColor;
}
}
*/
    }

    public void AddStatusEffect(StatusEffect s)
    {
        if (s.stack)
        {
            foreach (StatusEffect effect in statusEffects)
            {
                if (effect.name == s.name)
                {
                    effect.amount += s.amount;
                    UpdateDisplay();
                    return;
                }
            }
        }
        statusEffects.Add(s);
        UpdateDisplay();
    }
    
    public void ClearStatusEffects()
    {
        statusEffects.Clear();
        UpdateDisplay();
    }

    public void NewTurn()
    {
        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            var effect = statusEffects[i];
            if (effect.duration != -1)
            {
                effect.duration--;
                if (effect.duration <= 0)
                {
                    statusEffects.RemoveAt(i);
                }
            }
            if(effect.name == Status.Poison){
                GameAction poison = new SelfHarmAction()
                {
                    damage = effect.amount * 5,
                    dialog = new string[] { $"You take {effect.amount * 5} poison damage" }
                };
                GameManager.Instance.gameActions.Add(poison);
                effect.amount -= 1;
                if (effect.amount <= 0) statusEffects.RemoveAt(i);
            }
        }
        UpdateDisplay();
    }

    void Awake() {
        
    }

    public void SetStats(ClothingStats s, int hpPerHeart)
    {
        stats = s;
        baseStats = s;
        maxHp = s.hp * hpPerHeart;
        hp = maxHp;
        UpdateDisplay();
    }


    public int Damage(int damagePerHit = 10, bool guaranteedHit = false, int bonus = 0, DamageType[] damageType = null)
    {
        damageType ??= System.Array.Empty<DamageType>();
        GameObject prompt = Instantiate(Resources.Load<GameObject>("HitPrompt"), transform.position, Quaternion.identity);
        TMP_Text text = prompt.GetComponentInChildren<TMP_Text>();
        int roll = UnityEngine.Random.Range(1, 13);
        text.color = Color.magenta;
        bool weakCrit = false;
        if (roll + bonus >= stats.armor || guaranteedHit == true)
        {
            if (roll >= 11)
            {
                text.text = "TO THE NUTS!";
                text.color = Color.yellow;
                hp -= Mathf.RoundToInt((float)damagePerHit * 1.25f); GetComponentInChildren<Shaker>().Shake(0.3f);
                GameManager.Instance.swag += 1;
                if(weakness.Any(item => damageType.Contains(item)))
                {
                    text.text = "Super Effective!";
                    weakCrit = true;
                }
            } //Critical
            else
            {
                if (resistance.Any(item => damageType.Contains(item))) //If any damage types are resistant, reduce damage
                {
                    hp -= (int)(damagePerHit / 2);
                    text.text += " - Weak";
                }
                else
                {
                    hp -= damagePerHit;
                }
                
                print($"Hp: {hp} / {maxHp}");
                GetComponentInChildren<Shaker>().Shake(0.1f);
                string[] prompts = { "Ouch", "Destroyed", "Pain", "Smack", "Krack", "Pow", "Disrespected", "Shame" };
                text.text = prompts[UnityEngine.Random.Range(0, prompts.Length)];
            }
            GameManager.Instance.Sound("s_camera", UnityEngine.Random.Range(0.8f, 1.2f));

            SummonModel summonModel = GetComponentInParent<SummonModel>();
            if (summonModel != null)
            {
                //Permanently update health in overworld
                summonModel.teammate.equippedOutfit.damageTaken = maxHp - hp;
            }

            if (hp <= 0)
            {
                Enemy enemy = GetComponentInParent<Enemy>();
                if (enemy != null)
                {
                    enemy.Die();
                }
                if (summonModel != null && summonModel.playerState != PlayerState.Dead)
                {
                    print("KILLING PLAYER");
                    summonModel.SetState(PlayerState.Dead);
                }
                AbilityHandler abilityHandler = GetComponentInParent<AbilityHandler>();
                if (abilityHandler)
                {
                    abilityHandler.HandleDeath();
                }
                
                

            }
            UpdateDisplay();
            if (weakCrit) return 2; else return 1;
        }
        else
        {
            text.color = Color.cyan;
            string[] prompts = { "Miss me with that", "Nuh uh", "Not a chance", "Now you gotta kiss me", "Too smooth", "Effortless", "To the abs", "Try harder" };
            text.text = prompts[UnityEngine.Random.Range(0, prompts.Length)];
            GameManager.Instance.Sound("s_miss", UnityEngine.Random.Range(0.8f, 1.2f));
        }
        return 0;
    }
}
