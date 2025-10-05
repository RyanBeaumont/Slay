using System.Collections.Generic;
using UnityEngine;

public class DialogCollision : PersistentObject
{
    public List<Dialog> dialog;


    void OnTriggerEnter(Collider other)
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
