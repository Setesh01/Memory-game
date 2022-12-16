using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckColorSequence : MonoBehaviour {
	
	//label that displays number of colors yet to submit
	public Text colorsLeft;
	
	GameManager manager;
	
	int current;
	List<int> sequence;
	
	void Start(){
		//get the game manager
		manager = GetComponent<GameManager>();
	}
    
	public void StartCheck(List<int> colorSequence){
		//reset current input place and assign sequence
		sequence = colorSequence;
		current = 0;
		
		//set the colors left label
		colorsLeft.text = sequence.Count + "";
	}
	
	public void ColorInput(int color){
		//check the color input
		if(color != sequence[current]){
			manager.SequenceDone(false);
		}
		else{			
			if(current == sequence.Count - 1)
				manager.SequenceDone(true);
			
			current++;
			colorsLeft.text = (sequence.Count - current) + "";
		}
	}
}
