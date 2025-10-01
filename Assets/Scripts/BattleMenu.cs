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
                    if (GameManager.Instance.swag >= spell.cost)
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
        if(GameManager.Instance.swag >= spell.cost && (spell.charges > 0 || spell.charges == -1)){
            if (spell.charges > 0) spell.charges--;
            GameManager.Instance.swag -= spell.cost;
            switch (spell.name)
            {
                case "Gun Heels":
                    GameAction newAction = new AttackAction
                    {
                        caller = owner.gameObject,
                        target = null,
                        dice = 5,
                        bonus = 0,
                        animationName = "Skeleton_Gun_Heels"
                    };
                    GetComponent<Canvas>().enabled = false;
                    GameObject targeterObj = Instantiate(Resources.Load<GameObject>("Targeter"));
                    Targeter targeter = targeterObj.GetComponent<Targeter>();
                    targeter.Init(newAction, owner.gameObject, "Gun Heels", "EnemyCharacter", true);
                    break;
                default: break;
            }
        }
    }

    private void OnOptionClicked(string label, int index)
    {

        Debug.Log($"Clicked {index}: {label}");

        switch (label)
        {
            case "Change Clothes":
                GetComponent<Canvas>().enabled = false;
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
                    animationName = "Skeleton_Attack1"
                };
                GetComponent<Canvas>().enabled = false;
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
        }
    }
}
