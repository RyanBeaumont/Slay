using System.Collections.Generic;
using RyansFunctions;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class EnemyCard
{
    public string name = "Unnamed";
    public int cost = 1;
    public string function = "None";
    public string[] dialog;
    public Sprite icon = null;
    public GameObject summon = null;
    public int cooldown = 0;
    public ClothingStats stats;
}
public class MasterEnemy : MonoBehaviour
{
    public List<EnemyCard> deck;
    public List<EnemyCard> nextTurn = new List<EnemyCard>();
    public RectTransform iconContainer;
    public Sprite defaultIcon;

    void Start()
    {
        ChooseNextAttack();
    }

    public void ChooseNextAttack()
    {
        foreach (EnemyCard card in deck)
        {
            if (card.cooldown > 0) card.cooldown--;
        }
        nextTurn.Clear();
        foreach (Transform child in iconContainer) { Destroy(child.gameObject); }

        int money = GameManager.Instance.maxSwag;
        EnemyCard selectedCard = null;
        //Sort the deck list by cost, with most expensive first
        // Sort normally
        deck.Sort((a, b) => b.cost.CompareTo(a.cost));

        // Get the highest cost value
        int maxCost = deck[0].cost;

        // Collect all cards with this cost
        var topCards = deck.FindAll(c => c.cost == maxCost);

        // Pick one at random
        selectedCard = topCards[Random.Range(0, topCards.Count)];

        while (money > 0)
        {
            foreach (EnemyCard card in deck)
            {
                if (money >= card.cost && card.cooldown == 0)
                {
                    selectedCard = card;
                    break;
                }
            }
            if (selectedCard != null) //Currently this doesn't compile
            {
                nextTurn.Add(selectedCard);
                Debug.Log($"Added {selectedCard.name}");
                selectedCard.cooldown = 2;
                money -= selectedCard.cost;
                GameObject newIcon = Instantiate(Resources.Load<GameObject>("Icon"), iconContainer);
                TMP_Text txt = newIcon.GetComponentInChildren<TMP_Text>();
                if (selectedCard.function == "Summon") {
                    txt.text = "";
                } else {
                    txt.text = $"{selectedCard.stats.damage}";
                }
                
                newIcon.GetComponent<Image>().sprite = selectedCard.icon;
            }
            else
            {
                nextTurn.Add(new EnemyCard
                {
                    name = "Model",
                    stats = new ClothingStats { damage = Mathf.RoundToInt(money / 2) },
                    function = "Model",
                });
                GameObject newIcon = Instantiate(Resources.Load<GameObject>("Icon"), iconContainer);
                newIcon.GetComponentInChildren<TMP_Text>().text = $"{Mathf.RoundToInt(money / 2)}";
                newIcon.GetComponent<Image>().sprite = defaultIcon;
                Debug.Log($"Adding basic model for {Mathf.RoundToInt(money / 2)}");
            }
        }
    }
    
    public void AddAttackToQueue()
    {
        foreach (EnemyCard card in nextTurn)
        {
            GameAction newAction;
            if (card.function == "Summon")
            {
                newAction = new EnemySummonAction
                {
                    modelPrefab = card.summon,
                    dialog = card.dialog,
                };
                GameManager.Instance.gameActions.Add(newAction);
            }
            else if (card.function == "Model")
            {
                newAction = new EnemyModelAction
                {
                    caller = gameObject,
                    dice = card.stats.damage,
                    bonus = card.stats.bonus,
                    dialog = card.dialog,
                };
                GameManager.Instance.gameActions.Add(newAction);
            }
        }
    }
}
