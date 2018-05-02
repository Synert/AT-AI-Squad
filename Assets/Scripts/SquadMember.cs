using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadMember : MonoBehaviour {

    public Transform m_waypoint, c4, flashbang;
    Transform m_door, m_cover, m_target, m_covertarget, m_view, m_objective;
    Vector3 m_suppress;
    UnityEngine.AI.NavMeshAgent m_agent;

    //gun stuff
    Gun[] m_gun;
    GameObject[] arms = new GameObject[2];
    int gunOut = 1;
    Vector3[] origPos = new Vector3[2];
    Quaternion[] origRot = new Quaternion[2];
    Transform pistolHolster, rifleHolster;
    float inCombat = 0.0f;

    ViewScript m_monitor;

    //make player orders override what they're doing
    float order_override = 0.0f;
    //make them wait for sync
    bool waitSync = false;

    //select ring
    Transform selected, innerRing, outerRing;

    //entering doors
    bool stacking = false;
    bool doneStacking = false;
    float stackCounter = 0.0f;
    bool isFlashing = false;
    float flashWait = 0.0f;

    //are we checking a direction currently?
    bool checking = false;
    //which direction
    int dir = 0;
    int lastCheck = 0;

    //checking variables
    const float cooldown = 2000.0f;
    const float checkTime = 350.0f;
    float checkTimer, checkCooldown, recheck, rightSafe, leftSafe;

    //squadmate stuff
    public bool isLeader;
    public bool playerSquad;
    public SquadMember[] m_squad = new SquadMember[3];
    int squadNumber = 0;
    int pos = 0;
    bool hasOrder = false;
    bool registered = false;

    //walking 'animation'
    Vector3 prevPos;
    float walkCycle;
    int alt = 1;
    Transform leftLeg, rightLeg, crouchLegs;

    //health
    public int health = 100;
    public Transform ragdoll;

    //cover system
    bool takingCover = false;
    float inCover = 0.0f;
    bool regroup = false;
    float danger = 0.0f;
    float lastDanger = 0.0f;
    bool crouched = false;

    //peeking corners
    int peek = 0;
    float peekDur = 0.0f;
    float peekCooldown = 0.0f;
    bool crouchPeek = false;
    float crouchPeekCooldown = 1000.0f;
    float crouchPeekDur = 0.0f;

    //holding
    bool holding = false;

    public enum GoalState
    {
        GOAL_STAY,
        GOAL_DEFEND,
        GOAL_OBJECTIVE,
        GOAL_COVER,
        GOAL_AIM
    }

    public enum DoorState
    {
        DOOR_KICK,
        DOOR_BREACH,
        DOOR_FLASH,
        DOOR_PEEK
    }

    int m_goalstate = (int)GoalState.GOAL_STAY;
    int m_doorstate = (int)DoorState.DOOR_KICK;
    int m_doorside = 0;

    void Start() {
        //get the navmesh agent
        m_agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        m_gun = GetComponentsInChildren<Gun>();
        int gunCount = 0;
        foreach(Gun gun in m_gun)
        {
            gun.SetOwner(transform);
            origPos[gunCount] = gun.transform.position;
            origRot[gunCount] = gun.transform.rotation;
            gunCount++;
        }
        m_monitor = GetComponentInChildren<ViewScript>();

        m_target = null;
        m_covertarget = null;

        //get the view cone, the legs and the guns
        foreach (Transform child in gameObject.GetComponentsInChildren<Transform>())
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
            if (child.gameObject.name == "CrouchLegs")
            {
                crouchLegs = child;
            }
            if (child.gameObject.name == "RifleArms")
            {
                arms[1] = child.gameObject;
            }
            if (child.gameObject.name == "PistolArms")
            {
                arms[0] = child.gameObject;
            }
            if(child.gameObject.name == "RifleHolster")
            {
                rifleHolster = child;
            }
            if(child.gameObject.name == "PistolHolster")
            {
                pistolHolster = child;
            }
            if(child.gameObject.name == "Selected")
            {
                selected = child;
            }
            if (child.gameObject.name == "InnerRing")
            {
                innerRing = child;
            }
            if (child.gameObject.name == "OuterRing")
            {
                outerRing = child;
            }
        }

        selected.gameObject.SetActive(false);

        //hide the set of arms not being used
        arms[gunOut].SetActive(true);
        arms[(1 - gunOut)].SetActive(false);

        //hide crouch legs
        crouchLegs.gameObject.SetActive(false);

        if(gunOut == 1)
        {
            m_gun[0].gameObject.transform.position = pistolHolster.position;
            m_gun[0].gameObject.transform.rotation = pistolHolster.rotation;
        }
        else
        {
            m_gun[1].gameObject.transform.position = rifleHolster.position;
            m_gun[1].gameObject.transform.rotation = rifleHolster.rotation;
        }

        bool foundLeader = false;

        //get the squad leader
        foreach (SquadMember squad in m_squad)
        {
            if (squad != null)
            {
                if (squad.isLeader)
                {
                    m_waypoint = squad.transform;
                    foundLeader = true;
                    isLeader = false;
                    break;
                }
            }
        }

        if(!foundLeader)
        {
            isLeader = true;
        }

        //and set up the default waypoint
        if(m_waypoint != null)
        {
            m_agent.destination = m_waypoint.position;
        }
        else
        {
            m_waypoint = transform;
        }

        checkCooldown = 500.0f;
        prevPos = transform.position;
    }

    // Update is called once per frame
    void Update () {
        //register
        if (isLeader && !registered)
        {
            if (playerSquad)
            {
                FindObjectsOfType<SquadManager>()[0].PlayerRegisterSquad(transform);
            }
            else
            {
                FindObjectsOfType<SquadManager>()[0].RegisterSquad(transform);
            }
        }
        registered = true;
        if (health > 0)
        {
            MainLogic();

            //ThrowGrenade();

            //leader logic
            if (isLeader)
            {
                LeaderCommands();
                m_agent.speed = 6.0f;
                m_agent.acceleration = 10.0f;
            }
            else
            {
                m_agent.speed = 7.0f;
                m_agent.acceleration = 14.0f;
            }
            if (crouched)
            {
                m_agent.speed /= 2.0f;
                m_agent.acceleration /= 2.0f;
            }
        }
        else
        {
            m_agent.destination = transform.position;
        }
    }

    void LeaderCommands()
    {
        bool allCover = false;
        bool allDefend = false;
        switch(m_goalstate)
        {
            case (int)GoalState.GOAL_STAY:
                //
                break;
            case (int)GoalState.GOAL_OBJECTIVE:
                //use the objective
                if (m_agent.remainingDistance < 2.0f)
                {
                    Debug.Log(Time.time + " defusing");
                    allDefend = true;
                    //defuse the bomb
                    if (m_objective != null)
                    {
                        m_objective.GetComponent<BombObjective>().Defuse();
                        if (m_objective.GetComponent<BombObjective>().Defused())
                        {
                            m_goalstate = (int)GoalState.GOAL_STAY;
                        }
                    }
                    else
                    {
                        m_goalstate = (int)GoalState.GOAL_STAY;
                    }
                }

                break;
            case (int)GoalState.GOAL_DEFEND:
                //defend the given point
                if (m_agent.remainingDistance < 5.0f)
                {
                    //
                    Defend(m_objective);
                    allDefend = true;
                }

                    break;
            case (int)GoalState.GOAL_COVER:
                if(m_agent.remainingDistance < 5.0f)
                {
                    allCover = true;
                    //m_state = (int)GoalState.MOVE_STAY;
                    TakeCover();
                }
                break;
        }

        hasOrder = false;
        int squad_int = 0;
        int squad_count = 0;
        bool finishStack = doneStacking;

        //get valid squad members
        foreach (SquadMember squad in m_squad)
        {
            if (squad != null)
            {
                if(allCover)
                {
                    squad.TakeCover();
                }
                if(allDefend)
                {
                    squad.Defend(m_objective);
                }
                if (!squad.doneStacking)
                {
                    finishStack = false;
                }

                squad_count++;

                if (!takingCover)
                {
                    if (squad.takingCover)
                    {
                        squad.regroup = true;
                        squad.inCover /= 2.0f;
                    }
                    else if (squad.holding)
                    {
                        squad.Regroup();
                    }
                }
            }
        }

        if(waitSync)
        {
            finishStack = false;
        }

        if (finishStack)
        {
            if (m_door != null)
            {
                switch (m_doorstate)
                {
                    case (int)DoorState.DOOR_KICK:
                        m_door.GetComponentInParent<Door>().Kick();
                        break;
                    case (int)DoorState.DOOR_BREACH:
                        //spawn c4
                        Transform newC4 = Instantiate(c4, m_door.position + m_door.forward * 0.25f * m_doorside, m_door.rotation);
                        newC4.GetComponent<C4>().SetCharge(m_door);
                        if(m_doorside < 0)
                        {
                            Quaternion newRot = Quaternion.Euler(newC4.rotation.eulerAngles.x, newC4.rotation.eulerAngles.y + 180.0f,
                                newC4.rotation.eulerAngles.z);
                            newC4.rotation = newRot;
                        }
                        break;
                    case (int)DoorState.DOOR_FLASH:
                        if(!isFlashing)
                        {
                            Transform newFlash = Instantiate(flashbang, m_door.position + m_door.forward * 1.0f * m_doorside, m_door.rotation);
                            if (m_doorside < 0)
                            {
                                Quaternion newRot = Quaternion.Euler(newFlash.rotation.eulerAngles.x, newFlash.rotation.eulerAngles.y + 180.0f,
                                newFlash.rotation.eulerAngles.z);
                                newFlash.rotation = newRot;
                            }
                            newFlash.GetComponent<Rigidbody>().AddForce(-newFlash.forward * 500.0f);
                            Destroy(m_door.gameObject);
                            isFlashing = true;
                            flashWait = newFlash.GetComponent<Flashbang>().GetFuse();
                        }
                        break;
                    case (int)DoorState.DOOR_PEEK:
                        m_door.GetComponentInParent<Door>().OpenDoor(transform.position);
                        break;
                }
            }

            if(flashWait > 0.0f)
            {
                flashWait -= 1000.0f * Time.deltaTime;
            }

            if(flashWait <= 0.0f)
            {
                doneStacking = false;
                stacking = false;
                stackCounter = 0.0f;
            }
            else
            {
                finishStack = false;
                isFlashing = false;
            }
        }

        //now order them
        Transform tempMember = transform;
        foreach (SquadMember squad in m_squad)
        {
            if (squad != null && !squad.takingCover)
            {
                if (finishStack)
                {
                    squad.doneStacking = false;
                    squad.stacking = false;
                    squad.stackCounter = 0.0f;
                }

                squad.Follow(tempMember);
                //tempMember = squad.transform;

                squad.hasOrder = false;

                switch (squad_int)
                {
                    case 0:
                        if (squad_count == 1)
                        {
                            //squad.CheckBack();
                            //squad.hasOrder = true;
                            squad.pos = 3;
                        }
                        else if (squad_count == 3)
                        {
                            squad.CheckLeft();
                            squad.pos = 1;
                        }
                        break;
                    case 1:
                        if (squad_count == 2)
                        {
                            squad.CheckBack();
                            squad.hasOrder = true;
                            squad.pos = 3;
                        }
                        else if (squad_count == 3)
                        {
                            squad.CheckRight();
                            squad.pos = 2;
                        }
                        break;
                    default:
                        //check behind
                        squad.CheckBack();
                        squad.hasOrder = true;
                        squad.pos = 3;
                        break;
                }
                squad_int++;
            }
        }
    }

    void MainLogic()
    {
        if(order_override > 0.0f)
        {
            order_override -= 1000.0f * Time.deltaTime;
        }

        if (inCombat > 0.0f)
        {
            inCombat -= 1000.0f * Time.deltaTime;
        }

        if (!takingCover && danger > 50.0f && order_override <= 0.0f)
        {
            TakeCover();
        }

        if(danger - lastDanger > 50.0f && order_override <= 0.0f)
        {
            RefreshCover();
        }

        if (danger > 0.0f && (danger - lastDanger) <= 0.0f)
        {
            danger -= 1000.0f * Time.deltaTime;
        }

        lastDanger = danger;

        if (m_target != null)
        {
            AimAngle(m_target, false);
            inCombat = 3000.0f;
        }

        /*else if (m_covertarget != null)
        {
            AimAngle(m_covertarget);
            inCombat = 3000.0f;
        }*/

        if (stacking)
        {
            if (Vector3.Distance(transform.position, prevPos) * 50.0f < 0.1f)
            {
                if (stackCounter < 200.0f)
                {
                    stackCounter += Time.deltaTime * 1000.0f;
                }
                if (stackCounter >= 200.0f)
                {
                    doneStacking = true;
                }
            }
        }

        else if (m_waypoint != null)
        {
            if (!takingCover)
            {
                m_covertarget = null;
                switch (pos)
                {
                    //to the left
                    case 1:
                        m_agent.destination = m_waypoint.position - m_waypoint.right * 0.75f;
                        break;
                    //to the right
                    case 2:
                        m_agent.destination = m_waypoint.position + m_waypoint.right * 0.75f;
                        break;
                    //behind
                    case 3:
                        m_agent.destination = m_waypoint.position - m_waypoint.forward * 1.0f;
                        break;
                    //behind left
                    case 4:
                        m_agent.destination = m_waypoint.position - m_waypoint.forward * 2.0f - m_waypoint.right * 1.0f;
                        break;
                    //behind right
                    case 5:
                        m_agent.destination = m_waypoint.position - m_waypoint.forward * 2.0f + m_waypoint.right * 1.0f;
                        break;
                    default:
                        m_agent.destination = m_waypoint.position;
                        break;
                }
            }
            else
            {
                if (m_covertarget != null)
                {
                    m_target = m_covertarget;
                }
                if (peek != 0)
                {
                    if (m_target != null)
                    {
                        AimAngle(m_target, false);
                    }
                    m_agent.destination = m_cover.position + m_cover.right * peek * 2.0f;
                    peekDur -= 1000.0f * Time.deltaTime;
                    if (peekDur <= 0.0f)
                    {
                        m_agent.destination = m_cover.position;

                        peek = 0;

                        //target wasn't seen
                        if (!m_monitor.FinishMonitor())
                        {
                            ExitCover();
                        }
                        else
                        {
                            peekCooldown = 1000.0f;
                        }
                    }
                }
                else if (crouchPeek)
                {
                    if (m_target != null)
                    {
                        AimAngle(m_target, false);
                        if (m_monitor.IsMonitoring())
                        {
                            m_monitor.UpdateMonitor(m_target);
                        }
                    }
                    crouchPeekDur -= 1000.0f * Time.deltaTime;
                    if (crouchPeekDur <= 0.0f)
                    {
                        crouchPeek = false;

                        if (!m_monitor.FinishMonitor())
                        {
                            ExitCover();
                        }
                        else
                        {
                            crouchPeekCooldown = 2500.0f;
                        }
                    }
                }
                else
                {
                    m_agent.destination = m_cover.position;
                    if (inCover > 0.0f && Vector3.Distance(transform.position, m_cover.position) < 4.0f)
                    {
                        CoverSpot checkSpot = m_cover.GetComponent<CoverSpot>();
                        if (checkSpot != null)
                        {
                            if (checkSpot.IsShootable())
                            {
                                if (m_covertarget != null || checkSpot.IsLow())
                                {
                                    inCover += 1000.0f * Time.deltaTime;
                                    CoverShoot();
                                }
                                //holding = true;
                            }
                        }
                        else
                        {
                            inCover = 0.0f;
                        }
                        inCover -= 1000.0f * Time.deltaTime;
                        if (inCover <= 0.0f)
                        {
                            ExitCover();

                            if (regroup)
                            {
                                regroup = false;
                                Regroup();
                            }
                        }
                    }
                }
            }
        }

        Walk(Vector3.Distance(transform.position, prevPos) * 50.0f);
        prevPos = transform.position;

        if (!hasOrder && m_target == null)
        {
            CornerCheck();
        }

        if (holding && !takingCover && !stacking)
        {
            m_agent.destination = transform.position;
        }

        hasOrder = false;
    }

    public void SetObjective(Transform set)
    {
        m_objective = set;
    }

    public bool Defusing()
    {
        return (m_goalstate == (int)GoalState.GOAL_OBJECTIVE && m_objective != null);
    }

    void Defend(Transform position)
    {
        m_waypoint = position;
    }

    public void SetSquad(int squad)
    {
        squadNumber = squad;
    }

    void Walk(float distance)
    {
        if(Mathf.Abs(distance) <= 1.0f)
        {
            //walkCycle = 0.0f;
            if(leftLeg.rotation.eulerAngles.x > 1.0f && leftLeg.rotation.eulerAngles.x < 100.0f)
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
        if(walkCycle > 25.0f)
        {
            alt *= -1;
            walkCycle -= 75.0f;
        }

        leftLeg.RotateAround(transform.position, leftLeg.right, distance * alt);
        if(leftLeg.rotation.eulerAngles.x > 35.0f && leftLeg.rotation.eulerAngles.x < 100.0f)
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

    public void CheckBack()
    {
        if (m_target == null)
        {
            checking = false;
            checkTimer = 0.0f;
            if (m_view.transform.localRotation.eulerAngles.y < 180.0f)
            {
                m_view.RotateAround(transform.position, Vector3.up, 270.0f * Time.deltaTime);
            }
            if (m_view.transform.localRotation.eulerAngles.y > 180.0f)
            {
                m_view.RotateAround(transform.position, Vector3.up, -270.0f * Time.deltaTime);
            }
        }
    }

    public void CheckLeft()
    {
        if (m_target == null)
        {
            checking = true;
            dir = -1;
            checkTimer = 500.0f;
        }
    }

    public void CheckRight()
    {
        if (m_target == null)
        {
            checking = true;
            dir = 1;
            checkTimer = 500.0f;
        }
    }

    public void Follow(Transform toFollow)
    {
        if (!takingCover)
        {
            m_waypoint = toFollow;
            holding = false;
        }
    }

    public void SetWaypoint(Transform newpoint, int state)
    {
        waitSync = false;
        flashWait = 0.0f;
        isFlashing = false;
        holding = false;
        m_goalstate = state;
        m_waypoint = newpoint;
        order_override = 3500.0f;
        ExitCover();
        stacking = false;
        doneStacking = false;
        pos = 0;

        foreach(SquadMember squad in m_squad)
        {
            if(squad != null)
            {
                squad.order_override = 3500.0f;
                squad.ExitCover();
                squad.stacking = false;
                squad.doneStacking = false;
            }
        }
    }

    void Hold()
    {
        holding = true;
    }

    void Regroup()
    {
        takingCover = false;
        inCover = 0.0f;
        holding = false;
    }

    void CornerCheck()
    {
        Vector3 side = transform.TransformDirection(Vector3.right) * 6.5f;
        Vector3 forward = transform.TransformDirection(Vector3.forward) * 1.5f;
        Vector3 origin = transform.position + forward + transform.up * 0.75f;

        //corner checking
        if (!checking)
        {
            if (m_view.transform.localRotation.eulerAngles.y > 1.0f && m_view.transform.localRotation.eulerAngles.y < 150.0f)
            {
                m_view.RotateAround(transform.position, Vector3.up, -450.0f * Time.deltaTime);
            }
            if (m_view.transform.localRotation.eulerAngles.y < 359.0f && m_view.transform.localRotation.eulerAngles.y > 150.0f)
            {
                m_view.RotateAround(transform.position, Vector3.up, 450.0f * Time.deltaTime);
            }

            if (recheck > 0)
            {
                recheck -= 1000.0f * Time.deltaTime;
                if (recheck <= 0)
                {
                    lastCheck = 0;
                }
            }

            if (checkCooldown > 0.0f)
            {
                checkCooldown -= 1000.0f * Time.deltaTime;
            }
            
            if (checkCooldown <= cooldown - checkTime)
            {
                //fire off hitscans
                if (!Physics.Raycast(origin, transform.right, 6.5f) && ((lastCheck != 1 && checkCooldown <= 0.0f) || rightSafe > 100.0f))
                {
                    dir = 1;
                    lastCheck = dir;
                    checking = true;
                    checkTimer = checkTime;
                    rightSafe = 0.0f;
                }
                else if (!Physics.Raycast(origin, -transform.right, 6.5f) && ((lastCheck != -1 && checkCooldown <= 0.0f) || leftSafe > 100.0f))
                {
                    dir = -1;
                    lastCheck = dir;
                    checking = true;
                    checkTimer = checkTime;
                    leftSafe = 0.0f;
                }
            }
        }
        else if (checking)
        {
            if (isLeader)
            {
                int squadCount = 0;

                foreach (SquadMember squad in m_squad)
                {
                    if (squad != null)
                    {
                        squadCount++;
                    }
                }

                if (squadCount == 1)
                {
                    foreach (SquadMember squad in m_squad)
                    {
                        if (squad != null)
                        {
                            squad.checking = true;
                            squad.checkTimer = checkTimer * 2.5f;
                            squad.dir = -dir;
                        }
                    }
                }
            }

            checkTimer -= 1000.0f * Time.deltaTime;

            if (checkTimer <= 0.0f)
            {
                checking = false;
                checkCooldown = cooldown;
                checkTimer = 0.0f;
                recheck = 3000.0f;
            }

            if (dir == 1)
            {
                if (m_view.transform.localRotation.eulerAngles.y < 90.0f || m_view.transform.localRotation.eulerAngles.y > 200.0f)
                {
                    m_view.RotateAround(transform.position, Vector3.up, 270.0f * Time.deltaTime);
                }

                if (m_view.transform.localRotation.eulerAngles.y > 90.0f && m_view.transform.localRotation.eulerAngles.y < 150.0f)
                {
                    m_view.RotateAround(transform.position, Vector3.up, -270.0f * Time.deltaTime);
                }
            }
            else if (dir == -1)
            {
                if (m_view.transform.localRotation.eulerAngles.y > 270.0f || m_view.transform.localRotation.eulerAngles.y < 100.0f)
                {
                    m_view.RotateAround(transform.position, Vector3.up, -270.0f * Time.deltaTime);
                }

                if (m_view.transform.localRotation.eulerAngles.y > 150.0f && m_view.transform.localRotation.eulerAngles.y < 270.0f)
                {
                    m_view.RotateAround(transform.position, Vector3.up, 270.0f * Time.deltaTime);
                }
            }
        }

        //base corner checking
        if (Physics.Raycast(origin, transform.right, 6.5f))
        {
            if ((!checking || dir != 1) && rightSafe < 1000.0f) rightSafe += 1000.0f * Time.deltaTime;
            Debug.DrawRay(origin, side, Color.green, 0);
        }
        else
        {
            Debug.DrawRay(origin, side, Color.red, 0);
        }

        if (Physics.Raycast(origin, -transform.right, 6.5f))
        {
            if ((!checking || dir != -1) && leftSafe < 1000.0f) leftSafe += 1000.0f * Time.deltaTime;
            Debug.DrawRay(origin, -side, Color.green, 0);
        }
        else
        {
            Debug.DrawRay(origin, -side, Color.red, 0);
        }
    }

    void AimAngle(Transform target, bool force)
    {
        Quaternion oldRot = m_view.localRotation;
        m_view.LookAt(target);

        Quaternion newRot = m_view.localRotation;
        m_view.localRotation = oldRot;

        newRot = Quaternion.Euler(0.0f, newRot.eulerAngles.y, 0.0f);

        m_view.localRotation = Quaternion.Lerp(oldRot, newRot, 7.0f * Time.deltaTime);

        RaycastHit hit;
        //raycast towards them
        Vector3 dir = m_view.forward;
        Physics.Raycast(transform.position + transform.up * 0.8f, dir, out hit);
        Debug.DrawRay(transform.position + transform.up * 0.8f, dir * 20.0f);
        if ((hit.collider != null && (hit.collider.tag == "Enemy" || hit.collider.tag == "Dummy")) || force)
        {
            if (crouched)
            {
                //don't shoot your own cover
                Physics.Raycast(transform.position - transform.up * 0.5f, dir, out hit);
                if (hit.collider != null && (hit.collider.tag == "Enemy" || hit.collider.tag == "Dummy"))
                {
                    m_gun[gunOut].Shoot();
                    inCombat = 3000.0f;
                }
            }

            else
            {
                m_gun[gunOut].Shoot();
                inCombat = 3000.0f;
            }
        }
        //m_view.LookAt(target);
    }

    public void RefreshCover()
    {
        ExitCover();
        TakeCover();
    }

    public void TakeCover()
    {
        //find cover
        if(!takingCover && !stacking && order_override <= 0.0f)
        {
            //m_oldpoint = m_waypoint;

            //get nearest cover
            int layerMask = 1 << 9;

            Collider[] coverSpots = Physics.OverlapSphere(transform.position, 30.0f, layerMask, QueryTriggerInteraction.Collide);
            float value = 200000.0f;

            foreach(Collider spot in coverSpots)
            {
                CoverSpot thisSpot = spot.GetComponent<CoverSpot>();

                if (!thisSpot.InUse())
                {
                    UnityEngine.AI.NavMeshPath testPath = new UnityEngine.AI.NavMeshPath();
                    m_agent.CalculatePath(spot.transform.position, testPath);

                    float tempDist = 0.0f;
                    Vector3 prevCorner = testPath.corners[0];

                    for(int i = 1; i < testPath.corners.Length; i++)
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

                    float newDanger = thisSpot.GetEnemyDanger();

                    float shootable = 0.0f;
                    if(thisSpot.IsLow())
                    {
                        shootable = 40.0f;
                    }

                    float closeTarget = 0.0f;
                    if(m_covertarget != null)
                    {
                        closeTarget = Vector3.Distance(spot.transform.position, m_covertarget.position) * 15.0f;
                    }

                    float newValue = (tempDist * 15.0f) + newDanger - shootable + closeTarget;

                    if (newValue < value)
                    {
                        //if(isLeader) Debug.Log(tempDist + " " + newDanger + " " + newValue);
                        m_cover = spot.transform;
                        value = newValue;
                    }
                }
            }

            CoverSpot checkSpot = m_cover.GetComponent<CoverSpot>();
            if(checkSpot != null)
            {
                checkSpot.SetUse(true);
                checkSpot.SetOwner(transform);
                inCover = 1500.0f;
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
            Uncrouch();
            peek = 0;
            crouchPeek = false;
            peekCooldown = 500.0f;
            crouchPeekCooldown = 500.0f;
            peekDur = 0.0f;
            crouchPeekDur = 0.0f;
            //m_waypoint = m_oldpoint;
        }
    }

    public void ThrowGrenade()
    {
        //find cover with enemies
        Transform tempSpot = null;
        if (!stacking && order_override <= 0.0f)
        {
            //get nearest cover
            int layerMask = 1 << 9;

            Collider[] coverSpots = Physics.OverlapSphere(transform.position, 30.0f, layerMask, QueryTriggerInteraction.Collide);
            float value = 0.0f;

            foreach (Collider spot in coverSpots)
            {
                CoverSpot thisSpot = spot.GetComponent<CoverSpot>();

                float newValue = thisSpot.GetEnemyGrenade();

                if (newValue > value)
                {
                    tempSpot = spot.transform;
                    value = newValue;
                }
            }

            if(tempSpot != null)
            {
                Debug.DrawLine(tempSpot.transform.position, tempSpot.transform.position + tempSpot.transform.up * value * 0.2f, Color.magenta);
            }
        }
    }

    public void SetTarget(Transform target)
    {
        m_target = target;
        if(m_target != null)
        {
            m_covertarget = m_target;
        }

        /*foreach (SquadMember squad in m_squad)
        {
            if (squad != null)
            {
                if(squad.m_target == null)
                {
                    squad.m_target = m_target;
                }
            }
        }*/
    }

    public Transform GetTarget()
    {
        if(m_covertarget != null)
        {
            return m_covertarget;
        }
        if(m_target != null)
        {
            return m_target;
        }
        return null;
    }

    public void EnterDoor(Transform door, int side, int state)
    {
        Debug.Log("attempting to enter door");
        if (isLeader)
        {
            m_doorstate = state;
            m_doorside = side;

            if(takingCover)
            {
                takingCover = false;
                inCover = 0.0f;

                CoverSpot checkSpot = m_cover.GetComponent<CoverSpot>();
                if (checkSpot != null)
                {
                    checkSpot.SetUse(false);
                    checkSpot.SetOwner(null);
                    holding = false;
                }
            }

            stacking = true;
            //m_oldpoint = m_waypoint;
            //m_waypoint = door;

            m_agent.destination = door.position + door.forward * side;
            m_agent.destination += door.right * 2.5f;

            m_door = door;

            int stackLeft = 0;
            int stackRight = 1;
            int neg = -1;

            foreach (SquadMember squad in m_squad)
            {
                if (squad != null)
                {
                    int thisStack = 0;
                    if (neg == -1)
                    {
                        stackLeft++;
                        thisStack = stackLeft;
                    }
                    else
                    {
                        stackRight++;
                        thisStack = stackRight;
                    }

                    squad.stacking = true;

                    if (squad.takingCover)
                    {
                        squad.takingCover = false;
                        squad.inCover = 0.0f;

                        CoverSpot checkSpot = squad.m_cover.GetComponent<CoverSpot>();
                        if (checkSpot != null)
                        {
                            checkSpot.SetUse(false);
                            checkSpot.SetOwner(null);
                            squad.holding = false;
                        }

                        squad.m_door = door;

                        //squad.m_waypoint = squad.m_oldpoint;
                    }

                    squad.m_agent.destination = door.position + door.forward * side;
                    squad.m_agent.destination += door.right * (2.5f + (1.0f * thisStack)) * neg;

                    neg *= -1;
                }
            }
        }
    }

    public void Damage(int damage, Transform source)
    {
        if (health <= 0) return;
        inCombat += 500.0f;
        if (source != null && source.tag != "SquadMan")
        {
            m_target = source;
            m_covertarget = source;
            //AimAngle(m_target);
        }

        AddDanger(damage * 5);
        if(inCover > 0.0f)
        {
            if (inCover < 2000.0f)
            {
                inCover += 500.0f;
            }
        }
        health -= damage;

        if(health <= 0)
        {
            Instantiate(ragdoll, transform.position, transform.rotation);
            health = 0;
            Hold();
            ExitCover();
            if (isLeader)
            {
                foreach (SquadMember squad in m_squad)
                {
                    if (squad != null)
                    {
                        squad.isLeader = true;
                        squad.m_waypoint = m_waypoint;
                        break;
                    }
                }
            }

            //Destroy(gameObject);
            Crouch();
            holding = true;

            gameObject.SetActive(false);
        }
    }

    public void AddDanger(float add)
    {
        danger += add;
    }

    void CoverShoot()
    {
        if (m_covertarget != null)
        {
            m_target = m_covertarget;
        }

        bool isLeftCorner = false;
        bool isRightCorner = false;
        bool isLow = false;
        CoverSpot checkSpot = m_cover.GetComponent<CoverSpot>();
        if (checkSpot != null)
        {
            isLeftCorner = checkSpot.IsLeftCorner();
            isRightCorner = checkSpot.IsRightCorner();
            isLow = checkSpot.IsLow();

            if(isLow)
            {
                if (crouchPeekCooldown > 0.0f)
                {
                    crouchPeekCooldown -= 1000.0f * Time.deltaTime;
                }
                if(!m_gun[gunOut].Reloading() && !crouchPeek && crouchPeekCooldown <= 0.0f)
                {
                    //uncrouch
                    Uncrouch();
                    crouchPeek = true;
                    crouchPeekDur = 300.0f;
                    m_monitor.Monitor(m_target);
                }
                if(!crouchPeek)
                {
                    Crouch();
                }
            }
            else
            {
                AimAngle(m_target, false);

                if (peekCooldown > 0.0f)
                {
                    peekCooldown -= 1000.0f * Time.deltaTime;
                }

                if (!m_gun[gunOut].Reloading() && peek == 0 && peekCooldown <= 0.0f)
                {
                    Debug.Log("peeking");
                    m_monitor.Monitor(m_covertarget);
                    peekDur = 400.0f;
                    if (isLeftCorner)
                    {
                        peek = -1;
                    }
                    else
                    {
                        peek = 1;
                    }
                }
            }
        }
    }

    void Crouch()
    {
        if(!crouched)
        {
            crouched = true;
            holding = true;

            foreach(Transform child in transform)
            {
                child.Translate(new Vector3(0.0f, -0.75f, 0.0f));
            }

            selected.Translate(new Vector3(0.0f, 0.75f, 0.0f));

            rightLeg.gameObject.SetActive(false);
            leftLeg.gameObject.SetActive(false);
            crouchLegs.gameObject.SetActive(true);

            GetComponent<CapsuleCollider>().height = 1.0f;
        }
    }

    void Uncrouch()
    {
        if(crouched)
        {
            crouched = false;
            holding = false;

            foreach (Transform child in transform)
            {
                child.Translate(new Vector3(0.0f, 0.75f, 0.0f));
            }

            selected.Translate(new Vector3(0.0f, -0.75f, 0.0f));

            rightLeg.gameObject.SetActive(true);
            leftLeg.gameObject.SetActive(true);
            crouchLegs.gameObject.SetActive(false);

            GetComponent<CapsuleCollider>().height = 3.0f;
        }
    }

    public bool GetCrouch()
    {
        return crouched;
    }

    void SwapGun()
    {
        if(gunOut == 0)
        {
            //animation
            gunOut = 1;
            arms[0].SetActive(false);
            arms[1].SetActive(true);

            //m_gun[0].gameObject.SetActive(false);
            m_gun[1].gameObject.SetActive(true);

            m_gun[1].gameObject.transform.localPosition = origPos[1];
            m_gun[1].gameObject.transform.localRotation = origRot[1];

            m_gun[0].gameObject.transform.position = pistolHolster.position;
            m_gun[0].gameObject.transform.rotation = pistolHolster.rotation;
        }
        else
        {
            //animation
            gunOut = 0;
            arms[0].SetActive(true);
            arms[1].SetActive(false);

            m_gun[0].gameObject.SetActive(true);
            //m_gun[1].gameObject.SetActive(false);

            m_gun[0].gameObject.transform.localPosition = origPos[0];
            m_gun[0].gameObject.transform.localRotation = origRot[0];

            m_gun[1].gameObject.transform.position = rifleHolster.position;
            m_gun[1].gameObject.transform.rotation = rifleHolster.rotation;
        }
    }

    public int GetHealth()
    {
        return health;
    }

    public void SetPos(int newPos)
    {
        pos = newPos;
    }

    public bool GetCover()
    {
        return (inCover > 0.0f || takingCover);
    }

    public void SetOrder(bool newOrder)
    {
        hasOrder = newOrder;
    }

    public void SetSelect(bool set)
    {
        if(selected != null)
        {
            selected.gameObject.SetActive(set);
        }
        else
        {
            Debug.Log(Time.time);
        }
    }

    public void SetSelectColor(Color set)
    {
        set.r += 0.15f;
        set.g += 0.15f;
        set.b += 0.15f;
        set.a *= 1.5f;
        outerRing.GetComponent<Renderer>().material.SetColor("_Color", set);
        outerRing.GetComponent<Renderer>().material.SetColor("_EmissionColor", set);
        set *= 0.1f;
        set.a *= 2f;
        innerRing.GetComponent<Renderer>().material.SetColor("_Color", set);
        innerRing.GetComponent<Renderer>().material.SetColor("_EmissionColor", set);
    }

    public int GetGoalState()
    {
        return m_goalstate;
    }

    public int GetDoorState()
    {
        return m_doorstate;
    }

    public void SetSync(bool set)
    {
        waitSync = set;
    }

    public bool GetSync()
    {
        return waitSync;
    }

    public bool Reloading()
    {
        return m_gun[gunOut].Reloading();
    }

    public void TacticalReload()
    {
        if (inCombat <= 0.0f)
        {
            if (m_gun[gunOut].CheckAmmoPercentage() < 50.0f)
            {
                m_gun[gunOut].Reload();
            }
        }
    }

    public float GetAmmoPercentage()
    {
        return m_gun[gunOut].CheckAmmoPercentage();
    }
}