using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DialogEncounter : PersistentObject, IInteractable
{
    public List<Dialog> dialog;
    public string promptMessage = "Interact";
    public string newQuest = "";

    public string GetPromptMessage() => promptMessage;

    public void Interact()
    {
        if (active)
        {
            active = false;
            FindFirstObjectByType<DialogBox>().dialog = dialog;
            FindFirstObjectByType<DialogBox>().StartDialog();
            if (newQuest != "")
            {
                OverworldController.Instance.quests.Add(newQuest);
            }
            DisableObject();
        }
    }

}
