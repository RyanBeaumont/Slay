using System.Collections.Generic;
using UnityEngine;

public class Vanity : ChainedInteractable
{

    public override void Interact()
    {
        if (ClothingMenu.Instance.ui.enabled)
            ClothingMenu.Instance.HideUI();
        else
            ClothingMenu.Instance.ShowUI();
    }
}
