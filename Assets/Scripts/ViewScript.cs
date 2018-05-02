using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewScript : MonoBehaviour {

    public Transform gun;
    Transform target;
    Transform squadTarget;
    SquadMember squad;

    Transform monitor_target;
    bool monitoring = false;
    bool seen = false;

    float update = 0.0f;

    // Use this for initialization
    void Start()
    {
        squad = transform.GetComponentInParent<SquadMember>();
    }
	
	// Update is called once per frame
	void Update () {
        update += 100.0f * Time.deltaTime;

        if (update >= 100.0f)
        {
            update = 0.0f;
            if (target != null)
            {
                RaycastHit hit;
                //raycast towards them
                Vector3 dir = target.position - transform.parent.position;
                Physics.Raycast(transform.parent.position + transform.parent.up * 0.8f, dir, out hit);

                //Debug.DrawRay(transform.parent.position + transform.parent.up * 0.8f, dir);

                if ((hit.collider != null && hit.collider.gameObject.tag != "Enemy" && hit.collider.gameObject.tag != "Dummy") || hit.collider == null)
                {
                    target = null;
                    squad.SetTarget(null);
                }
                else
                {
                    if (squad.GetTarget() == null)
                    {
                        squad.SetTarget(target);
                    }
                    if (monitoring)
                    {
                        if (target == monitor_target)
                        {
                            seen = true;
                        }
                    }
                }
            }
            else
            {
                squad.SetTarget(null);
                //gun.transform.localRotation = Quaternion.identity;
            }
        }
	}

    void OnTriggerStay(Collider col)
    {
        if (target == null || col.transform == target)
        {
            if (col.tag == "Enemy" || col.tag == "Dummy")
            {
                RaycastHit hit;
                //raycast towards them
                Vector3 dir = col.transform.position - transform.parent.position;
                Physics.Raycast(transform.parent.position + transform.parent.up * 0.8f, dir, out hit);

                if (hit.collider != null && (hit.collider.tag == "Enemy" || hit.collider.tag == "Dummy"))
                {
                    //enemy can be seen, shoot them
                    target = hit.collider.transform;
                    if (squad.GetTarget() == null || squad.GetTarget() == target)
                    {
                        squad.SetTarget(target);
                    }
                    if (monitoring)
                    {
                        if (target == monitor_target)
                        {
                            seen = true;
                        }
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if(col.transform == target)
        {
            target = null;
            squad.SetTarget(null);
        }
    }

    public void Monitor(Transform watch)
    {
        if (watch != null)
        {
            monitoring = true;
            monitor_target = watch;
        }
    }

    public bool IsMonitoring()
    {
        return monitoring;
    }

    public void UpdateMonitor(Transform watch)
    {
        if (monitoring)
        {
            monitor_target = watch;
        }
    }

    public bool FinishMonitor()
    {
        if(!monitoring)
        {
            return false;
        }
        bool result = seen;
        seen = false;
        monitoring = false;
        monitor_target = null;
        return result;
    }
}
