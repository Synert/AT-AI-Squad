using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    UnityEngine.AI.NavMeshAgent m_agent;
    Transform m_target;
    Transform m_view;
    Gun m_gun;

    //make them randomly wander around
    float pickNewDest = 0.0f;
    Vector3 dest;
    bool kickDoors = false;
    float inCombat = 0.0f;

    //walking 'animation'
    Vector3 prevPos;
    float walkCycle;
    int alt = 1;
    Transform leftLeg;
    Transform rightLeg;

    //cover system
    Transform m_cover;
    bool takingCover = false;
    float inCover = 0.0f;
    bool regroup = false;
    float danger = 0.0f;
    float lastDanger = 0.0f;
    bool crouched = false;

    //flashbanged
    float blind = 0.0f;

    List<SquadMember> seen = new List<SquadMember>();

    float updateCooldown = 500.0f;

    bool holding = false;

    //health
    public int health = 30;

	// Use this for initialization
	void Start () {
        //get the navmesh agent
        m_agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        m_gun = GetComponentInChildren<Gun>();

        m_gun.SetOwner(transform);

        //get the view cone
        foreach (Transform child in transform)
        {
            if (child.gameObject.tag == "ViewCone")
            {
                m_view = child;
            }
            if (child.gameObject.tag == "LeftLeg")
            {
                leftLeg = child;
            }
            if (child.gameObject.tag == "RightLeg")
            {
                rightLeg = child;
            }
        }

        prevPos = transform.position;
        dest = transform.position;
        pickNewDest = Random.Range(1.0f, 6.0f);
    }
	
	// Update is called once per frame
	void Update () {

        MainLogic();

    }

    void MainLogic()
    {
        GenerateDestination();

        Walk(Vector3.Distance(transform.position, prevPos) * 50.0f);

        if(inCombat > 0.0f)
        {
            inCombat -= Time.deltaTime * 1000.0f;
        }

        if (blind <= 0.0f)
        {
            if(updateCooldown <= 0.0f)
            {
                UpdateSeen();
                updateCooldown = 500.0f;
            }
            UpdateSquad();
        }

        if((danger > 750.0f || lastDanger > 750.0f) && !takingCover)
        {
            TakeCover();
        }

        if (danger > 0.0f && (danger - lastDanger) <= 0.0f) danger -= 1000.0f * Time.deltaTime;

        lastDanger = danger;

        if (blind > 0.0f)
        {
            blind -= 1000.0f * Time.deltaTime;
        }

        if(updateCooldown > 0.0f)
        {
            updateCooldown -= 1000.0f * Time.deltaTime;
        }

        if (m_target != null && blind <= 0.0f)
        {
            AimAngle(m_target);
        }
        else
        {
            if (m_view.transform.localRotation.eulerAngles.y > 1.0f && m_view.transform.localRotation.eulerAngles.y < 150.0f)
            {
                m_view.RotateAround(transform.position, Vector3.up, -450.0f * Time.deltaTime);
            }
            if (m_view.transform.localRotation.eulerAngles.y < 359.0f && m_view.transform.localRotation.eulerAngles.y > 150.0f)
            {
                m_view.RotateAround(transform.position, Vector3.up, 450.0f * Time.deltaTime);
            }
        }

        if (inCover > 0.0f)
        {
            inCover -= 1000.0f * Time.deltaTime;
            if(inCover <= 0.0f)
            {
                ExitCover();
            }
        }
        if(takingCover)
        {
            m_agent.destination = m_cover.transform.position;
        }
        else if(blind <= 0.0f)
        {
            m_agent.destination = dest;
        }
        else
        {
            m_agent.destination = transform.position;
        }

        prevPos = transform.position;
    }

    void UpdateSeen()
    {
        int layerMask = 1 << 10;
        RaycastHit hit;
        Collider[] squad = Physics.OverlapSphere(transform.position, 25.0f, layerMask);

        layerMask = (1 << 8 | 1 << 10 | 1 << 12);

        foreach (Collider man in squad)
        {
            SquadMember squadman = man.GetComponent<SquadMember>();
            bool contains = seen.Contains(squadman);

            if (Physics.Raycast(transform.position + transform.up * 0.8f, man.transform.position - transform.position, out hit, 30.0f, layerMask))
            {
                if (squadman != null && hit.collider == man && !contains)
                {
                    seen.Add(squadman);
                }
                else if(contains)
                {
                    seen.Remove(squadman);
                }
            }
            else if(contains)
            {
                seen.Remove(squadman);
            }
        }
    }

    void UpdateSquad()
    {
        foreach(SquadMember squad in seen)
        {
            squad.AddDanger(500.0f * Time.deltaTime);
            AddDanger(300.0f * Time.deltaTime);
        }
    }

    public void AddDanger(float amount)
    {
        danger += amount;
    }

    void Walk(float distance)
    {
        if (Mathf.Abs(distance) <= 1.0f)
        {
            //walkCycle = 0.0f;
            if (leftLeg.rotation.eulerAngles.x > 1.0f && leftLeg.rotation.eulerAngles.x < 100.0f)
            {
                leftLeg.RotateAround(transform.position, leftLeg.right, -50.0f * Time.deltaTime);
            }
            if (leftLeg.rotation.eulerAngles.x < 359.0f && leftLeg.rotation.eulerAngles.x > 100.0f)
            {
                leftLeg.RotateAround(transform.position, leftLeg.right, 50.0f * Time.deltaTime);
            }

            if (rightLeg.rotation.eulerAngles.x > 1.0f && rightLeg.rotation.eulerAngles.x < 100.0f)
            {
                rightLeg.RotateAround(transform.position, rightLeg.right, -50.0f * Time.deltaTime);
            }
            if (rightLeg.rotation.eulerAngles.x < 359.0f && rightLeg.rotation.eulerAngles.x > 100.0f)
            {
                rightLeg.RotateAround(transform.position, rightLeg.right, 50.0f * Time.deltaTime);
            }
        }

        walkCycle += Mathf.Abs(distance);
        if (walkCycle > 25.0f)
        {
            alt *= -1;
            walkCycle -= 75.0f;
        }

        leftLeg.RotateAround(transform.position, leftLeg.right, distance * alt);
        if (leftLeg.rotation.eulerAngles.x > 35.0f && leftLeg.rotation.eulerAngles.x < 100.0f)
        {
            leftLeg.RotateAround(transform.position, leftLeg.right, -Mathf.Abs(distance));
        }
        if (leftLeg.rotation.eulerAngles.x < 325.0f && leftLeg.rotation.eulerAngles.x > 100.0f)
        {
            leftLeg.RotateAround(transform.position, leftLeg.right, Mathf.Abs(distance));
        }

        rightLeg.RotateAround(transform.position, rightLeg.right, -distance * alt);
        if (rightLeg.rotation.eulerAngles.x > 35.0f && rightLeg.rotation.eulerAngles.x < 100.0f)
        {
            rightLeg.RotateAround(transform.position, rightLeg.right, -Mathf.Abs(distance));
        }
        if (rightLeg.rotation.eulerAngles.x < 325.0f && rightLeg.rotation.eulerAngles.x > 100.0f)
        {
            rightLeg.RotateAround(transform.position, rightLeg.right, Mathf.Abs(distance));
        }
    }

    public void SetTarget(Transform target)
    {
        m_target = target;
    }

    void AimAngle(Transform target)
    {
        Quaternion oldRot = m_view.localRotation;
        m_view.LookAt(target);

        Quaternion newRot = m_view.localRotation;
        m_view.localRotation = oldRot;

        newRot = Quaternion.Euler(0.0f, newRot.eulerAngles.y, 0.0f);

        m_view.localRotation = Quaternion.Lerp(oldRot, newRot, 5.0f * Time.deltaTime);

        RaycastHit hit;
        //raycast towards them
        Vector3 dir = m_view.forward;
        Physics.Raycast(transform.position + transform.up * 0.8f, dir, out hit);

        if (hit.collider != null && (hit.collider.tag == "SquadMan" || hit.collider.tag == "Dummy"))
        {
            inCombat += 200.0f;
            m_gun.Shoot();
        }
        //this one's necessary for hitting crouched targets
        else
        {
            Physics.Raycast(transform.position, dir, out hit);
            if (hit.collider != null && (hit.collider.tag == "SquadMan" || hit.collider.tag == "Dummy"))
            {
                //but are they actually behind cover?
                Physics.Raycast(transform.position - transform.up * 0.5f, dir, out hit);

                if (hit.collider != null && (hit.collider.tag == "SquadMan" || hit.collider.tag == "Dummy"))
                {
                    m_gun.ShootLow();
                }
                else
                {
                    m_gun.Shoot();
                }
            }
        }
        //m_view.LookAt(target);
    }

    public void Damage(int damage)
    {
        inCombat += 500.0f;
        AddDanger(damage * 5.0f);
        health -= damage;
        TakeCover();
        if(health <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void TakeCover()
    {
        //find cover
        if (!takingCover)
        {
            //m_oldpoint = m_waypoint;

            //get nearest cover
            int layerMask = 1 << 9;

            Collider[] coverSpots = Physics.OverlapSphere(transform.position, 30.0f, layerMask, QueryTriggerInteraction.Collide);
            float value = 200000.0f;

            foreach (Collider spot in coverSpots)
            {
                CoverSpot thisSpot = spot.GetComponent<CoverSpot>();

                if (!thisSpot.InUse())
                {
                    UnityEngine.AI.NavMeshPath testPath = new UnityEngine.AI.NavMeshPath();
                    m_agent.CalculatePath(spot.transform.position, testPath);

                    float tempDist = 0.0f;
                    Vector3 prevCorner = testPath.corners[0];

                    for (int i = 1; i < testPath.corners.Length; i++)
                    {
                        Vector3 curCorner = testPath.corners[i];
                        float thisDist = Vector3.Distance(prevCorner, curCorner);
                        tempDist += thisDist;
                        if (Physics.Raycast(prevCorner, (curCorner - prevCorner), thisDist, (1 << 12)))
                        {
                            tempDist += 100.0f;
                        }
                        prevCorner = curCorner;
                    }

                    float newDanger = thisSpot.GetSquadDanger();

                    float shootable = 0.0f;
                    if (thisSpot.IsLow())
                    {
                        //shootable = 40.0f;
                    }

                    float closeTarget = 0.0f;
                    if (m_target != null)
                    {
                        //closeTarget = Vector3.Distance(spot.transform.position, m_target.position) * 15.0f;
                    }

                    float newValue = (tempDist * 15.0f) + newDanger - shootable + closeTarget;

                    if (newValue < value)
                    {
                        //Debug.Log(tempDist + " " + newDanger + " " + newValue);
                        m_cover = spot.transform;
                        value = newValue;
                    }
                }
            }

            CoverSpot checkSpot = m_cover.GetComponent<CoverSpot>();
            if (checkSpot != null)
            {
                checkSpot.SetUse(true);
                checkSpot.SetOwner(transform);
                inCover = 3000.0f;
                takingCover = true;
                holding = false;
            }
        }
    }

    public void ExitCover()
    {
        if (m_cover != null)
        {
            CoverSpot spot = m_cover.GetComponent<CoverSpot>();
            if (spot != null)
            {
                spot.SetUse(false);
                spot.SetOwner(null);
            }

            m_cover = null;

            inCover = 0.0f;
            takingCover = false;
            /*Uncrouch();
            peek = 0;
            crouchPeek = false;
            peekCooldown = 500.0f;
            crouchPeekCooldown = 500.0f;
            peekDur = 0.0f;
            crouchPeekDur = 0.0f;*/
            //m_waypoint = m_oldpoint;
        }
    }

    public void Blind(float amount)
    {
        blind += amount;
        TakeCover();
    }

    void GenerateDestination()
    {
        if (m_target != null)
        {
            dest = m_target.position;
            pickNewDest = Random.Range(0.25f, 1.0f);
            kickDoors = true;
            return;
        }
        if (Vector3.Distance(transform.position, dest) <= 2.0f)
        {
            pickNewDest -= Time.deltaTime;
            if (pickNewDest <= 0.0f)
            {
                kickDoors = false;
                bool goodSpot = false;
                do
                {
                    goodSpot = false;
                    Vector3 randomDirection = Random.insideUnitSphere;
                    randomDirection.Normalize();
                    float pathLength = Random.Range(3.0f, 10.0f);
                    randomDirection *= pathLength;
                    randomDirection += transform.position;
                    UnityEngine.AI.NavMeshHit hit;
                    if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, 2.0f, 1))
                    {
                        goodSpot = true;
                        dest = hit.position;
                        //Debug.Log(dest);
                        pickNewDest = Random.Range(pathLength / 3.0f, pathLength / 2.0f);
                        if (inCombat > 0.0f)
                        {
                            pickNewDest /= 2.0f;
                        }
                    }
                } while (!CheckDestinationPathing(dest) || !goodSpot);
            }
        }
    }

    bool CheckDestinationPathing(Vector3 pos)
    {
        if (kickDoors) return true;
        UnityEngine.AI.NavMeshPath testPath = new UnityEngine.AI.NavMeshPath();

        m_agent.CalculatePath(pos, testPath);

        Vector3 prevCorner = testPath.corners[0];
        Vector3 spawnPos = prevCorner;
        RaycastHit hit;

        for (int i = 1; i < testPath.corners.Length; i++)
        {
            Vector3 curCorner = testPath.corners[i];
            Vector3 dir = Vector3.Normalize((curCorner + Vector3.up) - (prevCorner + Vector3.up));
            for (float j = 0.0f; j < Vector3.Distance(prevCorner, curCorner); j += 1.0f)
            {
                if (!Physics.Raycast(prevCorner + (j - 1) * dir, dir, out hit, 1.0f, (1 << 12)))
                {
                    if (j >= (Vector3.Distance(prevCorner, curCorner) - 1.0f) && i >= testPath.corners.Length - 1)
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            prevCorner = curCorner;
        }

        return true;
    }

    public void ReactSound(float dist, int newDanger, Vector3 pos)
    {
        if (newDanger > 100.0f || danger > 300.0f)
        {
            kickDoors = true;
        }

        //is it dangerous enough
        if (newDanger > danger)
        {
            if (CheckDestinationPathing(pos))
            {
                //store danger
                pickNewDest = 2.0f;
                dest = pos;
            }
        }

        AddDanger(newDanger * dist);
    }

    public bool GetKickDoor()
    {
        return kickDoors;
    }

}
