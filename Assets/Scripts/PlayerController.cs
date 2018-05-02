using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour {

    CharacterController m_controller;
    public SquadMember[] m_squad = new SquadMember[3];
    bool[] following = new bool[3];

    //gun stuff
    Gun[] m_gun;
    GameObject[] arms = new GameObject[2];
    int gunOut = 1;
    Vector3[] origPos = new Vector3[2];
    Quaternion[] origRot = new Quaternion[2];
    Transform pistolHolster, rifleHolster;
    Transform m_torso;

    //walking 'animation'
    Vector3 prevPos;
    float walkCycle;
    int alt = 1;
    Transform leftLeg, rightLeg, crouchLegs;
    bool crouched = false;

    //select ring
    Transform selected;

    //mouse scroll
    float prevScroll = 0.0f;

    Camera m_cam;

    public float speed = 4.0f;
    Vector3 moveDirection;

    // Use this for initialization
    void Start () {
        m_cam = FindObjectOfType<Camera>();
        m_controller = GetComponent<CharacterController>();
        m_gun = GetComponentsInChildren<Gun>();
        int gunCount = 0;
        foreach (Gun gun in m_gun)
        {
            gun.SetOwner(transform);
            origPos[gunCount] = gun.transform.localPosition;
            origRot[gunCount] = gun.transform.localRotation;
            gunCount++;
        }

        foreach (Transform child in gameObject.GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.tag == "ViewCone")
            {
                m_torso = child;
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
            if (child.gameObject.name == "RifleHolster")
            {
                rifleHolster = child;
            }
            if (child.gameObject.name == "PistolHolster")
            {
                pistolHolster = child;
            }
            if (child.gameObject.name == "Selected")
            {
                selected = child;
            }
        }

        //hide the set of arms not being used
        arms[gunOut].SetActive(true);
        arms[(1 - gunOut)].SetActive(false);

        //hide crouch legs
        crouchLegs.gameObject.SetActive(false);

        if (gunOut == 1)
        {
            m_gun[0].gameObject.transform.position = pistolHolster.position;
            m_gun[0].gameObject.transform.rotation = pistolHolster.rotation;
        }
        else
        {
            m_gun[1].gameObject.transform.position = rifleHolster.position;
            m_gun[1].gameObject.transform.rotation = rifleHolster.rotation;
        }

        prevPos = transform.position;
    }
	
	// Update is called once per frame
	void Update () {
        Walk(Vector3.Distance(transform.position, prevPos) * 50.0f);
        prevPos = transform.position;

        if(Input.GetKey(KeyCode.LeftControl))
        {
            Crouch();
        }
        else
        {
            Uncrouch();
        }

        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        //moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= speed;
        if (crouched) moveDirection /= 2.0f;

        Vector3 oldPos = transform.position;
        transform.LookAt(transform.position + moveDirection);
        transform.position = oldPos;
        m_torso.position = new Vector3(oldPos.x, m_torso.position.y, oldPos.z);

        m_controller.Move(moveDirection * Time.deltaTime);
        m_controller.Move(new Vector3(0.0f, -10.0f) * Time.deltaTime);

        //look at the cursor
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {
            Vector3 newPos = hit.point;
            newPos.y = m_torso.position.y;
            oldPos = m_torso.position;
            m_torso.transform.LookAt(newPos);
            m_torso.position = oldPos;
        }

        if(Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            m_gun[gunOut].Shoot();
        }

        if (prevScroll > 0.0f)
        {
            prevScroll -= 1000.0f * Time.deltaTime;
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0.0f && prevScroll <= 0.0f && !m_gun[gunOut].Reloading())
        {
            SwapGun();
            prevScroll = 500.0f;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            m_gun[gunOut].Reload();
        }

        m_cam.transform.position = new Vector3(transform.position.x, 35.0f, transform.position.z);

        int squad_count = 0;
        int squad_int = 0;
        int internal_int = 0;
        for(int i = 0; i < 3; i++)
        {
            if(following[i])
            {
                squad_count++;
            }
        }

        foreach(SquadMember squad in m_squad)
        {
            if (squad != null && !squad.GetCover() && following[internal_int])
            {
                Debug.Log(internal_int);
                squad.Follow(transform);
                squad.SetOrder(false);

                switch (squad_int)
                {
                    case 0:
                        if (squad_count == 1)
                        {
                            //squad.CheckBack();
                            //squad.hasOrder = true;
                            squad.SetPos(3);
                        }
                        else if (squad_count == 3)
                        {
                            squad.CheckLeft();
                            squad.SetPos(1);
                        }
                        break;
                    case 1:
                        if (squad_count == 2)
                        {
                            squad.CheckBack();
                            squad.SetOrder(true);
                            squad.SetPos(3);
                        }
                        else if (squad_count == 3)
                        {
                            squad.CheckRight();
                            squad.SetPos(2);
                        }
                        break;
                    default:
                        //check behind
                        squad.CheckBack();
                        squad.SetOrder(true);
                        squad.SetPos(3);
                        break;
                }
                squad_int++;
            }
           internal_int++;
        }
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

    void SwapGun()
    {
        if (gunOut == 0)
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

    public void SetFollowing(int which, bool set)
    {
        following[which] = set;
    }

    public int GetAmmo()
    {
        return m_gun[gunOut].CheckAmmo();
    }

    public int GetMaxAmmo()
    {
        return m_gun[gunOut].magMax;
    }

    public bool GetReloading()
    {
        return m_gun[gunOut].Reloading();
    }

    void Crouch()
    {
        if (!crouched)
        {
            crouched = true;

            foreach (Transform child in transform)
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
        if (crouched)
        {
            crouched = false;

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
}
