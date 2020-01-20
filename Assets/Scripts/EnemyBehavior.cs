using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    public Path pathToFollow;
    //Path info
    public int currentWayPointID = 0;
    public float speed = 2;
    public float reachDistance = 0.4f;
    public float rotationSpeed = 5f;

    float distance; //Distance to next waypoint
    public bool useBezier;

    //State Machine
    public enum EnemyStates
    {
        ON_PATH, //On bezier path
        FLY_IN, //Moving into formation
        IDLE,
        DIVE //Moving to attack
    }
    public EnemyStates enemyState;

    public int enemyID;
    public Formation formation;

    //Health
    public int health = 2;

    //Effects
    public GameObject fx_Explosion;

    //Enemy Shots
    public GameObject bullet;
    float cur_delay;
    float fireRate = 2f;
    Transform target; //Player
    public Transform spawnPoint;
    public int bulletDamage = -1; //All damage vals should be negative

    //Score
    public int inFormationScore;
    public int notInFormationScore;

    void Start()
    {
        target = GameObject.Find("PlayerShip").transform;
    }

    
    void Update()
    {
        switch (enemyState)
        {
            case EnemyStates.ON_PATH:
                {
                    MoveOnThePath(pathToFollow);
                }
                break;
            case EnemyStates.FLY_IN:
                {
                    MoveToFormation();
                }
                break;
            case EnemyStates.IDLE:
                break;
            case EnemyStates.DIVE:
                MoveOnThePath(pathToFollow);
                //Shooting while diving
                SpawnBullet(); //Shoots towards the player once the shot delay has finished
                break;
        }

    }

    void MoveToFormation()
    {
        transform.position = Vector3.MoveTowards(transform.position, formation.GetVector(enemyID), speed * Time.deltaTime);
        //Rotation
        var direction = formation.GetVector(enemyID) - transform.position;
        if (direction != Vector3.zero)
        {
            direction.y = 0;
            direction = direction.normalized;
            var rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
        }
        //
        if (Vector3.Distance(transform.position, formation.GetVector(enemyID)) <= 0.0001f)
        {
            transform.SetParent(formation.gameObject.transform);
            transform.eulerAngles = new Vector3(0,0,0);

            formation.enemyList.Add(new Formation.EnemyFormation(enemyID,transform.localPosition.x,transform.localPosition.z,this.gameObject));

            enemyState = EnemyStates.IDLE;
        }
    }

    void MoveOnThePath(Path path)
    {
        if (useBezier)
        {
            //Enemy Movement
            distance = Vector3.Distance(path.bezierObjList[currentWayPointID], transform.position);
            transform.position = Vector3.MoveTowards(transform.position, path.bezierObjList[currentWayPointID], speed * Time.deltaTime);
            //Enemy Rotation
            var direction = path.bezierObjList[currentWayPointID] - transform.position;
            if(direction != Vector3.zero)
            {
                direction.y = 0;
                direction = direction.normalized;
                var rotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            //Enemy Movement
            distance = Vector3.Distance(path.pathObjList[currentWayPointID].position, transform.position);
            transform.position = Vector3.MoveTowards(transform.position, path.pathObjList[currentWayPointID].position, speed * Time.deltaTime);
            //Enemy Rotation
            var direction = path.pathObjList[currentWayPointID].position - transform.position;
            if (direction != Vector3.zero)
            {
                direction.y = 0;
                direction = direction.normalized;
                var rotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
            }
        }

        if (useBezier)
        {
            if (distance <= reachDistance)
            {
                currentWayPointID++;
            }
            if (currentWayPointID >= path.bezierObjList.Count)
            {
                currentWayPointID = 0;

                if (enemyState == EnemyStates.DIVE)
                {
                    transform.position = GameObject.Find("SpawnManager").transform.position;
                    Destroy(pathToFollow.gameObject); 
                }

                
                enemyState = EnemyStates.FLY_IN;
            }
        }
        else
        {
            if (distance <= reachDistance)
            {
                currentWayPointID++;
            }
            if (currentWayPointID >= path.pathObjList.Count)
            {
                currentWayPointID = 0;

                if (enemyState == EnemyStates.DIVE)
                {
                    transform.position = GameObject.Find("SpawnManager").transform.position;
                    Destroy(pathToFollow.gameObject);
                }

                enemyState = EnemyStates.FLY_IN;
            }
        }
    }

    public void SpawnSetup(Path path, int ID, Formation form)
    {
        pathToFollow = path;
        enemyID = ID;
        formation = form;
    }

    public void DiveSetup(Path path)
    {
        pathToFollow = path;
        transform.SetParent(transform.parent.parent); //'Un-parent' by setting parent back to the larger scene, from the formation
        enemyState = EnemyStates.DIVE;
    }

    public void TakeDamage(int amount)
    {
        health += amount;
        if (health<=0)
        {
            //Play sound

            //Instantiate particle effect
            if (fx_Explosion != null)
            {
                Instantiate(fx_Explosion, transform.position, Quaternion.identity); //Replace with our own explosion effect?
            }
            //Add score
            if (enemyState == EnemyStates.IDLE)
            {
                GameManager.instance.AddScore(inFormationScore);
            }
            else
            {
                GameManager.instance.AddScore(notInFormationScore);
            }

            //Report destruction to formation (& Cull from parent formation list)
            for (int i = 0; i < formation.enemyList.Count; i++)
            {
                if (formation.enemyList[i].index == enemyID)
                {
                    formation.enemyList.Remove(formation.enemyList[i]);
                }
            }
            //Report destruction to spawn manager
            SpawnManager sp = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
            /*for (int i = 0; i < sp.spawnedEnemies.Count; i++)
                        {
                            sp.spawnedEnemies.Remove(this.gameObject);
                        }*/
            sp.UpdateSpawnedEnemies(this.gameObject);

            //Report destruction to game manager
            //GameManager.instance.ReduceEnemy(); //Replaced

            Destroy(gameObject);
        }
    }

    void SpawnBullet()
    {
        cur_delay += Time.deltaTime;
        if (cur_delay >= fireRate && bullet != null && spawnPoint != null)
        {
            spawnPoint.LookAt(target); //aim at target
            GameObject newBullet = Instantiate(bullet, spawnPoint.position,spawnPoint.rotation) as GameObject;
            newBullet.GetComponent<Bullet>().SetDamage(bulletDamage);
            cur_delay = 0;
        }
    }
}
