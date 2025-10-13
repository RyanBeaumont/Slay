using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DialogEncounter : ChainedInteractable
{
    public List<Dialog> dialog;

    public override void Interact()
    {
        if (active)
        {
            DialogBox d = FindFirstObjectByType<DialogBox>();
            d.StartDialog(dialog);
            d.OnDialogFinished += OnDialogFinished;
        }
    }

     private void OnDialogFinished()
    {
        // Unsubscribe to avoid duplicate calls
        var dialogBox = FindFirstObjectByType<DialogBox>();
        dialogBox.OnDialogFinished -= OnDialogFinished;

        // Resume interaction chain
        CallNext();
    }

}
