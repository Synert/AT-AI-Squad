using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashbang : MonoBehaviour {

    public Transform flash;
    public Transform sound;
    float fuseTime = 1500.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        fuseTime -= 1000.0f * Time.deltaTime;
        if(fuseTime <= 0.0f)
        {
            Instantiate(flash, transform.position, Quaternion.identity);
            Transform newSound = Instantiate(sound, transform.position, Quaternion.identity);
            newSound.GetComponent<SoundPlayer>().PlaySound(25.0f, 50, false);
            Explode();
        }
	}

    public float GetFuse()
    {
        return fuseTime;
    }

    void Explode()
    {

        int layerMask = 1 << 11;

        Collider[] enemies = Physics.OverlapSphere(transform.position, 25.0f, layerMask);
        foreach (Collider enemy in enemies)
        {
            Debug.DrawRay(transform.position, (enemy.transform.position - transform.position), Color.yellow, 5.0f);
            Vector3 origin = transform.position + Vector3.up;
            float dist = Vector3.Distance(origin, enemy.transform.position);

            //only hit inside the room
            if (!Physics.Raycast(origin, (enemy.transform.position - origin),
                    dist, (1 << 8 | 1 << 12)))
            {
                Debug.DrawLine(origin, enemy.transform.position, Color.red, 5.0f);
                Enemy enemyComp = enemy.GetComponentInChildren<Enemy>();
                if (enemyComp != null)
                {
                    Debug.Log("Blinding enemy for " + (75.0f - dist * 2.0f) * 25.0f);
                    enemyComp.Blind((75.0f - dist * 3f) * 25.0f);
                }
            }
        }

        Destroy(gameObject);
    }
}
