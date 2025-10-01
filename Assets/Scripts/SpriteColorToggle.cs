using UnityEngine;
using System.Collections.Generic;

public class SpriteColorToggle : MonoBehaviour
{
    private List<SpriteRenderer> renderers = new List<SpriteRenderer>();
    private Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();

    private void OnEnable()
    {
        Reload();
    }

    public void Reload()
    {
        renderers.Clear();
        originalColors.Clear();

        renderers.AddRange(GetComponentsInChildren<SpriteRenderer>(includeInactive: true));

        foreach (var sr in renderers)
        {
            if (sr != null && !originalColors.ContainsKey(sr))
                originalColors[sr] = sr.color;
        }
    }

    public void SetBlack()
    {
        foreach (var sr in renderers)
        {
            if (sr != null)
                sr.color = Color.black;
        }
    }

    public void RestoreColors()
    {
        foreach (var sr in renderers)
        {
            if (sr != null && originalColors.TryGetValue(sr, out Color col))
            {
                sr.color = col;
            }
        }
    }
}
