using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

[RequireComponent(typeof(CharacterController))]

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform camProxy;
    [SerializeField] private float gravity;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpHeight;

    private float gravityAcceleration;
    private float moveSpeed;
    private float jumspeed;

    private bool[] inputs;
    private float yVelocity;
    private bool didTeleport;

    private void OnValidate() {
        if(controller==null){
            controller=GetComponent<CharacterController>();
        }
        if(player==null){
            player=GetComponent<Player>();
        }

        Initialize();
    }

    private void Start(){
        Initialize();
        inputs=new bool[6];
    }

    private void FixedUpdate(){
        Vector2 inputDirection = Vector2.zero;
        if(inputs[0]){
            inputDirection.y+=1;
        }
        if(inputs[1]){
            inputDirection.y-=1;
        }
        if(inputs[2]){
            inputDirection.x-=1;
        }
        if(inputs[3]){
            inputDirection.x+=1;
        }
        Move(inputDirection, inputs[4],inputs[5]);
    }

    private void Initialize(){
        gravityAcceleration = gravity*Time.fixedDeltaTime;
        moveSpeed = movementSpeed*Time.fixedDeltaTime;
        jumspeed = Mathf.Sqrt(jumpHeight*-2f*gravityAcceleration);
    }

    private void Move(Vector2 inputDirection, bool jump, bool sprint){
        Vector3 moveDirection = Vector3.Normalize(camProxy.right*inputDirection.x+Vector3.Normalize(FlattenVector3(camProxy.forward))*inputDirection.y);
        moveDirection *= moveSpeed;
        if(sprint){
            moveDirection*=2f;
        }
        if(controller.isGrounded){
            yVelocity=0f;
            if(jump){
                yVelocity=jumspeed;
            }
        }
        yVelocity+=gravityAcceleration;

        moveDirection.y=yVelocity;
        controller.Move(moveDirection);

        SendMovement();
    }
    public void Teleport(Vector3 newPosition)
    {
        didTeleport = true;
        controller.enabled = false;
        gameObject.transform.position = newPosition;
        controller.enabled = true;
    }

    private Vector3 FlattenVector3(Vector3 vector){
        vector.y=0;
        return vector;
    }

    public void SetInput(bool[] inputs, Vector3 forward){
        this.inputs=inputs;
        camProxy.forward = forward;
        if (!GameLogic.Singleton.gameStarted)
        {
            for(int i=0; i<inputs.Length;i++)
            {
                inputs[i] = false;
            }
        }
    }

    private void SendMovement(){
        if(NetworkManager.Singleton.CurrentTick%2 != 0){
            return;
        }
        Message message = Message.Create(MessageSendMode.unreliable, (ushort)ServerToClientId.playerMovement);
        message.AddUShort(player.Id);
        message.AddUShort(NetworkManager.Singleton.CurrentTick);
        message.AddBool(didTeleport);
        message.AddVector3(transform.position);
        message.AddVector3(camProxy.forward);
        NetworkManager.Singleton.Server.SendToAll(message);

        didTeleport=false;
    }
}
