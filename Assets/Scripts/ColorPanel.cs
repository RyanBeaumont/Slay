using UnityEngine;
using UnityEngine.UI;

public class ColorPanel : MonoBehaviour
{
    public UIClothing owner = null;
    public GameObject colorButtonPrefab;
    void Start()
    {
        foreach (Color c in ClothingMenu.Instance.colorOptions)
        {
            var prefab = Instantiate(colorButtonPrefab, transform);
            var button = prefab.GetComponent<Button>();
            Color capturedColor = c;
            SetColor(button, capturedColor);
            button.onClick.AddListener(() =>
            {
                owner.SetColor(capturedColor);
                gameObject.SetActive(false);
            });
        }
    }

    void SetColor(Button b, Color color)
    {
        ColorBlock cb = b.colors;
        cb.normalColor = color;
        cb.highlightedColor = color;
        cb.pressedColor = color;
        cb.selectedColor = color;
        cb.disabledColor = color;
        b.colors = cb;
    }
    
}
