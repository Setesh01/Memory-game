using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPanelButton : MonoBehaviour {
	
	//player and manager references
	Player player;
	GameManager manager;
    
	void Start(){
		//get the player and manager
		player = GameObject.FindObjectOfType<Player>();
		manager = GameObject.FindObjectOfType<GameManager>();
	}
	
	void OnTriggerEnter(Collider other){
		//if the player triggers the red button, show the color selection panel
		if(!other.gameObject.CompareTag("Player"))
			return;
		
		if(player.rb.velocity.y < 0)
			manager.ColorPanel(true);
	}
}
