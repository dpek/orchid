﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class UIFab : PointUI {
	
	public UnitFactory m_unitFactory;
	public AudioClip m_hoverSound;
	public AudioClip m_clickSound;
	public AudioClip m_cancelSound;
	public AudioClip m_buySound;
	public AudioClip m_launchSound;
	
	private string m_theme = "placeholder-";
	
	private GameObject m_fighterButton;
	private GameObject m_bomberButton;
	private GameObject m_icbmButton;
	
	private Queue<GameObject> m_spawnQueue = new Queue<GameObject>();
	private List<Entity.Type> m_spawnList = new List<Entity.Type>();
	
    private MoneyEffect m_moneyEffect;

	private PointFab m_point;

    private UnitQueuUI m_unitQueueUI;
	
	void Awake() {
		int m_xMargin = 15;
		int m_yMargin = 15;
		m_fighterButton = CreateButton("button-fighter","fighter",Entity.Type.fighter,new Vector2(-m_xMargin,m_yMargin));
		m_bomberButton = CreateButton("button-bomber","bomber",Entity.Type.bomber,new Vector2(0,m_yMargin));
		m_icbmButton = CreateButton("button-icbm","icbm",Entity.Type.icbm,new Vector2(m_xMargin,m_yMargin));
		m_children.Add(m_fighterButton);
		m_children.Add(m_bomberButton);
		m_children.Add(m_icbmButton);
		m_point = GetComponent<PointFab>();
	}

    void Start(){
        m_moneyEffect = gameObject.AddComponent<MoneyEffect>();        
        m_moneyEffect.SetParent(GameObject.Find("counter-money").transform);
        m_moneyEffect.SetOffset(new Vector2(-7, -20));

        m_unitQueueUI = GameObject.Find("unit-queue").GetComponent<UnitQueuUI>();
    }
	
	void Update() {
		Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		if(Input.GetMouseButtonDown(0)) {
			// Check if the click was on the point
			if(GetComponent<Collider2D>().OverlapPoint(clickPos)) {
				if(!IsOpen()) {
					AudioSource.PlayClipAtPoint(m_clickSound,transform.position);
				} else {
					AudioSource.PlayClipAtPoint(m_cancelSound,transform.position,0.5f);
					m_spawnList.Clear();
                    m_unitQueueUI.Clear();
				}
				SetOpen(!IsOpen());
			}
			// Check if it collides with any children
			else if(m_fighterButton.GetComponent<Collider2D>().OverlapPoint(clickPos)
					|| m_bomberButton.GetComponent<Collider2D>().OverlapPoint(clickPos)
					|| m_icbmButton.GetComponent<Collider2D>().OverlapPoint(clickPos)) {
				// Nothing to do, children are doing work
			}
			// Check if it collides with any other points
			else {
				Point destination = ClickedOnPoint(clickPos);
				if(null != destination && IsOpen()) {
					foreach(Entity.Type type in m_spawnList) {
						GameObject unit = m_unitFactory.SpawnUnit(type);
						unit.GetComponent<Unit>().SetSourceAndTarget(m_point,destination);
						unit.SetActive(false);
						m_spawnQueue.Enqueue(unit);
					}
					StopAllCoroutines();
					if(m_spawnList.Count > 0) {
						m_spawnList.Clear();
                        m_unitQueueUI.Clear();
						AudioSource.PlayClipAtPoint(m_launchSound,transform.position,0.5f);
						AI.m_userStarted = true;
					}
					StartCoroutine(SpawnUnits());
					SetOpen(false);
				}
				// Player clicked in an invalid location, cancel everything
				else {
					if(IsOpen()) {
						AudioSource.PlayClipAtPoint(m_cancelSound,transform.position,0.5f);
                        int diff = State.PlayerMoney;
						foreach(Entity.Type type in m_spawnList) {
							State.PlayerMoney += State.GetCostOf(type);
						}
                        diff =  State.PlayerMoney - diff;
                        if(diff > 0){
                            m_moneyEffect.StartEffect(diff);
                        }

						m_spawnList.Clear();
                        m_unitQueueUI.Clear();
					}
					SetOpen(false);
				}
			}
		}
	}
	
	void OnMouseEnter() {
		if(!IsOpen()) {
			AudioSource.PlayClipAtPoint(m_hoverSound,transform.position,0.5f);
		}
	}
	
	private GameObject CreateButton(string name, string img, Entity.Type type, Vector2 offset) {
		GameObject ret = new GameObject();
		ret.name = name;
		ret.transform.parent = this.transform;
		ret.transform.localPosition = offset;
		ret.SetActive(false);
		
		tk2dSprite sprite = ret.AddComponent<tk2dSprite>();
		sprite.SetSprite(GetComponent<tk2dSprite>().Collection, m_theme + img);
		
		BoxCollider2D box = ret.AddComponent<BoxCollider2D>();
		box.size = sprite.GetBounds().size;
		box.isTrigger = true;
		
		UIUnit unit = ret.AddComponent<UIUnit>();
		unit.m_buySound = m_buySound;
		unit.type = type;
		unit.fab = this;
		return ret;
	}
	
	public void AddToSpawnList(Entity.Type type) {
		if(IsOpen()) {
			m_spawnList.Add(type);
            m_unitQueueUI.AddUnit(type);
		}
	}
	
	public Point ClickedOnPoint(Vector3 pos) {
		pos.z = -1;
		RaycastHit2D hit = Physics2D.Raycast(pos,Vector2.zero);
		if(null == hit.collider || null == hit.collider.gameObject) {
			return null;
		}
		Point point = hit.collider.gameObject.GetComponent<Point>();
		if(point == null) {
			return null;
		}
		if(point.m_owner != m_point.m_owner) {
			if(!point.dead)
				return point;
		}
		return null;
	}

	public IEnumerator SpawnUnits() {
		while(0 < m_spawnQueue.Count) {
			GameObject go = m_spawnQueue.Dequeue();
			go.SetActive(true);
			yield return new WaitForSeconds(0.5f);
		}
	}
}
