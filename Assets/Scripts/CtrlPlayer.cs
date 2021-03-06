using UnityEngine;
using System.Collections;

public class CtrlPlayer : MonoBehaviour 
{
	public Player model;
	public Animator animator;

	//Sync values
	private float lastSynchronizationTime = 0f;
	private float syncDelay = 0f;
	private float syncTime = 0f;
	private Vector3 syncStartPosition = Vector3.zero;
	private Vector3 syncEndPosition = Vector3.zero;

	public int speed=1;
	public int turnSpeed=1;
	public float damping=0.5f;

	TheCube cube;
	GameObject daPlayer;
	GameObject daRangeAttack;

	public Vector3 curSpeed=Vector3.zero;
	TheCube.Direction playerDir=TheCube.Direction.NORTH;
	
	void Awake()
	{
		model = new Player();
		transform.Find("RangeToAttack").GetComponent<SphereCollider>().radius = model.stunRange;

		if (networkView.isMine)
		{
			GameObject.Find("UiManager").GetComponent<CtrlUiManager>().model = model;
			GameObject.Find("CameraMan").GetComponent<CameraRail>().target=this.gameObject;
		}

		daRangeAttack=transform.Find("RangeToAttack").gameObject;
//		daRangeAttack.SetActive(false);
	}

	void Start()
	{
		initDaMutha();
		cube=new TheCube();
		daPlayer=gameObject;
		GameObject.Find("UiManager").GetComponent<CtrlUiManager>().startCountDown=true;

	}

	
	void OnNetworkInstantiate (NetworkMessageInfo msg ) 
	{
		Debug.Log("OnNetworkInstantiate");
		if (networkView.isMine) 
		{
			gameObject.name="player_me_"+networkView.owner;
			model.team=int.Parse(networkView.owner.ToString())%2+1;
			Destroy(Camera.main.GetComponent<AudioListener>());
			gameObject.AddComponent<AudioListener>();

			GameObject theFloor=GameObject.Find(model.team==1?"Face1":"Face5");

			Camera.main.transform.parent.parent=theFloor.transform;
			Camera.main.transform.parent.localRotation=Quaternion.identity;
			Camera.main.transform.localRotation=theFloor.transform.localRotation;

			transform.Find("PointLight").GetComponent<Light>().color = model.team==1?Color.red:Color.blue;
		}
		else 
		{
			gameObject.name="player_"+msg.sender;
			model.team=int.Parse(msg.sender.ToString())%2+1;
		}
	}

	public void initDaMutha()
	{
		Debug.Log("initDaMutha");
		GameObject player=gameObject;
		GameObject theFloor=GameObject.Find(model.team==1?"Face1":"Face5");


		player.transform.parent=theFloor.transform;
		player.transform.localRotation=Quaternion.identity;

		int [,] map=theFloor.GetComponent<GenLevelCellular>().map;
		if (map!=null)
		{
			int[] spawnPoint=CellularAutomata.getSpawnPoint(map);
			player.transform.localPosition=new Vector3(spawnPoint[0]-16,0,spawnPoint[1]-16);
			Debug.Log("Spawned player at "+spawnPoint[0]+","+spawnPoint[1]);
		}
		else
		{
			player.transform.localPosition=Vector3.zero;
			Debug.Log("Spawned player at 0,0... map not found");
		}

//		player.transform.eulerAngles=Vector3.zero;
	}

	
	void Update()
	{
		if (networkView.isMine )
		{
			if (!isStunned())
			{
				curSpeed=Vector2.zero;
				InputMovement();
			}
			else
			{
				if (model.stunnedFor>0)
					model.stunnedFor-=Time.deltaTime;
			}

			 
		
		}
		else
		{
			SyncedMovement();
		}
	}

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		Vector3 syncPosition = Vector3.zero;
		Vector3 syncVelocity = Vector3.zero;
		
		if (stream.isWriting)
		{
			syncPosition = transform.position;
			stream.Serialize(ref syncPosition);
			
			syncVelocity = curSpeed;
			stream.Serialize(ref syncVelocity);
		}
		else
		{
			stream.Serialize(ref syncPosition);
			stream.Serialize(ref syncVelocity);
			
			syncTime = 0f;
			syncDelay = Time.time - lastSynchronizationTime;
			lastSynchronizationTime = Time.time;
			
			syncEndPosition = syncPosition + syncVelocity * syncDelay;
			syncStartPosition = transform.position;
		}
	}


	private void InputMovement()
	{
		Vector3 vec=transform.localPosition;
		bool moved=false;
		if (Input.GetKey(KeyCode.W))
		{
			//			transform.Translate(0,0,speed*Time.deltaTime);
			//			transform.Translate(Vector3.forward*speed*Time.deltaTime);
			curSpeed.z=speed;
			moved=true;
			playerDir=TheCube.Direction.NORTH;

		}
		
		if (Input.GetKey(KeyCode.S))
		{
			//			transform.Translate(0,0,-speed*Time.deltaTime);
			//			transform.Translate(Vector3.back*speed*Time.deltaTime);
			curSpeed.z=-speed;
			moved=true;
			playerDir=TheCube.Direction.SOUTH;
		}
		
		if (Input.GetKey(KeyCode.A))
		{
			//			transform.Translate(-speed*Time.deltaTime,0,0);
			//			transform.Translate(Vector3.left*speed*Time.deltaTime);
			curSpeed.x=-speed;
			moved=true;
			playerDir=TheCube.Direction.EAST;
		}
		
		if (Input.GetKey(KeyCode.D))
		{
			//			transform.Translate(speed*Time.deltaTime,0,0);
			//			transform.Translate(Vector3.right*speed*Time.deltaTime);
			curSpeed.x=speed;
			moved=true;
			playerDir=TheCube.Direction.WEST;
		}

//		if (Input.GetKey(KeyCode.Space))
//		{
//			stunTheBitches();
//		}

		curSpeed=curSpeed*damping;
		transform.Translate(curSpeed*Time.deltaTime);
		setDirection();

		if (curSpeed.magnitude > 0.5)
		{
			animator.SetBool("walking", true);
		}else
		{
			animator.SetBool("walking", false);
		}

		//		if (moved)
		{
			int[] mapPos=getMapPos();
			TheCube.Direction dir= cube.getOverflowDirection(mapPos);
			if (dir!=TheCube.Direction.ANY_FUCKIN_WHERE)
			{
				Debug.Log("Overflowed!!! Pos:"+mapPos[0]+","+mapPos[1]);
				int targetSide=cube.getNeighbour( dir);
				Debug.Log("Overflowed to "+dir+ " to target side "+targetSide );
				GameObject theFloor=GameObject.Find("Face"+targetSide);
				GameObject curFloor=gameObject.transform.parent.gameObject;

				daPlayer.transform.parent=theFloor.transform;
				Camera.main.transform.parent.parent=theFloor.transform;
				Camera.main.transform.parent.localRotation=Quaternion.identity;
				Camera.main.transform.localRotation=curFloor.transform.localRotation;
				daPlayer.transform.localRotation=Quaternion.identity;
				
				
				cube.currentSide=targetSide;
				
				int[] overflowedClampedPos=cube.getOverFlowClampedPos(mapPos);
				Debug.Log("ClampedPos:"+overflowedClampedPos[0]+","+overflowedClampedPos[1]);
				transform.localPosition=new Vector3(overflowedClampedPos[0]-16,0,overflowedClampedPos[1]-16);
				
				mapPos=getMapPos();
			}
			
			int xCoord=mapPos[0];
			int yCoord=mapPos[1];
			//			Debug.Log("Player map coord:" + xCoord+","+yCoord);

			if ( getMap()!=null && (getMap()[xCoord,yCoord]==1 || getMap()[xCoord,yCoord]==3))
			{
				transform.localPosition=vec;
				curSpeed=Vector3.Reflect(curSpeed,Vector3.right);
			}
		}

		int teamOfFloor = int.Parse(transform.parent.name.Substring(4))>3?2:1;
		if(teamOfFloor==model.team)
	    {
			daRangeAttack.SetActive(true);
	    }
		else
		{
			daRangeAttack.SetActive(false);
		}

	}

	public void stunTheBitches()
	{
		if (canStun())
		{
			daRangeAttack.SetActive(true);
			StartCoroutine("doNotStunTheBitches");
		}
	}

	public IEnumerator doNotStunTheBitches()
	{
		yield return new WaitForSeconds(0.5f);
		daRangeAttack.SetActive(false);
	}

	
	private void SyncedMovement()
	{
		syncTime += Time.deltaTime;
		transform.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
	}

	public void setDirection()
	{
		Transform t=gameObject.transform.Find("AncientJoe").transform;
		if (playerDir==TheCube.Direction.NORTH)
		{
			t.localRotation=Quaternion.Euler(new Vector3(270,0,0));
		}
		else
		if (playerDir==TheCube.Direction.SOUTH )
		{
			t.localRotation=Quaternion.Euler(new Vector3(270,180,0));
		}
		else
		if (playerDir==TheCube.Direction.EAST)
		{
			t.localRotation=Quaternion.Euler(new Vector3(270,-90,0));
		}
		else
		if (playerDir==TheCube.Direction.WEST)
		{
			t.localRotation=Quaternion.Euler(new Vector3(270,90,0));
		}

	}

	public bool hasResources()
	{
		return model.carryResources > 0;
	}

	public bool canCollect()
	{
		return model.carryResources < model.maxCarryResources;
	}

	public bool isStunned()
	{
		return model.stunnedFor > 0;
	}

	public bool canStun()
	{
		return !hasResources();
	}

	public void setStunned(float stunnedFor)
	{
		model.stunnedFor = stunnedFor;
		EventManager.onGotStunned("test");
	}

	public void onStuffPickup(int team)
	{
		model.points+= this.model.team==team?1:3;
		model.carryResources++;

		EventManager.onStuffPickup();

		//full loaded
		if (!canCollect())
		{
			EventManager.onStuffFullyLoaded();
		}
	}

	public void onStuffDepositTobase()
	{
		Transform t=GameObject.Find("ScoreDummy").transform;

		if (model.team==1)
		{
			t.localPosition=new Vector3(t.localPosition.x+model.carryResources,t.localPosition.y,0);
		}else
		{
			t.localPosition=new Vector3(t.localPosition.x,t.localPosition.y+model.carryResources,0); 
		}

		GetComponent<CtrlPlayerSoundManager>().playDeposit(model.carryResources);

		model.points = 0;
		model.carryResources = 0;

		EventManager.onStuffDepositTobase();
	}

	public int[,] getMap()
	{
		return gameObject.transform.parent.GetComponent<GenLevelCellular>().map;
	}
	
	public int[] getMapPos()
	{
		int xCoord=Mathf.CeilToInt(transform.localPosition.x)+16;
		int yCoord=Mathf.CeilToInt(transform.localPosition.z)+16;
		
		return new int[]{xCoord,yCoord};
	}
}
