    using UnityEngine;
    using System.Collections;
    using Unity.Mathematics;
    using System;
    using TMPro;
    using UnityEngine.UI;
    using System.Linq;
using System.Collections.Generic;
public static class GameConstants
    {
    public const float GridSize = 1.5f;
    }
    public class Enemy : MonoBehaviour
    {
    SmoothMove smoothMove;
    SummonModel[] allModels;
    Health health;
    public RectTransform iconContainer;
    public string characterName = "Enemy";
    List<string> nextAttack = new List<string>();
    List<GameObject> nextTarget = new List<GameObject>();
    public string[] attacks = { "BasicAttack", "Model" };
    public Sprite[] icons;
    public ClothingStats stats;
    public int xpFromKill = 20;
    public int cashFromKill = 20;
    public int[] lootDrops;
    public int maxActions = 1;
    public int hpPerHeart = 10;
    public int damagePerHit = 10;
    string thisAttack;
    EnemyAnimator enemyAnimator;
    CustomAnimator customAnimator;
    void Start()
    {
        smoothMove = GetComponent<SmoothMove>();
        health = GetComponentInChildren<Health>();
        ChooseNextAttack();
        health.SetStats(stats,hpPerHeart);
        health.SetName(characterName);
        enemyAnimator = GetComponent<EnemyAnimator>();
        customAnimator = GetComponent<CustomAnimator>();
    }



    public void Die()
    {
        int item = -1;
        if (lootDrops.Length > 0)
            item = lootDrops[UnityEngine.Random.Range(0, lootDrops.Length)];
        GameManager.Instance.AddLoot(xpFromKill, cashFromKill, item);
        GameAction enemyDeath = new EnemyDeathAction()
        {
            caller = gameObject,
        };
        GameManager.Instance.gameActions.Add(enemyDeath);
        GameManager.Instance.CheckForWin();
    }

        public void ChooseNextAttack()
    {
        nextAttack.Clear();
        nextTarget.Clear();
        foreach (Transform child in iconContainer) { Destroy(child.gameObject); }

        for (int i = 0; i < maxActions; i++)
        {
            // Pick a random attack
            int rand = UnityEngine.Random.Range(0, attacks.Length);
            string attackName = attacks[rand];

            GameObject target = null;

            // Choose targeting method based on attack
            switch (attackName)
            {
                case "BasicAttack":
                    target = ChooseTarget();
                    break;
                case "AttackUp":
                    target = ChooseTarget(targetAlly:true);
                    break;
                default:
                    target = ChooseTarget(); // default targeting
                    break;
            }

            if (target != null)
            {
                nextAttack.Add(attackName);
                nextTarget.Add(target);

                // Spawn icon
                GameObject newIcon = Instantiate(Resources.Load<GameObject>("Icon"), iconContainer);
                newIcon.GetComponentInChildren<TMP_Text>().text = $"{health.stats.damage}";
                newIcon.GetComponent<Image>().sprite = icons[rand];
            }
        }
    }

    public GameObject ChooseTarget(bool targetAlly = false)
    {
        if (targetAlly)
        {
            int j = UnityEngine.Random.Range(0, GameManager.Instance.enemies.Count);
            return GameManager.Instance.enemies[j].gameObject;
        }
        else
        {
            var livingCharacters = GameManager.Instance.models
            .Where(c =>
            {
                return c.playerState != PlayerState.Dead;
            }).ToArray();
            if (livingCharacters.Length == 0) return null;

            // Pick a random one
            int index = UnityEngine.Random.Range(0, livingCharacters.Length);
            return livingCharacters[index].gameObject;
        }
    }

    public void AddAttackToQueue()
    {
        for (int i = 0; i < nextAttack.Count; i++)
        {
            if (nextTarget[i] == null) { ChooseNextAttack(); return; }
            if (nextTarget[i].GetComponent<SummonModel>() && nextTarget[i].GetComponent<SummonModel>().playerState == PlayerState.Dead) { ChooseNextAttack(); return; }
            thisAttack = nextAttack[i];
            GameAction newAction;
            switch (thisAttack)
            {
                case "AttackUp":
                    string[] dialog = new string[] { $"{characterName} throws a knife to an ally, boosting their attack. Catch!" };
                    if (gameObject == nextTarget[i]) dialog = new string[] { $"{characterName} pulls out ANOTHER knife! She wouldn't go anywhere without at least seven" };
                    newAction = new StatusEffectAction
                    {
                        caller = gameObject,
                        target = nextTarget[i],
                        dialog = dialog,
                        statusEffect = new StatusEffect
                        {
                            duration = 2,
                            name = Status.AttackBoost,
                            amount = 1
                        }
                    };
                    break;
                default: //Basic Attack
                    newAction = new EnemyAttackAction
                    {
                        caller = gameObject,
                        target = nextTarget[i]
                    };
                    break;

            }
            GameManager.Instance.gameActions.Add(newAction);
        }

        ChooseNextAttack();
    }

    public IEnumerator StartAttack(SummonModel target)
    {
        GameManager.Instance.Sound("s_dbz_jump", 1);
        GameObject attackCutscene = GameObject.Find("AttackCutscene");
        attackCutscene.transform.Find("Background").GetComponent<Animator>().SetBool("Dead", false);
        print("Attack started");
        if (target != null)
        {
            //target.SetState(PlayerState.Defending);
        }
        GameManager.Instance.runTimer = false;
        smoothMove.MoveTo(GameObject.Find("AttackerPosition").transform.position, Quaternion.identity);
        target.GetComponentInChildren<SmoothMove>().MoveTo(GameObject.Find("DefenderPosition").transform.position, Quaternion.identity);
        GameManager.Instance.descriptionText.text = $"{characterName} uses {nextAttack}";
        GameObject n = Instantiate(Resources.Load<GameObject>("MoveName"), transform);
        
        n.GetComponentInChildren<TMP_Text>().text = thisAttack;
        {
            GameManager.Instance.runTimer = false;
            yield return new WaitForSeconds(1f);
            for (int i = 0; i < health.stats.damage; i++)
            {
                if(enemyAnimator != null) enemyAnimator.ChangeSprite(thisAttack, true, false);
                if (customAnimator != null) customAnimator.Play(thisAttack, 0, canLoop:false);
                if (target.GetComponentInChildren<Health>().Damage(damagePerHit:damagePerHit, bonus:health.stats.bonus))
                {
                    target.customAnimator.Play("Skeleton_Hurt", 0, canAutoUpdate: false);
                    if (target.GetComponentInChildren<Health>().hp <= 0)
                    {
                        attackCutscene.transform.Find("Background").GetComponent<Animator>().SetBool("Dead", true);
                        GameManager.Instance.Sound("s_explosion", 1);
                        break;
                    }
                }
                else
                {
                    target.customAnimator.Play("Skeleton_Dodge", 0, canLoop: false);
                }
                yield return new WaitForSeconds(0.6f);
                if(enemyAnimator != null) enemyAnimator.ChangeSprite("Idle", false, false);
                if (customAnimator != null) customAnimator.Play(thisAttack, 0, canAutoUpdate: false);
                target.customAnimator.Play("Skeleton_Idle", 0);
                yield return new WaitForSeconds(0.2f);
            }
            yield return new WaitForSeconds(1f);

            EndAttack();
        }
    }

    void EndAttack()
    {
        allModels = GameObject.FindObjectsByType<SummonModel>(FindObjectsSortMode.None);
        foreach (SummonModel model in allModels)
        {
            model.GetComponentInChildren<SmoothMove>().ReturnToStart();
            //if(model.playerState != PlayerState.Dead)
            //model.SetState(PlayerState.Active);
        }
        if(enemyAnimator != null)enemyAnimator.ResetSize();
        smoothMove.ReturnToStart();
        
        GameManager.Instance.ShowAll();
        GetComponentInChildren<SmoothMove>().ReturnToStart();
        GameManager.Instance.ShowAll();
        GameObject attackCutscene = GameObject.Find("AttackCutscene");
        attackCutscene.transform.Find("Background").GetComponent<SpriteRenderer>().enabled = false;
        GameManager.Instance.runTimer = true;
    }
    }

