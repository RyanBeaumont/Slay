using UnityEngine;

public class Ladder : ChainedInteractable
{
    public float climbHeight = 2f;
    public bool climbUp = true;
    public override void Interact()
    {
        PartyManager pm = GameObject.FindFirstObjectByType<PartyManager>();
        if (pm != null && pm.canMove)
        {
            pm.leader.transform.position = transform.position;
            float dir = climbUp ? 1f : -1f;
            pm.StartCoroutine(pm.ClimbLadder(dir * climbHeight));
        }
    }
}
