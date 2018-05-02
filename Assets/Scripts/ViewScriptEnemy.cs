using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewScriptEnemy : MonoBehaviour {

    public Transform gun;
    Transform target;
    Enemy enemy;

    float update = 0.0f;

    // Use this for initialization
    void Awake()
    {
        enemy = transform.GetComponentInParent<Enemy>();
    }
	
	// Update is called once per frame
	void Update () {
        update += 500.0f * Time.deltaTime;

        if (update >= 500.0f)
        {
            update = 0.0f;
            if (target != null)
            {
                enemy.SetTarget(target);

                RaycastHit hit;
                //raycast towards them
                Vector3 dir = target.position - transform.parent.position;
                Vector3 aimUp = transform.parent.up * 0.8f;

                SquadMember testCrouch = target.GetComponent<SquadMember>();

                if (testCrouch != null && testCrouch.GetCrouch())
                {
                    aimUp *= 0.0f;
                }

                Physics.Raycast(transform.parent.position + aimUp, dir, out hit);

                if (hit.collider != null && hit.collider.gameObject.tag != "SquadMan")
                {
                    target = null;
                    enemy.SetTarget(null);
                }
            }
            else
            {
                enemy.SetTarget(null);
                //gun.transform.localRotation = Quaternion.identity;
            }
        }
	}

    void OnTriggerStay(Collider col)
    {
        if(enemy == null)
        {
            //return;
        }
        if (target == null || col.transform == target)
        {
            if (col.tag == "SquadMan")
            {
                RaycastHit hit;
                //raycast towards them
                Vector3 dir = col.transform.position - transform.parent.position;
                Vector3 aimUp = transform.parent.up * 0.8f;

                SquadMember testCrouch = col.GetComponent<SquadMember>();

                if (testCrouch != null && testCrouch.GetCrouch())
                {
                    aimUp *= 0.0f;
                }

                Physics.Raycast(transform.parent.position + aimUp, dir, out hit);

                if (hit.collider != null && hit.collider.tag == "SquadMan")
                {
                    //enemy can be seen, shoot them
                    target = hit.collider.transform;
                    if(target == null)
                    {
                        Debug.Log("internal screaming");
                    }
                    enemy.SetTarget(target);
                    //gun.LookAt(hit.collider.transform);

                    //Destroy(hit.collider.gameObject);
                }
            }
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if(col.transform == target)
        {
            target = null;
            enemy.SetTarget(null);
        }
    }
}
