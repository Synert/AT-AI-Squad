using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LevelManager : MonoBehaviour {

    const int size = 6;

    public List<Transform> levelChunks;
    List<Transform> leftChunks, rightChunks, upChunks, downChunks;
    public Transform firstChunk, leftWall, rightWall, topWall, bottomWall;
    bool[,] placed = new bool[size,size];
    float chunkSize = 14.0f;
    bool first = true;
    bool doOnce = false;

    enum DIRECTION
    {
        RIGHT,
        LEFT,
        UP,
        DOWN
    }


	// Use this for initialization
	void Start () {
        leftChunks = new List<Transform>();
        rightChunks = new List<Transform>();
        upChunks = new List<Transform>();
        downChunks = new List<Transform>();
        foreach (Transform chunk in levelChunks)
        {
            LevelChunk thisChunk = chunk.GetComponent<LevelChunk>();
            if (thisChunk.Right())
            {
                rightChunks.Add(chunk);
            }
            if (thisChunk.Left())
            {
                leftChunks.Add(chunk);
            }
            if (thisChunk.Up())
            {
                upChunks.Add(chunk);
            }
            if (thisChunk.Down())
            {
                downChunks.Add(chunk);
            }
        }

        for(int i = 0; i < size - 1; i++)
        {
            //place chunks around the border
            //placed[i, 0] = true;
            //placed[i, (size - 1)] = true;
            //placed[0, i] = true;
            //placed[(size - 1), i] = true;

            Transform leftChunk = Instantiate(leftWall);
            Transform rightChunk = Instantiate(leftWall);
            Transform upChunk = Instantiate(bottomWall);
            Transform downChunk = Instantiate(bottomWall);

            Vector3 origin = new Vector3((-(int)Mathf.Round(size / 2)) * chunkSize, 0.0f, (-(int)Mathf.Round(size / 2)) * chunkSize);

            leftChunk.transform.position = new Vector3(0, 2.5f, i * chunkSize) + origin;
            rightChunk.transform.position = new Vector3((size - 1) * chunkSize, 2.5f, i * chunkSize) + origin;
            upChunk.transform.position = new Vector3(i * chunkSize, 2.5f, 0) + origin;
            downChunk.transform.position = new Vector3(i * chunkSize, 2.5f, (size - 1) * chunkSize) + origin;
        }

        PlaceChunk((int)Mathf.Round(size / 2), (int)Mathf.Round(size / 2), DIRECTION.UP);

        //FindObjectOfType<CoverManager>().RemakeCover();

    }
	
	// Update is called once per frame
	void Update () {
		if(!doOnce)
        {
            FindObjectOfType<CoverManager>().RemakeCover();
            doOnce = true;
        }
	}

    void PlaceChunk(int x, int y, DIRECTION dir)
    {
        //grab a random chunk
        if (placed[x, y])
        {
            //return;
        }

        Transform thisChunk = null;
        if (first)
        {
            thisChunk = Instantiate(firstChunk);
            first = false;
        }
        else
        {
            switch (dir)
            {
                case DIRECTION.RIGHT:
                    thisChunk = Instantiate(rightChunks[Random.Range(0, rightChunks.Count)]);
                    break;
                case DIRECTION.LEFT:
                    thisChunk = Instantiate(leftChunks[Random.Range(0, leftChunks.Count)]);
                    break;
                case DIRECTION.UP:
                    thisChunk = Instantiate(upChunks[Random.Range(0, upChunks.Count)]);
                    break;
                case DIRECTION.DOWN:
                    thisChunk = Instantiate(downChunks[Random.Range(0, downChunks.Count)]);
                    break;
            }
        }

        Vector3 origin = new Vector3((-(int)Mathf.Round(size / 2)) * chunkSize, 0.0f, (-(int)Mathf.Round(size / 2)) * chunkSize);

        thisChunk.transform.position = new Vector3(x * chunkSize, 2.5f, y * chunkSize) + origin;

        placed[x, y] = true;
        LevelChunk levelChunk = thisChunk.GetComponent<LevelChunk>();
        if (levelChunk.Right())
        {
            if (x + 1 < (size - 1) && !placed[x + 1, y])
            {
                PlaceChunk(x + 1, y, DIRECTION.LEFT);
            }
        }
        if (levelChunk.Left())
        {
            if (x - 1 >= 0 && !placed[x - 1, y])
            {
                PlaceChunk(x - 1, y, DIRECTION.RIGHT);
            }
        }
        if (levelChunk.Up())
        {
            if (y + 1 < (size - 1) && !placed[x, y + 1])
            {
                PlaceChunk(x, y + 1, DIRECTION.DOWN);
            }
        }
        if (levelChunk.Down())
        {
            if (y - 1 >= 0 && !placed[x, y - 1])
            {
                PlaceChunk(x, y - 1, DIRECTION.UP);
            }
        }
    }
}
