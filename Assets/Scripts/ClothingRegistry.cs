using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.U2D.Animation;
using System.Linq.Expressions;
using System.Linq;
public enum AttackTrigger
{
    OnTurnStart, OnEnter, OnDeath, None
}
[Serializable] public class Spell {
    public string name;
    public string description;
    public int cost = 1;
    public int index = 0;
    public int charges = 1;
}

[Serializable]
    public class TriggeredAttack
    {
        public AttackTrigger trigger;
        public string attackFunction; // must match a function on this component
    }

[System.Serializable]
public class ClothingStats
{
    public string name;  public string description; public string abilityDescription; public int hp; public int damage; public int armor; public int bonus; public int cost;
    public List<TriggeredAttack> attackTriggers = new List<TriggeredAttack>();
}
public class ClothingRegistry : MonoBehaviour
{
    private static ClothingRegistry _instance;

    public static ClothingRegistry Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindFirstObjectByType<ClothingRegistry>();
            }

            return _instance;
        }
    }

    public int GetIndex(string clothingName)
    {
        for (int i = 0; i < clothing.Count; i++)
        {
            if (clothing[i] != null && clothing[i].name == clothingName)
            {
                return i;
            }
        }

        Debug.LogWarning($"Clothing item '{clothingName}' not found in list.");
        return -1; // not found
    }

    public List<GameObject> clothing = new List<GameObject>();
    public List<ClothingStats> clothingStats;
    public List<GameObject> characters;
    public List<Spell> spells;

    [Header("Spawn Target")]
    public GameObject skeletonPrefab;
    private static readonly Dictionary<string, int> orderMap = new()
    {
        { "Hair",  0 },
        { "LArm",   2 },
        { "LLeg",   4 },
        {"LFoot", 13},
        { "Bottom", 9 },
        { "TopTuckedIn",  6 },
        { "Top",  10 },
        { "Jacket",  9 },
        { "Belt",  7 },
        { "RLeg",   8 },
        {"RFoot", 14},
        { "RArm",  12 },
        { "Head",  13 },
        { "Accessory",  14 },

    };

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        //clothing = new List<GameObject>(Resources.LoadAll<GameObject>("Clothes"));
        // Extract stats
        foreach (GameObject prefab in clothing)
        {
            PrefabStats statsHolder = prefab.GetComponent<PrefabStats>();
            if (statsHolder != null)
            {
                clothingStats.Add(statsHolder.stats); // assuming `PrefabStats` has a field `public ClothingStats stats;`
            }
            else
            {
                Debug.LogWarning($"Prefab {prefab.name} has no PrefabStats component!");
                clothingStats.Add(new ClothingStats());
            }
        }

        Debug.Log($"Loaded {clothing.Count} clothing prefabs and {clothingStats.Count} stat entries.");
    }

    public GameObject SpawnCharacter(int character, Outfit outfit, Transform p)
    {
        GameObject skeletonInstance = Instantiate(skeletonPrefab, p);
        Transform skeletonRoot = skeletonInstance.transform.Find("bone_1");
        Transform clothingRoot = skeletonInstance.transform.Find("Clothing");
        if (character != -1)
        {
            GameObject body = SpawnBody(character, skeletonRoot);
        }
        SpawnClothing(outfit, clothingRoot, skeletonRoot);
        return skeletonInstance;
    }

    public GameObject SpawnBody(int character, Transform skeletonRoot)
    {
        GameObject prefab = characters[character];
        if (prefab != null)
        {
            GameObject characterObj = Instantiate(prefab, skeletonRoot.parent);
            characterObj.transform.localPosition = Vector3.zero;
            characterObj.transform.localRotation = Quaternion.identity;
            characterObj.transform.localScale = Vector3.one;
            foreach (Transform child in characterObj.transform)
            {
                SpriteSkin skin = child.GetComponent<SpriteSkin>();
                if (skin != null)
                {
                    skin.SetRootBone(skeletonRoot);
                    skin.enabled = true;
                }
            }
            return characterObj;
        }
        return null;
    }
    public void SpawnClothing(Outfit o, Transform clothingRoot, Transform skeletonRoot)
    {
        foreach (GameObject oldClothing in clothingRoot)
        {
            Destroy(oldClothing);
        }
        for (int i = 0; i < o.outfit.Length; i++)
        {
            int id = o.outfit[i];
            GameObject prefab = clothing[id];
            Color thisColor = Color.white;
            if (o.colors.Length > i)
                thisColor = o.colors[i];
            if (prefab != null)
            {

                // Instantiate clothing and parent to skeleton
                GameObject clothingObj = Instantiate(prefab, clothingRoot);
                clothingObj.transform.localPosition = Vector3.zero;
                clothingObj.transform.localRotation = Quaternion.identity;
                clothingObj.transform.localScale = Vector3.one;
                foreach (Transform child in clothingObj.transform)
                {
                    SpriteSkin skin = child.GetComponent<SpriteSkin>();
                    SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
                    if (skin != null && renderer != null)
                    {
                        renderer.color = thisColor;
                        skin.SetRootBone(skeletonRoot);
                        skin.enabled = true;
                        foreach (var kv in orderMap)
                        {
                            if (child.name.Contains(kv.Key))
                            {
                                renderer.sortingOrder = kv.Value;
                                renderer.sortingLayerName = "Characters";
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning($"No clothing prefab found for ID {id}");
            }
        }
    }

    public ClothingStats GetStats(int[] outfit, ClothingStats baseStats)
    {
        ClothingStats newStats = baseStats;
        newStats.hp = 3; //newStats.armor = 0; newStats.bonus = 0; newStats.damage = 0; newStats.cost = 0;
        foreach (int index in outfit)
        {
            ClothingStats addStats = clothingStats[index];
            newStats.hp += addStats.hp;
            newStats.damage += addStats.damage;
            newStats.armor += addStats.armor;
            newStats.bonus += addStats.bonus;
            newStats.cost += addStats.cost;
            newStats.attackTriggers.AddRange(addStats.attackTriggers);
            newStats.name = addStats.name;
            newStats.description = addStats.description;
        }
        return newStats;
    }

    public List<Spell> GetSpells(Outfit outfit)
    {
        List<Spell> mySpells = new List<Spell>();
        //Loop thru all spells in master list and add all with matching Index to the clothing piece index
        foreach (Spell s in spells)
        {
            if (outfit.outfit.Contains<int>(s.index))
            {
                mySpells.Add(s);
            }
        }

        return mySpells;
    }
}

