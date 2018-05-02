using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class SquadManager : MonoBehaviour {

    public Transform pointMarker;
    PlayerController player;
    SquadMember[] playerSquad = new SquadMember[3];
    bool[] playerSelect = new bool[3] { true, true, true };
    Transform[] playerLastPoint = new Transform[3];
    SquadMember[,] squad = new SquadMember[4,4];
    Transform[] lastPoint = new Transform[4];
    Color[] colors = new Color[] { Color.red, Color.blue, Color.green };
    string[] names = new string[] { "Alpha", "Beta", "Charlie", "Delta", "Echo" };
    int squadCount = 0;
    int squadSelect = 0;
    bool setpoint = false;
    float drawCooldown = 0.0f;
    GUI m_gui;
    Camera m_cam;
    Color tempColor;
    Vector3 tempPos;
    Transform tempDoor, tempObjective;
    int tempSide = 2;
    bool placing = false;
    bool ignoreInput = false;
    bool setPlayerSquad = false;

    bool prevClosed = false;
    bool canInput = false;

    Vector3 mousePos;

    // Use this for initialization
    void Start () {
        m_gui = transform.parent.GetComponentInChildren<GUI>();
        m_cam = FindObjectOfType<Camera>();
        player = FindObjectOfType<PlayerController>();
    }
	
	// Update is called once per frame
	void Update () {

        if (!setPlayerSquad && playerSquad[0] != null)
        {
            player.m_squad = playerSquad;
            setPlayerSquad = true;
        }

        canInput = prevClosed;
        if(drawCooldown > 0.0f)
        {
            drawCooldown -= 1000.0f * Time.deltaTime;
        }
        if (placing && drawCooldown <= 0.0f)
        {
            PlayerDrawPath(false, tempPos, true);
            drawCooldown = 15.0f;
        }
        if (!m_gui.IsOpen())
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                PlayerFollow();
            }
            if (Input.GetMouseButtonUp(2) && canInput)
            {
                if (gameObject.GetComponent<LineRenderer>().positionCount != 0)
                {
                    PlayerDrawPath(true, Vector3.zero, false);
                    drawCooldown = 15.0f;
                    placing = true;
                }
            }
            else if (Input.GetMouseButton(2) && !placing && drawCooldown <= 0.0f)
            {
                PlayerDrawPath(false, Vector3.zero, false);
                drawCooldown = 15.0f;
            }
        }
        else if(Input.GetKeyDown(KeyCode.Q))
        {
            m_gui.Close();
            placing = false;
            gameObject.GetComponent<LineRenderer>().positionCount = 0;
        }

        if(setPlayerSquad)
        {
            int lowest = 0;
            float lastAmmo = 100.0f;
            bool canReload = true;
            for(int i = 0; i < 3; i++)
            {
                if(playerSquad[i].Reloading())
                {
                    canReload = false;
                }
                if(playerSquad[i].GetAmmoPercentage() < lastAmmo)
                {
                    lastAmmo = playerSquad[i].GetAmmoPercentage();
                    lowest = i;
                }
            }
            if(canReload)
            {
                playerSquad[lowest].TacticalReload();
            }
        }

        prevClosed = !m_gui.IsOpen();

        //CameraPosition();
    }

    public int GetPlayerSquadHealth(int which)
    {
        if (playerSquad[which] != null)
        {
            return playerSquad[which].GetHealth();
        }
        return 0;
    }

    void CameraPosition()
    {
        Bounds squadBounds = new Bounds();
        squadBounds.Encapsulate(playerSquad[0].transform.position);
        squadBounds.Encapsulate(playerSquad[1].transform.position);
        squadBounds.Encapsulate(playerSquad[2].transform.position);

        m_cam.transform.position = new Vector3(squadBounds.center.x, 30.0f + squadBounds.size.magnitude * 0.5f, squadBounds.center.z);
    }

    public void RegisterSquad(Transform leader)
    {
        squad[squadCount, 0] = leader.GetComponent<SquadMember>();
        squad[squadCount, 0].SetSquad(squadCount);

        int count = 1;
        foreach (SquadMember member in squad[squadCount, 0].m_squad)
        {
            squad[squadCount, count] = member;
            member.SetSquad(squadCount);
            count++;
        }

        squadCount++;
    }

    void SquadMove(Transform location)
    {
        int count = 0;
        while (squad[squadSelect, count] != null)
        {
            if (squad[squadSelect, count].isLeader)
            {
                squad[squadSelect, count].SetWaypoint(location, (int)SquadMember.GoalState.GOAL_STAY);
                break;
            }
            count++;
        }
    }

    void DrawPath(bool setPath)
    {
        UnityEngine.AI.NavMeshPath testPath = new UnityEngine.AI.NavMeshPath();
        UnityEngine.AI.NavMeshAgent agent = squad[squadSelect, 0].GetComponent<UnityEngine.AI.NavMeshAgent>();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            agent.CalculatePath(hit.point, testPath);

            Vector3 prevCorner = testPath.corners[0];
            Vector3 spawnPos = prevCorner;
            float lifeSpan = 2.0f;

            for (int i = 1; i < testPath.corners.Length; i++)
            {
                Vector3 curCorner = testPath.corners[i];
                Vector3 dir = Vector3.Normalize((curCorner + Vector3.up) - (prevCorner + Vector3.up));
                for (float j = 0.0f; j < Vector3.Distance(prevCorner, curCorner); j += 1.0f)
                {
                    if (!Physics.Raycast(prevCorner + (j - 1) * dir, dir, out hit, 1.0f, (1 << 12)))
                    {
                        if (!setPath)
                        {
                            PointMarker thisPoint = Instantiate(pointMarker, prevCorner + Vector3.Normalize(curCorner - prevCorner) * j, Quaternion.identity).GetComponent<PointMarker>();
                            thisPoint.SetLifespan(0.005f);
                            thisPoint.SetColorMult(lifeSpan);
                            thisPoint.SetColor(colors[squadSelect]);
                            lifeSpan += 0.15f;
                        }
                        else if(j >= (Vector3.Distance(prevCorner, curCorner) - 1.0f) && i >= testPath.corners.Length - 1)
                        {
                            PointMarker thisPoint = Instantiate(pointMarker, prevCorner + Vector3.Normalize(curCorner - prevCorner) * j, Quaternion.identity).GetComponent<PointMarker>();
                            thisPoint.SetColumn(true, colors[squadSelect]);
                            if(lastPoint[squadSelect] != null)
                            {
                                Destroy(lastPoint[squadSelect].gameObject);
                            }
                            lastPoint[squadSelect] = thisPoint.transform;
                            SquadMove(thisPoint.transform);
                        }
                    }
                    else
                    {
                        i = 90000;
                        if(setPath)
                        {
                            PointMarker thisPoint = Instantiate(pointMarker, prevCorner + Vector3.Normalize(curCorner - prevCorner) * (j - 1.0f), Quaternion.identity).GetComponent<PointMarker>();
                            thisPoint.SetColumn(true, colors[squadSelect]);
                            if (lastPoint[squadSelect] != null)
                            {
                                Destroy(lastPoint[squadSelect].gameObject);
                            }
                            lastPoint[squadSelect] = thisPoint.transform;
                            SquadMove(thisPoint.transform);
                        }
                        break;
                    }
                }
                prevCorner = curCorner;
            }
        }
    }

    //all the player's squad functions
    int PlayerGetLeader(int squadMember)
    {
        if (playerSquad[squadMember] == null)
        {
            return -1;
        }
        if (playerSquad[squadMember].isLeader)
        {
            return squadMember;
        }
        for (int i = 0; i < 2; i++)
        {
            if (playerSquad[squadMember].m_squad[i] != null)
            {
                if (playerSquad[squadMember].m_squad[i].isLeader)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (playerSquad[j].transform == playerSquad[squadMember].m_squad[i].transform)
                        {
                            return j;
                        }
                    }
                }
            }
        }
        return -1;
    }

    public void PlayerSquadMove(int breach, int state, bool sync)
    {
        gameObject.GetComponent<LineRenderer>().positionCount = 0;
        placing = false;
        PointMarker thisPoint = Instantiate(pointMarker, tempPos, Quaternion.identity).GetComponent<PointMarker>();
        thisPoint.SetColumn(true, tempColor);
        PlayerRemovePoint(playerSelect);
        for (int k = 0; k < 3; k++)
        {
            if (playerSelect[k])
            {
                playerLastPoint[k] = thisPoint.transform;
                playerSquad[k].SetOrder(false);
            }
        }

        PlayerUpdateLeaders();

        int firstSelect = -1;
        for (int i = 0; i < 3; i++)
        {
            if (playerSelect[i])
            {
                if (firstSelect == -1)
                {
                    firstSelect = i;
                }
                m_gui.SetText(i, "MOVING");
                if(breach != -1)
                {
                    SquadMember.DoorState thisState = (SquadMember.DoorState)breach;
                    string stateString = thisState.ToString();

                    stateString = stateString.Replace("DOOR_", "");
                    if(sync)
                    {
                        stateString = "SYNC " + stateString;
                    }
                    else
                    {
                        stateString += "ING DOOR";
                    }
                    m_gui.SetText(i, stateString);
                }
                player.SetFollowing(i, false);
            }
        }
        if (PlayerGetLeader(firstSelect) == -1)
        {
            return;
        }
        else
        {
            if (state == (int)SquadMember.GoalState.GOAL_OBJECTIVE)
            {
                playerSquad[PlayerGetLeader(firstSelect)].SetObjective(tempObjective);
            }
            playerSquad[PlayerGetLeader(firstSelect)].SetWaypoint(thisPoint.transform, state);
            if(breach != -1) 
            {
                playerSquad[PlayerGetLeader(firstSelect)].EnterDoor(tempDoor, tempSide, breach);
                playerSquad[PlayerGetLeader(firstSelect)].SetSync(sync);
            }
        }
    }

    void PlayerUpdateLeaders()
    {
        int num_unselected = -1;
        int num_selected = -1;

        bool[] leaderStatus = new bool[3] { false, false, false };
        SquadMember[,] newSquad = new SquadMember[3, 2];
        int third = 0;

        for (int i = 0; i < 3; i++)
        {
            Debug.Log(Time.time + " " + playerSelect[i] + " " + i + " " + PlayerGetLeader(i));
            if (!playerSelect[i])
            {
                if (num_unselected == -1)
                {
                    if ((PlayerGetLeader(i) != -1 && playerSelect[PlayerGetLeader(i)]) || PlayerGetLeader(i) == -1)
                    {
                        if(PlayerGetLeader(i) != -1)
                        {
                            playerSquad[i].SetWaypoint(playerSquad[PlayerGetLeader(i)].m_waypoint, playerSquad[PlayerGetLeader(i)].GetGoalState());
                        }
                        //playerSquad[i].isLeader = true;
                        leaderStatus[i] = true;
                        num_unselected = i;
                        newSquad[i, 0] = null;
                        newSquad[i, 1] = null;
                    }
                    else if(PlayerGetLeader(i) != -1 && PlayerGetLeader(i) == i)
                    {
                        leaderStatus[i] = true;
                        newSquad[i, 0] = null;
                        newSquad[i, 1] = null;
                    }
                    else if(PlayerGetLeader(i) != -1)
                    {
                        newSquad[i, 0] = playerSquad[PlayerGetLeader(i)];
                        newSquad[i, 1] = null;
                    }
                }
                else
                {
                    if (PlayerGetLeader(i) != -1 && (playerSelect[PlayerGetLeader(i)] || PlayerGetLeader(i) == num_selected))
                    {
                        newSquad[i, 0] = playerSquad[num_unselected];
                        newSquad[num_unselected, 0] = playerSquad[i];
                    }
                    else
                    {
                        leaderStatus[i] = true;
                        newSquad[i, 0] = null;
                    }
                    newSquad[i, 1] = null;
                }
            }
            else
            {
                if (num_selected == -1)
                {
                    leaderStatus[i] = true;
                    newSquad[i, 0] = null;
                    newSquad[i, 1] = null;
                    num_selected = i;
                }
                else
                {
                    newSquad[i, 0] = playerSquad[num_selected];
                    if (newSquad[num_selected, 0] == null)
                    {
                        newSquad[num_selected, 0] = playerSquad[i];
                        third = i;
                    }
                    else
                    {
                        //all three were selected
                        newSquad[num_selected, 1] = playerSquad[i];
                        newSquad[i, 1] = newSquad[num_selected, 0];
                        newSquad[third, 1] = playerSquad[i];
                    }
                }
            }
        }

        for(int i = 0; i < 2; i++)
        {
            playerSquad[0].m_squad[i] = newSquad[0, i];
            playerSquad[1].m_squad[i] = newSquad[1, i];
            playerSquad[2].m_squad[i] = newSquad[2, i];
        }

        playerSquad[0].isLeader = leaderStatus[0];
        playerSquad[1].isLeader = leaderStatus[1];
        playerSquad[2].isLeader = leaderStatus[2];

        for (int i = 0; i < 3; i++)
        {
            Debug.Log(Time.time + " " + playerSelect[i] + " " + i + " " + PlayerGetLeader(i));
        }
    }

    void PlayerRemovePoint(bool[] toRemove)
    {
        int[] toKeep = new int[3];
        int count = 0;
        Transform[] toCheck = new Transform[3];
        for (int i = 0; i < 3; i++)
        {
            if (toRemove[i])
            {
                if (playerLastPoint[i] != null)
                {
                    toCheck[count] = playerLastPoint[i];
                    count++;
                }
            }
        }
        for (int i = 0; i < 3; i++)
        {
            if (!toRemove[i])
            {
                for (int j = 0; j < count; j++)
                {
                    if (playerLastPoint[i] != null)
                    {
                        if (playerLastPoint[i] == toCheck[j])
                        {
                            toKeep[j]++;
                        }
                    }
                }
            }
        }

        for (int j = 0; j < count; j++)
        {
            if (toKeep[j] == 0)
            {
                Destroy(toCheck[j].gameObject);
            }
        }
    }

    void PlayerDrawPath(bool setPath, Vector3 goalPos, bool drawToGoal)
    {
        int selected = 0;
        int firstSelect = -1;

        Color newColor = Color.clear;
        for (int i = 0; i < 3; i++)
        {
            if (playerSelect[i])
            {
                selected++;
                newColor += colors[i];
                if (firstSelect == -1)
                {
                    firstSelect = i;
                }
            }
        }

        if (selected == 0)
        {
            return;
        }

        //Debug.Log(Time.time + " " + setPath + " " + goalPos + " " + drawToGoal);

        newColor /= (float)selected;

        UnityEngine.AI.NavMeshPath testPath = new UnityEngine.AI.NavMeshPath();
        UnityEngine.AI.NavMeshAgent agent = playerSquad[firstSelect].GetComponent<UnityEngine.AI.NavMeshAgent>();
        LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits;
        RaycastHit tempHit;
        if (Physics.Raycast(ray, out tempHit) || drawToGoal)
        {
            Vector3 drawTo;
            if(drawToGoal)
            {
                drawTo = goalPos;
            }
            else
            {
                drawTo = tempHit.point;
            }
            agent.CalculatePath(drawTo, testPath);

            lineRenderer.positionCount = 0;

            if (testPath.corners.Length == 0)
            {
                placing = false;
                return;
            }

            //lineRenderer.SetPositions(testPath.corners);
            //lineRenderer.SetPosition(lineRenderer.positionCount, drawTo);
            lineRenderer.SetWidth(0.5f, 0.5f);
            lineRenderer.material.SetColor("_EmissionColor", newColor);
            int corners = 0;

            Vector3 prevCorner = testPath.corners[0];
            Vector3 spawnPos = prevCorner;
            bool doorPlaced = false;

            lineRenderer.positionCount++;
            lineRenderer.SetPosition(corners, prevCorner);
            corners++;

            for (int i = 1; i < testPath.corners.Length; i++)
            {
                Vector3 curCorner = testPath.corners[i];
                Vector3 dir = Vector3.Normalize((curCorner + Vector3.up) - (prevCorner + Vector3.up));

                hits = Physics.RaycastAll(prevCorner, dir, Vector3.Distance(prevCorner, curCorner), (1 << 12)).OrderBy(h => h.distance).ToArray();

                if (hits.Length == 0)
                {
                    lineRenderer.positionCount++;
                    lineRenderer.SetPosition(corners, curCorner);
                    corners++;
                    if (i >= testPath.corners.Length - 1 && setPath)
                    {
                        tempPos = curCorner;
                        tempColor = newColor;
                        if (doorPlaced)
                        {
                            m_gui.PlayerDoor();
                        }
                        else
                        {
                            m_gui.PlayerMove();
                        }
                        placing = true;
                    }
                }
                else
                {
                    foreach (RaycastHit hit in hits)
                    {
                        if(hit.collider.tag == "Objective")
                        {
                            i = 90000;
                            lineRenderer.positionCount++;
                            lineRenderer.SetPosition(corners, hit.point);
                            corners++;
                            if (setPath)
                            {
                                tempObjective = hit.collider.transform;
                                tempPos = hit.point + new Vector3(0.0f, -1.0f);
                                tempColor = newColor;
                                placing = true;
                                m_gui.PlayerObjective();
                                gameObject.GetComponent<LineRenderer>().positionCount = 0;
                            }
                            break;
                        }
                        if (doorPlaced && hit.collider.transform != tempDoor)
                        {
                            i = 90000;
                            lineRenderer.positionCount++;
                            lineRenderer.SetPosition(corners, hit.point);
                            corners++;
                            if (setPath)
                            {
                                tempPos = hit.point + new Vector3(0.0f, -1.0f);
                                tempColor = newColor;
                                placing = true;
                                m_gui.PlayerDoor();
                                gameObject.GetComponent<LineRenderer>().positionCount = 0;
                            }
                            break;
                        }
                        else if(!doorPlaced)
                        {
                            if(i >= testPath.corners.Length - 1 && hits.Length == 1)
                            {
                                lineRenderer.positionCount++;
                                lineRenderer.SetPosition(corners, curCorner);
                                corners++;
                                if(setPath)
                                {
                                    tempPos = curCorner;
                                    tempColor = newColor;
                                    placing = true;
                                    m_gui.PlayerDoor();
                                    gameObject.GetComponent<LineRenderer>().positionCount = 0;
                                }
                            }
                            else
                            {
                                lineRenderer.positionCount++;
                                lineRenderer.SetPosition(corners, hit.point);
                                corners++;
                            }
                            doorPlaced = true;
                            if (setPath)
                            {
                                tempDoor = hit.collider.transform;
                                Vector3 doorPos = hit.point + new Vector3(0.0f, -1.0f);
                                if (Vector3.Distance(doorPos, tempDoor.position + tempDoor.forward * 2.0f) >
                                    Vector3.Distance(doorPos, tempDoor.position - tempDoor.forward * 2.0f))
                                {
                                    tempSide = -2;
                                }
                                else
                                {
                                    tempSide = 2;
                                }
                            }
                        }
                    }
                }
                prevCorner = curCorner;
            }
        }
    }

    public void PlayerRegisterSquad(Transform leader)
    {
        playerSquad[0] = leader.GetComponent<SquadMember>();
        playerSquad[0].SetSquad(squadCount);
        playerSquad[0].SetSelect(true);
        playerSquad[0].SetSelectColor(colors[0]);

        int count = 1;
        foreach(SquadMember member in playerSquad[0].m_squad)
        {
            playerSquad[count] = member;
            member.SetSquad(squadCount);
            playerSquad[count].SetSelect(true);
            playerSquad[count].SetSelectColor(colors[count]);
            count++;
        }

        squadCount++;
    }

    public void PlayerSelect(int select, bool set)
    {
        playerSelect[select] = set;
        playerSquad[select].SetSelect(set);
    }

    void PlayerFollow()
    {
        PlayerUpdateLeaders();
        PlayerRemovePoint(playerSelect);
        for(int i = 0; i < 3; i++)
        {
            if(playerSelect[i])
            {
                playerSquad[i].isLeader = false;
                playerSquad[i].m_squad[0] = null;
                playerSquad[i].m_squad[1] = null;
                playerSquad[i].SetWaypoint(player.transform, (int)SquadMember.GoalState.GOAL_STAY);
                player.SetFollowing(i, true);
                m_gui.SetText(i, "FOLLOWING");
            }
        }
    }

    public bool[] GetSelect()
    {
        return playerSelect;
    }

    public void CancelPlace()
    {
        placing = false;
    }

    public void SyncBreach()
    {
        for(int i = 0; i < 3; i++)
        {
            playerSquad[i].SetSync(false);
        }
    }

    public bool Registered()
    {
        return setPlayerSquad;
    }

    public bool CheckSync()
    {
        for(int i = 0; i < 3; i++)
        {
            if(playerSquad[i] != null && playerSquad[i].GetSync())
            {
                return true;
            }
        }
        return false;
    }

}
