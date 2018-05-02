using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointMarker : MonoBehaviour {

    GameObject m_column, m_sphere;
    float lifeSpan = 2.0f;
    float initSpan = 2.0f;
    float m_mult = 1.0f;
    bool remove = true;
    Renderer m_rend;
    Color m_emit;

	// Use this for initialization
	void Awake () {
		foreach(Transform child in transform)
        {
            if(child.name == "Column")
            {
                m_column = child.gameObject;
            }
            if (child.name == "Sphere")
            {
                m_sphere = child.gameObject;
            }
        }

        m_rend = m_sphere.GetComponent<Renderer>();
        m_emit = m_rend.material.GetColor("_EmissionColor");
        m_column.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        if (remove)
        {
            if (lifeSpan <= 0.0f)
            {
                Destroy(gameObject);
            }
            m_rend.material.SetColor("_EmissionColor", m_emit * (lifeSpan / initSpan) * m_mult);
            lifeSpan -= 1.0f * Time.deltaTime;
        }
	}

    public void SetLifespan(float span)
    {
        lifeSpan = span;
        initSpan = span;
    }

    public void SetColorMult(float mult)
    {
        m_mult = mult;
    }

    public void SetColor(Color color)
    {
        m_emit = color;
        m_emit.a = 0.2f;
        m_emit *= 0.1f;
    }

    public void SetColumn(bool set, Color color)
    {
        m_column.SetActive(set);
        remove = !set;

        if(set)
        {
            m_rend.material.SetColor("_EmissionColor", color * 8.0f);
        }
    }
}
