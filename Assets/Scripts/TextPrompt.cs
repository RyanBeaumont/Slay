using System.Threading;
using TMPro;
using UnityEngine;

public class TextPrompt : MonoBehaviour
{
    float timer = 0f;

    // Update is called once per frame
    void Update()
    {
        transform.position += new Vector3(0f, Time.deltaTime * 2f, 0f);
        timer += Time.deltaTime;
        if (timer > 1f) Destroy(gameObject);
    }
}
