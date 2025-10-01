using System.Collections;
using TMPro;
using UnityEngine;

public class MoveName : MonoBehaviour
{
    TMP_Text text;
    private void Start()
    {
        text = GetComponentInChildren<TMP_Text>();
        StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        for (int i = 0; i < 8; i++)
        {
            if (text.color == Color.yellow)
                text.color = new Color(255f, 186f, 0f);
            else
                text.color = Color.yellow;
            yield return new WaitForSeconds(0.2f);
        }
        Destroy(gameObject);
    }
}
