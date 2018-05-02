using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    public Transform enemy;

    // Use this for initialization
    void Start()
    {
        Transform newEnemy = Instantiate(enemy, transform.position, transform.rotation);
        newEnemy.GetComponent<Enemy>().health = Random.Range(40, 80);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
