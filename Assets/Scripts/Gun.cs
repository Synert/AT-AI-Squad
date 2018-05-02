using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {

    public float reloadTime = 1750.0f;
    public float fireDelay = 200.0f;
    float fireCooldown = 0.0f;
    float reloading = 0.0f;
    public int magMax = 30;
    int magCur = 0;
    public int damage = 5;

    Light flash;
    float flashLength = 0.0f;
    bool flashInit = false;

    public Transform magazine;
    public Transform bullet;
    public Transform sound;

    Transform parent;
    Transform view;

	// Use this for initialization
	void Start () {
        magCur = magMax;
        flash = GetComponentInChildren<Light>();
        flash.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
        if(flashLength > 0.0f)
        {
            flash.enabled = true;
            if(flashInit)
            {
                flash.intensity -= 400.0f * Time.deltaTime;
                flash.range -= 100.0f * Time.deltaTime;
                flashLength -= 1000.0f * Time.deltaTime;
            }
            else
            {
                flashInit = true;
            }
            if(flashLength <= 0.0f)
            {
                flash.enabled = false;
            }
        }
		if(fireCooldown > 0.0f)
        {
            fireCooldown -= 1000.0f * Time.deltaTime;
        }

        if(reloading > 0.0f)
        {
            reloading -= 1000.0f * Time.deltaTime;

            if(reloading <= 0.0f)
            {
                magCur = magMax;
            }
        }
     }

    public void SetOwner(Transform owner)
    {
        parent = owner;

        foreach (Transform child in parent)
        {
            if (child.gameObject.tag == "ViewCone")
            {
                view = child;
                break;
            }
        }
    }

    public void Reload()
    {
        if(reloading <= 0.0f && magCur < magMax)
        {
            reloading = reloadTime;

            //spawn a dropped mag
            Transform newMag = Instantiate(magazine, parent.transform.position, Quaternion.identity);
            newMag.GetComponentInChildren<Rigidbody>().AddForce(-view.transform.right * Random.Range(100f, 300f));
        }
    }

    public bool Reloading()
    {
        return (reloading > 0.0f);
    }

    public void Shoot()
    {
        if(fireCooldown <= 0.0f && reloading <= 0.0f)
        {
            if (magCur > 0)
            {
                //play fire sound, spawn bullet
                if (sound != null)
                {
                    Transform newSound = Instantiate(sound, transform.position, Quaternion.identity);
                    newSound.GetComponent<SoundPlayer>().PlaySound(20.0f, 200, parent.tag == "Enemy");
                }
                flashLength = Random.Range(100.0f, 250.0f);
                flash.intensity = Random.Range(6.0f, 12.0f);
                flash.range = Random.Range(5.0f, 10.0f);
                flashInit = false;
                magCur--;
                fireCooldown = fireDelay;

                Quaternion newRot = Quaternion.Euler(view.transform.rotation.eulerAngles);

                Bullet newBullet = Instantiate(bullet, view.transform.position + view.forward * 1.75f + view.up * 0.65f, newRot).GetComponentInChildren<Bullet>();

                newBullet.SetOwner(parent);
                newBullet.SetDamage(damage);
            }
            else
            {
                //play click sound, start reload
                Reload();
            }
        }
    }

    public void ShootLow()
    {
        if (fireCooldown <= 0.0f && reloading <= 0.0f)
        {
            if (magCur > 0)
            {
                //play fire sound, spawn bullet
                flashLength = Random.Range(100.0f, 250.0f);
                flash.intensity = Random.Range(6.0f, 12.0f);
                flash.range = Random.Range(5.0f, 10.0f);
                flashInit = false;
                magCur--;
                fireCooldown = fireDelay;

                Quaternion newRot = Quaternion.Euler(view.transform.rotation.eulerAngles);

                Bullet newBullet = Instantiate(bullet, view.transform.position + view.forward * 1.75f, newRot).GetComponentInChildren<Bullet>();

                newBullet.SetOwner(parent);
                newBullet.SetDamage(damage);
            }
            else
            {
                //play click sound, start reload
                Reload();
            }
        }
    }

    public int CheckAmmo()
    {
        return magCur;
    }

    public float CheckAmmoPercentage()
    {
        return (magCur / magMax) * 100.0f;
    }

    public bool CanShoot()
    {
        return (fireCooldown <= 0.0f && reloading <= 0.0f);
    }
}
