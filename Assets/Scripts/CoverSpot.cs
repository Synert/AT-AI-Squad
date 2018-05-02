using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverSpot : MonoBehaviour {

    float danger = 0.0f;
    float lastDanger = 0.0f;
    float dangerCap = 5000.0f;
    float squadDanger = 0.0f;
    float lastSquadDanger = 0.0f;
    float grenadeSquad = 0.0f;
    float enemyDanger = 0.0f;
    float lastEnemyDanger = 0.0f;
    float grenadeEnemy = 0.0f;
    bool inUse = false;
    bool isLow = false;
    bool isLeftCorner = true;
    bool isRightCorner = true;
    bool destroyed = false;
    Transform m_owner;

    float updateCooldown = 0.0f;

	// Use this for initialization
	void Start () {
        updateCooldown = Random.Range(0.0f, 2000.0f);
        //make sure the spot exists on a corner, or on a low piece of cover

        int layerMask = 1 << 8;

        bool wallLeft = Physics.Raycast(transform.position - transform.right * 1.5f, -transform.forward, 2.0f, layerMask)
            || Physics.Raycast(transform.position, -transform.right, 0.5f, layerMask);
        bool wallRight = Physics.Raycast(transform.position + transform.right * 1.5f, -transform.forward, 2.0f, layerMask)
            || Physics.Raycast(transform.position, transform.right, 0.5f, layerMask);
        isLow = !Physics.Raycast(transform.position + transform.up * 1.5f, -transform.forward, 2.0f, layerMask);

        bool tooFarLeft = !Physics.Raycast(transform.position - transform.right * 0.25f, -transform.forward, 2.0f, layerMask);
        bool tooFarRight = !Physics.Raycast(transform.position + transform.right * 0.25f, -transform.forward, 2.0f, layerMask);

        if(tooFarLeft || tooFarRight)
        {
            Destroy(gameObject);
        }

        if (wallRight)
        {
            isRightCorner = false;
        }

        if (wallLeft)
        {
            isLeftCorner = false;
        }

        if(!isRightCorner && !isLeftCorner && !isLow)
        {
            Destroy(gameObject);
        }

        UnityEngine.AI.NavMeshHit hit;

        if (!UnityEngine.AI.NavMesh.SamplePosition(transform.position, out hit, 2.6f, UnityEngine.AI.NavMesh.AllAreas))
        {
            Destroy(gameObject);
        }
    }
	
	// Update is called once per frame
	void Update () {
        Debug.DrawLine(transform.position, transform.position + transform.up * grenadeEnemy * 0.025f, Color.red);

        if (inUse)
        {
            if(m_owner == null)
            {
                inUse = false;
            }
            //Debug.DrawLine(transform.position + transform.right * 0.1f, transform.position + transform.right * 0.1f + transform.up * 10.0f, Color.blue);
        }
        else
        {
            if(isLow)
            {
                //Debug.DrawLine(transform.position + transform.right * 0.1f, transform.position + transform.right * 0.1f + transform.up * 10.0f, Color.yellow);
            }
            else if(isLeftCorner)
            {
                //Debug.DrawLine(transform.position + transform.right * 0.1f, transform.position + transform.right * 0.1f + transform.up * 10.0f, Color.green);
            }
            else if(isRightCorner)
            {
                //Debug.DrawLine(transform.position + transform.right * 0.1f, transform.position + transform.right * 0.1f + transform.up * 10.0f, Color.magenta);
            }
        }

        float squadDangerDiff = squadDanger - lastSquadDanger;
        float enemyDangerDiff = enemyDanger - lastEnemyDanger;

        if (enemyDangerDiff > 50.0f)
        {
            if(m_owner != null)
            {
                SquadMember squad = m_owner.GetComponent<SquadMember>();
                if(squad != null)
                {
                    inUse = false;
                    m_owner = null;
                    squad.RefreshCover();
                }
            }
        }

        if (squadDangerDiff > 50.0f)
        {
            if (m_owner != null)
            {
                Enemy enemy = m_owner.GetComponent<Enemy>();
                if (enemy != null)
                {
                    inUse = false;
                    m_owner = null;
                    //enemy.RefreshCover();
                }
            }
        }

        lastSquadDanger = squadDanger;
        lastEnemyDanger = enemyDanger;

        if(updateCooldown > 0.0f)
        {
            updateCooldown -= 1000.0f * Time.deltaTime;
        }

        if(updateCooldown <= 0.0f)
        {
            updateCooldown = 1000.0f;
            squadDanger = 0.0f;
            enemyDanger = 0.0f;
            grenadeSquad = 0.0f;
            grenadeEnemy = 0.0f;
            UpdateDanger();
        }
	}

    void UpdateDanger()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, 35.0f, (1 << 11));
        Collider[] squads = Physics.OverlapSphere(transform.position, 35.0f, (1 << 10));
        RaycastHit hit;

        foreach(Collider enemy in enemies)
        {
            Enemy thisEnemy = enemy.GetComponent<Enemy>();
            if(thisEnemy == null)
            {
                continue;
            }

            //Debug.DrawRay(transform.position + transform.up * 1.8f, enemy.transform.position - (transform.position + transform.up * 1.8f), Color.magenta, 0.5f);

            //int layerMask = (1 << 5 | 1 << 10);
            int layerMask = (1 << 8 | 1 << 12);
            //layerMask = ~layerMask;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);

            if (!Physics.Raycast(transform.position + transform.up * 0.5f,
                enemy.transform.position - (transform.position + transform.up * 0.5f), out hit, dist, layerMask, QueryTriggerInteraction.Ignore))
            {
                //if (hit.collider.tag == "Enemy" && hit.collider.GetComponent<Enemy>() == thisEnemy)
                //{
                    //Debug.DrawRay(transform.position + transform.up * 0.5f, enemy.transform.position - (transform.position + transform.up * 0.5f), Color.magenta, 0.5f);
                    enemyDanger += 100.0f;
                    grenadeEnemy += 35.0f - dist;
                //}
            }
        }

        foreach (Collider squad in squads)
        {
            SquadMember thisSquad = squad.GetComponent<SquadMember>();
            PlayerController player = squad.GetComponent<PlayerController>();

            //Debug.DrawRay(transform.position + transform.up * 1.8f, squad.transform.position - (transform.position + transform.up * 1.8f), Color.cyan, 0.5f);

            //int layerMask = (1 << 5 | 1 << 11);
            int layerMask = (1 << 8 | 1 << 12);
            //layerMask = ~layerMask;

            float dist = Vector3.Distance(transform.position, squad.transform.position);

            if (!Physics.Raycast(transform.position + transform.up * 0.5f,
                squad.transform.position - (transform.position + transform.up * 0.5f), out hit, dist, layerMask, QueryTriggerInteraction.Ignore))
            {
                //if (hit.collider.tag == "SquadMan" && hit.collider.GetComponent<SquadMember>() == thisSquad)
                //{
                    //Debug.DrawRay(transform.position + transform.up * 0.5f, squad.transform.position - (transform.position + transform.up * 0.5f), Color.cyan, 0.5f);
                    squadDanger += 100.0f;
                    grenadeSquad += 35.0f - dist;
                //}
            }
        }
    }

    public float GetEnemyDanger()
    {
        return enemyDanger;
    }

    public float GetSquadDanger()
    {
        return squadDanger;
    }

    public float GetEnemyGrenade()
    {
        return grenadeEnemy;
    }

    public float GetSquadGrenade()
    {
        return grenadeSquad;
    }

    public float GetDanger()
    {
        return lastDanger;
    }

    public void AddDanger(float add)
    {
        danger += add;
        if(danger > dangerCap)
        {
            danger = dangerCap;
        }
    }

    public bool InUse()
    {
        return inUse;
    }

    public void SetUse(bool set)
    {
        inUse = set;
    }

    public void SetOwner(Transform owner)
    {
        m_owner = owner;
    }

    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.layer == 8 || col.gameObject.layer == 9 || col.gameObject.layer == 12)
        {
            if (col.gameObject.layer == 9)
            {
                if (!col.GetComponent<CoverSpot>().destroyed && !destroyed)
                {
                    col.GetComponent<CoverSpot>().destroyed = true;
                    Destroy(col.gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    public bool IsLow()
    {
        return isLow;
    }

    public bool IsLeftCorner()
    {
        return isLeftCorner;
    }

    public bool IsRightCorner()
    {
        return isRightCorner;
    }

    public bool IsCorner()
    {
        return (isRightCorner || isLeftCorner);
    }

    public bool IsShootable()
    {
        return (isRightCorner || isLeftCorner || isLow);
    }

    public void Stagger(float t)
    {
        updateCooldown = t;
    }

}
