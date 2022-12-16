using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CheckColorSequence))]
public class GameManager : MonoBehaviour {
    
	//public variables adjustable through inspector
	public GameObject platform;
	public GameObject colorPlatform;
	public float minDistance;
	public float maxDistance;
	public Transform target;
	public float spawnDistance;
	public float zSpawnPos;
	public float xMaxDistance;
	public float spawnDelay;
	public int spawnCount;
	public Animator startPanel;
	public Animator gamePanel;
	public Animator colorPanel;
	public Animator colorResultPanel;
	public Animator gameOverPanel;
	public GameObject pausePanel;
	public Text scoreLabel;
	public int minPathLength;
	public int maxPathLength;
	public Text gameOverScore;
	public Text gameOverBest;
	public Animator fade;
	
	//the four colors
	public Color[] colors;
	
	Player player;
	Border border;
	CheckColorSequence sequenceCheck;
	
	float highestPlatform = -10f;
	float xLastPlatform;
	float tapTime;
	
	bool started;
	bool spawning;
	bool gameOver;
	
	int score;
	int platformsUntilColor;
	
	List<int> sequence = new List<int>();
	int sequenceIndex;
	
	void Start(){
		//get the player, border and sequence checker
		player = target.GetComponent<Player>();
		border = player.border;
		sequenceCheck = GetComponent<CheckColorSequence>();
		
		//unpause the game
		Pause(false);
		
		//get the start path length
		platformsUntilColor = PathLength();
		
		//make a new color sequence
		NewColorSequence();
	}
	
	void Update(){
		//if the player jumped high enough, spawn some new platforms
		if(target != null && Mathf.Abs(highestPlatform - target.position.y) < spawnDistance && !spawning)
			StartCoroutine(SpawnNew());
		
		if(Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()){
			if(!(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))){
				//record the start tap time (to distinguish between taps and swipe)
				if(started && !colorPanel.GetBool("visible"))
					tapTime = 0;
				
				//tapping when the game is over will reload scene
				if(gameOver){
					StartCoroutine(Reload());
				}
				else if(!started){
					//otherwise if the game hadn't yet started, start game
					StartGame();
				}
			}
		}
		
		//pause the game on click
		if(Input.GetMouseButtonUp(0) && tapTime < 0.15f && !gameOver)
			Pause(Time.timeScale == 1);
		
		//increase tap time
		tapTime += Time.deltaTime;
	}
	
	//pause the game and show panel
	void Pause(bool pause){
		Time.timeScale = pause ? 0 : 1;
		pausePanel.SetActive(pause);
	}
	
	void NewColorSequence(){
		//clear the sequence and remember the last sequence length
		int lastLength = sequence.Count;
		sequence.Clear();
		
		//add random colors to the sequence and make sure it's one longer than the previous one (lastLength + 1)
		for(int i = 0; i < lastLength + 1; i++){
			int randomColor = Random.Range(0, colors.Length);
			sequence.Add(randomColor);
		}
	}
	
	IEnumerator SpawnNew(){
		//start spawning
		spawning = true;
		
		//for the number of platforms
		for(int i = 0; i < spawnCount; i++){
			//get random height
			float randomHeight = Random.Range(minDistance, maxDistance);
			
			//add random height to current platform height
			highestPlatform += randomHeight;
			
			float platformWidth = platform.transform.localScale.x * 0.5f;
			
			//calculate the max left and right values so the random platform position won't fall off the screen
			float left = xLastPlatform - xMaxDistance < border.left + platformWidth ? border.left + platformWidth : xLastPlatform - xMaxDistance;
			float right = xLastPlatform + xMaxDistance > border.right - platformWidth ? border.right - platformWidth : xLastPlatform + xMaxDistance;
			
			//get a random position between the left and right borders (based on the previous platform position as well)
			float platformX = Random.Range(left, right);
			//get the new position
			Vector3 platformPosition = new Vector3(platformX, highestPlatform, zSpawnPos);
			
			if(sequenceIndex != sequence.Count){
				//create a new platform
				GameObject newPlatform = Instantiate(platform, platformPosition, platform.transform.rotation);
			
				if(platformsUntilColor == 0){
					//if this one needs color, color the platform and get the new path length
					newPlatform.GetComponent<Renderer>().material.color = colors[sequence[sequenceIndex]];
					platformsUntilColor = PathLength();
					
					//increase the current sequence index by one
					sequenceIndex++;
				}
			}
			else{
				if(platformsUntilColor != 0){
					//create a normal platform
					Instantiate(platform, platformPosition, platform.transform.rotation);
				}
				else{
					//create the platform with the red button
					Instantiate(colorPlatform, platformPosition, platform.transform.rotation);
					
					//set new path length and reset sequence index
					platformsUntilColor = PathLength();
					sequenceIndex = 0;
					
					//change the min and max path length
					minPathLength++;
					maxPathLength += 2;
				}
			}
			
			//decrease the number of platforms until the next color
			if(platformsUntilColor != 0)
				platformsUntilColor--;
			
			//set the x position of the last platform (this one) and the new x max distance (to make the game more difficult over time)
			xLastPlatform = platformX;
			xMaxDistance += 0.05f;
			
			//wait for next platform to spawn
			yield return new WaitForSeconds(spawnDelay);
		}
		
		//stop spawning
		spawning = false;
	}
	
	//return a random path length
	int PathLength(){
		int length = Random.Range(minPathLength, maxPathLength);		
		return length;
	}
	
	//start the player and update the UI
	public void StartGame(){
		player.StartPlayer();
		started = true;
		
		startPanel.SetTrigger("hide");
		gamePanel.SetTrigger("show");
	}
	
	public void GameOver(){
		gameOver = true;
		
		//update best score
		if(score > PlayerPrefs.GetInt("Best"))
			PlayerPrefs.SetInt("Best", score);
		
		//show scores
		gameOverBest.text = "BEST: " + PlayerPrefs.GetInt("Best");
		gameOverScore.text = "SCORE: " + score;
		
		//update UI
		gameOverPanel.SetTrigger("Game over");
		gamePanel.SetTrigger("show");
	}
	
	public void AddPoints(int points){
		//add points and update the label
		score += points;
		scoreLabel.text = score + "";
	}
	
	public void SequenceDone(bool succesful){
		//get a new color sequence
		NewColorSequence();
		
		//either double the score, or show the fail effect
		if(succesful){
			AddPoints(score);
			colorResultPanel.SetTrigger("succes");
		}
		else{
			colorResultPanel.SetTrigger("fail");
		}
		
		//disable the color panel
		ColorPanel(false);
	}
	
	//(un)pause the player and change the visibility of the color panel
	public void ColorPanel(bool state){
		player.pause = state;
		colorPanel.SetBool("visible", state);
		
		if(state)
			sequenceCheck.StartCheck(sequence);
	}
	
	IEnumerator Reload(){
		//fade and reload the game
		fade.SetTrigger("Fade");
		
		yield return new WaitForSeconds(0.4f);
		
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}
