using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using TMPro;

public class Player : MonoBehaviour
{
    private static TMP_InputField killByName;
    private static Player useless;
    public int teamNumber;
    private static int team0Score = 0, team1Score = 0;

    public static Dictionary<ushort, Player> list =new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public string Username {get; private set; }

    public int health = 100;
    public float time = 0;

    public PlayerMovement Movement => movement;

    [SerializeField] private PlayerMovement movement;

    private void OnDestoy(){ //never really called
        list.Remove(Id);
    }

    public static void NewRound()//all players are spawned and wait for round to start
    {
        foreach (Player otherPlayer in list.Values)
        {
            otherPlayer.Respawn();
        }
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.newRound);
        message.AddInt(0);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    public static void StartRound()//players can start killing round starts
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.newRound);
        message.AddInt(1);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    private void Start()
    {

    }
    private void Update()
    {
        
    }

    public void AfterAnyKill()
    {
        bool team0 = false, team1 = false;
        foreach (Player player in list.Values)
        {
            if (player.teamNumber == 0 && player.gameObject.activeInHierarchy)
            {
                team0 = true;
            }
            else if (player.teamNumber == 1 && player.gameObject.activeInHierarchy)
            {
                team1 = true;
            }
        }
        if (!team0 || !team1)
        {
            GameLogic.Singleton.SetTimeForNewRound();
            if (!team0)
            {
                Player.team0Score++;
                Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.teamScore);
                message.AddInt(team0Score);
                message.AddInt(team1Score);
                NetworkManager.Singleton.Server.SendToAll(message);
            }
            if (!team1)
            {
                Player.team1Score++;
                Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.teamScore);
                message.AddInt(team0Score);
                message.AddInt(team1Score);
                NetworkManager.Singleton.Server.SendToAll(message);
            }
        }
    }

    public void ReduceHealth(int hp,Vector3 bulletVelocity)
    {
        health -= hp;
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.playerHP);
        message.AddInt(health);
        NetworkManager.Singleton.Server.Send(message, Id);
        Debug.Log(health);
        if (health <= 0 || health >100)
        {
            health = 100;
            Kill(bulletVelocity);
            gameObject.SetActive(false);
        }
    }

    private void Kill(Vector3 bulletVelocity)
    {
        gameObject.SetActive(false);
        AfterAnyKill();

        //sending death message to all clients
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.playerDied);
        message.AddUShort(Id);
        //message.AddVector3(bulletVelocity);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    private void Respawn()
    {
        ReduceHealth(0,new Vector3(0,0,0));
        gameObject.SetActive(true);
        if (teamNumber % 2 == 0)
        {
            GameObject spawnPointParent = GameObject.FindGameObjectWithTag("spawnPoints");
            SpawnPoints spawnPoints = spawnPointParent.GetComponent<SpawnPoints>();
            Vector3 spawnPoint = spawnPoints.GetSpawnPoint();
            gameObject.GetComponent<PlayerMovement>().Teleport(spawnPoint);
        }
        else
        {
            GameObject spawnPointParent = GameObject.FindGameObjectWithTag("spawnPointsOpponents");
            SpawnPoints spawnPoints = spawnPointParent.GetComponent<SpawnPoints>();
            Vector3 spawnPoint = spawnPoints.GetSpawnPoint();
            gameObject.GetComponent<PlayerMovement>().Teleport(spawnPoint);
        }
    }

    public static void PrintAllPlayers(){
        string s="";
        foreach(Player otherPlayer in list.Values){
            s+=":"+otherPlayer.Id.ToString();
        }
        Debug.Log(s);
    }
    public static void Spawn(ushort id, string username, int teamnumber){
        bool respawn=false;
        ushort removeLast=0;
        Player temp = useless;
        foreach (Player otherPlayer in list.Values){        //sending newly connected client all previous clients positions
            if(otherPlayer.Username != username){
                otherPlayer.SendSpawned(id);
            }else{
                respawn = true;
                //sending other clients to change Id of reconnecting client to new id;
                otherPlayer.SendReconnect(otherPlayer.Id, id);

                //reconnecting client(removing <oldId,player>, adding <newId,player> in list)
                temp = list[otherPlayer.Id];
                removeLast = otherPlayer.Id;

                otherPlayer.Id=id;
                otherPlayer.SendSpawned(id);//sending the reconnecting client its own location
            }
        }
        ///need to change
        if (respawn) {
            list.Remove(removeLast);
            removeLast = 0;
            list[id] = temp;
            
            return;
        }
        if (teamnumber % 2 == 0) //team1
        {
            GameObject spawnPointParent = GameObject.FindGameObjectWithTag("spawnPoints");
            SpawnPoints spawnPoints = spawnPointParent.GetComponent<SpawnPoints>();
            Vector3 spawnpoint = spawnPoints.GetSpawnPoint();
            Player player = Instantiate(GameLogic.Singleton.PlayerPrefab, spawnpoint, Quaternion.identity).GetComponent<Player>();
            player.name = $"Player {id} {(string.IsNullOrEmpty(username) ? "Guest" : username)}";
            player.Id = id;
            player.teamNumber = teamnumber;
            player.Username = string.IsNullOrEmpty(username) ? "Guest {id}" : username;

            player.SendSpawned();
            list.Add(id, player);
        }
        else  //team 2
        {
            GameObject spawnPointParent = GameObject.FindGameObjectWithTag("spawnPointsOpponents");
            SpawnPoints spawnPoints = spawnPointParent.GetComponent<SpawnPoints>();
            Vector3 spawnpoint = spawnPoints.GetSpawnPoint();
            Player player = Instantiate(GameLogic.Singleton.PlayerPrefab, spawnpoint, Quaternion.identity).GetComponent<Player>();
            player.name = $"Player {id} {(string.IsNullOrEmpty(username) ? "Guest" : username)}";
            player.Id = id;
            player.teamNumber = teamnumber;
            player.Username = string.IsNullOrEmpty(username) ? "Guest {id}" : username;

            player.SendSpawned();
            list.Add(id, player);
        }
    }

    #region Messages
    private void SendSpawned(){
        NetworkManager.Singleton.Server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.playerSpawned)));
    }

    private void SendSpawned(ushort toClientId){
        NetworkManager.Singleton.Server.Send(AddSpawnData(Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.playerSpawned)), toClientId);
    }

    private void SendReconnect(ushort oldId, ushort newId)
    {
        Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.playerReconnect);
        message.AddUShort(oldId);
        message.AddUShort(newId);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    private Message AddSpawnData(Message message){
        message.AddUShort(Id);
        message.AddInt(teamNumber);
        message.AddString(Username);
        message.AddVector3(transform.position);
        return message;
    }

    [MessageHandler((ushort)ClientToServerId.name)]

    private static void Name(ushort fromClientId, Message message){
        Spawn(fromClientId, message.GetString(), message.GetInt());
    }

    [MessageHandler((ushort)ClientToServerId.input)]
    private static void Input(ushort fromClientId, Message message){
        if(list.TryGetValue(fromClientId, out Player player)){
            player.Movement.SetInput(message.GetBools(6), message.GetVector3());
        }
    }

    [MessageHandler((ushort)ClientToServerId.playerShoot)]

    private static void Shoot(ushort fromClientId, Message message)
    {
        int teamnumber = 0;
        if (list.TryGetValue(fromClientId, out Player player))
        {
            teamnumber = player.teamNumber;
        }
        Vector3 bulletPosition = message.GetVector3();
        Vector3 bulletVelocity = message.GetVector3();
        int gunType = message.GetInt();
        GameObject bullet = Instantiate(GameLogic.Singleton.Bullet, bulletPosition, Quaternion.identity);
        bullet.GetComponent<bullet>().teamNumber = teamnumber;
        bullet.GetComponent<bullet>().gunType = gunType;
        bullet.GetComponent<bullet>().Velocity(bulletVelocity);
        ShootToOtherClient(bulletPosition,bulletVelocity,fromClientId);
    }

    private static void ShootToOtherClient(Vector3 bulletPosition, Vector3 bulletVelocity, ushort excludeId)
    {
        Message message = Message.Create(MessageSendMode.unreliable, (ushort)ServerToClientId.bullets);
        message.AddVector3(bulletPosition);
        message.AddVector3(bulletVelocity);
        NetworkManager.Singleton.Server.SendToAll(message, excludeId);
    }
    #endregion
}
