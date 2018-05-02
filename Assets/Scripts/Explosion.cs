using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {

    Light[] m_lights;

	// Use this for initialization
	void Start () {
        m_lights = GetComponentsInChildren<Light>();
	}
	
	// Update is called once per frame
	void Update () {
		foreach(Light light in m_lights)
        {
            light.intensity *= 0.9f;
            if(light.intensity <= 0.05f)
            {
                Destroy(gameObject);
                break;
            }
        }
	}
}
