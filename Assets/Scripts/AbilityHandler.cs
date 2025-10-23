using UnityEngine;
using System;
using System.Runtime.CompilerServices;

public class AbilityHandler : MonoBehaviour
{
    ClothingStats clothingStats;
    bool hasDied = false;
    private void OnEnable() { if (GameManager.Instance != null) GameManager.Instance.OnTurnStart += HandleTurnStart; GameManager.Instance.OnAnyDeath += HandleAnyDeath; }

    private void OnDisable() { if (GameManager.Instance != null) GameManager.Instance.OnTurnStart -= HandleTurnStart; GameManager.Instance.OnAnyDeath -= HandleAnyDeath; }

    public void SetStats(ClothingStats newStats)
    {
        clothingStats = newStats;
        ExecuteForTrigger(AttackTrigger.OnEnter);
    }

    void HandleTurnStart()
    {
        ExecuteForTrigger(AttackTrigger.OnTurnStart);
    }
    public void HandleDeath()
    {
        if (!hasDied) { ExecuteForTrigger(AttackTrigger.OnDeath); hasDied = true; }
    }
    public void HandleAnyDeath()
    {
        ExecuteForTrigger(AttackTrigger.OnAnyDeath);
    }

    private void ExecuteForTrigger(AttackTrigger trigger)
    {
        if (clothingStats == null) return;
        foreach (var attack in clothingStats.attackTriggers)
        {
            if (attack.trigger == trigger)
            {
                // Call the function dynamically
                SendMessage(attack.attackFunction, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public void Heal()
    {
        GameAction newAction = new HealAction
        {
            dialog = new string[] { $"You do a YOGA POSE, healing 1 HP. God, why didn't you stretch before this fight?" },
            caller = gameObject,
            target = gameObject,
            amount = 1
        };
        GameManager.Instance.gameActions.Add(newAction);
    }
    public void SteelChainz()
    {
        GameAction newAction = new RemoveBuffs
        {
            dialog = new string[] { $"Your steel chainz help you CUT THROUGH THE BULLSHIT", "You clear away all buffs and debuffs" },
            caller = gameObject,
            target = gameObject,
        };
        GameAction targetAction = new TargetAction
        {
            gameAction = newAction,
            caller = gameObject,
            description = "Heal Ally",
            targetTag = "EnemyCharacter"
        };
        GameManager.Instance.gameActions.Add(targetAction);
    }
    public void MedicBag()
    {
        GameAction newAction = new HealAction
        {
            dialog = new string[] { $"You pass a duchie, on the DL. You feel revitalized" },
            caller = gameObject,
            target = gameObject,
            amount = 2
        };
        GameAction targetAction = new TargetAction
        {
            gameAction = newAction,
            caller = gameObject,
            description = "Heal Ally",
            targetTag = "Character"
        };
        GameManager.Instance.gameActions.Add(targetAction);
    }
    public void Borks()
    {
        GameAction diceAttack = new FireAttackAction
        {
            caller = gameObject,
            animName = "Skeleton_Pose",
        };
        GameAction targetAction = new TargetAction
        {
            gameAction = diceAttack,
            caller = gameObject,
            description = "Throw the Chancla",
            targetTag = "EnemyCharacter"
        };
        GameManager.Instance.gameActions.Add(targetAction);
    }
    public void HotPants()
    {
        GameAction diceAttack = new FireAttackAction
        {
            caller = gameObject,
            animName = "Skeleton_Defense4",
            hits = 2,
        };
        GameAction targetAction = new TargetAction
        {
            gameAction = diceAttack,
            caller = gameObject,
            description = "Pop That Ass",
            targetTag = "EnemyCharacter"
        };
        GameManager.Instance.gameActions.Add(targetAction);
    }
    public void Bracers()
    {
        GameAction bracers = new StatusEffectAction
        {
            caller = gameObject,
            target = gameObject,
            statusEffect = new StatusEffect()
            {
                name = Status.ArmorBoost,
                amount = 8,
                duration = 1
            },
            dialog = new string[] { "You raise your bracelets up in an anime block", "You look down at your biceps and give them a little pop", "God, you feel so strong. You gain 8 armor this turn" }
        };
        GameManager.Instance.gameActions.Add(bracers);
    }
    public void DoubleAttack()
    {
        GameAction bracers = new StatusEffectAction
        {
            caller = gameObject,
            target = gameObject,
            statusEffect = new StatusEffect()
            {
                name = Status.AttackBoost,
                amount = GetComponentInChildren<Health>().stats.damage,
                duration = 1
            },
            dialog = new string[] { "The gold DOUBLE G on your belt glints in the sunlight as you thrust your hips forward","Your attack doubles" }
        };
        GameManager.Instance.gameActions.Add(bracers);
    }
    public void InvinciblePants()
    {
        GameAction bracers = new StatusEffectAction
        {
            caller = gameObject,
            target = gameObject,
            statusEffect = new StatusEffect()
            {
                name = Status.ArmorBoost,
                amount = 8,
                duration = 1
            },
            dialog = new string[] { "Peace and love man. You are invincible this turn. Groovy." }
        };
        GameManager.Instance.gameActions.Add(bracers);
    }
    public void Heartbreaker()
    {
        GameAction heartbreaker = new StatusEffectAction
        {
            caller = gameObject,
            target = gameObject,
            statusEffect = new StatusEffect()
            {
                name = Status.AttackBoost,
                amount = 1,
                duration = 1
            },

            dialog = new string[] { "Girl, you are lookin' hella fresh" }
        };
        GameAction targetAction = new TargetAction
        {
            gameAction = heartbreaker,
            caller = gameObject,
            description = "Boost Attack",
            targetTag = "Character"
        };
        GameManager.Instance.gameActions.Add(targetAction);
    }
    public void Mime()
    {
        GameAction heartbreaker = new StatusEffectAction
        {
            caller = gameObject,
            target = gameObject,
            statusEffect = new StatusEffect()
            {
                name = Status.ArmorBoost,
                amount = 4,
                duration = 1
            },

            dialog = new string[] { $"You build an invisible wall. It grants 4 armor this turn"}
        };
        GameAction targetAction = new TargetAction
            {
                gameAction = heartbreaker,
                caller = gameObject,
                description = "Boost Armor",
                targetTag = "Character"
            };
        GameManager.Instance.gameActions.Add(targetAction);
    }
     public void MurderGloves()
    {
        GameAction gloves = new StatusEffectAction
        {
            caller = gameObject,
            target = gameObject,
            statusEffect = new StatusEffect()
            {
                name = Status.AttackBoost,
                amount = 1,
                duration = -1
            },
            dialog = new string[] { "Blood! The crimson droplets of life washing over your gloves. It fills you with grotesque joy. You feel alive." }
        };
    }
    public void CatBag()
    {
        GameAction diceAttack = new AttackAction
        {
            dice = 2,
            bonus = 10,
            caller = gameObject,
            animationName = "Skeleton_Cat_Bag",
            statusEffect = new StatusEffect() { name = Status.AttackBoost, amount = -2, duration = 1 }
        };
        GameAction targetAction = new TargetAction
        {
            gameAction = diceAttack,
            caller = gameObject,
            description = "Handbag Swing",
            targetTag = "EnemyCharacter"
        };
        GameManager.Instance.gameActions.Add(targetAction);
    }
    public void HairSword()
    {
        GameAction diceAttack = new AttackAction
        {
            dice = 1,
            bonus = 4,
            caller = gameObject,
            animationName = "Skeleton_Hair_Sword",
            damageTypes = new DamageType[] {DamageType.Sassy},
            endTurn = false,
        };
        GameAction targetAction = new TargetAction
        {
            gameAction = diceAttack,
            caller = gameObject,
            description = "Throw the Hair Sword",
            targetTag = "EnemyCharacter"
        };
        GameManager.Instance.gameActions.Add(targetAction);
    }
    public void GunBoots()
    {
        GameAction diceAttack = new AttackAction
        {
            dice = 5,
            bonus = 0,
            caller = gameObject,
            damageTypes = new DamageType[] { DamageType.Hot },
            animationName = "Skeleton_Gun_Heels",
        };
        GameAction targetAction = new TargetAction
        {
            gameAction = diceAttack,
            caller = gameObject,
            description = "Gun Boots",
            targetTag = "EnemyCharacter"
        };
        GameManager.Instance.gameActions.Add(targetAction);
    }
    public void Jeans()
    {
        GameAction diceAttack = new AttackAction
        {
            dice = 2,
            bonus = 4,
            caller = gameObject,
            damageTypes = new DamageType[] { DamageType.Sassy },
            animationName = "Skeleton_Kick",
        };
        GameAction targetAction = new TargetAction
        {
            gameAction = diceAttack,
            caller = gameObject,
            description = "Kickin' Jeans",
            targetTag = "EnemyCharacter"
        };
        GameManager.Instance.gameActions.Add(targetAction);
    }
    public void Impale()
    {
        GameAction diceAttack = new AttackAction
        {
            dice = 1,
            bonus = 10,
            caller = gameObject,
            damageTypes = new DamageType[] { DamageType.Sassy },
            statusEffect = new StatusEffect() {name = Status.ArmorBoost, amount = -4, duration = 1},
            animationName = "Skeleton_Kick",
        };
        GameAction targetAction = new TargetAction
        {
            gameAction = diceAttack,
            caller = gameObject,
            description = "Impale with Heel",
            targetTag = "EnemyCharacter"
        };
        GameManager.Instance.gameActions.Add(targetAction);
    }
    public void BigHoops()
    {
        GameAction newAction = new TextAction
        {
            dialog = new string[] { $"You become unimaginably sassy. Bougie. Ratchet.", "Your swagger doubles" },
            caller = gameObject,
            target = gameObject,
            animName = "Skeleton_Pose"
        };
        GetComponent<SummonModel>().AddSwag(GetComponent<SummonModel>().swag);
        GameManager.Instance.gameActions.Add(newAction);
    }
    public void Amulet()
    {
        GameAction newAction = new TextAction
        {
            dialog = new string[] { $"You rub the amulet. A genie pops out.", "It says, FUCK OFF", "Ugh. You have to do everything yourself around here. Oh well.", "You gain an extra turn" },
            caller = gameObject,
            target = gameObject,
            animName = "Skeleton_Pose"
        };
        GetComponent<SummonModel>().actions++;
        GameManager.Instance.gameActions.Add(newAction);
    }
    public void ExtraTurn()
    {
        GameAction newAction = new TextAction
        {
            dialog = new string[] { $"Because you are the main character, you gain an extra turn" },
            caller = gameObject,
            target = gameObject,
            animName = "Skeleton_Pose"
        };
        GetComponent<SummonModel>().actions++;
        GameManager.Instance.gameActions.Add(newAction);
    }

    public void Flutterskirt()
    {
        print("Fluuterskirt Not Programmed");
    }
    public void PoisonCloud()
    {
        GameAction poisonAction = new FireAllAttack()
        {
            damageEnemies = true,
            damageModels = true,
            hits = 1,
            caller = gameObject,
            animName = "PoisonAttack",
            dialog = new string[] { $"The enemy explodes in a cloud of fire." }
        };
        GameManager.Instance.gameActions.Add(poisonAction);
    }
    public void Kamehameha()
    {
        GameAction poisonAction = new FireAllAttack()
        {
            damageEnemies = true,
            damageModels = true,
            hits = 2,
            caller = gameObject,
            animName = "Skeleton_Kamehameha",
        };
        GameManager.Instance.gameActions.Add(poisonAction);
    }
    public void RippedTop()
    {
        GameAction selfHarm = new SelfHarmAction()
        {
            target = gameObject,
            caller = gameObject,
            damage = 10,
            dialog = new string[] { "Wearing this top out was a bad idea. It fills you with shame." }
        };
        GameManager.Instance.gameActions.Add(selfHarm);
    }
    public void Haste()
    {
        GameAction newAction = new HasteAction
        {
            dialog = new string[] { $"In a zen-like burst of laser focus, you change clothes immediately.","You do your makeup in 5 seconds flat.","You look like shit. But you are ready for war" },
            caller = gameObject,
        };
        GameManager.Instance.gameActions.Add(newAction);
    }
}
