using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public Transform fleshImpact;
    public Transform wallImpact;
    public Transform sound;
    Transform parent;
    float lifeSpan = 0.0f;
    int damage = 5;

	// Use this for initialization
	void Start () {
        Rigidbody m_rigidbody = GetComponent<Rigidbody>();

        m_rigidbody.AddForce(transform.parent.forward * 2000.0f);
	}
	
	// Update is called once per frame
	void Update () {
        lifeSpan += 1000.0f * Time.deltaTime;
        if(lifeSpan >= 3000.0f)
        {
            Destroy(gameObject);
        }
	}

    public void SetDamage(int set)
    {
        damage = set;
    }

    public void SetOwner(Transform owner)
    {
        parent = owner;
    }

    private void OnCollisionEnter(Collision col)
    {
        bool spawnedImpact = false;
        if(col.transform.tag == "SquadMan")
        {
            if (col.transform.name == "Player")
            {
                //
                Instantiate(fleshImpact, transform.position, transform.parent.rotation);
                spawnedImpact = true;
            }
            else
            {
                SquadMember squad = col.gameObject.GetComponent<SquadMember>();
                squad.Damage(damage * Random.Range(1, 3), parent);
                Instantiate(fleshImpact, transform.position, transform.parent.rotation);
                spawnedImpact = true;
            }
        }
        if (col.transform.tag == "Enemy")
        {
            Enemy enemy = col.gameObject.GetComponent<Enemy>();
            enemy.Damage(damage * Random.Range(2, 5));
            Instantiate(fleshImpact, transform.position, transform.parent.rotation);
            spawnedImpact = true;
        }
        if (col.transform.tag == "Dummy")
        {
            Dummy dummy = col.gameObject.GetComponent<Dummy>();
            dummy.Damage(damage * Random.Range(2, 4));
            Instantiate(fleshImpact, transform.position, transform.parent.rotation);
            spawnedImpact = true;
        }
        if(!spawnedImpact)
        {
            Instantiate(wallImpact, transform.position, transform.parent.rotation);
        }

        Transform newSound = Instantiate(sound, transform.position, Quaternion.identity);
        newSound.GetComponent<SoundPlayer>().PlaySound(5.0f, 50, false);

        Destroy(gameObject);
    }
}
