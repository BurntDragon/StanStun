﻿using UnityEngine;
using System.Collections;

public class Player 
{
	public float speed = 10f;

	public int carryResources = 0;
	public int maxCarryResources = 6;
	public int points=0;

	public float stunnedFor = 0.0f;

	public float stunRange = 0.7f;
	public float stunTimeCollide = 3.0f;
	public float stunTimeRange = 2.0f;

	public int team=1;
}
