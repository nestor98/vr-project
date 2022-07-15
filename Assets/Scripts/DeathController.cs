using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathController : MonoBehaviour
{

    public GameObject player, textPrefab;

    public GameObject gameOverScreen;

    public TimeManager timeManager; // Stop time on death


    public float accelerateAtHeight = 50f; // At 50m, the plane moves even if player doesnt move
    //public float
    public List<float> accelerationHeights, speedAtHeight;
    public int currHeightIdx = 0; // idx of the current height region


    public float maximumDistanceY = 20f;


    public float smoothTime = 0.3F; // Smoothing time for the movement
    private Vector3 velocity = Vector3.zero; // Current velocity towards the player

    private string playerTag = "player";

    // 1 sec before actually dying:
    private bool dead = false;
    private float timeToDie = 1f;

    // Start is called before the first frame update
    void Start()
    {
        playerTag = player.tag;
    }

    // Update is called once per frame
    void Update()
    {

        if (dead)
        {
            timeToDie -= Time.deltaTime; // Dont die for a sec
            if (timeToDie <= 0f)
            {
                gameOverScreen.SetActive(true);
                gameOverScreen.GetComponent<GameOverController>().UpdateStats();
                //timeManager.StopTime(); // Then die
            }
        }
        else
        {
            UpdateHeightIdx();
            if (player.transform.position.y - transform.position.y > maximumDistanceY)
            { // Catch up with the player
                Vector3 targetPosition = player.transform.position - new Vector3(0, maximumDistanceY, 0);

                // Smoothly move the camera towards that target position
                Vector3 newPos = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

                if (velocity.y < speedAtHeight[currHeightIdx])
                {// If this velocity is lower than the one from the region, it should move faster
                    newPos = transform.position + new Vector3(0, speedAtHeight[currHeightIdx], 0) * Time.deltaTime;
                }

                transform.position = newPos;
            }
            else
            { // Plane is higher than the <maxDistanceY> from the player, but we may still want to accelerate
                transform.position += new Vector3(0, speedAtHeight[currHeightIdx], 0) * Time.deltaTime;
            }
        }
    }

    // Updates the index of the current speed (currHeightIdx) to match the current height
    private void UpdateHeightIdx()
    {
        if (currHeightIdx >= accelerationHeights.Count - 1) return; // Dont overflow
        float currY = transform.position.y;
        while (currY >= accelerationHeights[currHeightIdx+1])
        {
            currHeightIdx++;
            if (currHeightIdx >= accelerationHeights.Count - 1) return; // Dont overflow
        } // Here, currHeightIdx corresponds to the actual current region
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == playerTag)
        {
            Death();
        }
    }

    void ShowText(string text)
    {
        if (textPrefab)
        {
            var textParent = Instantiate(textPrefab, transform.position + Camera.main.transform.forward * 25f, Quaternion.identity);
            textParent.GetComponentInChildren<TextMesh>().text = text;
            Destroy(textParent, 5f); // Destroy after 1 second
        }
    }

    public float GetDistanceToPlayer()
    {
        return player.transform.position.y - transform.position.y - 1f;
    }


    void Death()
    {
        //ShowText("__________________ DEATH __________________\n" +
        //         "(pls quit and restart, \nwe havent implemented this yet)\n" +
        //         "___________________________________________");
        ////player.GetComponent<Rigidbody>().isKinematic = true;
        //Debug.Log("TODO: implement death");
        GameObject bow = player.transform.GetChild(0).gameObject; // not bow, camera
        bow = bow.transform.GetChild(0).gameObject; // actually bow
        Destroy(bow);

        dead = true;
    }
}
