using UnityEngine;
using System;

public class AbilityHandler : MonoBehaviour
{
    ClothingStats clothingStats;
    private void OnEnable() { if (GameManager.Instance != null) GameManager.Instance.OnTurnStart += HandleTurnStart; }

    private void OnDisable() { if (GameManager.Instance != null) GameManager.Instance.OnTurnStart -= HandleTurnStart; }

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
        ExecuteForTrigger(AttackTrigger.OnDeath);
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
    public void Borks()
    {
        GameAction diceAttack = new FireAttackAction
        {
            caller = gameObject,
            animName = "Skeleton_Pose"
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
    public void Heartbreaker()
    {
        GameAction diceAttack = new DiceAttackAction
        {
            dice = 1,
            bonus = 4,
            caller = gameObject,
            animName = "Skeleton_Pose"
        };
        GameAction targetAction = new TargetAction
        {
            gameAction = diceAttack,
            caller = gameObject,
            description = "Heartbreaker - Deal 1 Damage",
            targetTag = "EnemyCharacter"
        };
        GameManager.Instance.gameActions.Add(targetAction);
    }
    public void Flutterskirt()
    {
        print("Fluuterskirt Not Programmed");
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
