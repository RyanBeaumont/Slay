using System.Collections.Generic;
using UnityEngine;

public class Encounter : PersistentObject
{
    public List<GameObject> enemies;
    public float radius = 0.5f;                  // Distance from cube center
    float heightOffset = -0.5f;
    void Awake()
    {
        SpawnBillboards();
        
    }

    public void OnTriggerEnter(Collider collision)
    {
        if (active && collision.gameObject.CompareTag("Player"))
        {
            OverworldController o = FindAnyObjectByType<OverworldController>();
            DisableObject();
            Vector3 pos = collision.gameObject.transform.position;
            o.playerPosition = new Vector3(
                Mathf.Round(pos.x),
                Mathf.Round(pos.y),
                Mathf.Round(pos.z)
            );
            o.spawnPointIndex = -1;
            o.Encounter(enemies);
        }
    }

    public override void DisableObject()
    {
        base.DisableObject();
        Destroy(gameObject);
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
