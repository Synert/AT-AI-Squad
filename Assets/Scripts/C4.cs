using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C4 : MonoBehaviour {

    public Transform explosion;
    Transform m_door;
    Light m_light;
    float alt = 0.0f;
    float timer = 1500.0f;
    bool armed = false;
    bool exploded = false;

	// Use this for initialization
	void Start () {
        m_light = GetComponentInChildren<Light>();
	}
	
	// Update is called once per frame
	void Update () {
        if(exploded)
        {
            Explode();
        }
		else if(armed)
        {
            alt += 1000.0f * Time.deltaTime;
            if(alt >= 200.0f)
            {
                alt = 0.0f;
                m_light.enabled = !m_light.enabled;
            }
            timer -= 1000.0f * Time.deltaTime;
            if(timer <= 0.0f)
            {
                if (m_door != null)
                {
                    Destroy(m_door.gameObject);
                }
                exploded = true;
            }
        }
	}

    public void SetCharge(Transform door)
    {
        m_door = door;
        armed = true;
    }

    void Explode()
    {
        Transform new_explosion = Instantiate(explosion, transform.position - transform.forward, transform.rotation);

        int layerMask = 1 << 11;

        Collider[] enemies = Physics.OverlapSphere(transform.position, 10.0f, layerMask);
        foreach(Collider enemy in enemies)
        {
            //Debug.DrawRay(transform.position + transform.up * 0.3f, (enemy.transform.position - transform.position), Color.yellow, 5.0f);
            Vector3 origin = transform.position - transform.forward;
            float dist = Vector3.Distance(origin, enemy.transform.position);

            //only hit inside the room
            if (Vector3.Distance(enemy.transform.position, origin + transform.forward) >
                Vector3.Distance(enemy.transform.position, origin - transform.forward))
            {
                if (!Physics.Raycast(origin + transform.up * 0.5f, (enemy.transform.position - origin),
                    dist, (1 << 8 | 1 << 12)))
                {
                    //Debug.DrawLine(origin + transform.up * 0.5f, enemy.transform.position + transform.up * 0.5f, Color.red, 5.0f);
                    Enemy enemyComp = enemy.GetComponentInChildren<Enemy>();
                    Dummy dummyComp = enemy.GetComponentInChildren<Dummy>();
                    if (enemyComp != null)
                    {
                        Debug.Log("Damaging enemy for " + (int)(75.0f - dist * 7.5));
                        enemyComp.Damage((int)(75.0f - dist * 7.5));
                    }
                    if (dummyComp != null)
                    {
                        dummyComp.Damage(20);
                    }
                }
            }
        }

        Destroy(gameObject);
    }
}
