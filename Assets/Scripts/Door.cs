using UnityEngine;

public class Door : ChainedInteractable
{
    public int spawnPointIndex;
    public string sceneName;

    public override void Interact()
    {
        OverworldController.Instance.spawnPointIndex = spawnPointIndex;
        OverworldController.Instance.currentScene = sceneName;
        OverworldController.Instance.ReturnToOverworld();
    }
}
    
