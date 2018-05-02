using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverManager : MonoBehaviour {

    public Transform coverSpot;
    public bool procedural = false;
    const float gridSize = 0.75f;

	// Use this for initialization
	void Start () {
        if (!procedural)
        {
            RemakeCover();
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void RemakeCover()
    {
        float staggerTime = 0.0f;
        float staggerAmount = 50.0f;
        Bounds size = new Bounds(Vector3.zero, Vector3.zero);
        Renderer[] objects = FindObjectsOfType<Renderer>();
        foreach (Renderer i in objects)
        {
            size.Encapsulate(i.bounds);
        }

        //get all walls
        int layerMask = 1 << 8;
        //layerMask = ~layerMask;

        for (float x = size.min.x; x < size.max.x; x += gridSize)
        {
            foreach (RaycastHit hit in Physics.RaycastAll(new Vector3(x, 0.5f, size.min.z), Vector3.forward, size.max.z - size.min.z, layerMask))
            {
                //if (hit.collider.tag == "Level")
                //{
                    //spawn cover object
                    Transform newSpot = Instantiate(coverSpot, hit.point - new Vector3(0.0f, 0.0f, 0.6f), Quaternion.Euler(new Vector3(0.0f, -180.0f)));
                newSpot.GetComponent<CoverSpot>().Stagger(staggerTime);
                staggerTime += staggerAmount;
                //}
            }

            //and now the other way
            foreach (RaycastHit hit in Physics.RaycastAll(new Vector3(x, 0.5f, size.max.z), -Vector3.forward, size.max.z - size.min.z, layerMask))
            {
                Transform newSpot = Instantiate(coverSpot, hit.point + new Vector3(0.0f, 0.0f, 0.6f), Quaternion.Euler(new Vector3(0.0f, 0.0f)));
                newSpot.GetComponent<CoverSpot>().Stagger(staggerTime);
                staggerTime += staggerAmount;
            }

            //Debug.DrawRay(new Vector3(x, 1.0f, size.min.z), Vector3.forward * (size.max.z - size.min.z), Color.cyan, 10.0f);
        }

        for (float z = size.min.z; z < size.max.z; z += gridSize)
        {
            foreach (RaycastHit hit in Physics.RaycastAll(new Vector3(size.min.x, 0.5f, z), Vector3.right, size.max.x - size.min.x, layerMask))
            {
                Transform newSpot = Instantiate(coverSpot, hit.point - new Vector3(0.6f, 0.0f, 0.0f), Quaternion.Euler(new Vector3(0.0f, -90.0f)));
                newSpot.GetComponent<CoverSpot>().Stagger(staggerTime);
                staggerTime += staggerAmount;
            }

            //and now the other way
            foreach (RaycastHit hit in Physics.RaycastAll(new Vector3(size.max.x, 0.5f, z), -Vector3.right, size.max.x - size.min.x, layerMask))
            {
                Transform newSpot = Instantiate(coverSpot, hit.point + new Vector3(0.6f, 0.0f, 0.0f), Quaternion.Euler(new Vector3(0.0f, 90.0f)));
                newSpot.GetComponent<CoverSpot>().Stagger(staggerTime);
                staggerTime += staggerAmount;
            }

            //Debug.DrawRay(new Vector3(size.min.x, 1.0f, z), Vector3.right * (size.max.x - size.min.x), Color.red, 10.0f);
        }
    }
}
