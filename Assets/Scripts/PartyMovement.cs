using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PartyManager : MonoBehaviour
{
    public Transform leader;
    public List<Transform> followers;
    public float stepDuration = 0.4f;
    public float gridSize = 2f;
    public bool canMove = true;

    public LayerMask floorLayer; // assign in inspector

    private Queue<Vector3> positionHistory = new Queue<Vector3>();


    public bool isMoving = false; //comment 
    private Vector3 currentDir = Vector3.zero;

    void Start()
    {
        foreach (var f in followers)
            f.GetComponent<Player>().facing = 1;
    }
    public void SetStart(Vector3 pos)
    {
        leader.position = pos;
        positionHistory.Enqueue(pos);
    }

    void Update()
    {
        DialogBox d = FindFirstObjectByType<DialogBox>();
        if (d != null && d.GetComponent<Canvas>().enabled) return;
        if (leader == null) return;
        // Read directional input
        if (!isMoving && canMove)
        {
            if (Input.GetKey(KeyCode.W)) currentDir = Vector3.forward;
            else if (Input.GetKey(KeyCode.S)) currentDir = Vector3.back;
            else if (Input.GetKey(KeyCode.A)) currentDir = Vector3.left;
            else if (Input.GetKey(KeyCode.D)) currentDir = Vector3.right;
            else currentDir = Vector3.zero;

            if (currentDir != Vector3.zero)
                TryMove(currentDir);
        }
    }

    public void RefreshOutfits()
    {
        leader.GetComponent<Player>().Init(OverworldController.Instance.yourTeam[0]);
        for (int i = 0; i < followers.Count; i++)
        {
            followers[i].GetComponent<Player>().Init(OverworldController.Instance.yourTeam[i + 1]);
        }
    }

    void TryMove(Vector3 dir)
    {
        Vector3 targetPos = leader.position + dir * gridSize;

        // ✅ Floor check: is there a floor tile under the target position?
        if (!Physics.Raycast(targetPos + (Vector3.up * 1f), Vector3.down, gridSize, floorLayer))
        {
            // No floor under target
            isMoving = false;
            currentDir = Vector3.zero;
            Debug.Log("No floor tile here!");
            return;
        }

        // ✅ Update leader facing if moving horizontally
        if (dir == Vector3.left) leader.GetComponent<Player>().facing = 1;
        else if (dir == Vector3.right) leader.GetComponent<Player>().facing = -1;

        // Start movement coroutine
        StartCoroutine(MoveStep(leader, targetPos, () =>
        {
            // After step completes, immediately check if key still held
            if (Input.GetKey(KeyCode.W)) currentDir = Vector3.forward;
            else if (Input.GetKey(KeyCode.S)) currentDir = Vector3.back;
            else if (Input.GetKey(KeyCode.A)) currentDir = Vector3.left;
            else if (Input.GetKey(KeyCode.D)) currentDir = Vector3.right;
            else currentDir = Vector3.zero;

            isMoving = false; // allow next step
        }));

        // Record leader’s new position
        positionHistory.Enqueue(targetPos);

        int maxHistory = followers.Count * 2;
        while (positionHistory.Count > maxHistory)
            positionHistory.Dequeue();

        // Move followers
        for (int i = 0; i < followers.Count; i++)
        {
            Vector3[] posArray = positionHistory.ToArray();
            Vector3 followTarget = posArray[Mathf.Max(0, posArray.Length - (i + 1) - 1)];

            Vector3 moveDir = followTarget - followers[i].position;
            if (moveDir.x < 0) followers[i].GetComponent<Player>().facing = 1;
            else if (moveDir.x > 0) followers[i].GetComponent<Player>().facing = -1;

            StartCoroutine(MoveStep(followers[i], followTarget, null));
        }

        isMoving = true;
    }

    System.Collections.IEnumerator MoveStep(Transform character, Vector3 targetPos, System.Action onComplete)
    {
        character.GetComponent<Player>().Walk();
        Vector3 startPos = character.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / stepDuration;
            character.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        character.position = targetPos;
        character.GetComponent<Player>().EndWalk();
        onComplete?.Invoke();
    }

    public System.Collections.IEnumerator ClimbLadder(float climbAmount)
    {
        canMove = false;   // disable normal movement
        isMoving = true;
        float climbDuration = Mathf.Abs(climbAmount) / gridSize * stepDuration;
        positionHistory.Clear();

        Vector3 startPos = leader.position;
        Vector3 targetPos = startPos + Vector3.up * climbAmount;

        float t = 0f;
        Player player = leader.GetComponent<Player>();
        player.facing = -1;
        player.Walk();
        player.Climb();

        while (t < 1f)
        {
            t += Time.deltaTime / climbDuration;
            leader.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        leader.position = targetPos;

        // Also move followers instantly behind leader, or stagger if you want
        for (int i = 0; i < followers.Count; i++)
        {
            followers[i].position = leader.position;
        }
        isMoving = false;
        canMove = true;
        player.EndClimb();
}
}
