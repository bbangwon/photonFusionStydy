using UnityEngine;

public static class Utils
{
    public static Vector3 GetRandomSpawnPoint()
    {
        return new Vector3(Random.Range(-20, 20), 4, Random.Range(-20, 20));
    }

    public static void SetRenderLayerInChildren(Transform transform, int layer)
    {
        foreach(Transform trans in transform.GetComponentsInChildren<Transform>(true))
        {
            if (trans.CompareTag("IgnoreLayerChange"))
                continue;

            trans.gameObject.layer = layer;
        }
    }
}
