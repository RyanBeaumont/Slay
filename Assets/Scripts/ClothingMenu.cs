using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Linq;
public class ClothingMenu : MonoBehaviour
{
    [HideInInspector] public int currentOutfit = 0;
    public Canvas ui;
    public RectTransform model;
    public RectTransform uiContent;
    public RectTransform equippedContent;
    public TMP_InputField nameField;
    public GameObject inventoryItemPrefab;
    public Canvas toolTip;
    public TMP_Text statsField;
    public static ClothingMenu Instance;
    public Color[] colorOptions;
    public GameObject colorPanel;

    void Start()
    {
        ui.enabled = false;
        toolTip.enabled = false;
        colorPanel.SetActive(false);
    }

    void Awake()
    {
         Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (ui.enabled) HideUI();
            else ShowUI();
        }
    }

    public void ShowTooltip(string title, string stats, string description, Vector3 position)
    {
        toolTip.enabled = true;
        toolTip.transform.Find("Panel/Name").GetComponent<TMP_Text>().text = title;
        toolTip.transform.Find("Panel/Stats").GetComponent<TMP_Text>().text = stats;
        toolTip.transform.Find("Panel/Description").GetComponent<TMP_Text>().text = description;
        //toolTip.transform.Find("Panel").position = position;
    }

    public void HideTooltip()
    {
        toolTip.enabled = false;
    }

    public void ShowUI()
    {
        ui.enabled = true;
        //Take primary outfits first
        for (int i = 0; i < OverworldController.Instance.yourTeam.Count; i++)
        {
            Outfit outfitToInsert = OverworldController.Instance.yourTeam[i].equippedOutfit;
            OverworldController.Instance.yourOutfits.Insert(i, outfitToInsert);
        }
        while (OverworldController.Instance.yourDiscard.Count > 0)
        {
            Outfit thisItem = OverworldController.Instance.yourDiscard[0];
            OverworldController.Instance.yourOutfits.Add(thisItem);
            OverworldController.Instance.yourDiscard.RemoveAt(0);
        }

        Dictionary<int, int> itemCounts = new Dictionary<int, int>();
        foreach (int id in OverworldController.Instance.yourClothes)
        {
            if (itemCounts.ContainsKey(id))
                itemCounts[id]++;
            else
                itemCounts[id] = 1;
        }

        // Create UI entry for each unique item
        foreach (KeyValuePair<int, int> entry in itemCounts)
        {
            int itemID = entry.Key;
            int count = entry.Value;

            GameObject itemGO = Instantiate(inventoryItemPrefab, uiContent);

            // Example: set icon & count text
            UIClothing uic = itemGO.GetComponent<UIClothing>();
            if (uic != null)
            {
                uic.index = itemID;
                uic.count = count;
                uic.Spawn();
            }
            itemGO.transform.Find("ColorSelect").GetComponent<Button>().enabled = false; //Can't select color if not equipped
            Button newButton = itemGO.GetComponentInChildren<Button>();
            newButton.onClick.AddListener(() =>
            {
                ClickEquip(uic.index);
            });
        }
        RefreshModel();
    }

    public void RefreshModel()
    {
        //Show existing clothes
        foreach (Transform child in model) { Destroy(child.gameObject); }
        foreach (RectTransform child in equippedContent) { Destroy(child.gameObject); }
        Outfit thisOutfit = OverworldController.Instance.yourOutfits[currentOutfit];
        for(int i=0; i< thisOutfit.outfit.Length; i++)
        {
            int item = thisOutfit.outfit[i];
            Color color = thisOutfit.colors[i];
            GameObject itemGO = Instantiate(inventoryItemPrefab, equippedContent);
            UIClothing uic = itemGO.GetComponent<UIClothing>();
            uic.index = item;
            uic.count = 1;
            uic.Spawn();
            uic.InitColor(color);
            Button newButton = itemGO.GetComponentInChildren<Button>();
            newButton.onClick.AddListener(() =>
            {
                ClickUnequip(uic.index);
            });
        }
        var body = ClothingRegistry.Instance.SpawnCharacter(currentOutfit % OverworldController.Instance.yourTeam.Count, thisOutfit, model);
        body.transform.localScale = new Vector3(154f, 154f, 154f);
        ClothingStats stats = ClothingRegistry.Instance.GetStats(thisOutfit.outfit, new ClothingStats());
        statsField.text = "";
        if (stats.cost > 0) statsField.text += $"Swagger Cost: {stats.cost}\n";
        if (stats.damage > 0) statsField.text += $"Attack Dice: +{stats.damage}\n";
        if (stats.bonus > 0) statsField.text += $"Bonus: +{stats.bonus} each roll\n";
        if (stats.hp > 0) statsField.text += $"Health: +{stats.hp}\n";
        if (stats.armor > 0) statsField.text += $"Armor Class: +{stats.damage}\n";
        nameField.text = thisOutfit.name;
    }

    void ClickEquip(int index)
{
    Outfit thisOutfit = OverworldController.Instance.yourOutfits[currentOutfit];

    // Equip the new item
    thisOutfit.outfit = thisOutfit.outfit.Concat(new int[] { index }).ToArray();
    thisOutfit.colors = thisOutfit.colors.Concat(new Color[] { Color.white }).ToArray();
    thisOutfit.spells = ClothingRegistry.Instance.GetSpells(thisOutfit);

    // Get the type of the new item
    ClothingStats myStats = ClothingRegistry.Instance.GetStats(new int[] { index }, new ClothingStats());
    string thisType = myStats.type;
        print($"This type: {thisType}");

    // Collect duplicates to remove
        List<int> toUnequip = new List<int>();
    foreach (int indexToCheck in thisOutfit.outfit)
    {
        ClothingStats statsToCheck = ClothingRegistry.Instance.GetStats(new int[] { indexToCheck }, new ClothingStats());
        string typeToCheck = statsToCheck.type;
        if (typeToCheck == thisType && thisType != "Accessory" && indexToCheck != index)
        {
            toUnequip.Add(indexToCheck);
        }
    }

    // Now unequip them safely
    foreach (int unequipIndex in toUnequip)
    {
        ClickUnequip(unequipIndex);
    }

    // Remove the newly equipped item from inventory
    OverworldController.Instance.yourClothes.Remove(index);

    // Save back
    OverworldController.Instance.yourOutfits[currentOutfit] = thisOutfit;

    HideUI();
    ShowUI();
}

    void ClickUnequip(int index)
    {
        print($"Unequipping index {index}");

        Outfit thisOutfit = OverworldController.Instance.yourOutfits[currentOutfit];

        Debug.Log($"This outfit {thisOutfit.outfit}");

        // Find the position of this clothing item in the outfit array
        int pos = System.Array.IndexOf(thisOutfit.outfit, index);
        if (pos >= 0)
        {
            // Remove from outfit array
            List<int> newOutfit = new List<int>(thisOutfit.outfit);
            newOutfit.RemoveAt(pos);
            thisOutfit.outfit = newOutfit.ToArray();

            // Remove matching color at the same position
            List<Color> newColors = new List<Color>(thisOutfit.colors);
            newColors.RemoveAt(pos);
            thisOutfit.colors = newColors.ToArray();

            // Add back into yourClothes
            List<int> newClothes = new List<int>(OverworldController.Instance.yourClothes);
            newClothes.Add(index);
            OverworldController.Instance.yourClothes = newClothes;

            // Save the updated outfit back
            OverworldController.Instance.yourOutfits[currentOutfit] = thisOutfit;

            // Refresh UI
            HideUI();
            ShowUI();
        }
        else
        {
            Debug.LogWarning($"Tried to unequip index {index}, but it wasn't found in the outfit!");
        }
    }

    public void ShowNextOutfit()
    {
        currentOutfit++;
        if (currentOutfit > OverworldController.Instance.yourTeam.Count) currentOutfit = 0;
        HideUI();
        ShowUI();
    }
    public void ShowPreviousOutfit()
    {
        currentOutfit--;
        if (currentOutfit < 0) currentOutfit = OverworldController.Instance.yourTeam.Count-1;
        HideUI();
        ShowUI();
    }

    public void HideUI()
    {
        foreach (RectTransform child in uiContent) { Destroy(child.gameObject); }
        foreach (RectTransform child in equippedContent) { Destroy(child.gameObject); }
        foreach (Transform child in model) { Destroy(child.gameObject); }
        ui.enabled = false;

        for (int i = 0; i < OverworldController.Instance.yourTeam.Count; i++)
        {
            Outfit outfitToInsert = OverworldController.Instance.yourOutfits[0];
            OverworldController.Instance.yourOutfits.RemoveAt(0);
            OverworldController.Instance.yourTeam[i].equippedOutfit = outfitToInsert;
        }

        FindFirstObjectByType<PartyManager>().RefreshOutfits();
        HideTooltip();
    }

}
