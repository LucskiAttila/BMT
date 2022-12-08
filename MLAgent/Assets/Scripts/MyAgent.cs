using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MyAgent : Agent
{
    public GameObject enemy;
    public GameObject[] beers;
    public GameObject[] doors;
    public GameObject girl;
    Rigidbody m_rigidbody;
    EnemyObject enemyScript;
    Animator agentAnimator;
    float speed = 7.5f;
    bool isAttack = false;
    bool isOnGirl = false;
    float girlPosition = 1.97f;
    const float doorSize = 4.475f;
    float maxDistance;
    bool isOnGround = false;
    float rew = 0.0f;

    private Vector3 startingPosition = new Vector3(-39.5f, 0.0f, 0.0f);
    private Quaternion startingAngel = Quaternion.Euler(new Vector3(0, 0, 0));

    void Start()
    {
        girlPosition = doors[0].transform.position.x - girlPosition;
        enemyScript = enemy.GetComponent<EnemyObject>();
        m_rigidbody = GetComponent<Rigidbody>();
        agentAnimator = gameObject.GetComponent<Animator>();
        agentAnimator.Play("Idle_SwordShield");
        
        Random.InitState(0);
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log(rew);
        rew = 0;
        transform.localPosition = startingPosition;
        transform.rotation = startingAngel;
        enemy.SetActive(true);
        enemyScript.enabled = true;
        enemyScript.Initer(transform.position.x + enemyScript.borderDistanceFromZero, transform.position.x - enemyScript.borderDistanceFromZero, Random.Range(0,1));
        foreach (GameObject beer in beers) {
            beer.SetActive(true);
            beer.GetComponent<BeerObject>().enabled = true;
        }
        foreach(GameObject door in doors){
            door.SetActive(true);
        }
        int[] indexes = new int[3]{-1,-1,-1};
        for(int i = 0; i < 3; i++){
            while(true){
                int index = Random.Range(0, doors.Length-1);
                bool isMatch = false;
                for(int j = 0; j < indexes.Length-1; j++){
                    if(index == indexes[j]){
                        isMatch = true;
                        break;
                    }
                }
                if(!isMatch){
                    doors[index].SetActive(false);
                    indexes[i] = index;
                    break;
                }
            }
        }
        int girlIndex = Random.Range(0, indexes.Length-1);
        girl.transform.position = SetZ(girl.transform.position, indexes[girlIndex]);
        girl.SetActive(true);
        maxDistance = calcDistance();
    }

    Vector3 SetZ(Vector3 vector, int index)
    {
        vector.x = girlPosition - index * doorSize;
        return vector;
    }

    float calcDistance(){
        return Vector3.Distance(girl.transform.localPosition, transform.localPosition);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> actions = actionsOut.ContinuousActions;
        ActionSegment<int> actionDiscrete = actionsOut.DiscreteActions;

        actionDiscrete[0] = 0;
        actionDiscrete[1] = 0;
        actionDiscrete[2] = 0;
        //actions[0] = 0;
        actions[0] = 0;
        isAttack = false;
        isOnGirl = false;

        if (Input.GetKey("w"))
            actionDiscrete[2] = -1;
            //actions[0] = 1;

        /*if (Input.GetKey("s"))
            actions[0] = -1;*/

        if (Input.GetKey("d"))
            actions[0] = +0.5f;

        if (Input.GetKey("a"))
            actions[0] = -0.5f;
        
        if (Input.GetKey("space") && !isOnGirl){
            actionDiscrete[0] = 1;
            isAttack = true;
        }

        if (Input.GetKey("left shift") && !isAttack){
            actionDiscrete[1] = 1;
            isOnGirl = true;
        }
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        enemyScript.doUpdate();
        var actionTaken = actions.ContinuousActions;
        var actionTakenDiscrete = actions.DiscreteActions;

        //float actionSpeed = actionTaken[0];
        float actionSteering = actionTaken[0];
        int actionHit = actionTakenDiscrete[0];
        int actinoGetHit = actionTakenDiscrete[1];
        int isBackward = actionTakenDiscrete[2];

        if(actinoGetHit == 1){
            agentAnimator.Play("GetHit_SwordShield");
        }
        else{
            //if(actionSpeed != 0){
                if(actionHit == 1){
                    agentAnimator.Play("NormalAttack02_SwordShield");
                }
                else{
                    agentAnimator.Play("Run_SwordShield");
                }
            /*}
            else{
                if(actionHit == 1){
                    agentAnimator.Play("NormalAttack01_SwordShield");
                }
               else{
                    agentAnimator.Play("Idle_SwordShield");
                }
            }*/
        }
        if(isBackward == -1){
            transform.Translate(isBackward * Vector3.forward * speed * Time.fixedDeltaTime);
        }
        else{
            transform.Translate(/*actionSpeed * */Vector3.forward * speed * Time.fixedDeltaTime);
        }
        transform.rotation = Quaternion.Euler(new Vector3(0, actionSteering * 180, 0));

        float distanceScaled = calcDistance() / maxDistance;
        rew = rew - distanceScaled/1000;
        AddReward(-distanceScaled / 1000);
        /*if(transform.position.z >= beers[0].transform.position.z)
            AddReward(+1);
        if(transform.position.z >= beers[1].transform.position.z)
            AddReward(+2);
        if(transform.position.z >= beers[4].transform.position.z)
            AddReward(+3);
        if(transform.position.z >= girl.transform.position.z)
            AddReward(+5);*/
    }

    private void OnTriggerEnter(Collider other)
    {
    }

    IEnumerator Die()
    {
        float animationLength = agentAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSecondsRealtime(animationLength);
    }

    public void OnCollisionEnter(Collision collision){
        GameObject collided = collision.gameObject;
        System.String tagCollision = collided.GetComponent<Collider>().tag;
         if(tagCollision == "Enemy"){
            if(!isAttack){
                agentAnimator.Play("Die_SwordShield");
                Die();
                rew = rew - 1;
                AddReward(-1);
                EndEpisode();
            }
            else{
                //AddReward(+1);
                if(collided == enemy){
                    collided.GetComponent<Animation>().Play("Death");
                    collided.GetComponent<EnemyObject>().enabled = false;
                }
                else{
                    collided.GetComponent<Animator>().Play("Death");
                    collided.GetComponent<BeerObject>().enabled = false;
                }
                collided.SetActive(false);
            }
        }
        else if(tagCollision == "Floor"){
            if(isOnGround){
                //float distanceScaled = calcDistance() / maxDistance;
                //AddReward(-distanceScaled / 10);
            }
            isOnGround = true;
        }
        else if(tagCollision == "Wall"){
            //float distanceScaled = calcDistance() / maxDistance;
            rew = rew - 1;
            AddReward(-1);
            EndEpisode();
        }
        else if(tagCollision == "Girl"){
            if(isAttack){
                rew = rew + 1;
                AddReward(+1);
                EndEpisode();
            }
            else if(!isOnGirl){
                rew = rew + 1;
                AddReward(+1);
                EndEpisode();
            }
            
            else{
                rew = rew + 1;
                AddReward(+1);
                EndEpisode();
            }
        }
    }
}
