﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    int damage;
    public float speed = 10f;

    public enum Targets //bullet allegiance (value is what it will harm)
    {
        ENEMY,
        PLAYER
    }

    public Targets target;

    void Start()
    {
        Destroy(gameObject, 1.5f);//Automatically destroys itself after x seconds
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * speed); //Move bullet forward
    }

    public void SetDamage(int amount)
    {
        damage = amount;
    }

    private void OnTriggerEnter(Collider col)
    {
        if(target == Targets.PLAYER)
        {
            if (col.tag == "Player")
            {
                col.gameObject.GetComponent<PlayerBehavior>().TakeDamage(damage);

                Destroy(gameObject);
            }
        }
        
        if(target == Targets.ENEMY)
        {
            if (col.tag == "Enemy")
            {
                col.gameObject.GetComponent<EnemyBehavior>().TakeDamage(damage);

                Destroy(gameObject);
            }
            if (col.tag == "Wall") // Destroy bullets when they hit the wall at the top of the screen
            {
                Destroy(gameObject);
            }
        }
        
    }
}