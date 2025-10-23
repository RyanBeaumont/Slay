using System.Collections.Generic;
using UnityEngine;
public class TriggerInteractable : ChainedInteractable
{
    public void OnTriggerEnter(Collider collision)
    {
        if (active && collision.gameObject.CompareTag("Player"))
        {
            Interact();
        }
    }

    public override void Interact()
    {
        if (active)
        {
            CallNext();
        }  
    }
}
