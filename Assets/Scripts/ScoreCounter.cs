using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCounter : MonoBehaviour
{
    public GameObject player;

    public float updatePeriod = 0.1f; // update only once every 0.1 sec
    private float updateCD = 0f;

    private BowController bow;

    // If the player is going at x2speed, double points, etc.
    public float x2speed = 50f, x3speed = 100f, x4speed=200f;
    private Text uiMultiplier;
    private GameObject uiMultiplierObj;


    private Rigidbody playerRB;

    private Text text;
    private int currentScore = 0;




    // Stats:
    private float maxHeight = 0f, maxSpeed = 0f;
    private int[] shotQualityCount = { 0, 0, 0 };// {perfect, good, minimum}

    void Start()
    {
        uiMultiplierObj = transform.GetChild(0).gameObject;
        uiMultiplier = uiMultiplierObj.GetComponent<Text>();

        playerRB = player.GetComponent<Rigidbody>();
        text = GetComponent<Text>();
        currentScore = 0;
        SetScore();

        bow = player.GetComponentInChildren<BowController>();
    }

    void Update()
    {
        updateCD -= Time.deltaTime;
        if (updateCD < 0f)
        {
            SetScore();
            UpdateMaxHeight();
            UpdateMaxSpeed();

            updateCD = updatePeriod;
        }
    }


    private void UpdateMaxHeight()
    {
        float y = player.transform.position.y;
        if (y > maxHeight) maxHeight = y;
    }

    private void UpdateMaxSpeed()
    {
        float s = playerRB.velocity.y;
        if (s > maxSpeed) maxSpeed = s;
    }

    private void SetScore()
    {
        int mult = GetSpeedMultiplier();
        text.text = currentScore.ToString();
        SetUIMultiplier(mult);// text.text = text.text + "   x" + mult.ToString();
    }

    private void SetUIMultiplier(int mult)
    {
        if (mult>1)
        {
            uiMultiplierObj.SetActive(true);
            uiMultiplier.text = "x" + mult.ToString();
        }
        else uiMultiplierObj.SetActive(false);
    }


    public int GetShotCount()
    {
        return bow.GetShotCount();
    }

    public int GetHits()
    {
        int hits = 0;
        foreach (int count in shotQualityCount)
        {
            hits += count;
        }
        return hits;
    }
    public float GetMaxHeight()
    {
        return maxHeight;
    }
    public float GetMaxSpeed()
    {
        return maxSpeed;
    }


    public float GetAccuracy()
    {
        return (float)GetHits() / (float)GetShotCount();
    }

    public int GetHitsScored(int type)
    {
        return shotQualityCount[type];
    }

    public void AddScore(int score)
    {
        shotQualityCount[score]++;
        // target score goes from 0=perfect to 2=minimum
        score = (int)Mathf.Pow(10f, (3 - score)); // 10 100 1000
        currentScore += score * GetSpeedMultiplier();
        SetScore();
    }

    public int GetScore()
    {
        return currentScore;
    }

    // Give some extra score if the player was going fast
    private int GetSpeedMultiplier()
    {
        float speed = Mathf.Abs(playerRB.velocity.y);

        if (speed > x4speed)
        {
            return 4;
        }
        if (speed > x3speed)
        {
            return 3;
        }
        if (speed > x2speed)
        {
            return 2;
        }
        return 1;
    }

}
