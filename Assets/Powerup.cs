using UnityEngine;
using TMPro;

public class Powerup : MonoBehaviour
{
    public float ySpeed = 0.5f;      // Initial upward velocity
    public float grav = -0.1f;       // Gravity per frame
    public float moveSpeed = 1f;     // Horizontal/forward movement speed
    public Transform target;         // Who the powerup is flying toward
    private Transform sprite;
    public StatusEffect statusEffect;
    public bool useStatusEffect = false;
    public int swag = 0;
    public Sprite[] sprites;

    void Start()
    {
        sprite = transform.Find("Powerup");
        SpriteRenderer sr = sprite.GetComponent<SpriteRenderer>();
        if (swag > 0) sr.sprite = sprites[0];
        if (useStatusEffect)
        {
            if (statusEffect.name == Status.ArmorBoost) sr.sprite = sprites[1];
            if (statusEffect.name == Status.AttackBoost) sr.sprite = sprites[2];
            if (statusEffect.name == Status.BonusBoost) sr.sprite = sprites[3];
            if (statusEffect.name == Status.Poison) sr.sprite = sprites[4];
        }
    }

    void Update()
    {
        if (target == null) return;
        // --- Move toward target horizontally ---
        Vector3 direction = (target.position - transform.position).normalized;
        Vector3 horizontal = new Vector3(direction.x, 0f, direction.z) * moveSpeed * Time.deltaTime;

        // --- Apply gravity-like vertical motion ---
        Vector3 vertical = Vector3.up * ySpeed * Time.deltaTime;
        ySpeed += grav * Time.deltaTime * 60f; // simulate gravity per frame

        // --- Apply both movements ---
        transform.position += horizontal;
        sprite.localPosition += vertical;

        // --- If close enough to target, destroy and trigger pickup ---
        if (sprite.transform.localPosition.y < 5f && ySpeed < 0)
        {
            GameObject prompt = Instantiate(Resources.Load<GameObject>("HitPrompt"), transform.position, Quaternion.identity);
            TMP_Text text = prompt.GetComponentInChildren<TMP_Text>();
            if (statusEffect != null && useStatusEffect)
            {
                target.GetComponentInChildren<Health>().AddStatusEffect(statusEffect);
                if (statusEffect.name == Status.AttackBoost) text.text = $"{statusEffect.amount} Attack";
                if (statusEffect.name == Status.ArmorBoost) text.text = $"{statusEffect.amount} Armor";
                if (statusEffect.name == Status.BonusBoost) text.text = $"{statusEffect.amount} Accuracy";
                if (statusEffect.name == Status.Poison) text.text = $"{statusEffect.amount} Poison";
            }
            if (swag > 0)
            {
                target.GetComponentInChildren<SummonModel>().AddSwag(swag);
                text.text = $"+{swag} swag";
            }
            Destroy(gameObject);
        }
    }
}
