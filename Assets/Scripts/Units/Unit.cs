﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : Entity {

    public AudioClip m_shootSound;

    private Point m_source, m_destination;
    private bool m_reachedDestination = false;

    private Entity m_attackTarget;
    
    public float velocity = 100; // pixels per second

    //private GameObject m_range;
    public float range = 10;

    private Munition m_munition;
    private LineRenderer m_lineRenderer;

    private Timer m_attackTimer;

    public Point destination
    {
        get { return m_destination; }
    }
    public Point source
    {
        get { return m_source; }
    }

    void Awake() {
        InitBody();
        InitRange();
    }

    //Use this for initialization
    new void Start()
    {
        base.Start();

        if(null != m_source && null != m_destination) {
            SetSourceAndTarget(m_source, m_destination);
        }

        m_munition = gameObject.GetComponent<Munition>();
        m_lineRenderer = gameObject.AddComponent<LineRenderer>();
        
        m_attackTimer = new Timer();
        m_attackTimer.time = m_munition.attack_time + Random.Range(0, m_munition.attack_time_variance);
    }

    void InitBody(){
        Rigidbody2D rigidBody2D = gameObject.AddComponent<Rigidbody2D>();
        rigidBody2D.isKinematic = false;
        rigidBody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
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
    new void Update ()
    {
        base.Update();
        if (m_attackTarget)
        { 
            
            m_attackTimer.elapsed += Time.deltaTime;
            while (m_attackTimer.HasElapsed())
            {
                Attack(m_attackTarget);
                m_attackTimer.SetBack();
                //m_attackTimer.time = m_munition.attack_time + Random.Range(0, m_munition.attack_time_variance);
            }
        }
    }

    public void SetSourceAndTarget(Point source, Point target)
    {
        // Move the unit to the source
        m_source = source;
        this.transform.position = source.position;

        // at some point assign the target point and make it point in that direction
        m_destination = target;

        Vector2 direction = m_destination.position - m_source.position;
        gameObject.GetComponent<Rigidbody2D>().velocity = direction.normalized * velocity;
        transform.LookAt(destination.gameObject.transform, transform.up);
        transform.Rotate(0 , -90, 0);
        transform.position = transform.position + new Vector3(0 , 0, -2);
    }

    bool CanAttack(Entity target){
        if(target.m_owner == this.m_owner || target.dead){
            return false;
        }

        switch (this.type)
	    {
            case Type.bomber:
                return target.type == Type.point;
            case Type.fighter:
                return true;
            case Type.icbm:
                return target.gameObject.name == destination.gameObject.name;
            case Type.point:
                return false;
            default:
                Debug.LogError("We are attacking with an unchecked type.");
                return false;
	    }
    }

    //overidable method used to attack an object
    void Attack(Entity target)
    {
        int damage = m_munition.damage;

        Laser laser = new Laser(position, target.position, m_munition.colour);
        StartCoroutine(laser.Fade());

        AudioSource.PlayClipAtPoint(m_shootSound,transform.position, Random.Range(0.01f,0.3f));

        float hitChance = Random.Range(0, 1.0f);
        if (hitChance <= m_munition.accuracy) {
            target.TakeDamage(damage);
        }
        if(type == Type.icbm){
            ReachedDestination();
        }
    }

    public new void TakeDamage(int damage){
        base.TakeDamage(damage);
    }

    public void ReachedDestination(){
        if(type == Type.icbm){
            Explode();      
        }
        else{
            gameObject.GetComponent<Rigidbody2D>().velocity *= -1;
            transform.LookAt(source.gameObject.transform, transform.up);
            transform.Rotate(0 , -90, 0);
            transform.position = transform.position + new Vector3(0 , 0, -2);
            m_reachedDestination = true;
        }
    }

    public void FinishMission(){
        if(m_reachedDestination){
            source.UnitReturned(m_money);
            Destroy(gameObject);
        }
    }

    //OnTriggerEnter is called when the Collider other enters the trigger.
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other is BoxCollider2D)
        {
            Entity possibleTarget = other.gameObject.GetComponent<Entity>();
            if(possibleTarget == null) Debug.LogError("No Entity on other.gameobject:: " + other.gameObject);
            else{
                if(CanAttack (possibleTarget)){
                m_attackTarget = possibleTarget;

                m_attackTimer.elapsed = m_attackTimer.time;
                }
            }
        }
    }

    //OnTriggerEnter is called when the Collider other enters the trigger.
    void OnTriggerExit2D(Collider2D other)
    {
        m_attackTarget = null;
    }   
}
