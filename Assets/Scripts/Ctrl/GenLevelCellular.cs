﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenLevelCellular : MonoBehaviour 
{
	public GameObject[] floor;
	public GameObject[] wall;
	public GameObject[] treasure;
	public GameObject[] debris;
	public GameObject playerBase;

	public Material floorMaterial;
//	public List<int[,]> maps=new List<int[,]>();

	public int[,] map;

	bool created=false;

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (!created && Network.isServer)
		{
			createLevel();
			created=true;
		}

		if (Input.GetMouseButtonUp(0))
		{
			//createLevel();
		}
	}

	public void createLevel()
	{
//		int childs = transform.childCount;
//		for (int i = 0; i > childs; i++	)
		{
//			GameObject theFloor=transform.GetChild(i).gameObject;
			GameObject theFloor=gameObject;

			CellularAutomata ca=new CellularAutomata(32,32);
			
			map= ca.generateMap();
			ca.simplePlaceTreasure(map,6);
			ca.zeroLimits(map);
			ca.placeBase(map);

			//		ca.simplePlaceObject(map,.2f);
			
			for (int x = 0; x < map.GetLength(0); x++)
			{
				for (int y = 0; y < map.GetLength(1); y++)
				{
					GameObject go=null;
					if (map[x,y]==0) 
					{
//						go=(GameObject)GameObject.Instantiate(floor[Random.Range(0,floor.Length-1)]);
//						go.transform.Find("Cube").GetComponent<MeshFilter>().mesh.uv
//							mainTextureOffset.x=x/width;
//						go.transform.Find("Cube").renderer.material.mainTextureOffset.y=y/width;
					}
					else if (map[x,y]==1) 
					{
						go=(GameObject)GameObject.Instantiate(wall[Random.Range(0,wall.Length-1)]);
					}
					else if (map[x,y]==2) 
					{
//						go=(GameObject)GameObject.Instantiate(floor[Random.Range(0,floor.Length-1)]);
//						go.transform.parent=theFloor.transform;
//						go.transform.localPosition=new Vector3(x- map.GetLength(0)/2,0,y-map.GetLength(1)/2);
//						go.transform.localRotation=Quaternion.identity;

						go=(GameObject)GameObject.Instantiate(treasure[Random.Range(0,treasure.Length-1)]);
					}
					
					else if (map[x,y]==3) 
					{
						go=(GameObject)GameObject.Instantiate(floor[Random.Range(0,floor.Length-1)]);
						go.transform.parent=theFloor.transform;
						go.transform.localPosition=new Vector3(x- map.GetLength(0)/2,0,y-map.GetLength(1)/2);
						go.transform.localRotation=Quaternion.identity;

						go=(GameObject)GameObject.Instantiate(debris[Random.Range(0,debris.Length-1)]);
					}
					else if (map[x,y]==7) 
					{
						go=(GameObject)GameObject.Instantiate(playerBase);
					}

					if (go!=null)
					{
						go.transform.parent=theFloor.transform;
						go.transform.localPosition=new Vector3(x- map.GetLength(0)/2,0,y-map.GetLength(1)/2);
						go.transform.localRotation=Quaternion.identity;
						go.name="LVL"+x+","+y;
					}
				}
			}

		}


	}

}
