using UnityEngine;
using UnityEngine.UI;  // if using UI Text

public class PlayerInteractor : MonoBehaviour
{
    public float interactRange = 2f;
    public KeyCode interactKey = KeyCode.E;
    public GameObject promptPrefab;  // assign a prefab in Inspector

    private IInteractable currentTarget;
    private GameObject currentPrompt;

    void Update()
    {
        FindInteractable();

        if (currentTarget != null)
        {
            if (currentPrompt == null)
            {
                // Spawn prompt above target
                currentPrompt = Instantiate(promptPrefab,  ((MonoBehaviour)currentTarget).transform.position + Vector3.up * 2f, Quaternion.identity);
                currentPrompt.GetComponentInChildren<TMPro.TMP_Text>().text = $"[E] { currentTarget.GetPromptMessage()}";
            }
            else
            {
                // Keep prompt positioned above target
                currentPrompt.transform.position =  ((MonoBehaviour)currentTarget).transform.position + Vector3.up * 2f;
            }

            // Interact
            if (Input.GetKeyDown(interactKey))
            {
                currentTarget.Interact();
            }
        }
        else
        {
            if (currentPrompt != null) Destroy(currentPrompt);
        }
    }

    void FindInteractable()
    {
        currentTarget = null;

        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange);
        float closest = Mathf.Infinity;

        foreach (var hit in hits)
        {
            var interactable = hit.GetComponent<IInteractable>();
            if (interactable != null)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < closest)
                {
                    closest = dist;
                    currentTarget = interactable;
                }
            }
        }
    }
}
