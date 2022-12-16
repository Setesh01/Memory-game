using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {
    
	//visible in the inspector
	public Transform player;
	public float camHeight;
	public float smoothness;
	
	float highestPoint;
	Vector3 velocity;
	
	void Start(){
		//set the highestPoint to the current player y position
		highestPoint = player.position.y;
	}
	
	void LateUpdate(){
		//don't do anything if there's no player character
		if(player == null)
			return;
		
		//update the highest point
		if(player.position.y > highestPoint)
			highestPoint = player.position.y;
		
		//update the camera
		UpdateCamera();
	}
	
	void UpdateCamera(){
		//get the targetHeight
		float targetHeight = highestPoint + camHeight;
		
		//create a target position
		Vector3 pos = new Vector3(transform.position.x, targetHeight, transform.position.z);
		
		//if we've not yet reached the target, adjust camera position
		if(transform.position.y < targetHeight)
			transform.position = Vector3.SmoothDamp(transform.position, pos, ref velocity, smoothness);
	}
}
