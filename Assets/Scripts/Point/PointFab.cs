﻿using UnityEngine;
using System.Collections;

public class PointFab : Point {
	
	void Awake() {
		m_hp = 10;
		m_money = 10;
		
		GetComponent<tk2dSprite>().color = m_owner == Owner.PLAYER ? Color.blue : Color.red;
	}
}
