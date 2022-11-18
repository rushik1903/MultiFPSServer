using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;
using TMPro;

public class GameLogic : MonoBehaviour
{
    private static GameLogic _singleton;
    public TMP_Text gameStatusText;
    private RemoveSpawnBarriers spawnBarrierScript;

    public float roundTime = 90, roundCurrentTime = 0, roundBreakTime = 15;
    private bool roundBreakOver = false;
    public bool gameStarted = false, bulletDamage = false;

    public static GameLogic Singleton{
        get=>_singleton;
        private set{
            if(_singleton==null){
                _singleton=value;
            }
            else if(_singleton!=value){
                Debug.Log($"{nameof(GameLogic)} instance already exists, destroying duplicate");
                Destroy(value);
            }
        }
    }

    private void Start()
    {
        spawnBarrierScript = gameObject.GetComponent<RemoveSpawnBarriers>();
    }

    private void FixedUpdate()
    {
        if (!gameStarted) { return; }
        gameStatusText.text = roundCurrentTime.ToString();
        //respawning all player every 90 secs (1 round = 90secs)
        roundCurrentTime += Time.deltaTime;
        if (roundCurrentTime >= roundBreakTime && !roundBreakOver)
        {
            Debug.Log("startRound"); //players start fight
            Player.StartRound();
            spawnBarrierScript.DestroySpawnBarriers();
            roundBreakOver = true;
            bulletDamage = true;
        }
        if(roundCurrentTime>=roundBreakTime+ roundTime)
        {
            Debug.Log("newRound");  //players respawn and wait in spawn
            Player.NewRound();
            spawnBarrierScript.RebuildSpawnBarriers();
            bulletDamage = false;
            roundCurrentTime = 0;
            roundBreakOver = false;
        }
    }

    public void PauseAndPlayGame()
    {
        gameStarted = !gameStarted;
    }

    public void SetTimeForNewRound()
    {
        roundCurrentTime = roundBreakTime + roundTime - 4;
    }

    public GameObject PlayerPrefab=>playerPrefab;
    public GameObject Bullet => bullet;

    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject bullet;

    private void Awake(){
        Singleton=this;
    }
}
