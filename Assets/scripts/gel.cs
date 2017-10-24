using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class gel : MonoBehaviour {

    public bool IsTouchable = false;
    public int Col = 0;
    public int Row = 0;
    public float ShrinkDuration = 0.15f;

    private bool isMatched = false;

	// Use this for initialization
	void Start () {		
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void SetMatched()
    {
        isMatched = true;
        transform.localScale = new Vector3(0.5f, 0.5f);
    }

    public bool IsMatched
    {
        get { return isMatched;  }
    }

    public void Pop()
    {
        transform.DOScale(0.01f, ShrinkDuration)
            .OnComplete(() => Destroy(this.gameObject));
    }
}
