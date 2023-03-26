using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Startup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InstantiatePrefabs()
    {
        Debug.Log("-- Instantiating objects --");

        GameObject[] prefabsToInstantiate = Resources.LoadAll<GameObject>("InstantiateOnLoad/");

        foreach (GameObject pref in prefabsToInstantiate)
        {
            Debug.Log($"Creating {pref.name}");

            Object.Instantiate(pref);
        }

        Debug.Log("-- Instantiating objects done --");
    }

}
