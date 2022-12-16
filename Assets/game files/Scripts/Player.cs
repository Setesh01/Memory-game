using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//border struct
public struct Border {
	public float left;
	public float right;
}

//type of (mobile) input
public enum InputType {
	Drag,
	Tilt
}

public class Player : MonoBehaviour {
	
	//visible in the inspector
	public InputType input;
	public float sensitivity;
	public Rigidbody rb;
	public int jumpForce;
	public float widthEffect;
	public Animator anim;
	public float trampolineEffect;
	
	[HideInInspector]
	public bool pause;
    
	float dragStart;
	float xStart;
	Vector3 startScale;
	bool jumpAnimation;
	Transform currentPlatform;
	bool canMove;
	
	public Border border = new Border();
	
	void Awake(){
		//calculate the player border
		GetBorder();
		
		//set default scale to current scale
		startScale = transform.localScale;
	}
	
	void Update(){				
		if(jumpAnimation){
			//move current platform and move player with it
			currentPlatform.Translate(Vector3.up * Time.deltaTime * -trampolineEffect);
			transform.Translate(Vector3.up * Time.deltaTime * -trampolineEffect);
		}
		
		//if paused, don't update player
		if(!rb.useGravity || Time.timeScale == 0)
			return;
			
		if(input == InputType.Tilt){
			//check for acceleration to move player
			transform.Translate(Vector3.right * Input.acceleration.x * sensitivity * 18);
			
			float x = transform.position.x;
			
			//check for the screen edge to move to the opposite side
			if(EdgeCheck(ref x))
				transform.position = new Vector3(x, transform.position.y, transform.position.z);
		}
		else{
			//otherwise use drag input
			DragInput();
		}
		
		//make sure scale is based on player velocity for jump effect
		float xScale = startScale.x + -Mathf.Abs(rb.velocity.y * widthEffect);
		//float xScale = startScale.x + (rb.velocity.y * widthEffect);
		float yScale = startScale.y/xScale;
		
		//update scale
		transform.localScale = new Vector3(xScale, yScale, transform.localScale.z);
		
		//get distance between player and camera position
		float dist = (transform.position - Camera.main.transform.position).z;
		
		//if player falls too far down, game over
		if(transform.position.y < Camera.main.ViewportToWorldPoint(new Vector3(0, 0, dist)).y){
			GameObject.FindObjectOfType<GameManager>().GameOver();
			
			Destroy(gameObject);
		}
	}
	
	void DragInput(){
		//start drag input
		if(Input.GetMouseButtonDown(0)){
			ResetDrag();
			canMove = true;
		}
		else if(Input.GetMouseButton(0) && canMove){
			//check difference in drag start and current drag position
			float x = xStart + ((Input.mousePosition.x - dragStart) * sensitivity);
			//check for edge
			bool crossingScreenEdge = EdgeCheck(ref x);
			
			//update player position
			transform.position = new Vector3(x, transform.position.y, transform.position.z);
			
			//reset drag motion when player gets to opposite side of the screen
			if(crossingScreenEdge)
				ResetDrag();
		}
	}
	
	//reset drag input
	void ResetDrag(){
		dragStart = Input.mousePosition.x;
		xStart = transform.position.x;
	}
	
	bool EdgeCheck(ref float x){
		//check left border
		if(x < border.left){
			x = border.right - 0.1f;
			return true;
		}
		else if(x > border.right){
			//check right border as well
			x = border.left + 0.1f;
			return true;
		}
		
		//if x is in between borders, do nothing
		return false;
	}
	
	public void PlatformTrigger(Transform platform){
		if(rb.velocity.y > 0)
			return;
		
		//show jump effect when player triggers platform
		currentPlatform = platform;
		StartCoroutine(Jump());
	}
	
	//enable player gravity
	public void StartPlayer(){
		rb.useGravity = true;
	}
	
	IEnumerator Jump(){	
		//disable player gravity and reset velocity
		rb.velocity = Vector3.zero;
		transform.localScale = startScale;
		rb.useGravity = false;
		
		//show pause animation on the player (where he looks around and jumps)
		if(pause)
			anim.SetTrigger("pause");
		
		//wait for the player to unpause
		while(pause){
			yield return 0;
		}
		
		//start jump effect
		jumpAnimation = true;
		
		//jump animation
		anim.SetTrigger("jump");
		//wait
		yield return new WaitForSeconds(1f/4f);
		
		//re-enable gravity
		rb.useGravity = true;
		
		//add jump force up and stop jump animation
		rb.AddForce(Vector3.up * jumpForce);
		jumpAnimation = false;
		
		//tell the platform we've jumped off
		currentPlatform.GetComponent<Platform>().Used();
	}
	
	void GetBorder(){
		//calculate the border based on the camera in world space
		float dist = (transform.position - Camera.main.transform.position).z;
		
		border.left = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, dist)).x;
		border.right = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, dist)).x;
		//border.top = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, dist)).y;
		//border.bottom = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, dist)).y;
	}
}
