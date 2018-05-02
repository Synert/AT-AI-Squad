using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RadialMenu : MonoBehaviour {

    Button[] m_buttons;

	// Use this for initialization
	void Awake () {
        m_buttons = GetComponentsInChildren<Button>();
    }
	
	// Update is called once per frame
	void Update () {

	}

    public void Setup(GUI gui)
    {
        m_buttons[0].onClick.AddListener(gui.CommandBreach);
        m_buttons[1].onClick.AddListener(gui.CommandFlash);
        m_buttons[2].onClick.AddListener(gui.CommandPeek);
    }

}
