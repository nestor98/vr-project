using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetController : MonoBehaviour
{

    public GameObject player,
                      textPrefab;
    public ScoreCounter scoreManager;

    //public float maxHitSpeed = 10.0f;
    public List<float> speedsForScores; // Speed for each score 

    public string arrowTag, deathTag="death";

    // Scoring thresholds:
    public float badDistThreshold = 0.57f, // if dist>this, bad shot
                 goodDistThreshold = 0.2f; // else if dist > this, good shot, 
                                           // Otherwise, perfect shot

    public List<string> perfectHitText,
                        goodHitText,
                        badHitText;

    public AudioSource HitAudioSource;

    void Start()
    {
    }

    void Update()
    {

    }

    void OnCollisionEnter(Collision obj)
    {
        if (obj.gameObject.tag == arrowTag)
        {
            int score = GetScore(obj.gameObject); 
            float addedVel = speedsForScores[score];
            var rb = player.GetComponent<Rigidbody>();
            rb.velocity = new Vector3(0, Mathf.Max(rb.velocity.y + addedVel, addedVel), 0); // Never be negative
            string text = GetPhraseForScore(score); // Get the phrase for the score, currently always "terrible shot"
            ShowText(text);
            if (scoreManager)
            {
                scoreManager.AddScore(score);
            }
            float pitch = 1.0f;
            if (score == 0) {
                pitch = 1.2f;
            } else if (score == 1) {
                pitch = 1.0f;
            } else if (score == 2) {
                pitch = 0.8f;
            }
            HitAudioSource.pitch = pitch;
            HitAudioSource.Play();
        }
        else if (obj.gameObject.tag == deathTag)
        {
            //Debug.Log("Hit death plane"); // TODO: this does not work
            Destroy(this, 2f); // Destroy 2 secs after touching the death plane
        }
    }


    // Returns 0 for perfect, 1 for good, 2 for bad
    int GetScore(GameObject arrow)
    {
        // Distance to center in local xy plane:
        Vector3 localPos = transform.InverseTransformPoint(arrow.transform.position);
        localPos = Vector3.Scale(localPos, transform.localScale);
        float dist = new Vector2(localPos.x, localPos.y).magnitude;
        // Score:
        if (dist >= badDistThreshold) return 2;
        else if (dist >= goodDistThreshold) return 1;
        else return 0;
    }

    // Implemented following https://www.youtube.com/watch?v=lKEKTWK9efE
    void ShowText(string text)
    {
        if (textPrefab)
        {
            var textParent = Instantiate(textPrefab, transform.position, transform.rotation);
            textParent.GetComponentInChildren<TextMesh>().text = text;
            Destroy(textParent, 1.5f); // Destroy after 1 second
        }
    }

    // Gets a phrase according to the score (1: perfect, 0: terrible)
    string GetPhraseForScore(int score)
    {
        List<string> phrases;
        if (score == 0) phrases = perfectHitText;
        else if (score == 1) phrases = goodHitText;
        else phrases = badHitText;
        int idx = Random.Range(0, phrases.Count);
        return phrases[idx];
    }

}
