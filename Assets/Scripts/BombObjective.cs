using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombObjective : MonoBehaviour {

    public Transform explosion;
    Transform defuser;
    Light m_light;
    float alt = 0.0f;
    public float timer = 120.0f;
    public float defuseTime = 5.0f;
    bool armed = true;
    bool exploded = false;

    // Use this for initialization
    void Start()
    {
        m_light = GetComponentInChildren<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        if(defuser != null)
        {
            if(!defuser.GetComponent<SquadMember>().Defusing())
            {
                defuser = null;
            }
        }
        if (exploded)
        {
            Explode();
        }
        else if (armed)
        {
            alt += 1000.0f * Time.deltaTime;
            if (alt >= 200.0f)
            {
                alt = 0.0f;
                m_light.enabled = !m_light.enabled;
            }
            int minutes = (int)Mathf.Floor(timer / 60.0f);
            int seconds = (int)(timer) % 60;

            GetComponentInChildren<TextMesh>().text = minutes + ":" + seconds;

            timer -= Time.deltaTime;
            if (timer <= 0.0f)
            {
                exploded = true;
            }
        }
    }

    void Explode()
    {
        Transform new_explosion = Instantiate(explosion, transform.position, transform.rotation);

        /*int layerMask = 1 << 11;

        Collider[] enemies = Physics.OverlapSphere(transform.position, 10.0f, layerMask);
        foreach (Collider enemy in enemies)
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
        }*/

        Destroy(gameObject);
    }

    public void Defuse()
    {
        defuseTime -= Time.deltaTime;
        if(defuseTime <= 0.0f)
        {
            armed = false;
        }
    }

    public bool Defused()
    {
        return !armed;
    }

    public void SetDefuser(Transform set)
    {
        defuser = set;
    }

    public bool GetDefuser()
    {
        return defuser != null;
    }
}
