﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : Entity {


    private Point m_source, m_destination;
    private Entity m_attackTarget;
    
    public float velocity = 100; // pixels per second

    //private GameObject m_range;
    public float range = 10;

    private Munition m_munition;
    private LineRenderer m_lineRenderer;


    private Timer m_attackTimer;

    public Vector2 position
    {
        get { return transform.position; }
    }

    //Use this for initialization
    void Start()
    {
        InitBody();
        InitRange();

        //////This is for testing
        ////    gameObject.GetComponent<Rigidbody2D>().velocity = (new Vector2(1,0)) * velocity;
        ///////////////////////////

        m_munition = gameObject.GetComponent<Munition>();
        m_lineRenderer = gameObject.AddComponent<LineRenderer>();
        
        m_attackTimer = new Timer();
        m_attackTimer.time = m_munition.attack_time + Random.Range(0, m_munition.attack_time_variance);
    }

    void InitBody(){
        Rigidbody2D rigidBody2D = gameObject.AddComponent<Rigidbody2D>();
        rigidBody2D.isKinematic = false;
        rigidBody2D.fixedAngle = true;
        rigidBody2D.gravityScale = 0f;


        BoxCollider2D colliderTemp = gameObject.AddComponent<BoxCollider2D>();
        colliderTemp.size = gameObject.GetComponent<tk2dSprite>().GetBounds().size;
        colliderTemp.isTrigger = true;
    }

    void InitRange(){
        //m_range = new GameObject();
        //m_range.name = "UnitRange";
        //m_range.transform.position = transform.position;
        //m_range.transform.parent = transform;
        CircleCollider2D colliderTemp = gameObject.AddComponent<CircleCollider2D>();
        colliderTemp.radius = range;
        colliderTemp.isTrigger = true;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (m_attackTarget)
        { 
            m_attackTimer.elapsed += Time.deltaTime;
            while (m_attackTimer.HasElapsed())
            {
                Attack(m_attackTarget);

                m_attackTimer.SetBack();
                m_attackTimer.time = m_munition.attack_time + Random.Range(0, m_munition.attack_time_variance);
            }
        }
	}

    void Target(Point target)
    {
        // at some point assign the target point and make it point in that direction
        m_destination = target;

        Vector2 direction = m_destination.position - m_source.position;
        gameObject.GetComponent<Rigidbody2D>().velocity = direction.normalized * velocity;
    }

    //TODO this needs a better name
    void Source(Point source)
    {
        m_source = source;
    }

    //bool canAttack(Unit target){
    //    if(this.type == UnitType.fighter){
    //        if(target.type != 
    //        }
    //}

    //overidable method used to attack an object
    void Attack(Entity target)
    {
        
        int damage = m_munition.damage;

        Laser laser = new Laser(position, target.position, m_munition.colour);
        StartCoroutine(laser.Fade());

        float hitChance = Random.Range(0, 1.0f);
        if (hitChance <= m_munition.accuracy)
        {
            target.TakeDamage(damage);
        }
    }

    //OnTriggerEnter is called when the Collider other enters the trigger.
    void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log(gameObject.name + " triggered by " + other.GetType());
        if (other is BoxCollider2D)
        {
            m_attackTarget = other.gameObject.GetComponent<Unit>();
        }
    }

    //OnTriggerEnter is called when the Collider other enters the trigger.
    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log(other.GetType() + " left " + gameObject.name);
        m_attackTarget = null;
    }   
}
