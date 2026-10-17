using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RuntimeInitializer : MonoBehaviour
{
    [SerializeField] private GameObject[] persistentSystemsPrefabs;

    private static bool initialized = false;

    private void Awake()
    {

        if (initialized)
            return;


        foreach (var prefab in persistentSystemsPrefabs)
        {
            Instantiate(prefab);
        }


        initialized = true;
    }

    [Button] 
    private void ResetInitBool()
    {
        initialized = false;
    }
}