using UnityEngine;

public class Ladder : MonoBehaviour, IInteractable
{
    public string promptMessage;
    public string GetPromptMessage() => promptMessage;
    public float climbHeight = 2f;
    public bool climbUp = true;
    public void Interact()
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
