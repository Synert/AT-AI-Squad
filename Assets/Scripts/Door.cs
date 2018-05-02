using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {

    bool destroyed = false;
    bool opened = false;
    float openTime = 0.0f;
    int openDirection = 0;
    bool checkInputs = false;
    SphereCollider r;
    Transform player;
    Transform cube;

	// Use this for initialization
	void Start () {
        cube = transform.GetComponentInChildren<BoxCollider>().transform;
        r = GetComponent<SphereCollider>();
        r.radius = 1.5f;
	}
	
	// Update is called once per frame
	void Update () {
        if(checkInputs)
        {
            if(Input.GetKeyDown(KeyCode.F))
            {
                //ToggleDoor(player.position);
            }
            if(Input.GetKeyDown(KeyCode.Space))
            {
                Kick();
            }
        }
		if(openTime > 0.0f)
        {
            openTime -= Time.deltaTime;
            if(openTime <= 0.0f)
            {
                CloseDoor();
            }
        }
	}

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Door" && !col.GetComponent<Door>().destroyed && !destroyed)
        {
            Destroy(col.gameObject);
            col.GetComponent<Door>().destroyed = true;
            //attempt to cast to SquadMember
            //SquadMember squad = col.GetComponent<SquadMember>();
            //squad.EnterDoor(transform);
        }
        if(col.name == "Player")
        {
            checkInputs = true;
            player = col.transform;
        }
        if(col.gameObject.tag == "Enemy")
        {
            if(col.GetComponent<Enemy>().GetKickDoor())
            {
                Kick();
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if(col.name == "Player")
        {
            checkInputs = false;
        }
    }

    public void OpenDoor(Vector3 pos)
    {
        if (opened) return;
        if (Vector3.Distance(pos, transform.position + transform.forward * 2.0f) >
                                    Vector3.Distance(pos, transform.position - transform.forward * 2.0f))
        {
            openDirection = -1;
        }
        else
        {
            openDirection = 1;
        }

        cube.RotateAround(transform.position - new Vector3(2.25f, 0.0f, 0.0f), Vector3.up, 90.0f * openDirection);
        GetComponentInChildren<BoxCollider>().enabled = false;
        opened = true;
    }

    public void CloseDoor()
    {
        if (!opened) return;
        cube.RotateAround(transform.position - new Vector3(2.25f, 0.0f, 0.0f), Vector3.up, -90.0f * openDirection);
        GetComponentInChildren<BoxCollider>().enabled = true;
        opened = false;
    }

    public void ToggleDoor(Vector3 pos)
    {
        if (opened) CloseDoor();
        else OpenDoor(pos);
    }

    public bool IsOpen()
    {
        return opened;
    }

    public void Kick()
    {
        Destroy(gameObject);
    }
}
