using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoints : MonoBehaviour
{
    public GameObject spawnPointsParent;
    int[] spawnfilled;
    Transform[] spawnChildren;

    private void Start()
    {
        spawnChildren = spawnPointsParent.GetComponentsInChildren<Transform>();
        spawnfilled = new int[spawnChildren.Length];
        for (int i = 0; i < spawnfilled.Length; i++)
        {
            spawnfilled[i] = 0;
        }

    }

    public Vector3 GetSpawnPoint()
    {
        int num = 1000;
        int index = 0;
        for (int i = 0; i < spawnfilled.Length; i++)
        {
            if (num > spawnfilled[i])
            {
                num = spawnfilled[i];
                index = i;
            }
        }
        spawnfilled[index]++;
        return spawnChildren[index].position;

    }
}
