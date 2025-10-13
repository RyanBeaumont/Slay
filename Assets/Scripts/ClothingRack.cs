using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClothingRack : PersistentObject, IInteractable
{
    public string promptMessage = "Clothing Rack";
    public int item;

    public string GetPromptMessage() => promptMessage;

    public void Interact()
    {
        if (active)
        {
            active = false;
            OverworldController.Instance.yourOutfits.Add(new Outfit()
            {
                outfit = new int[] { 0, 1, 2 },
                colors = new Color[] { Color.white, Color.white, Color.white },
                name = "New Outfit"
            });

            FindFirstObjectByType<DialogBox>().StartDialog(new List<Dialog>()
            {
                new Dialog()
                {
                    text = $"New outfit slot unlocked! Your wardrobe is up to {OverworldController.Instance.yourOutfits.Count} outfits!"
                }
            });
            
            DisableObject();
        }
    }
}