using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BattleMenu : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private RectTransform container; // ScrollView/Content or any RectTransform
    [SerializeField] private Button buttonPrefab;      // Your MenuButton prefab
    [SerializeField] private TMP_Text characterName;

    // Fired when a menu option is clicked
    public event Action<string, int> OptionClicked;
    public SummonModel owner;

    public void SetOptions(SummonModel caller)
    {
        print("SETTING OPTIONS");
        Clear();
        owner = caller;
        characterName.text = $"{caller.teammate.name} // {caller.teammate.equippedOutfit.name}";
        GetComponent<Canvas>().enabled = true;
        if (caller.playerState == PlayerState.Dead)
        {
            /*
                var btn = Instantiate(buttonPrefab, container);
                // Set visible text
                var tmp = btn.GetComponentInChildren<TMP_Text>();
                if (tmp != null) tmp.text = "[2] Revive";

                // Hook click
                btn.onClick.AddListener(() => OnOptionClicked("[2] Revive", 0));
                */
        }
        else
        {
            //Standard options
            for (int i = 0; i < caller.battleMenuOptions.Length; i++)
            {
                var index = i;              // capture loop index safely
                var label = caller.battleMenuOptions[i];      // capture label safely

                var btn = Instantiate(buttonPrefab, container);
                // Set visible text
                var tmp = btn.GetComponentInChildren<TMP_Text>();
                if (tmp != null) tmp.text = label;
                if (label == "Change Clothes" && OverworldController.Instance.yourOutfits.Count <= 0)
                {
                    tmp.color = Color.gray;
                    tmp.text += " (GASP - No Outfits)";
                }
                else
                {
                    btn.onClick.AddListener(() => OnOptionClicked(label, index));
                }

                // Hook click
                
            }

            //Spells
            foreach (Spell spell in caller.teammate.equippedOutfit.spells)
            {
                if (spell.charges > 0 || spell.charges == -1)
                {
                    var btn = Instantiate(buttonPrefab, container);
                    // Set visible text
                    var tmp = btn.GetComponentInChildren<TMP_Text>();
                    if (tmp != null) tmp.text = $"[{spell.cost}] {spell.name}";
                    if (spell.charges > 0) tmp.text += $"({spell.charges}x Use)";
                    btn.onClick.AddListener(() => UseSpell(spell));
                    if (caller.swag >= spell.cost)
                    {
                        tmp.color = Color.yellow;
                    }
                    else
                    {
                        tmp.color = Color.gray;
                    }
                }
                
            }
        }

        // Optional: force layout update after building
            LayoutRebuilder.ForceRebuildLayoutImmediate(container);
    }

    public void Clear()
    {
        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);
    }

    void Update()
    {
        Canvas c = GetComponent<Canvas>();
        if (owner == null)
        {
            c.enabled = false;
        }
        if (Input.GetKey(KeyCode.Escape) && c.enabled)
        {
            c.enabled = false;
        }
    }
    private void UseSpell(Spell spell)
    {
        if(owner.swag >= spell.cost && (spell.charges > 0 || spell.charges == -1)){
            if (spell.charges > 0) spell.charges--;
            owner.swag -= spell.cost;
            AbilityHandler a = owner.GetComponent<AbilityHandler>();
            if (a != null)
            {
                a.Invoke(spell.function, 0f);
            }
            GetComponent<Canvas>().enabled = false;
        }
    }

    private void OnOptionClicked(string label, int index)
    {

        Debug.Log($"Clicked {index}: {label}");
        GetComponent<Canvas>().enabled = false;
        switch (label)
        {
            case "Look Good":
                GameAction modelAction = new ModelAction
                {
                    caller = owner.gameObject,
                    target = owner.gameObject,
                };
                GameManager.Instance.gameActions.Add(modelAction);
                break;
            case "Change Clothes":
                GameManager.Instance.handOfCards.SetActive(true);
                GameManager.Instance.handManager.InitializeHand();
                FindFirstObjectByType<HandManager>().SetCardCharacter(owner.teammate.index);
                break;
            case "Kick Ass":
                GameAction newAction = new AttackAction
                {
                    caller = owner.gameObject,
                    target = null,
                    dice = owner.clothingStats.damage,
                    bonus = owner.clothingStats.bonus,
                    damageTypes = owner.clothingStats.damageTypes,
                    animationName = "Skeleton_Attack1"
                };
                
                GameAction targetAction = new TargetAction
                {
                    caller = owner.gameObject,
                    description = "Kick Whose Ass?",
                    gameAction = newAction,
                    targetTag = "EnemyCharacter",
                    consumeAction = true
                };
                GameManager.Instance.gameActions.Add(targetAction);
                GameManager.Instance.timer = 0;
                break;
            case "Swag Strike":
                GameAction swagAction = new AttackAction
                {
                    caller = owner.gameObject,
                    target = null,
                    dice = owner.swag,
                    bonus = 0,
                    damageTypes = owner.clothingStats.damageTypes,
                    animationName = "Skeleton_Pose"
                };
                GameAction targetAction2 = new TargetAction
                {
                    caller = owner.gameObject,
                    description = "Swag Strike",
                    gameAction = swagAction,
                    targetTag = "EnemyCharacter",
                    consumeAction = false,
                };
                owner.AddSwag(-owner.swag);
                GameManager.Instance.gameActions.Add(targetAction2);
                GameManager.Instance.timer = 0;
                break;
        }
    }
}
