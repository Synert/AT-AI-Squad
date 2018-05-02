using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelChunk : MonoBehaviour {

    public bool right, left, up, down;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public bool Right()
    {
        return right;
    }

    public bool Left()
    {
        return left;
    }

    public bool Up()
    {
        return up;
    }

    public bool Down()
    {
        return down;
    }
}
