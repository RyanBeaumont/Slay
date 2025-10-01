using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpriteEntry
{
    public string key;
    public Sprite sprite;
}


public class EnemyAnimator : MonoBehaviour
{
    [SerializeField] private List<SpriteEntry> spriteList;
    private Dictionary<string, Sprite> sprites;
    float _pulseSize = 1.15f;
    float _returnSpeed = 5f;
    float shake = 0f;
    Vector2 originalPos;
    float decayRate = -8f;
    Vector3 startSize;
    Vector3 size;

    void Awake()
    { //Rebuild the dictionary here because Unity can't serialize key value pairs
        sprites = new Dictionary<string, Sprite>();
        foreach (var entry in spriteList)
            sprites[entry.key] = entry.sprite;
    }
    void Start()
    {
        startSize = transform.localScale;
        size = transform.localScale;
        originalPos = transform.localPosition;
    }

    public void SetSize(float newSize) { size = new Vector3(newSize, newSize, newSize); }
    public void ResetSize() { size = startSize; }

    public void ChangeSprite(string name, bool pulse, bool doShake)
    {
        Sprite s = sprites[name];
        if (s != null)
            transform.GetComponentInChildren<SpriteRenderer>().sprite = s;
        if (pulse) Pulse();
        if (doShake) shake = 0.05f;
    }

    public void Pulse()
    {
        transform.localScale = size * _pulseSize;
    }


    void FixedUpdate()
    {
       
        transform.Find("Sprite").localPosition = originalPos + new Vector2(Random.Range(-shake, shake), Random.Range(-shake, shake));
        shake *= Mathf.Exp(decayRate * Time.deltaTime);
    }

    void Update()
    {
         transform.localScale = Vector3.Lerp(transform.localScale, size, Time.deltaTime * _returnSpeed);
    }
}
