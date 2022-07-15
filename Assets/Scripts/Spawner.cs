using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Spawner : MonoBehaviour
{

    public GameObject targetPrefab, planeOfDeath,
                      uiCanvas, uiTargetIconPrefab;
    public AudioSource audioSource;
    // Pool of targets:
    public List<GameObject> targets;
    public int nTargets = 100;
    private int lowestTargetIdx = 0;

    public ScoreCounter scoreManager;


    public float updatePeriod = 2f; // only check targets every 2 secs
    private float currentUpdateCD = 0f;


    public GameObject player;

    public float heightVar = 5.0f; // height variation for a new target to spawn
    public float newTargetDist = 15.0f; // base distance for a new target to spawn
    public float targetDistanceSpawn; // If player.y > highestTarget.y - targetDistanceSpawn, a new one is spawned

    public float radius; // Radius of the cylinder


    public bool spawnUITargetIndicators = true;

    private Camera cam;


    // TODO (performance): make the list of targets a pool instead of instantiating them every time


    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;

        for (int i = targets.Count; i < nTargets; i++)
        {
            SpawnTarget();
        }
        if (nTargets != targets.Count)
        {
            Debug.Log("?????????????");
        }
    }

    // Update is called once per frame
    void Update()
    {
        currentUpdateCD -= Time.deltaTime;
        if (currentUpdateCD < 0f)
        {
            float minHeight = planeOfDeath.transform.position.y;
            UpdateTargetPool(minHeight);
            currentUpdateCD = updatePeriod;
        }
    }

    // Check which targets in the pool are lower than lowestHeight
    // move those up
    private void UpdateTargetPool(float lowestHeight)
    {
        int newLowestIdx = lowestTargetIdx;
        for (int i = 0; i < nTargets; i++)
        {
            int idx = (i + lowestTargetIdx) % nTargets;
            var target = targets[idx];
            if (target.transform.position.y < lowestHeight)
            {
                MoveTarget(target);
                newLowestIdx = (idx+1) % nTargets;
            }
            else break; // If the lowest is higher, the rest will also be 
        }
        lowestTargetIdx = newLowestIdx;
    }

    private int GetHighestTargetIdx()
    {
        int idx = lowestTargetIdx - 1; // the highest target idx is just before the lowest
        if (idx < 0) idx = nTargets - 1; // unless lowest = 0
        return idx;
    }

    private void MoveTarget(GameObject target)
    {
        int highestIdx = GetHighestTargetIdx();
        float highest = targets[highestIdx].transform.position.y;

        // Angle to spawn in the cylinder
        float th = Random.Range(0, 2f * Mathf.PI);

        // Position to spawn
        Vector3 pos = new Vector3(radius * Mathf.Cos(th),
                                  highest + newTargetDist + Random.Range(-heightVar, heightVar),
                                  radius * Mathf.Sin(th));
        // Instantiate the object

        target.transform.position = pos;
        target.transform.rotation = Quaternion.Euler(0, 90f - Mathf.Rad2Deg * th,0);
        target.SetActive(true);
    }

    // Returns true if a new target is needed
    // for now, if the player is higher than a threshold from the highest target
    private bool shouldSpawnTarget()
    {
        GameObject highestTarget = targets.Last(); // Highest target is always the last
        float highest = highestTarget.transform.position.y;
        return cam.transform.position.y > highest - targetDistanceSpawn;
    }

    private void SpawnTarget()
    {
        float highest = targets.Last().transform.position.y;

        // Angle to spawn in the cylinder
        float th = Random.Range(0, 2f * Mathf.PI);

        // Position to spawn
        Vector3 pos = new Vector3(radius * Mathf.Cos(th),
                                  highest + newTargetDist + Random.Range(-heightVar, heightVar),
                                  radius * Mathf.Sin(th));
        // Instantiate the object
        var newTarget = Instantiate(targetPrefab, pos, Quaternion.Euler(0, 90f - Mathf.Rad2Deg * th, 0));
        newTarget.SetActive(true);
        newTarget.transform.parent = transform;
        TargetController tc = newTarget.GetComponent<TargetController>();
        tc.player = player;
        tc.scoreManager = scoreManager;
        tc.HitAudioSource = audioSource;

        targets.Add(newTarget);

        // UI indicator:
        if (spawnUITargetIndicators)
        {
            var uiTarget = Instantiate(uiTargetIconPrefab);
            uiTarget.SetActive(true);
            uiTarget.transform.SetParent(uiCanvas.transform, false);
            var uiController = uiTarget.GetComponent<targetIndicatorController>();
            uiController.target = newTarget;
        }
    }
}
