using UnityEngine;

public class Fire : MonoBehaviour
{
     public Transform target;          // Assign in Inspector or via script
    public float initialSpeed = 2f;   // Random range multiplier
    public float acceleration = 5f;   // How fast it accelerates toward target
    public float arriveThreshold = 0.5f; // Distance at which it "arrives"
    public int damage = 10;

    private Vector2 velocity;

    void Start()
    {
        // Pick a random direction for initial velocity
        Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
        velocity = randomDir * initialSpeed;
    }

    void Update()
    {
        if (target == null) return;

        // Accelerate toward target
        Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
        velocity = Vector2.Lerp(velocity, dir * (velocity.magnitude + acceleration * Time.deltaTime), 0.1f);

        // Move projectile
        transform.position += (Vector3)velocity * Time.deltaTime;

        // Check arrival
        float dist = Vector2.Distance(transform.position, target.position);
        if (dist <= arriveThreshold)
        {
            Arrive();
        }
    }

    void Arrive()
    {
        Health otherHealth = target.GetComponentInChildren<Health>();
        if (otherHealth != null) otherHealth.Damage(damagePerHit:damage, guaranteedHit:true, damageType: new DamageType[]{ DamageType.Hot});
        Destroy(gameObject);  // optional: remove projectile
    }
}
