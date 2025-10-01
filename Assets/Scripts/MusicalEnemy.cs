    using UnityEngine;
    using System.Collections;
    using Unity.Mathematics;
    using System;
    using TMPro;
    using UnityEngine.UI;
    using System.Linq;

[Serializable]
public struct AttackPattern
{
    public string attackName;
    public float[] beat;
    public GameObject[] projectile;
}

public class MusicalEnemy : MonoBehaviour
{
    SmoothMove smoothMove;
    SummonModel[] allModels;
    Health health;
    public RectTransform iconContainer;
    public string characterName = "Enemy";
    public AttackPattern[] attacks;
    int attackIndex;
    bool attacking;
    float beatIndex;
    bool waitForDownBeat = true;
    AttackPattern nextAttack;
    SummonModel target;
    public Sprite[] images;
    void Start()
    {
        smoothMove = GetComponent<SmoothMove>();
        health = GetComponentInChildren<Health>();
        ChooseNextAttack();
    }

    public bool Damage(int roll, int bonus)
    {
        health.Damage(roll, bonus);
        if (health.stats.hp <= 0) { Destroy(gameObject); return true; }
        return false;
    }

    public void ChangeSprite(int index)
    {
        if (images.Length > index)
        {
            transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = images[index];
        }
    }

    void ChooseNextAttack()
    {
        int rand = UnityEngine.Random.Range(0, attacks.Length);
        nextAttack = attacks[rand]; //Randomly choose an attack from list
        print(nextAttack.attackName);
    }

    public void AddAttackToQueue()
    {
        GameAction newAction;
        allModels = FindObjectsByType<SummonModel>(FindObjectsSortMode.None);
        target = null;
        if (allModels.Length > 0) { target = allModels[UnityEngine.Random.Range(0, allModels.Length)]; }
        newAction = new EnemyAttackAction
        {
            //caller = this,
            //target = target,
        };

        GameManager.Instance.gameActions.Add(newAction);
    }

    public void StartAttack(SummonModel target)
    {
        attackIndex = 0;
        waitForDownBeat = true;
        GameManager.Instance.runTimer = false;
        smoothMove.MoveTo(GameObject.Find("AttackerPosition").transform.position, Quaternion.identity);
        GameManager.Instance.descriptionText.text = $"{characterName} uses {nextAttack}";
        GameObject n = Instantiate(Resources.Load<GameObject>("MoveName"), transform);
        n.GetComponentInChildren<TMP_Text>().text = nextAttack.attackName;
        attacking = true;
        beatIndex = 1;
    }

    public void DownBeat()
    {
        print("DownBeat");
        if (attacking && waitForDownBeat)
        {
            waitForDownBeat = false;
            GameManager.Instance.Sound("s_orchestra_hit", 1f);
        }
    }
    public void Beat()
    {
        if (attacking && !waitForDownBeat)
        {
            if (CheckForDeath(target))
            {
                attacking = false;
                EndAttack();
            }
            else
            {
                print($"Beat Number {beatIndex} Attack Number {attackIndex}");

                // Make sure we are within bounds before accessing
                if (attackIndex < nextAttack.beat.Length && beatIndex >= nextAttack.beat[attackIndex])
                {
                    // Spawn projectile BEFORE incrementing attackIndex
                    GameObject bullet = Instantiate(nextAttack.projectile[attackIndex], transform.position, Quaternion.identity);
                    bullet.GetComponent<Projectile>().target = target;
                    GameManager.Instance.Sound("s_select2", 1f);

                    attackIndex++;

                    // If we've reached the end, stop attacking
                    if (attackIndex >= nextAttack.beat.Length)
                    {
                        attacking = false;
                        EndAttack();
                    }
                }
            }

            beatIndex += 0.25f;
        }
    }

    bool CheckForDeath(SummonModel target)
    {
        if (target == null || target.playerState == PlayerState.Dead)
        {
            foreach (Projectile p in GameObject.FindObjectsByType<Projectile>(FindObjectsSortMode.None))
            {
                Destroy(p.gameObject);
            }
            return true;
        }
        return false;

    }

    void EndAttack()
    {
        attacking = false;
        smoothMove.ReturnToStart();
        ChooseNextAttack();
        GameManager.Instance.runTimer = true;
    }
}

