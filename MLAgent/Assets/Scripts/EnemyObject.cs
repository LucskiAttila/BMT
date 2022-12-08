using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyObject : MonoBehaviour
{
    float speed = 5;
    Animation enemyAnimation;
    Vector3 startingPosition = new Vector3(-20.0f, 0.0f, 0.0f);
    float borderRight;
    float borderLeft;
    public float borderDistanceFromZero = 13.0f;

    public void Initer(float borderRight, float borderLeft, int angel){
        this.borderRight = borderRight;
        this.borderLeft = borderLeft;
        transform.localPosition = startingPosition;
        float angelF = 270.0f;
        if(angel == 0){
            angelF = 90.0f;
        }
        transform.rotation = Quaternion.Euler(new Vector3(0, angelF, 0));;
        enemyAnimation.Play("Run");
    }
    
    void Start()
    {
        enemyAnimation = gameObject.GetComponent<Animation>();
    }

    public void doUpdate()
    {
        if(transform.position.x >= borderRight){
            transform.RotateAround (transform.position, transform.up, 180f);
        }
        else if(transform.position.x <= borderLeft){
            transform.RotateAround (transform.position, transform.up, 180f);
        }
        transform.Translate(Vector3.forward * speed * Time.fixedDeltaTime);
    }
}
