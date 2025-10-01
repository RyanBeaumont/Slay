using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.Mathematics;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
    {

    public bool runTimer = true;
    public bool waitForInput = false;
    [HideInInspector] public HandManager handManager;
    public GameObject handOfCards;
    public GameObject summonModelPrefab;
    [HideInInspector] public float timer = 1f;
    public Transform background;
    public DialogBox dialogBox;
    public TMP_Text descriptionText;
    public int[] startingClothing;
    [HideInInspector] public List<GameAction> gameActions = new List<GameAction>();
    GameObject playerModel; GameObject enemyModel;
    BattleMenu battleMenu;
    public RectTransform victoryScreen;
    public RectTransform defeatScreen;
    public TMP_Text debugActions;
    bool endTurn = false;
    
    public int maxSwag = 10;
    public int swag = 1;
    int totalXP;
    int totalCash;
    List<int> totalLoot = new List<int>();
    public TMP_Text swagMeter;
    bool hasExecuted = false;
    int actionToDelete;
    public float orchestraPitch = 1f;
    public List<SummonModel> models = new List<SummonModel>();
    public List<Enemy> enemies = new List<Enemy>();
    bool turnStillGoing = false;
    public event Action OnTurnStart;
    public event Action OnSummon; //When any character is summoned
    public event Action OnDeath; //When any character dies
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindFirstObjectByType<GameManager>();
            }

            return _instance;
        }
    }

 


    void Start()
    {
        handManager = FindFirstObjectByType<HandManager>();
        battleMenu = FindFirstObjectByType<BattleMenu>();
        battleMenu.transform.GetComponent<Canvas>().enabled = false;
        descriptionText.color = Color.green;
        
        handOfCards.SetActive(false);

    //NextTurn();
    }

    public void Init(List<Teammate> yourTeam, List<GameObject> startingEnemies)
    {
        //GetComponent<MasterLifebar>().Init(hp, armor, enemyHP, enemyArmor);

        GameObject summonPoint = GameObject.Find("SummonPoint");
        //Spawn main character
        for (int i = 0; i < yourTeam.Count; i++)
        {
            GameObject summonModel = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("SummonPrefab"), summonPoint.transform.position + new Vector3(1f * i, 0.3f * i), Quaternion.identity, summonPoint.transform);
            SummonModel s = summonModel.GetComponent<SummonModel>();
            s.Init(yourTeam[i]);
            
        }
        foreach (GameObject e in startingEnemies)
        {
            EnemySummonAction action = new EnemySummonAction
            {
                modelPrefab = e,
            };
            gameActions.Add(action);
        }
    }

     public IEnumerator SlowTime(float slowDuration = 0.2f, float rampDuration = 0.5f)
    {
        // Go to half speed instantly
        Time.timeScale = 0.25f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // keep physics consistent

        // Stay slowed for a short moment
        yield return new WaitForSecondsRealtime(slowDuration);

        // Ramp back to 1.0 smoothly over rampDuration (use unscaled time so it's independent of timeScale)
        float t = 0f;
        while (t < rampDuration)
        {
            t += Time.unscaledDeltaTime;
            float factor = t / rampDuration;
            Time.timeScale = Mathf.Lerp(0.5f, 1f, factor);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            yield return null;
        }

        // Ensure full reset
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    void Update()
    {
        models = FindObjectsOfType<SummonModel>(true).ToList();
        enemies = FindObjectsOfType<Enemy>(true).ToList();

            background.transform.position = Camera.main.transform.position * 0.8f;
            swagMeter.text = $"Swagger: {swag}/{maxSwag}";
        string debugText = "";
        foreach (GameAction action in gameActions)
        {
            debugText += $"{action.GetType()}\n";
        }
        if(runTimer){ debugActions.color = Color.black; }else{ debugActions.color = Color.yellow; }
        debugActions.text = debugText;
        if (gameActions.Count > 0)
        {

            descriptionText.color = Color.yellow;
            if (!hasExecuted)
            {
                GameAction thisAction = gameActions[gameActions.Count - 1];
                actionToDelete = gameActions.Count - 1;
                if (thisAction.dialog.Length > 0 && !dialogBox.GetComponent<Canvas>().enabled)
                {
                    dialogBox.BasicDialog(thisAction.dialog);
                    //Don't go any further until dialog disappears
                    return;
                }
                FindFirstObjectByType<CameraController>().MoveCamera(Vector3.zero, 6f);
                thisAction.Execute();
                hasExecuted = true;
            }
            GameObject[] dice = GameObject.FindGameObjectsWithTag("Dice");
            if (dice.Length == 0 && runTimer && !waitForInput) { timer -= Time.deltaTime; }
            if (timer <= 0 && runTimer && !waitForInput)
            {
                ShowAll();
                FindFirstObjectByType<CameraController>().MoveCamera(Vector3.zero, 6f);
                gameActions.RemoveAt(actionToDelete);
                hasExecuted = false;
                waitForInput = false;
                timer = 0.5f;
                if (gameActions.Count == 0 && turnStillGoing) //End of the round
                {
                    BeginNewTurn();
                    turnStillGoing = false;
                }
            }
        }
        else
        {
            descriptionText.color = Color.green;
            descriptionText.text = "Your turn - click a model";
            if (handOfCards.activeInHierarchy)
            {
                //battleMenu.transform.GetComponent<Canvas>().enabled = false;
                if (Input.GetKey(KeyCode.Escape) || Input.GetMouseButtonDown(2)) { handOfCards.SetActive(false); battleMenu.transform.GetComponent<Canvas>().enabled = true; }
            }
            else
            {
                //battleMenu.transform.GetComponent<Canvas>().enabled = true;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                EndPlayerTurn();
            }
            if (endTurn == true)
            {
                endTurn = false;
                EndPlayerTurn();
            }
        }
                
        }
    void BeginNewTurn()
    {
        battleMenu.transform.GetComponent<Canvas>().enabled = false;
        OnTurnStart?.Invoke(); //Tell the players to trigger their turn start event
        swag = Mathf.Min(maxSwag, swag + 1);
        //deckManager.DrawCard(handManager);
        models = FindObjectsByType<SummonModel>(FindObjectsSortMode.InstanceID).ToList();
        foreach (SummonModel m in models)
        {
            m.NewTurn();
        }
    }

    public void TryEndTurn()
    {
        foreach (SummonModel m in models)
        {
            if (m.playerState != PlayerState.Dead && m.actions > 0)
            {
                return; //Don't advance
            }
        }
        endTurn = true; //Advance only if code didn't stop before
    }

    void EndPlayerTurn()
    {
        print("Turn over");
        foreach (Enemy enemy in GameObject.FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        { enemy.AddAttackToQueue(); }
        foreach (SummonModel model in GameObject.FindObjectsByType<SummonModel>(FindObjectsSortMode.None))
        {
            model.RestoreColors();
        }
        turnStillGoing = true;
        orchestraPitch = 1f;
    }

    public void ShowAll()
    {
        foreach (Enemy e in enemies) { e.gameObject.SetActive(true); }
        foreach (SummonModel m in models)
        {
            m.gameObject.SetActive(true);
        }
    }
    public void HideExcept(GameObject[] objects)
    {
        // Enemies
        foreach (Enemy e in enemies)
        {
            if (e != null)
            {
                bool keepActive = System.Array.IndexOf(objects, e.gameObject) >= 0;
                e.gameObject.SetActive(keepActive);
            }
        }

        // SummonModels
        foreach (SummonModel m in models)
        {
            if (m != null)
            {
                bool keepActive = System.Array.IndexOf(objects, m.gameObject) >= 0;
                m.gameObject.SetActive(keepActive);
            }
        }
    }

    public void CheckForWin()
    {
        print("Checking for win");
        if (enemies.Count <= 1 && !victoryScreen.gameObject.activeInHierarchy)
            Win();
    }
    void Win()
    {
        print("Win");
        victoryScreen.gameObject.SetActive(true);
        var lootContainer = victoryScreen.Find("WinPanel/Loot");
        var characterContainer = victoryScreen.Find("WinPanel/Characters");
        var lootText = Instantiate(Resources.Load<GameObject>("LootText"),lootContainer).GetComponent<TMP_Text>();
        lootText.text = $"XP: +{totalXP}";
        lootText = Instantiate(Resources.Load<GameObject>("LootText"),lootContainer).GetComponent<TMP_Text>();
        lootText.text = $"Moneys: +{totalCash}";

        foreach (Teammate teammate in OverworldController.Instance.yourTeam)
        {
            var characterPrefab = Instantiate(Resources.Load<GameObject>("LevelUpPanel"), characterContainer);
            var tt = characterPrefab.GetComponent<RectTransform>();
            tt.Find("Image").GetComponent<Image>().sprite = OverworldController.Instance.faces[teammate.index];
            tt.Find("Name").GetComponent<TMP_Text>().text = $"{teammate.name} - Lv {teammate.level}";
            StartCoroutine(AnimateSlider(tt, teammate, totalXP));
            OverworldController.Instance.cash += totalCash;
            OverworldController.Instance.yourClothes.AddRange(totalLoot);
            //OverworldController.Instance.LevelUp(totalXP); Supposedly AnimateSlider directly updates the level

        }
    }

    public void CheckForLose()
    {
        foreach (SummonModel m in models)
        {
            if (m.playerState != PlayerState.Dead) return;
        }
        //If all dead
        Lose();
    }
    void Lose()
    {
        Debug.Log("YOU LOSE");
        defeatScreen.gameObject.SetActive(true);
    }

    IEnumerator AnimateSlider(RectTransform tt, Teammate teammate, int xpGain)
{
    Slider slider = tt.GetComponentInChildren<Slider>();
    TMP_Text levelText = tt.Find("Name").GetComponent<TMP_Text>();
    TMP_Text xpText = tt.Find("XP").GetComponent<TMP_Text>();
        PulseToTheBeat pulse = tt.GetComponentInChildren<PulseToTheBeat>();
    int xpToNextLevel = OverworldController.Instance.xpPerLevelMultiplier * teammate.level 
                        + OverworldController.Instance.xpToLevelUp;

    // Add XP gradually (animation speed)
    float speed = 200f; // XP per second; tweak as needed
    float remaining = xpGain;

    while (remaining > 0f)
    {
        float step = Mathf.Min(speed * Time.deltaTime, remaining);
        remaining -= step;
        teammate.xp += step;
        slider.value += step;
        xpText.text = $"{Mathf.Round(teammate.xp)}/{xpToNextLevel}";
        // Check level-up condition
        while (teammate.xp >= xpToNextLevel)
        {
            teammate.xp -= xpToNextLevel;
            teammate.level++;
            levelText.text = $"{teammate.name} - Lv {teammate.level}";
                pulse.Pulse();
            // reset slider
            slider.value = teammate.xp;
            slider.maxValue = xpToNextLevel = OverworldController.Instance.xpPerLevelMultiplier * teammate.level
                                              + OverworldController.Instance.xpToLevelUp;
        }

        yield return null; // wait one frame
    }

    // Final correction
    slider.value = teammate.xp;
}

    public void AddLoot(int xp, int cash, int item)
    {
        totalXP += xp; totalCash += cash; if(item != -1) totalLoot.Add(item);
    }

    public void ShowBattleMenu(SummonModel caller)
    {
        if (gameActions.Count == 0 || gameActions[0] is DeathSwap)
        {
            battleMenu.SetOptions(caller);
            FindFirstObjectByType<CameraController>().MoveCamera(caller.transform.position + new Vector3(0f, 3f, 0f), 5.5f);
        }
    }

    public void ReturnToOverworld()
    {
        OverworldController.Instance.ReturnToOverworld();
    }

    
    public void LoadLastGame()
    {
        ReturnToOverworld(); //PLACEHOLDER
    }

    public void Sound(string sound, float pitch)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = Resources.Load<AudioClip>($"Sound/{sound}");
        source.pitch = pitch;
        source.Play();
        Destroy(source, source.clip.length);
    }
}
