using UnityEngine;

public class Shaker : MonoBehaviour
{
    float shake = 0f;
    Vector2 originalPos;
    public float decayRate = -8f;
    void Start()
    {
        originalPos = transform.localPosition;
    }

    void FixedUpdate()
    {
        transform.localPosition = originalPos + new Vector2(Random.Range(-shake, shake), Random.Range(-shake, shake));
        shake *= Mathf.Exp(decayRate * Time.deltaTime);
    }

    public void Shake(float amount)
    {
        shake = amount;
    }
}
