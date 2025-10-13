using System.Collections.Generic;
using UnityEngine;

public class DialogCollision : PersistentObject
{
    public List<Dialog> dialog;


    void OnTriggerEnter(Collider other)
    {
        if (active)
        {
            FindFirstObjectByType<DialogBox>().StartDialog(dialog);
            DisableObject();
        }
    }

}
