using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Impact : MonoBehaviour {

    float lifeSpan = 200.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        lifeSpan -= 1000.0f * Time.deltaTime;
        if(lifeSpan <= 0.0f)
        {
            Destroy(gameObject);
        }
	}
}
