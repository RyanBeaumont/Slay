using UnityEngine;
using TMPro;
using UnityEngine.UI;
using RyansFunctions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class CardDisplay : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    Outfit outfit;
    public Image cardImage;
    public TMP_Text nameText;
    public TMP_Text damageText;
    public TMP_Text armorText;
    public TMP_Text hpText;
    public TMP_Text bonusText;
    public TMP_Text costText;
    public Image selectedBorder;
    public Vector3 targetLocalPos;
    public Quaternion targetLocalRot;
    ClothingStats clothingStats;
    public int characterIndex = 0;
    public GameObject summonModelPrefab;
    Transform summonPoint;
    public float smoothFactor = 0.125f;
    public float horOffset = 2f;
    public float vertOffset = 0.5f;

    public void SetData(Outfit Outfit)
    {
        outfit = Outfit; 
        clothingStats = ClothingRegistry.Instance.GetStats(Outfit.outfit,new ClothingStats());
        UpdateCardDisplay();
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        int swag = OverworldController.Instance.yourTeam[characterIndex].fashionPoints;
        if (swag >= clothingStats.cost)
            selectedBorder.color = Color.green;
        else
            selectedBorder.color = Color.yellow;
        selectedBorder.enabled = true;
        targetLocalPos += new Vector3(0f, 0.5f, 0f);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        selectedBorder.enabled = false;
        targetLocalPos -= new Vector3(0f, 0.5f, 0f);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        //if (GameManager.Instance.swag >= clothingStats.cost)
        //{
            SummonModel thisModel = FindFirstObjectByType<BattleMenu>().owner;
            GameAction action = new SummonAction
            {
                outfit = outfit,
                caller = thisModel.gameObject,
                cost = ClothingRegistry.Instance.GetStats(outfit.outfit, new ClothingStats()).cost
            };
        GameManager.Instance.waitForInput = false;
        GameManager.Instance.gameActions.Add(action);
            HandManager h = FindFirstObjectByType<HandManager>();
            h.cardsInHand.Remove(this.gameObject);
            h.UpdateHandVisuals();
        GameManager.Instance.handOfCards.SetActive(false);
            Destroy(gameObject); 
        //}
    }
    void Update()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPos, smoothFactor);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetLocalRot, smoothFactor);
    }

    public void UpdateCardDisplay()
    {
        foreach(Transform child in transform.Find("Model")){ Destroy(child.gameObject); }
        GameObject character = ClothingRegistry.Instance.SpawnCharacter(characterIndex, outfit, transform.Find("Model"));
        foreach (SpriteRenderer sr in character.GetComponentsInChildren<SpriteRenderer>(includeInactive: true))
        {
            sr.sortingLayerName = "Cards"; // RenderLayer is your string variable
        }
        character.transform.localPosition = Vector3.zero;
        selectedBorder.enabled = false;
        summonPoint = GameObject.Find("SummonPoint").transform;
        //character.transform.localScale = new Vector2(0.75f, 0.75f);
        nameText.text = outfit.name;
        hpText.text = $"{clothingStats.hp}";
        damageText.text = $"{clothingStats.damage}";
        if (clothingStats.bonus > 0)
            bonusText.text = $"+{clothingStats.bonus}";
        else bonusText.text = "";
        armorText.text = $"{clothingStats.armor}";
        costText.text = $"{clothingStats.cost}";
    }
}
