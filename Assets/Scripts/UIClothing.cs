using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIClothing : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int index = 0;
    public int count;
    public Color defaultColor;
    public TMP_Text countText;
    public TMP_Text costText;
    Transform root;
    ClothingStats stats;
    int hiddenColorIndex = 0;
    public Button colorButton;

    public void OnPointerEnter(PointerEventData eventData)
    {
        string statLine = "";
        if (stats.damage > 0) statLine += $"+{stats.damage} Dice, ";
        if (stats.bonus > 0) statLine += $"+{stats.bonus} Each Roll, ";
        if (stats.hp > 0) statLine += $"+{stats.hp} HP, ";
        if (stats.armor > 0) statLine += $"+{stats.armor} Armor Class, ";
        ClothingMenu.Instance.ShowTooltip(stats.name, statLine, stats.description, Input.mousePosition);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ClothingMenu.Instance.HideTooltip();
    }

    public void Spawn()
    {
        root = transform.Find("Model");
        Outfit o = new Outfit()
        {
            outfit = new int[] { index },
            colors = new Color[] {defaultColor}
        };
        GameObject body = ClothingRegistry.Instance.SpawnCharacter(0, o, root);
        stats = ClothingRegistry.Instance.GetStats(new int[] { index }, new ClothingStats());
        costText.text = $"{stats.cost}";
        foreach (SpriteRenderer sr in body.GetComponentsInChildren<SpriteRenderer>(includeInactive: true))
        {
            sr.sortingLayerName = "Cards";
        }
        foreach (SpriteRenderer sr in body.transform.Find("Ella(Clone)").GetComponentsInChildren<SpriteRenderer>(includeInactive: true))
        {
            sr.color = Color.black;
        }
        if (count > 1)
            countText.text = $"x{count}";
        else countText.text = "";

        body.transform.localScale = new Vector3(50f, 50f, 50f);
    }

    public void InitColor(Color color)
    {
        ColorBlock cb = colorButton.colors;
        cb.normalColor = color;
        cb.highlightedColor = color;
        cb.pressedColor = color;
        cb.selectedColor = color;
        cb.disabledColor = color;
        colorButton.colors = cb;
        defaultColor = color;
    }

    public void SetColor(Color color)
    {
        print("Clicked");
        InitColor(color);
        Outfit thisOutfit = OverworldController.Instance.yourOutfits[ClothingMenu.Instance.currentOutfit];

        int pos = System.Array.IndexOf(thisOutfit.outfit, index);
        if (pos >= 0)
        {
            thisOutfit.colors[pos] = defaultColor;
            OverworldController.Instance.yourOutfits[ClothingMenu.Instance.currentOutfit] = thisOutfit;
        }

        ClothingMenu.Instance.RefreshModel();
    }

    public void OnColorClick()
    {
        ClothingMenu.Instance.colorPanel.gameObject.SetActive(true);
        ClothingMenu.Instance.colorPanel.GetComponent<ColorPanel>().owner = this;
    }

}
