using System.Collections.Generic;
using UnityEngine;

public class Vanity : MonoBehaviour, IInteractable
{
    public string promptMessage = "Change Clothes";

    public string GetPromptMessage() => promptMessage;

    public void Interact()
    {
        if (ClothingMenu.Instance.ui.enabled)
            ClothingMenu.Instance.HideUI();
        else
            ClothingMenu.Instance.ShowUI();
    }
}
