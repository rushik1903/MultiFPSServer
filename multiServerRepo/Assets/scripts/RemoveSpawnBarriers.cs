using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveSpawnBarriers : MonoBehaviour
{
    private static RemoveSpawnBarriers _singleton;
    public GameObject[] spawnBarriers;

    public static RemoveSpawnBarriers Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
            {
                _singleton = value;
            }
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(RemoveSpawnBarriers)} instance already exists, destroying duplicate");
                Destroy(value);
            }
        }
    }

    public void DestroySpawnBarriers()
    {
        foreach(GameObject wall in spawnBarriers)
        {
            Debug.Log("D");
            wall.SetActive(false);
        }
    }
    public void RebuildSpawnBarriers()
    {
        foreach (GameObject wall in spawnBarriers)
        {
            Debug.Log("R");
            wall.SetActive(true);
        }
    }

}
