﻿using UnityEngine;
using UnityEngine.EventSystems;
using UI;

//Handles control and spawn of player prefab


namespace PlayGroup
{

	public class PlayerManager : Photon.PunBehaviour, IPunObservable
	{


		[Header("The current Health of our player")]
		public float Health = 100f;

		[Header("The PlayerPrefabs. Instantiate for each player")]
		public Transform playerPrefab;

		[Header("SpawnPoint: TODO: SpawnPoints Array")]
		public Transform spawnPoint;

		[HideInInspector]
		public static GameObject LocalPlayerObj;

		[HideInInspector]
		public static PlayerScript LocalPlayerScript;

		//reporting
		public bool hasSpawned {
			get {
				if (LocalPlayerObj == null) {
					return false;
				} else {
					return true;
				}
			}
		}


		//True, when the user is firing
		bool IsFiring;

	

		public void Awake()
		{
			if (hasSpawned) {
			
				Camera2DFollow.followControl.target = LocalPlayerObj.transform;
			
			}

		}


	

		public void CheckIfSpawned(){

			Debug.Log ("CHECK IF SPAWNED");
			if (!hasSpawned && GameData.control.isInGame) {

				GameObject spawnPlayer = Instantiate (playerPrefab, spawnPoint.position,Quaternion.identity).gameObject; //TODO: More spawn points and a way to iterate through them
				LocalPlayerObj = spawnPlayer;
				LocalPlayerScript = spawnPlayer.GetComponent<PlayerScript> ();
				LocalPlayerScript.isMine = true; // Set this object to yours, the rest are for network players
				Managers.control.playerScript = LocalPlayerScript; // Set this on the manager so it can be accessed by other components/managers
				Camera2DFollow.followControl.target = LocalPlayerObj.transform;

				return;
			}

			if (hasSpawned && GameData.control.isInGame && Managers.control.playerScript == null) { //if we lost the player reference somehow (unforeseen problems) then just give the ref back
			
				Managers.control.playerScript = LocalPlayerScript; 
				Camera2DFollow.followControl.target = LocalPlayerObj.transform;
			
			}


		}

		public void Update()
		{
			// we only process Inputs and check health if we are the local player

			//FIXME CHECK WITH PHOTONVIEW COMPONENT WHEN IT IS ADDED
//			if (photonView.isMine)
			if(hasSpawned && LocalPlayerScript != null)
			{
				this.ProcessInputs();

				if (this.Health <= 0f)
				{
					//TODO DEATH
				}
			}

	
		}


		/// <summary>
		/// Processes the inputs. This MUST ONLY BE USED when the player has authority over this Networked GameObject (photonView.isMine == true)
		/// </summary>
		void ProcessInputs()
		{
			//INPUT CONTROLS HERE
			if (Input.GetKeyUp (KeyCode.W) || Input.GetKeyUp (KeyCode.A) || Input.GetKeyUp (KeyCode.S) || Input.GetKeyUp (KeyCode.D)) {
				LocalPlayerScript.physicsMove.MoveInputReleased ();
			}

			//hold key down inputs. clampPos is used to snap player to an axis on movement
			if (Input.GetKey (KeyCode.D) && !LocalPlayerScript.physicsMove.isMoving || Input.GetKey (KeyCode.D) && LocalPlayerScript.physicsMove.isMoving && LocalPlayerScript.physicsMove._moveDirection == Vector2.left) {
				//RIGHT
				LocalPlayerScript.MovePlayer (Vector2.right);

			} 
			if (Input.GetKey (KeyCode.A) && !LocalPlayerScript.physicsMove.isMoving || Input.GetKey (KeyCode.A) && LocalPlayerScript.physicsMove.isMoving && LocalPlayerScript.physicsMove._moveDirection == Vector2.right) {
				//LEFT
				LocalPlayerScript.MovePlayer (Vector2.left);

			}
			if (Input.GetKey (KeyCode.S) && !LocalPlayerScript.physicsMove.isMoving || Input.GetKey (KeyCode.S) && LocalPlayerScript.physicsMove.isMoving && LocalPlayerScript.physicsMove._moveDirection == Vector2.up) {
				//DOWN
				LocalPlayerScript.MovePlayer (Vector2.down);

			} 
			if (Input.GetKey (KeyCode.W) && !LocalPlayerScript.physicsMove.isMoving || Input.GetKey (KeyCode.W) && LocalPlayerScript.physicsMove.isMoving && LocalPlayerScript.physicsMove._moveDirection == Vector2.down) {
				LocalPlayerScript.MovePlayer (Vector2.up);

			} 


		}

	



		// IPunObservable implementation

		public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			if (stream.isWriting)
			{
				// We own this player: send the others our data
				stream.SendNext(this.IsFiring);
				stream.SendNext(this.Health);
			}
			else
			{
				// Network player, receive data
				this.IsFiring = (bool)stream.ReceiveNext();
				this.Health = (float)stream.ReceiveNext();
			}
		}




	}
}