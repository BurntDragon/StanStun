﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour 
{
	private const string typeName = "BurntDragonGame";
	public string gameName = "Da RoYal TemplE";

	private HostData[] hostList;
	
	public GameObject playerPrefab;
	public GameObject button;
	public Canvas ui;

	public int playerCount=0;

	private void StartServer()
	{
//		MasterServer.ipAddress = "127.0.0.1";
//		MasterServer.port = 23466;
//		Network.natFacilitatorIP = "127.0.0.1";
//		Network.natFacilitatorPort = 50005;

		Network.InitializeServer(4, 25000, !Network.HavePublicAddress());
		MasterServer.RegisterHost(typeName, gameName);
	}

	void OnServerInitialized()
	{
		Debug.Log("Server Initialized");
		SpawnPlayer();
	}

	void OnPlayerConnected()
	{
		Debug.Log("Client connected");
		GameObject world=GameObject.Find("World");
		for (int i=0;i<world.transform.childCount;i++)
		{
			world.transform.GetChild(i).GetComponent<GenLevelCellular>().newMapGenerated();
		}

	}

	void OnMasterServerEvent(MasterServerEvent msEvent)
	{
		if (msEvent == MasterServerEvent.HostListReceived)
		{
			hostList = MasterServer.PollHostList();
		}
	}

	void OnConnectedToServer()
	{
		Debug.Log("Server Joined");
		SpawnPlayer();

	}

	void OnGUI()
	{
		if (!Network.isClient && !Network.isServer)
		{
			if (GUI.Button(new Rect(100, 100, 250, 100), "New Game"))
				StartServer();
			
			if (GUI.Button(new Rect(100, 250, 250, 100), "Join Game"))
				RefreshHostList();

			if (hostList != null)
			{
				for (int i = 0; i < hostList.Length; i++)
				{
					if (GUI.Button(new Rect(400, 100 + (110 * i), 300, 100), hostList[i].gameName))
					{
						JoinServer(hostList[i]);
					}
				}
			}
		}
	}

	private void RefreshHostList()
	{
		MasterServer.RequestHostList(typeName);

//		if (hostList != null)
//		{
//			for (int i = 0; i < hostList.Length; i++)
//			{
//				foreach (HostData d in hostList)
//				{
//					GameObject go = (GameObject)Instantiate(button);
//					
//					go.transform.SetParent(ui.transform);
//					go.transform.localScale = new Vector3(1, 1, 1);
//					Button b = go.GetComponent<Button>();
//					b.onClick.AddListener(() => JoinServer(d));
//
//					go.transform.Find("Text").GetComponent<Text>().text = d.gameName;
//				}
//			}
//		}

	}

	private void JoinServer(HostData hostData)
	{
		Network.Connect(hostData);
	}

	private void SpawnPlayer()
	{
		playerCount++;
		GameObject player = (GameObject) Network.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity, 0);


	}
}
