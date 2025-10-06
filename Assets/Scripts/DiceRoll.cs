using UnityEngine;
using TMPro;
using Unity.VisualScripting;
public class DiceRoll : MonoBehaviour
{
    public Transform diceBody;
    public TMP_Text number;
    Vector3 vel;
    public Health target;
    float zSpeed;
    public float grav;
    float rSpeed;
    float moveSpeed = 0f;
    int roll = 0;
    public int bonus = 0;
    public bool friendly = true;
    bool canSound = true;
    public bool fashion = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        vel = new Vector3(Random.Range(-6, 6), Random.Range(-6, 6), 0f);
        rSpeed = Random.Range(-4f, 4f);
        roll = (int)Random.Range(1, 13) + bonus;
        zSpeed = Random.Range(3, 8);
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null && fashion == false) { Destroy(gameObject); return; }


        if (diceBody.localPosition.y <= 0f && zSpeed <= 0)
        {
            if (canSound)
            {
                canSound = false;
                GameManager.Instance.Sound("s_blip", 0.8f + (roll - 1f) * 0.4f / 11f);
            }
            number.text = $"{roll+bonus}";
            if (bonus > 0) number.color = Color.cyan;
            if (roll == 12) number.color = Color.yellow;
            diceBody.localPosition = Vector3.zero;
            zSpeed = 0f; grav = 0f;
            if (target != null)
            {
                moveSpeed = Mathf.Min(20f, moveSpeed + (Time.deltaTime * 10));
                transform.position = Vector2.MoveTowards(transform.position, target.transform.position, moveSpeed * Time.deltaTime);
                if (Vector2.Distance(transform.position, target.transform.position) < 1f)
                {
                    target.Damage(bonus:bonus);
                    Destroy(gameObject);
                }
            }
            else
            {
                moveSpeed = Mathf.Min(20f, moveSpeed + (Time.deltaTime * 10));
                Vector3 targetPos = new Vector3(0f, 4f, 0f);
                transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                if (Vector2.Distance(transform.position, targetPos) < 2f)
                {
                    MasterLifebar.Instance.Damage(roll, bonus, friendly);
                    Destroy(gameObject);
                }
            }
        }
            else
            {
                zSpeed += grav * Time.deltaTime;
                vel *= Mathf.Exp(-2 * Time.deltaTime);
                transform.position += vel * Time.deltaTime;
                diceBody.Rotate(new Vector3(0f, 0f, rSpeed));
            }
        diceBody.localPosition = new Vector3(0f, diceBody.localPosition.y + (zSpeed * Time.deltaTime), 0f);
    }
        
}
