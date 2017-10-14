using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gel : MonoBehaviour {

    public bool IsTouchable = false;
    public int Col = 0;
    public int Row = 0;

	// Use this for initialization
	void Start () {		
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void Init(int col, int row)
    {
        Col = col;
        Row = row;
    }
}
