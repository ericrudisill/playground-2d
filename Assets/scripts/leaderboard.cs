using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaderboard : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void rank() {
		// determine what player has a higher rank
	}
		
		
	public enum LeaderboardType {
		PERSONAL, ONLINE, CROSSPLAY, GEOFENCE
	}

	public Leaderboard CreateLeaderboard(LeaderboardType t, Score s) {
		// Create a leaderboard 
		// Get the score of current game and add it to leaderboard


		return Leaderboard;
	}

	public void DeleteLeaderboard(LeaderboardType t) {
		// remove leaderboard

	}

	public Leaderboard updateLeaderboard() {
		// update the leaderboard based off of current rankings
		// return an up to date leaderboard
		return Leaderboard;
	}





}

