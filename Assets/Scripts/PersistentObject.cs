using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    [HideInInspector] public string encounterID;
    [HideInInspector] public bool active = true;
    void Awake()
    {
        encounterID = $"{gameObject.scene.name}_{transform.position}";
    }

    public virtual void DisableObject()
    {
        OverworldController.Instance.finishedEncounters.Add(encounterID);
        active = false;
    }
}
