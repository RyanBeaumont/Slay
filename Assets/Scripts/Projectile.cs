using Unity.VisualScripting;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    Vector3 startPos;
    Vector3 endPos;
    float startBeat;
    public float destinationBeat;
    public float travelBeats = 2f;
    BeatManager beatManager;
    public bool resolved = false;
    public SummonModel target;

    void Start()
    {
        startPos = transform.position;
        endPos = GameObject.Find("BulletTarget").transform.position;
        beatManager = FindFirstObjectByType<BeatManager>();
        startBeat = beatManager.elapsedBeats;
        destinationBeat = startBeat + travelBeats;
    }

    void Update()
    {
        float elapsedBeats = beatManager.elapsedBeats - startBeat;
        float t = Mathf.Clamp01(elapsedBeats / travelBeats);

        transform.position = Vector3.Lerp(startPos, endPos, t);

        if (t >= 1.2f) Destroy(gameObject);
    }

    public bool TryHit()
    {
        print("TryHit");
    float diff = Mathf.Abs(beatManager.elapsedBeats - destinationBeat);
        if (diff <= beatManager.perfectTolerance)
        {
            GameManager.Instance.Sound("s_camera", 1f);
            resolved = true;
            return true;
        }
        else if (diff <= beatManager.keyTolerance)
        {
            GameManager.Instance.Sound("s_blip", 1f);
            resolved = true;
            return true; // key press consumed
        }
        else
        {
            Health h = target.gameObject.GetComponentInChildren<Health>();
            if (h != null && target.CompareTag("Character"))
            {
                h.Damage(1, 0);
                target.gameObject.GetComponentInChildren<Shaker>().Shake(1);
            }
        }
        Destroy(gameObject);
        return false;
    }
}
