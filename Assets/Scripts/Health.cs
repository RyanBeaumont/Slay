using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public enum Status { AttackBoost, BonusBoost, ArmorBoost}
[Serializable]
public class StatusEffect
{
    public Status name;
    public int amount;
    public int duration;
}
[Serializable]
public class Attributes
{
    public int vitality = 10;
    public int spirit = 10;
    public int swagger = 1;
}
public class Health : MonoBehaviour
{
    public ClothingStats stats;
    public int maxHp = 10;
    public TMP_Text powerText;
    public GameObject heartPrefab;
    public RectTransform heartContainer;
    public TMP_Text nameText;
    public TMP_Text bonusText;
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
    public void UpdateDisplay()
    {
        //Apply status effects
        stats = new ClothingStats() {hp = stats.hp, armor = baseStats.armor, bonus = baseStats.bonus, damage = baseStats.damage};
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
    }

    public void AddStatusEffect(StatusEffect s)
    {
        statusEffects.Add(s);
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
        }
        UpdateDisplay();
    }

    void Awake() {
        
    }

    public void SetStats(ClothingStats s)
    {
        stats = s;
        baseStats = s;
        maxHp = s.hp;
        UpdateDisplay();
    }


    public bool Damage(int roll, int bonus)
    {
        GameObject prompt = Instantiate(Resources.Load<GameObject>("HitPrompt"), transform.position, Quaternion.identity);
        TMP_Text text = prompt.GetComponentInChildren<TMP_Text>();
        text.color = Color.magenta;
        if (roll + bonus >= stats.armor)
        {
            if (roll == 12)
            {
                text.text = "TO THE NUTS!";
                text.color = Color.yellow;
                stats.hp -= 2; GetComponentInChildren<Shaker>().Shake(0.3f);
            } //Critical
            else
            {
                stats.hp -= 1;
                GetComponentInChildren<Shaker>().Shake(0.1f);
                string[] prompts = { "Ouch", "Destroyed", "Pain", "Smack", "Krack", "Pow", "Disrespected", "Shame" };
                text.text = prompts[UnityEngine.Random.Range(0, prompts.Length)];
            }
            GameManager.Instance.Sound("s_punch", UnityEngine.Random.Range(0.8f, 1.2f));

            SummonModel summonModel = GetComponentInParent<SummonModel>();
            if (summonModel != null)
            {
                //Permanently update health in overworld
                summonModel.teammate.equippedOutfit.damageTaken = maxHp - stats.hp;
            }

            if (stats.hp <= 0)
            {
                AbilityHandler abilityHandler = GetComponentInParent<AbilityHandler>();
                if (abilityHandler)
                {
                    abilityHandler.HandleDeath();
                }
                if (summonModel != null && summonModel.playerState != PlayerState.Dead)
                {
                    print("KILLING PLAYER");
                    summonModel.SetState(PlayerState.Dead);
                }
                Enemy enemy = GetComponentInParent<Enemy>();
                if (enemy != null)
                {
                    enemy.Die();
                }

            }
            UpdateDisplay();
            return true;
        }
        else
        {
            text.color = Color.cyan;
            string[] prompts = { "Miss me with that", "Nuh uh", "Not a chance", "Now you gotta kiss me", "Too smooth", "Effortless", "To the abs", "Try harder" };
            text.text = prompts[UnityEngine.Random.Range(0, prompts.Length)];
            GameManager.Instance.Sound("s_miss", UnityEngine.Random.Range(0.8f, 1.2f));
        }
        return false;
    }
}
