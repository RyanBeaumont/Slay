using System;
using System.Collections;
using System.Linq;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.EventSystems;


public enum PlayerState {Idle, Dead, Attacking}
public class SummonModel : MonoBehaviour, IPointerDownHandler
{
    public Teammate teammate;
    public string[] battleMenuOptions = { "Look Good", "Kick Ass", "TWERK" };
    public int actions = 1;
    [HideInInspector] public string characterName;
    public PlayerState playerState = PlayerState.Idle;
    Vector3 startPos;
    int xx = 0; int yy = 0;
    public int maxActions = 1;
    public int swag = 0;
    public ClothingStats clothingStats;
    Transform model;
    public CustomAnimator customAnimator;
    GameObject skeleton;
    Transform defenderPos;
    SpriteColorToggle spriteColorToggle;
    SmoothMove smoothMove;
    Health health;
    AbilityHandler abilityHandler;
    SpriteRenderer selectedArrow;
    Transform swagContainer;
    void Awake()//
    {
        model = transform.Find("Model");
        model.localScale = new Vector2(-0.6f, 0.6f);
        startPos = transform.position;
        health = GetComponentInChildren<Health>();
        smoothMove = GetComponent<SmoothMove>();
        defenderPos = GameObject.Find("DefenderPosition").transform;
        abilityHandler = GetComponent<AbilityHandler>();
        selectedArrow = transform.Find("SelectedArrow").GetComponent<SpriteRenderer>();
        swagContainer = transform.Find("SwagContainer");
    }

    public void Init(Teammate t)
    {
        teammate = t;
        ChangeSkin();
        clothingStats = ClothingRegistry.Instance.GetStats(t.equippedOutfit.outfit, t.GetBaseStats()); //EDIT TO INCLUDE new stats
        SetState(PlayerState.Idle);
        actions = maxActions;
    }

    void ChangeSkin()
    {
        Transform oldSkeleton = model.Find("Skeleton");
        if (oldSkeleton != null) Destroy(oldSkeleton.gameObject);
        skeleton = ClothingRegistry.Instance.SpawnCharacter(teammate.index, teammate.equippedOutfit, model);
        skeleton.name = "Skeleton";
        spriteColorToggle = skeleton.GetComponent<SpriteColorToggle>();
        spriteColorToggle.enabled = true;
        spriteColorToggle.Reload();
        customAnimator = skeleton.GetComponent<CustomAnimator>();

        //Change stats
        clothingStats = ClothingRegistry.Instance.GetStats(teammate.equippedOutfit.outfit, teammate.GetBaseStats());
        clothingStats.hp -= teammate.equippedOutfit.damageTaken;
        health.SetStats(clothingStats,10);
        abilityHandler.SetStats(clothingStats);
        characterName = teammate.name;
        health.SetName(characterName);
        customAnimator = skeleton.GetComponent<CustomAnimator>();
    }

    public void ChangeCharacter(Outfit newOutfit)
    {
        if (playerState == PlayerState.Dead)
            SetState(PlayerState.Idle);
        //Swap data
        OverworldController.Instance.yourOutfits.Remove(newOutfit);
        teammate.equippedOutfit = newOutfit;
        ChangeSkin();
        ConsumeAction();
        if (actions <= 0)
        {
            selectedArrow.enabled = false;
        }
    }

    public void ConsumeAction()
    {
        actions--;
        if (actions <= 0) selectedArrow.enabled = false;
    }

    public void SetState(PlayerState state)
    {
        playerState = state;
        customAnimator.loop = true;
        customAnimator.autoUpdate = true;
        customAnimator.Play("Skeleton_Idle", UnityEngine.Random.Range(0,6));
        model.localScale = new Vector2(-0.6f, 0.6f);
        smoothMove.ReturnToStart();
        if (state == PlayerState.Idle)
        {

        }
        if (state == PlayerState.Dead)
        {
            string[] choices = { "Your new outfit has... a TEAR??! You immediately collapse into a heap. It's been a good life, you think",
            "If you're gonna go out... You're gonna go out looking hot",
            "You take a look at your dead body in the mirror... So dramatic. Grotesque. Evocative. You belong in a museum",
            "All this blood surrounding you... You look good in red",
            "You see your guts spilling out. Ew. Not a good look",
            "You begin to die... Oh well, nothing plastic surgery can't fix"};
            string[] message = new string[] { choices[UnityEngine.Random.Range(0, choices.Length)] };
            print("DEath action added");
            GameAction deathAction = new DeathAction()
            {
                caller = gameObject,
                dialog = message
            };
             if(GetComponent<AbilityHandler>() != null)
                {
                    GetComponent<AbilityHandler>().HandleDeath();
                }

            GameManager.Instance.gameActions.Add(deathAction);
        }
        if (state == PlayerState.Attacking)
        {
            model.localScale = new Vector2(-1f, 1f);
            customAnimator.autoUpdate = false;
            smoothMove.MoveTo(new Vector3(-2f, -6f, 0f), Quaternion.identity);
        }
    }

    public IEnumerator Attack(int dice, int bonus, Enemy target, string animationName, DamageType[] damageTypes, StatusEffect statusEffect)
    {
        GameManager.Instance.Sound("s_dbz_jump", 1);
        GameManager.Instance.runTimer = false;
        SetState(PlayerState.Attacking);
        customAnimator.Play(animationName, 0, canAutoUpdate: false);
        GameObject attackCutscene = GameObject.Find("AttackCutscene");
        attackCutscene.transform.Find("Background").GetComponent<Animator>().SetBool("Dead", false);
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < dice; i++)
        {
            if (target == null) { yield return new WaitForSeconds(1);  EndAttack(); yield break; }

            int roll = UnityEngine.Random.Range(1, 13);

            var health = target.GetComponentInChildren<Health>();
            if (health == null) { EndAttack(); yield break; }

            var damageResult = health.Damage(bonus: bonus, damageType: damageTypes);
            if (damageResult > 0)
            {
                var anim = target.GetComponent<EnemyAnimator>();
                if (anim != null)
                    anim.ChangeSprite("Hurt", false, true);
                if(statusEffect.name != Status.None)
                    {
                        var p = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Powerup"),target.transform.position,Quaternion.identity);
                        p.GetComponent<Powerup>().target = target.transform;
                        p.GetComponent<Powerup>().statusEffect = statusEffect;
                        p.GetComponent<Powerup>().useStatusEffect = true;
                    }
            }
            if(damageResult == 2) //Weakness
            {
                actions += 1;
                var p = Instantiate(Resources.Load<GameObject>("Powerup"),transform.position,Quaternion.identity);
                p.GetComponent<Powerup>().target = transform;
                p.GetComponent<Powerup>().swag = 2;
            }
            customAnimator.Play(animationName, 1, fps:8, canLoop:false);


            // Check lethal break
            if (target == null || target.GetComponentInChildren<Health>().hp <= 0)
            {
                attackCutscene.transform.Find("Background").GetComponent<Animator>().SetBool("Dead", true);
                GameManager.Instance.Sound("s_cha_ching", 1);
                GameManager.Instance.Sound("s_explosion", 1);
                spriteColorToggle.SetBlack();
                yield return new WaitForSeconds(1);
                spriteColorToggle.RestoreColors();
                EndAttack();
                yield break;
            }
            if (health != null && health.hp <= 0) break;

            
            yield return new WaitForSeconds(0.3f);

            if (target == null) { EndAttack(); yield break; }
            var anim2 = target.GetComponent<EnemyAnimator>();
            if (anim2 != null)
                anim2.ChangeSprite("Idle", false, false);

            customAnimator.Play(animationName, 0, canAutoUpdate: false);
            yield return new WaitForSeconds(0.4f);

            
        }
        yield return new WaitForSeconds(1f);
        if (target != null)
        {
            target.GetComponentInChildren<SmoothMove>().ReturnToStart();
            target.GetComponentInChildren<EnemyAnimator>().ResetSize();
        }
        EndAttack();
    }

    void EndAttack()
    {
        customAnimator.Play("Skeleton_Idle", 0);
        GetComponentInChildren<SmoothMove>().ReturnToStart();
        GameManager.Instance.ShowAll();
        SetState(PlayerState.Idle);
        if (actions <= 0) selectedArrow.enabled = false;
        GameObject attackCutscene = GameObject.Find("AttackCutscene");
        attackCutscene.transform.Find("Background").GetComponent<SpriteRenderer>().enabled = false;
        GameManager.Instance.runTimer = true;
    }

    public void SetBlack()
    {
        spriteColorToggle.SetBlack();
    }
    public void RestoreColors()
    {
        print($"Restoring color to {characterName}");
        spriteColorToggle.RestoreColors();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            health.Damage(guaranteedHit: true);
        }
        else
        {
            if (actions > 0)
            {
                GameManager.Instance.ShowBattleMenu(this);
            }
        }
    }
    
    public void AddSwag(int amount)
    {
        swag = Mathf.Clamp(swag + amount, 0, 10);
        foreach (Transform child in swagContainer) Destroy(child.gameObject);
        for(int i=0; i<swag; i++)
        {
            Instantiate(Resources.Load("SwagImage"), swagContainer);
        }
    }

    public void NewTurn()
    {
        if (playerState != PlayerState.Dead)
            SetState(PlayerState.Idle);
        actions = maxActions;
        RestoreColors();
        selectedArrow.enabled = true;

        var p = Instantiate(Resources.Load<GameObject>("Powerup"),transform.position,Quaternion.identity);
        p.GetComponent<Powerup>().target = transform;
        p.GetComponent<Powerup>().swag = 1;

        //Swag overload
        
        int outfitCost = ClothingRegistry.Instance.GetStats(teammate.equippedOutfit.outfit, teammate.GetBaseStats()).cost;
        int swagOverload = Math.Max(0,outfitCost - teammate.fashionPoints);
        if (swagOverload > 0)
        {
            GameManager.Instance.gameActions.Add(new SelfHarmAction()
            {
                damage = swagOverload * 10,
                dialog = new string[] {$"{swagOverload} The power of the outfit... It is too much for you! It lashes out at you as you struggle to contain it"}
            });
        }
        
    }


}
