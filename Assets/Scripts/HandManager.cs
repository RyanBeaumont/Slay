using UnityEngine;
using RyansFunctions;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.InputSystem;

public class HandManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform handTransform; //root of hand position
    public float fanSpread = 5f;
    public float cardSpacing = 5f;
    public float verticalCardSpacing = 0.18f;
    public List<GameObject> cardsInHand = new List<GameObject>();
    void Start()
    {
        InitializeHand();
    }

    public void InitializeHand()
    {
        foreach (GameObject child in cardsInHand) Destroy(child);
        cardsInHand.Clear();
        foreach (Outfit outfit in OverworldController.Instance.yourOutfits)
        {
            AddCardToHand(outfit);
        }
    }

    public void AddCardToHand(Outfit outfit)
    {
        ClothingStats stats = ClothingRegistry.Instance.GetStats(outfit.outfit, new ClothingStats());
        GameObject newCard = Instantiate(cardPrefab, handTransform.position, Quaternion.identity, handTransform);
        cardsInHand.Add(newCard);
        newCard.GetComponent<CardDisplay>().SetData(outfit);
        UpdateHandVisuals();
    }

    public void SetCardCharacter(int index)
    {
        foreach (GameObject card in cardsInHand)
        {
            card.GetComponent<CardDisplay>().characterIndex = index;
            card.GetComponent<CardDisplay>().UpdateCardDisplay();
        }
    }

    void Update()
    {
        //UpdateHandVisuals();
    }

    public void UpdateHandVisuals()
    {
        int cardCount = cardsInHand.Count;
        for (int i = 0; i < cardCount; i++)
        {
            float rotationAngle = (fanSpread * (i - (cardCount - 1) / 2f));
            cardsInHand[i].GetComponent<CardDisplay>().targetLocalRot = Quaternion.Euler(0f, 0f, rotationAngle);
            float horizontalOffset = 0f; float normalizedPosition = 0f;
            if (cardCount > 1)
            {
                horizontalOffset = i * cardSpacing;
                normalizedPosition = (2f * i / (cardCount - 1) - 1f);
            }
            float verticalOffset = verticalCardSpacing * (1 - normalizedPosition * normalizedPosition);
            cardsInHand[i].GetComponent<CardDisplay>().targetLocalPos = new Vector3(horizontalOffset, verticalOffset, 0f);
        }
    }
}
