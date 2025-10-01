using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class MasterLifebar : MonoBehaviour
{
    [SerializeField] int friendlyHp = 20, enemyHp = 20;
    int maxFriendlyHp, maxEnemyHp;
    [SerializeField] int friendlyArmor, enemyArmor = 6;
    public Shaker friendlyShaker, enemyShaker;
    public Slider friendlySlider, enemySlider;
    public TMP_Text friendlyArmorText, enemyArmorText;

private static MasterLifebar _instance;

    public static MasterLifebar Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindFirstObjectByType<MasterLifebar>();
            }

            return _instance;
        }
    }
    void Start()
    {
        maxEnemyHp = enemyHp;
        maxFriendlyHp = friendlyHp;
        SetStats();
    }

    public void Init(int HP, int ARMOR, int EHP, int EARMOR)
    {
        friendlyArmor = ARMOR; friendlyHp = HP; enemyHp = EHP; enemyArmor = EARMOR;
        SetStats();
    }

    void SetStats() {
        friendlyArmorText.text = $"{friendlyArmor}"; enemyArmorText.text = $"{enemyArmor}";
        friendlySlider.maxValue = maxFriendlyHp; enemySlider.maxValue = maxEnemyHp;
        friendlySlider.value = friendlyHp; enemySlider.value = enemyHp;
    }

    public void Damage(int roll, int bonus, bool friendly)
    {
        int armor = friendlyArmor;
        if (!friendly) armor = enemyArmor;
        GameObject prompt = Instantiate(Resources.Load<GameObject>("HitPrompt"), transform.position, Quaternion.identity);
        TMP_Text text = prompt.GetComponentInChildren<TMP_Text>();
        text.color = Color.magenta;
        if (roll + bonus >= armor)
        {
            if (roll == 12)
            {
                text.text = "Critical";
                text.color = Color.yellow;
                if (friendly) { enemyHp -= 2; enemyShaker.Shake(0.3f); }
                else { friendlyHp -= 2; friendlyShaker.Shake(0.3f); }

            } //Critical
            else
            {
                if (friendly) { enemyHp -= 1; enemyShaker.Shake(0.1f); }
                else { friendlyHp -= 1; friendlyShaker.Shake(0.1f); }
                string[] prompts = { "Flawless","Iconic","Stunning","Modern","Chic","Retro","OMG","Queen","Go Off","Girlll","Slay"};
                text.text = prompts[Random.Range(0, prompts.Length)];
            }
            GameManager.Instance.Sound("s_camera", Random.Range(0.8f, 1.2f));
        }
        else
        {
            text.color = Color.cyan;
            string[] prompts = { "Uncool","Shame","Bland","Basic","Try harder","Boring","Meh","Ugly"};
            text.text = prompts[Random.Range(0, prompts.Length)];
            GameManager.Instance.Sound("s_miss", UnityEngine.Random.Range(0.8f, 1.2f));
        }
        SetStats();
    }
}
