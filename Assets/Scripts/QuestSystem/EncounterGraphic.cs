using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class EncounterGraphic : ChainedInteractable
{
    float heightOffset = 0f;
    public float radius = 1f;
    List<GameObject> enemies = new List<GameObject>();
    void Start()
    {
        enemies = new List<GameObject>(GetComponent<Encounter>().enemies);
    }
    public override void Interact()
    {
        SpawnCharacters();
    }
    public IEnumerator SpawnCharacters()
    {
        SpawnBillboards();
        yield return new WaitForSeconds(1f);
        CallNext();
    }
    void SpawnBillboards()
    {
        if (enemies == null || enemies.Count == 0) return;

        float angleStep = 360f / enemies.Count;

        for (int i = 0; i < enemies.Count; i++)
        {
            // Calculate position on a circle around the cube
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 spawnPos = transform.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            spawnPos.y += heightOffset;

            // Instantiate billboard
            GameObject billboard = Instantiate(Resources.Load<GameObject>("BillboardCharacter"), transform);

            // Find the child "Sprite" object and update its SpriteRenderer
            Transform spriteChild = billboard.transform.Find("Sprite");
            if (spriteChild != null)
            {
                SpriteRenderer sr = spriteChild.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    // If your enemy prefab has a SpriteRenderer, copy its sprite
                    SpriteRenderer enemySR = enemies[i].GetComponent<SpriteRenderer>();
                    if (enemySR != null)
                    {
                        sr.sprite = enemySR.sprite;
                    }
                }
            }

            // Parent to the cube so they move with it
            billboard.transform.SetParent(transform);
        }
    }
}
