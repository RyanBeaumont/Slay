using System.Collections.Generic;
using UnityEngine;

public class Encounter : ChainedInteractable
{
    public List<GameObject> enemies;

    public override void Interact()
    {
        if (active)
        {
            OverworldController o = FindAnyObjectByType<OverworldController>();
            o.Encounter(enemies, gameObject);
        }
    }
}
