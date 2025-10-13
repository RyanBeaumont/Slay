using UnityEngine;
using UnityEngine.SceneManagement;

public static class Bootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureGameController()
    {
        if (ClothingRegistry.Instance == null)
        {
            var prefab = Resources.Load<GameObject>("ClothingRegistry"); // keep in Resources
            Object.Instantiate(prefab);
        }
        if (OverworldController.Instance == null)
        {
            var prefab = Resources.Load<GameObject>("OverworldController"); // keep in Resources
            Object.Instantiate(prefab);
        }
        
    }
}