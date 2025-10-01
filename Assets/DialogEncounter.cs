using System.Collections.Generic;
using UnityEngine;

public class DialogEncounter : PersistentObject, IInteractable
{
    public List<Dialog> dialog;
    public string promptMessage = "Interact";

    public string GetPromptMessage() => promptMessage;

    public void Interact()
    {
        if (active)
        {
            active = false;
            FindFirstObjectByType<DialogBox>().dialog = dialog;
            FindFirstObjectByType<DialogBox>().StartDialog();
            DisableObject();
        }
    }

}
