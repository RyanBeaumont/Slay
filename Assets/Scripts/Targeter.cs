using TMPro;
using UnityEngine;

public class Targeter : MonoBehaviour
{
    GameAction gameAction;
    GameObject caller;
    string targetType;
    bool consumeAction = false;
    public TMP_Text label;

    public void Init(GameAction action, GameObject cAller, string text, string targetTag, bool consume)
    {
        gameAction = action; caller = cAller; targetType = targetTag; consumeAction = consume; label.text = text;
        FindFirstObjectByType<CameraController>().MoveCamera(Vector3.zero, 6f);
    }

    void SelectTarget(GameObject target) //Add the attack to the stack once the target is chosen
    {
        gameAction.target = target;
        gameAction.caller = caller;
        GameManager.Instance.gameActions.Add(gameAction);
        GameManager.Instance.waitForInput = false;
    }

    private GameObject currentTarget;

    void Update()
    {
        // 1. Find closest object to the mouse with matching tag
        if (targetType == null) return;
        GameObject[] candidates = GameObject.FindGameObjectsWithTag(targetType);
        float closestDist = float.MaxValue;
        GameObject bestTarget = null;
        Vector3 mousePos = Input.mousePosition;
        foreach (GameObject obj in candidates)
        {
            if (obj == null) continue;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(obj.transform.position);
            float dist = Vector2.Distance(mousePos, screenPos);
            if (dist < closestDist)
            {
                closestDist = dist;
                bestTarget = obj;
            }
        }

        currentTarget = bestTarget;
        if (currentTarget != null)
            transform.position = currentTarget.transform.position;

        // 2. If left mouse is clicked and a valid target exists, select it
        if (Input.GetMouseButtonDown(0) && currentTarget != null)
        {
            SelectTarget(currentTarget);
            if (consumeAction)
            {
                caller.GetComponent<SummonModel>().ConsumeAction();
            }
            Destroy(gameObject);
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            Destroy(gameObject);
        }
    }
}
