﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Boundary
{
    public float xMin, xMax, zMin, zMax;
}

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerBehavior : MonoBehaviour
{
    bool isDragged;
    Vector3 screenPoint;
    Vector3 offset;

    //Bullet 
    public Transform[] bulletSpawns;
    public int bulletLevel = 1;
    int bulletDamage = -1; //Damage vals are negative (amt of health subtracted)
    float nextFireBullet;
    public float fireRate = 0.5f;
    public GameObject bullet;

    //Health
    int health = 1;

    //Effects
    public GameObject fx_Explosion;

    //Reset player on 'death'
    Vector3 initPosition;
    bool isDead;

    //Change to keyboard control scheme - new movement method (includes bounded to screen)
    private Rigidbody rb;
    public float speed;
    public Boundary boundary;
    public float tilt;

    // Start is called before the first frame update
    void Start()
    {
        initPosition = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if ((Input.GetButton("Fire1") || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Return)) && Time.time > nextFireBullet && !isDead)
        {
            nextFireBullet = Time.time + fireRate;
            for (int i = 0; i < bulletLevel; i++)
            {
                GameObject newBullet = Instantiate(bullet, bulletSpawns[i].position, bulletSpawns[i].rotation) as GameObject;
                //Deal damage with bullet
                newBullet.GetComponent<Bullet>().SetDamage(bulletDamage);
            }
        }
    }

    /*void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,screenPoint.z));
    }*/

    /*void OnMouseDrag() //Drag ship around - replace with keys
    {
        if (!isDead)
        {
            Vector3 currentScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
            Vector3 currentPosition = Camera.main.ScreenToWorldPoint(currentScreenPoint) + offset;

            isDragged = true;
            transform.position = currentPosition;
        }
    }

    void OnMouseUp()
    {
        isDragged = false;
    }*/

    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        rb.velocity = movement * speed;

        rb.position = new Vector3
        (
             Mathf.Clamp(rb.position.x, boundary.xMin, boundary.xMax),
             0.0f,
             Mathf.Clamp(rb.position.z, boundary.zMin, boundary.zMax)
        );

        rb.rotation = Quaternion.Euler(0.0f, 0.0f, rb.velocity.x * -tilt);
    }

    public void TakeDamage(int amount)
    {
        health += amount;

        if (health <= 0)
        {
            //Play sound

            //Play particle effect
            if (fx_Explosion != null)
            {
                Instantiate(fx_Explosion, transform.position, Quaternion.identity); //Replace with our own explosion effect?
            }

            //Destroy or Disable Player
            StartCoroutine(Reset());

        }
    }

    IEnumerator Reset() //Remove ship from board, and 'respawn'
    {
        GameManager.instance.DecreaseLives();
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        isDead = true;

        transform.position = initPosition;

        yield return new WaitForSeconds(3f); //wait x seconds

        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
        isDead = false;
    }

    void OnTriggerEnter(Collider col) //Deal damage to player on contact with enemy
    {
        if (col.CompareTag("Enemy"))
        {
            TakeDamage(-10); //Remember damage is negative
        }
    }
}
