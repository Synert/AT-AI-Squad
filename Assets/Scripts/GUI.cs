using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUI : MonoBehaviour {

    Canvas m_canvas;
    public Transform squadSelect;
    public SquadManager manager;
    public Transform baseMenu, playerMenu, doorMenu, moveMenu;
    PlayerController player;
    Toggle[] playerButtons;
    Text[] playerText;
    Transform RMB, MMB;
    Text ammoCounter;
    Transform currentMenu;
    bool open = false;
    bool openDelay = false;
    int state = (int)States.CLOSED;
    Transform tempDoor;
    bool tempSync;

    enum States
    {
        CLOSED,
        SELECT_SQUAD,
        SELECT_PLAYER,
        MOVE,
        MOVE_ALT,
        DOOR,
        DOOR_ALT,
        OBJECTIVE
    }

	// Use this for initialization
	void Start () {
        player = FindObjectOfType<PlayerController>();
        playerText = new Text[3];
        m_canvas = GetComponentInChildren<Canvas>();
        playerButtons = playerMenu.GetComponentsInChildren<Toggle>();
        playerText[0] = playerButtons[0].GetComponentInChildren<Text>();
        playerText[1] = playerButtons[1].GetComponentInChildren<Text>();
        playerText[2] = playerButtons[2].GetComponentInChildren<Text>();
        ammoCounter = m_canvas.GetComponentInChildren<Text>();

        foreach (Transform child in gameObject.GetComponentsInChildren<Transform>())
        {
            if(child.name == "RMB")
            {
                RMB = child;
            }
            if (child.name == "MMB")
            {
                MMB = child;
            }
        }

        Close();
    }
	
	// Update is called once per frame
	void Update () {
        ammoCounter.text = "Ammo: " + player.GetAmmo() + "/" + player.GetMaxAmmo();
        if(player.GetReloading())
        {
            ammoCounter.text = "Reloading...";
        }

        if (manager.Registered())
        {
            for (int i = 0; i < playerButtons.Length; i++)
            {
                bool dead = !(manager.GetPlayerSquadHealth(i) > 0);

                if (dead)
                {
                    playerButtons[i].isOn = false;
                    playerText[i].text = "DEAD";
                }

                manager.PlayerSelect(i, playerButtons[i].isOn);

                foreach (Transform child in playerButtons[i].transform)
                {
                    if (child.name == "Health")
                    {
                        //do thing
                        int health = manager.GetPlayerSquadHealth(i);

                        if (health >= 75)
                        {
                            child.GetComponent<Image>().color = new Color((100 - health) / 25.0f, (150 - health) / 75.0f, 0);
                        }
                        else if (health >= 50)
                        {
                            child.GetComponent<Image>().color = new Color(1, (health / 50.0f) - 0.5f, 0);
                        }
                        else
                        {
                            child.GetComponent<Image>().color = new Color((health / 50.0f) + 0.5f, health / 100.0f, 0);
                        }
                        // break;
                    }
                    if (child.name == "Dead")
                    {
                        //do thing
                        child.GetComponent<Image>().gameObject.SetActive(dead);
                        //break;
                    }
                }
            }
        }

        if(open)
        {
            if (openDelay)
            {
                if (Input.GetMouseButtonUp(2))
                {
                    switch (state)
                    {
                        case (int)States.MOVE:
                            CommandMove();
                            Close();
                            break;
                        case (int)States.DOOR:
                            CommandDoor();
                            Close();
                            break;
                        case (int)States.OBJECTIVE:
                            ObjectiveUse();
                            break;
                        case (int)States.CLOSED:
                            //shouldn't ever happen
                            Close();
                            break;
                    }
                }
                else if (Input.GetMouseButtonUp(1))
                {
                    switch (state)
                    {
                        case (int)States.MOVE:
                            CommandCover();
                            Close();
                            break;
                        case (int)States.DOOR:
                            PlayerDoorAlt();
                            break;
                        case (int)States.OBJECTIVE:
                            ObjectiveDefend();
                            break;
                        case (int)States.CLOSED:
                            //shouldn't ever happen
                            Close();
                            break;
                    }
                }
            }
            else
            {
                openDelay = true;
            }
        }
        else
        {
            if (Input.GetMouseButtonUp(1))
            {
                manager.SyncBreach();
            }
        }
    }

    public void Open()
    {
        //m_canvas.gameObject.SetActive(true);
        if(open)
        {
            Close();
        }
        open = true;
    }

    public void Close()
    {
        open = false;
        openDelay = false;
        manager.CancelPlace();
        //m_canvas.gameObject.SetActive(false);
        if (currentMenu != null)
        {
            Destroy(currentMenu.gameObject);
        }
        state = (int)States.CLOSED;

        Text[] text = baseMenu.GetComponentsInChildren<Text>();
        text[0].text = "MOVE";
        text[2].text = "REGROUP";

        if(manager.CheckSync())
        {
            text[1].text = "SYNC BREACH";
            RMB.gameObject.SetActive(true);
        }
        else
        {
            text[1].text = "";
            RMB.gameObject.SetActive(false);
        }

        MMB.gameObject.SetActive(true);
    }

    public void PlayerMove()
    {
        Open();
        state = (int)States.MOVE;
        //currentMenu = Instantiate(playerMove, m_canvas.transform);

        Text[] text = baseMenu.GetComponentsInChildren<Text>();
        text[0].text = "MOVE";
        text[1].text = "COVER";
        text[2].text = "CANCEL";

        RMB.gameObject.SetActive(true);
        MMB.gameObject.SetActive(true);

        //Button[] buttons = currentMenu.GetComponentsInChildren<Button>();
        //buttons[0].onClick.AddListener(ButtonPlayerMove);
    }

    public void PlayerObjective()
    {
        Open();
        state = (int)States.OBJECTIVE;
        //currentMenu = Instantiate(playerMove, m_canvas.transform);

        Text[] text = baseMenu.GetComponentsInChildren<Text>();
        text[0].text = "DEFUSE";
        text[1].text = "DEFEND";
        text[2].text = "CANCEL";

        RMB.gameObject.SetActive(true);
        MMB.gameObject.SetActive(true);

        //Button[] buttons = currentMenu.GetComponentsInChildren<Button>();
        //buttons[0].onClick.AddListener(ButtonPlayerMove);
    }

    public void CommandMove()
    {
        manager.PlayerSquadMove(-1, (int)SquadMember.GoalState.GOAL_STAY, false);
        Close();
    }

    public void CommandCover()
    {
        manager.PlayerSquadMove(-1, (int)SquadMember.GoalState.GOAL_COVER, false);
        Close();
    }

    public void CommandGrenade()
    {
        //manager.Grenade();
        Close();
    }

    public void PlayerDoor()
    {
        Open();
        state = (int)States.DOOR;
        //currentMenu = Instantiate(playerDoor, m_canvas.transform);

        Text[] text = baseMenu.GetComponentsInChildren<Text>();
        text[0].text = "KICK";
        text[1].text = "BREACH MENU";
        text[2].text = "CANCEL";

        RMB.gameObject.SetActive(true);
        MMB.gameObject.SetActive(true);

        //Button[] buttons = currentMenu.GetComponentsInChildren<Button>();
        //buttons[0].onClick.AddListener(ButtonPlayerDoor);
    }

    public void PlayerDoorAlt()
    {
        Open();
        state = (int)States.DOOR_ALT;
        currentMenu = Instantiate(doorMenu, m_canvas.transform);

        Button[] buttons = currentMenu.GetComponentsInChildren<Button>();

        buttons[0].onClick.AddListener(CommandBreach);
        buttons[1].onClick.AddListener(CommandFlash);
        //buttons[2].onClick.AddListener(CommandPeek);

        tempSync = false;

        currentMenu.GetComponentInChildren<Toggle>().onValueChanged.AddListener((value) => {
                SetSync(value);
            }
         );

        Text[] text = baseMenu.GetComponentsInChildren<Text>();
        text[0].text = "";
        text[1].text = "";
        text[2].text = "CANCEL";

        RMB.gameObject.SetActive(false);
        MMB.gameObject.SetActive(false);

        //Button[] buttons = currentMenu.GetComponentsInChildren<Button>();
        //buttons[0].onClick.AddListener(ButtonPlayerDoor);
    }

    public void CommandDoor()
    {
        manager.PlayerSquadMove((int)SquadMember.DoorState.DOOR_KICK, (int)SquadMember.GoalState.GOAL_STAY, false);
        Close();
    }

    public void CommandBreach()
    {
        manager.PlayerSquadMove((int)SquadMember.DoorState.DOOR_BREACH, (int)SquadMember.GoalState.GOAL_STAY, tempSync);
        Close();
    }

    public void CommandFlash()
    {
        manager.PlayerSquadMove((int)SquadMember.DoorState.DOOR_FLASH, (int)SquadMember.GoalState.GOAL_STAY, tempSync);
        Close();
    }

    public void CommandPeek()
    {
        manager.PlayerSquadMove((int)SquadMember.DoorState.DOOR_PEEK, (int)SquadMember.GoalState.GOAL_STAY, tempSync);
        Close();
    }

    public void ObjectiveUse()
    {
        manager.PlayerSquadMove(-1, (int)SquadMember.GoalState.GOAL_OBJECTIVE, false);
        Close();
    }

    public void ObjectiveDefend()
    {
        manager.PlayerSquadMove(-1, (int)SquadMember.GoalState.GOAL_DEFEND, false);
        Close();
    }

    public bool IsOpen()
    {
        return open;
    }

    public void SetText(int player, string set)
    {
        playerText[player].text = set;
    }

    void SetSync(bool set)
    {
        tempSync = set;
        Debug.Log(set);
    }
}
