using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartServerWithBtn : MonoBehaviour
{
    [SerializeField] private GameObject networkManagerObject, canvas;
    public void StartServer()
    {
        networkManagerObject.SetActive(true);
        canvas.SetActive(false);
    }
}
