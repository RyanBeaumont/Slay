using Unity.Mathematics;
using UnityEngine;

public class SmoothMove : MonoBehaviour
{
    public float smoothFactor = 0.125f;
    Vector3 startPos;
    Vector3 targetPos;
    Quaternion targetRot = Quaternion.identity;
    void Start()
    {
        startPos = transform.position;
        targetPos = startPos;
    }

    public void MoveTo(Vector3 pos, Quaternion rot)
    {
        targetPos = pos;
        targetRot = rot; 
    }
    public void ReturnToStart()
    {
        targetPos = startPos;
        targetRot = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothFactor);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, smoothFactor);
    }
}
