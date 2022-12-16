using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour {
	
	//visible in the inspector
	public float finalLifetime;
	public float movespeed;
	public float targetPositionZ;
	
	Player player;
	GameManager manager;
	float trampolineEffect;
	
	bool movingUp;
	bool moveBack;
	
	bool inPosition;
	
	void Start(){
		//get the game manager
		manager = GameObject.FindObjectOfType<GameManager>();
	}
	
	void Update(){
		//move forward towards camera (looks nice)
		if(!inPosition){
			transform.Translate(Vector3.forward * Time.deltaTime * -movespeed);
			
			//when close enough to the camera, stop moving
			if(transform.position.z <= targetPositionZ){
				transform.position = new Vector3(transform.position.x, transform.position.y, targetPositionZ);
				inPosition = true;
			}
		}
		
		//move up for the jumpy trampoline effect
		if(movingUp){
			transform.Translate(Vector3.up * Time.deltaTime * trampolineEffect);
		}
		else if(moveBack){
			//move back away from the camera
			transform.Translate(Vector3.forward * Time.deltaTime * movespeed);
			
			//if far enough away, destroy platform
			if(transform.position.z > 150)
				Destroy(gameObject);
		}
	}
    
	void OnTriggerEnter(Collider other){
		//check for the player
		if(!other.gameObject.CompareTag("Player"))
			return;
		
		if(player == null){
			//assign player and effect
			player = other.gameObject.GetComponent<Player>();
			trampolineEffect = player.trampolineEffect;
		}
		
		//call PlatformTrigger() on the player
		player.PlatformTrigger(gameObject.transform);
	}
	
	//after the player jumped on this platform, add points and remove this
	public void Used(){
		StartCoroutine(Remove());
		manager.AddPoints(1);
	}
	
	IEnumerator Remove(){
		//move back up
		movingUp = true;
		
		yield return new WaitForSeconds(1f/4f);
		
		//stop moving up
		movingUp = false;
		
		yield return new WaitForSeconds(finalLifetime);
		
		//move away from the camera
		moveBack = true;
	}
}
