using UnityEngine;
using System;
using System.Collections;
using System.Linq;

public class GameActions
{
    
}

[Serializable]
public abstract class GameAction
{
    public string[] dialog = new string[0];
    public GameObject caller = null;
    public GameObject target = null;
    public abstract void Execute();
}

[Serializable]
public sealed class TextAction : GameAction
{
    [TextArea] public string[] text;
    public override void Execute()
    {
        throw new NotImplementedException();
    }
}

[Serializable]
public sealed class SummonAction : GameAction
{
    public Outfit outfit;
    public int cost;
    public override void Execute()
    {
        if (caller == null)
        {
            Debug.Log("CALLER IS NULL");
            return;

        }
        //Add the outfit back to the active list only if you're not dead
        if (caller.GetComponent<SummonModel>().playerState != PlayerState.Dead)
        {
            OverworldController.Instance.yourOutfits.Add(caller.GetComponent<SummonModel>().teammate.equippedOutfit);
        }
        GameManager.Instance.descriptionText.text = $"{outfit.name} is revealed to the world";
        caller.GetComponent<SummonModel>().ChangeCharacter(outfit);
        
        GameManager.Instance.runTimer = true;
    }
}

[Serializable]
public sealed class EnemyDeathAction : GameAction
{
     
    public override void Execute()
    {
        if (caller == null)
    {
        Debug.Log("CALLER IS NULL");
        return;

    }
        Debug.Log("ENEMY DEATH");
        UnityEngine.Object.Destroy(caller.transform.gameObject);
    }
}

[Serializable]
public sealed class DeathAction : GameAction
{
    public override void Execute()
    {
        if (caller == null)
    {
        Debug.Log("CALLER IS NULL");
        return;

    }
        Debug.Log("DEATH ACTION TRIGGER");
        SummonModel model = caller.GetComponent<SummonModel>();
        Teammate teammate = model.teammate;
        OverworldController.Instance.yourDiscard.Add(teammate.equippedOutfit);
        if (OverworldController.Instance.yourOutfits.Count > 0)
        {
            GameManager.Instance.gameActions.Add(new DeathSwap()
            {
                characterIndex = teammate.index,
                caller = caller,
                dialog = new string[] { "You feel your life force fading... You need to... change.. clothes... ASAP..." }
            }); //Trigger the swap out immediately
        }
        else
        {
            GameManager.Instance.CheckForLose();
            //GameManager.Instance.runTimer = true;
        }
        model.customAnimator.Play("Skeleton_Dead", 0, canLoop: false);
        
    }
}

[Serializable]
public sealed class DeathSwap : GameAction
{
    public int characterIndex;
    public override void Execute()
    {
        Camera.main.GetComponent<CameraController>().MoveCamera(caller.transform.position + new Vector3(0f, 3f, 0f), 3f);
        UnityEngine.Object.FindFirstObjectByType<BattleMenu>().owner = caller.GetComponent<SummonModel>();
        GameManager.Instance.handOfCards.SetActive(true);
        GameManager.Instance.handManager.InitializeHand();
        UnityEngine.Object.FindFirstObjectByType<HandManager>().SetCardCharacter(characterIndex);
        GameManager.Instance.waitForInput = true;
    }
}

[Serializable]
public sealed class ModelAction : GameAction
{
    public int dice;
    public int bonus;
    public override void Execute()
    {
        if (caller == null) return;
        Camera.main.GetComponent<CameraController>().MoveCamera(caller.transform.position + new Vector3(0f, 3f, 0f), 5.5f);
        GameManager.Instance.Sound("s_orchestra_hit", GameManager.Instance.orchestraPitch);
        for (int i = 0; i < dice; i++)
        {
            GameManager.Instance.Sound("s_dice", 1f);
            var d = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("D12"), caller.transform.position, Quaternion.identity);
            d.GetComponent<DiceRoll>().bonus = bonus;
            d.GetComponent<DiceRoll>().friendly = true;
        }

        int frame = (int)UnityEngine.Random.Range(0, 10);
        caller.GetComponentInChildren<CustomAnimator>().autoUpdate = false;
        caller.GetComponentInChildren<CustomAnimator>().Play("Skeleton_Pose", frame);
        Camera.main.GetComponent<Animator>().SetTrigger("Punch");
    }

}
[Serializable]
public sealed class TargetAction : GameAction
{
    //Holds a game action reference until a target can be chosen, then executes it
    public GameAction gameAction;
    public string description = "Attack";
    public string targetTag = "EnemyCharacter";
    public bool consumeAction = false;
    public override void Execute()
    {
        GameManager.Instance.waitForInput = true;
        GameObject targeterObj = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Targeter"));
        Targeter targeter = targeterObj.GetComponent<Targeter>();
        targeter.Init(gameAction, caller.gameObject, description, targetTag, consumeAction);
    }
}
[Serializable]
public sealed class HasteAction : GameAction
{
    public override void Execute()
    {
        Camera.main.GetComponent<CameraController>().MoveCamera(caller.transform.position + new Vector3(0f, 3f, 0f), 5.5f);
        caller.GetComponent<SummonModel>().actions = caller.GetComponent<SummonModel>().maxActions;
    }
}
[Serializable]
public sealed class HealAction : GameAction
{
    public int amount;
    public override void Execute()
    {
        if (caller == null || target == null) return;
        Camera.main.GetComponent<CameraController>().MoveCamera(target.transform.position + new Vector3(0f, 3f, 0f), 5.5f);
        GameManager.Instance.Sound("s_orchestra_hit", GameManager.Instance.orchestraPitch);
        Health h = target.GetComponent<Health>();
        h.stats.hp = Mathf.Min(h.stats.hp + amount, h.maxHp);

        int frame = (int)UnityEngine.Random.Range(0, 2);
        caller.GetComponentInChildren<CustomAnimator>().autoUpdate = false;
        caller.GetComponentInChildren<CustomAnimator>().Play("Skeleton_Heal", frame);
        Camera.main.GetComponent<Animator>().SetTrigger("Punch");
    }

}
[Serializable]
public sealed class SelfHarmAction : GameAction
{
    public int amount;
    public override void Execute()
    {
        if (caller == null) return;
        Camera.main.GetComponent<CameraController>().MoveCamera(target.transform.position + new Vector3(0f, 3f, 0f), 5.5f);
        Health h = caller.GetComponent<Health>();
        for(int i=0; i<amount; i++)
            h.Damage(1, 20);
    }
}
[Serializable]
public sealed class AttackAction : GameAction
{
    public int dice;
    public int bonus;
    public string animationName;
    public override void Execute()
    {
        GameManager.Instance.HideExcept(new GameObject[] { caller, target });
        GameObject attackCutscene = GameObject.Find("AttackCutscene");
        SummonModel model = caller.GetComponent<SummonModel>();
        target.GetComponent<EnemyAnimator>().SetSize(2f);
        attackCutscene.transform.Find("Background").GetComponent<SpriteRenderer>().enabled = true;
        target.GetComponent<SmoothMove>().MoveTo(new Vector3(2f, 0f, 0f), Quaternion.identity);
        model.StartCoroutine(model.Attack(dice, bonus, target.GetComponent<Enemy>(), animationName));
    }
}

public sealed class StatusEffectAction : GameAction
{
    public StatusEffect statusEffect;
    public override void Execute()
    {
        target.GetComponentInChildren<Health>().AddStatusEffect(statusEffect);
    }
}

public sealed class DiceAttackAction : GameAction
{
    public int dice;
    public int bonus;
    public string animName;
    public override void Execute()
    {
        if (caller == null) return;
        Camera.main.GetComponent<CameraController>().MoveCamera(caller.transform.position + new Vector3(0f, 3f, 0f), 5.5f);
        GameManager.Instance.Sound("s_orchestra_hit", GameManager.Instance.orchestraPitch);
        for (int i = 0; i < dice; i++)
        {
            GameManager.Instance.Sound("s_dice", 1f);
            var d = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("D12"), caller.transform.position, Quaternion.identity);
            d.GetComponent<DiceRoll>().bonus = bonus;
            d.GetComponent<DiceRoll>().target = target.GetComponentInChildren<Health>();
        }

        int frame = (int)UnityEngine.Random.Range(0, 10);
        CustomAnimator c = caller.GetComponentInChildren<CustomAnimator>();
        if (c != null)
        {
            c.Play(animName, 0, false, 8);
        }
        EnemyAnimator a = caller.GetComponentInChildren<EnemyAnimator>();
        if (a != null)
        {
            a.ChangeSprite(animName, true, false);
        }
        Camera.main.GetComponent<Animator>().SetTrigger("Punch");
    }
}

[Serializable]
public sealed class EnemyAttackAction : GameAction
{
    public override void Execute()
    {
        if (caller != null && target != null)
        {
            GameManager.Instance.HideExcept(new GameObject[] { caller, target });
            caller.GetComponent<Enemy>().StartCoroutine("StartAttack",target.GetComponent<SummonModel>());
            caller.GetComponent<EnemyAnimator>().SetSize(2f);
            GameObject attackCutscene = GameObject.Find("AttackCutscene");
            attackCutscene.transform.Find("Background").GetComponent<SpriteRenderer>().enabled = true;
        }

        {
            GameManager.Instance.descriptionText.text = "Enemy cannot attack - They don't exist anymore";
        }
    }
}
[Serializable]
public sealed class EnemySummonAction : GameAction
{
    public GameObject modelPrefab;
    public override void Execute()
    {
        GameObject summonPoint = GameObject.Find("EnemySummonPoint");
        int enemyCount = GameObject.FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;
        GameObject summon = UnityEngine.Object.Instantiate(modelPrefab, summonPoint.transform.position + new Vector3(1.2f * enemyCount, -0.4f * enemyCount), Quaternion.identity, summonPoint.transform);
        Camera.main.GetComponent<CameraController>().MoveCamera(summon.transform.position + new Vector3(0f, 3f, 0f), 5.5f);
    }
}
[Serializable]
public sealed class EnemyModelAction : GameAction
{
    public int dice;
    public int bonus;
    public override void Execute()
    {
        Camera.main.GetComponent<CameraController>().MoveCamera(caller.transform.position + new Vector3(0f, 3f, 0f), 5.5f);
        Camera.main.GetComponent<Animator>().SetTrigger("Punch");
        GameManager.Instance.Sound("s_dice", 1f);
        GameManager.Instance.Sound("s_orchestra_hit", GameManager.Instance.orchestraPitch);
        GameManager.Instance.orchestraPitch += 0.05f;
        caller.GetComponent<Enemy>().ChooseNextAttack();
        for (int i = 0; i < dice; i++)
        {
            var d = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("D12"), caller.transform.position, Quaternion.identity);
            d.GetComponent<DiceRoll>().bonus = bonus;
            d.GetComponent<DiceRoll>().friendly = false;
        }
    }
}