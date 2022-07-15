using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement; // Reload

public class GameOverController : MonoBehaviour
{
    public ScoreCounter scoreCounter;
    public Text statsTableText, scoreText;


    public void UpdateStats()
    {
        scoreText.text = scoreCounter.GetScore().ToString();

        // Max height a bit nicer:
        float maxHeight = scoreCounter.GetMaxHeight(); // meters
        string maxHeightText;
        if (maxHeight > 1000f)
        { // km
            maxHeightText = (maxHeight / 1000f).ToString("F2") + " km";
        }
        else
        { // m
            maxHeightText = maxHeight.ToString("F2") + " m";
        }

        // Max speed: 
        float speed = scoreCounter.GetMaxSpeed();
        //string maxSpeedText = speed.ToString("F2") + " m/s  (" + (speed * 3.6f).ToString("F2") + " km/h)"; // Too long
        string maxSpeedText = (speed * 3.6f).ToString("F2") + " km/h";


        statsTableText.text = scoreCounter.GetHits() + "\n" +
                                scoreCounter.GetHitsScored(0) + "\n" +
                                scoreCounter.GetHitsScored(1) + "\n" +
                                scoreCounter.GetHitsScored(2) + "\n" +
                                (scoreCounter.GetAccuracy() * 100f).ToString("F0") + " %\n" +
                                maxHeightText + "\n" +
                                maxSpeedText;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Time.timeScale = 0.0001f;
        if (Input.GetMouseButtonDown(0))
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadSceneAsync(scene.name);
        }
    }
}
