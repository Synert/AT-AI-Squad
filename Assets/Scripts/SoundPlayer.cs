using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour {

    float lifeSpan = 2.0f;

	// Use this for initialization
	void Start () {
        //GetComponentInChildren<ParticleSystem>().Stop();
    }
	
	// Update is called once per frame
	void Update () {
        lifeSpan -= Time.deltaTime;
        if (lifeSpan <= 0.0f) Destroy(gameObject);
	}

    public void PlaySound(float radius, int danger, bool enemyOrigin)
    {
        if (enemyOrigin)
        {
            GetComponentInChildren<ParticleSystem>().Stop();
            return;
        }
        GetComponentInChildren<ParticleSystem>().startSize = radius * 2.0f;
        GetComponentInChildren<ParticleSystem>().Play();

        int layerMask = 1 << 11;

        Collider[] enemies = Physics.OverlapSphere(transform.position, radius, layerMask);
        foreach (Collider enemy in enemies)
        {
            Enemy enemyComp = enemy.GetComponentInChildren<Enemy>();
            if (enemyComp != null)
            {
                Debug.DrawRay(transform.position, (enemy.transform.position - transform.position), Color.yellow, 5.0f);
                Vector3 origin = transform.position + Vector3.up;
                float dist = Vector3.Distance(origin, enemy.transform.position);
                dist += radius;
                dist /= (radius * 2.0f);
                enemyComp.ReactSound(dist, danger, transform.position);
            }
        }
    }
}
