using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class DialogQuest : PersistentObject, IInteractable
{
    public string questName;
    public List<Dialog> completeDialog;
    public List<Dialog> incompleteDialog;
    public string promptMessage = "Interact";

    public string GetPromptMessage() => promptMessage;

    public void Interact()
    {
        if (active)
        {
            if (OverworldController.Instance.quests.Contains(questName))
            {
                OverworldController.Instance.quests.Remove(questName);
                FindFirstObjectByType<DialogBox>().dialog = completeDialog;
                FindFirstObjectByType<DialogBox>().StartDialog();
                DisableObject();
            }
            else
            {
                FindFirstObjectByType<DialogBox>().dialog = incompleteDialog;
                FindFirstObjectByType<DialogBox>().StartDialog();
            }
            
        }
    }

}
