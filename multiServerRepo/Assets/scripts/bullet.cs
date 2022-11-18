using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    public float lifeTime=2f;
    public int teamNumber,gunType;
    public Vector3 velocity;
    private int bodyDamage, headDamange;
    private void Start()
    {
        if (gunType == 0)
        {
            bodyDamage = 20;
            headDamange = 100;
        }else if (gunType == 1)
        {
            bodyDamage = 33;
            headDamange = 100;
        }
        else
        {
            Debug.Log("gunTypeGone");
            bodyDamage = 20;
            headDamange = 100;
        }
        Invoke("KillBullet", lifeTime);
    }
    
    public void Velocity(Vector3 Velocity)
    {
        velocity = Velocity;
        gameObject.GetComponent<Rigidbody>().velocity = velocity;
    }
    private void OnCollisionEnter(Collision collisionInfo)
    {
        if (!GameLogic.Singleton.bulletDamage)
        {
            KillBullet();
            return;
        }
        if (collisionInfo.gameObject.GetComponent<Player>() != null)
        {
            Player damagedPlayer= collisionInfo.gameObject.GetComponent<Player>();
            Debug.Log("hit");
            if (damagedPlayer.teamNumber != teamNumber)
            {
                damagedPlayer.ReduceHealth(bodyDamage, velocity);
            }
        }
        else if(collisionInfo.gameObject.name == "head")
        {
            Player damagedPlayer = collisionInfo.gameObject.GetComponentInParent<Player>();
            Debug.Log("head");
            if (damagedPlayer.teamNumber != teamNumber)
            {
                damagedPlayer.ReduceHealth(headDamange, velocity);
            }
        }
        KillBullet();
    }

    private void KillBullet()
    {
        Destroy(gameObject);
    }
}
