using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathDistanceMeasurer : MonoBehaviour
{
    public float updatePeriod = 0.1f,
                 maxDistShow = 100f;
    private float currCD = 0f;

    public DeathController deathPlane;
    public Text metersText;

    private GameObject child;

    // Start is called before the first frame update
    void Start()
    {
        child = transform.GetChild(0).gameObject;
        
    }

    // Update is called once per frame
    void Update()
    {

        currCD -= Time.deltaTime;
        if (currCD < 0f)
        {
            UpdateText();
            currCD = updatePeriod;
        }
        
    }

    private void UpdateText()
    {
        float dist = deathPlane.GetDistanceToPlayer();
        if (dist > maxDistShow)
        {
            child.SetActive(false);
        }
        else
        {
            child.SetActive(true);
            metersText.text = dist.ToString("F0");
        }
    }
}
