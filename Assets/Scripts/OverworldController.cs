using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class Outfit
{
    public int[] outfit = { 0, 1 };
    public Color[] colors = { Color.white, Color.white };
    public string name = "Noob Outfit";
    public int damageTaken = 0;
    public List<Spell> spells = new List<Spell>();
}

[Serializable]
public class Teammate
{
    public int index;
    public string name;
    public int level;
    public float xp;
    public Outfit equippedOutfit;
    public int attackPoints = 1;
    public int defensePoints = 1;
    public int fashionPoints = 1;
    public int unassignedPoints = 0;
    public ClothingStats GetBaseStats()
    {
        return new ClothingStats() {
            damage = 2,// + attackPoints / 2,
            hp = 0,//2 + yourTeam[i].defensePoints / 2, //HEALTH can't change since it is stored in the card data
            armor = 4,// + defensePoints / 2,
            bonus = 0// + attackPoints / 2,
        };
    }
}
public class OverworldController : MonoBehaviour
{
    public string currentScene;
    public List<Teammate> yourTeam;
    public List<Outfit> yourOutfits;
    public List<Outfit> yourDiscard;
    public List<int> yourClothes;
    public GameObject playerPrefab;
    public Material playerMaterial;
    public int spawnPointIndex;
    [HideInInspector] public Vector3 playerPosition = Vector3.zero;
    public HashSet<string> finishedEncounters = new HashSet<string>();
    public int hp;
    public int xpToLevelUp = 100;
    public int xpPerLevelMultiplier = 25;
    [HideInInspector] public int cash = 0;
    public Sprite[] faces;
    List<Quest> activeQuests = new List<Quest>();
    List<Quest> completedQuests = new List<Quest>();
    public int armor;
    List<GameObject> enemiesForNextEncounter;
    int enemyHp, enemyArmor;

    public static OverworldController Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // enforce singleton
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        foreach (Outfit thisOutfit in yourOutfits)
        {
            thisOutfit.spells = ClothingRegistry.Instance.GetSpells(thisOutfit);
        }

        //Initial outfits
        foreach (Teammate t in yourTeam)
        {
            if (yourOutfits.Count > 0)
            {
                Outfit thisOutfit = yourOutfits[0];
                t.equippedOutfit = thisOutfit;
                yourOutfits.Remove(thisOutfit);
            }
            else
            {
                Debug.Log("Too few outfits - deploying NOOB OUTFIT");
            }
        }

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) //Debug: Add all clothing to registry
        {
            for (int i = 0; i < ClothingRegistry.Instance.clothing.Count; i++)
                yourClothes.Add(i);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // always unsubscribe!
    }

    public void ReturnToOverworld()
    {
        SceneManager.LoadScene(currentScene);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find the GameManager in the new scene
        if (SceneManager.GetActiveScene().name != "BattleScene" && SceneManager.GetActiveScene().name != "TitleScene")
            currentScene = SceneManager.GetActiveScene().name;
        GameManager gm = FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            gm.enabled = true;
            gm.Init(yourTeam, enemiesForNextEncounter); // <-- call your function here
        }
        PartyManager pm = FindAnyObjectByType<PartyManager>();
        if (pm != null)
        {
            PersistentObject[] encounters = FindObjectsByType<PersistentObject>(FindObjectsSortMode.None);
            foreach (PersistentObject e in encounters)
            {
                if (finishedEncounters.Contains(e.encounterID))
                {
                    e.DisableObject();
                }
            }
            foreach (OverworldSpawnPoint s in FindObjectsByType<OverworldSpawnPoint>(FindObjectsSortMode.None))
            {
                if (s.index == spawnPointIndex) playerPosition = s.transform.position;
            }
            for (int i = 0; i < yourTeam.Count; i++)
            {
                var player = Instantiate(playerPrefab, playerPosition, Quaternion.identity);
                player.GetComponent<Player>().Init(yourTeam[i]);
                if (i == 0) pm.leader = player.transform;
                else pm.followers.Add(player.transform);
                pm.SetStart(player.transform.position);
                player.GetComponent<Player>().ChangeMaterials(playerMaterial);

            }
            FindFirstObjectByType<OverworldCamera>().target = pm.leader;
            pm.leader.GetComponent<PlayerInteractor>().enabled = true;
            GameObject dialogBox = Instantiate(Resources.Load<GameObject>("Dialog"));
            dialogBox.GetComponent<Canvas>().worldCamera = Camera.main;
            dialogBox.GetComponent<Canvas>().sortingLayerName = "Foreground";
        }

    }

    public void LevelUp(int newXp)
    {
        foreach (Teammate teammate in yourTeam)
        {
            int maxXp = teammate.level * xpPerLevelMultiplier + xpToLevelUp;
            teammate.xp += newXp;
            while (teammate.xp >= maxXp)
            {
                teammate.xp -= maxXp;
                teammate.level++;
                maxXp = teammate.level * xpPerLevelMultiplier + xpToLevelUp;
                teammate.attackPoints++; teammate.defensePoints++; teammate.fashionPoints++; teammate.unassignedPoints++;
            }
        }
    }
    
    //Quest logic
    public void StartQuest(Quest quest)
    {
        if (!activeQuests.Contains(quest))
            activeQuests.Add(quest);
    }

    public float CompleteQuest(string questName, float progress)
    {
    Quest quest = activeQuests.FirstOrDefault(q => q.name == questName);
    if (!activeQuests.Contains(quest))
    {
        Debug.LogWarning($"[QuestManager] Tried to update quest '{quest.name}' but it isn't active.");
        return 0f;
    }

    // Increment progress safely
    quest.progress += progress;
    quest.progress = Mathf.Clamp01(quest.progress); // ensures it stays between 0–1

        if (quest.progress >= 1f)
        {
            // Quest complete
            activeQuests.Remove(quest);
            completedQuests.Add(quest);
            Debug.Log($"[QuestManager] Quest '{quest.name}' completed!");
        }
        else
        {
            Debug.Log($"[QuestManager] Quest '{quest.name}' progress: {quest.progress:P0}");
        }
        return quest.progress;
}

    public bool IsQuestComplete(string quest) => completedQuests.FirstOrDefault(q => q.name == quest) != null;
    public bool IsQuestActive(string quest) => activeQuests.FirstOrDefault(q => q.name == quest) != null;

    public void Encounter(List<GameObject> enemyList)
    {
        enemiesForNextEncounter = enemyList;
        SceneManager.LoadScene("BattleScene");
    }
}
