using UnityEngine;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    GameObject body;
    CustomAnimator customAnimator;
    public float moveSpeed = 2f;
    public float scale = 0.25f;
    [HideInInspector] public int facing = 1;

    public void Init(Teammate teammate)
    {
        Outfit thisOutfit = teammate.equippedOutfit;
        foreach (Transform child in transform.Find("Character"))
        {Destroy(child.gameObject);}
        body = ClothingRegistry.Instance.SpawnCharacter(teammate.index, thisOutfit, transform.Find("Character"));
        body.transform.localScale = new Vector3(scale, scale, scale);
        customAnimator = body.GetComponent<CustomAnimator>();
        customAnimator.Play("Skeleton_Walk", 0, canAutoUpdate: false, canLoop: true, fps: 8);
    }

    public void ChangeMaterials(Material m)
    {
        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
        {
            sr.material = m;
        }
    }

    public void Walk()
    {
        customAnimator.autoUpdate = true;
    }

    public void EndWalk()
    {
        customAnimator.autoUpdate = false;
    }

    public void Climb()
    {
        customAnimator.Play("Skeleton_Climb", 0, canLoop: true);
    }
    public void EndClimb()
    {
        customAnimator.Play("Skeleton_Walk", 0, canAutoUpdate: false, canLoop: true, fps: 8);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (body != null)
        {
            body.transform.localScale = new Vector2(facing * scale, scale);
            transform.LookAt(transform.position + Camera.main.transform.forward);
        }
    }
}
