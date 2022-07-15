using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class HourglassController : MonoBehaviour
{
    public RectMask2D topMask, bottomMask;

    //private topRect, bottomRect;

    private GameObject hourglass;

    private float slowdownDuration = 10f,
                  currTime = 0f;

    public float heightTop = 60f, heightBottom = 60f;


    // Start is called before the first frame update
    void Start()
    {
        //topRect = topMask.GetComponent<>
        hourglass = transform.GetChild(0).gameObject;
        //heightTop = topMask.rectTransform.sizeDelta.y;
        //heightBottom = bottomMask.rectTransform.sizeDelta.y;
    }

    public void NotifySlowdownStart(float duration)
    {
        slowdownDuration = duration;
        currTime = duration;
    }

    // Update is called once per frame
    void Update()
    {
        if (currTime <= 0f)
        {
            hourglass.SetActive(false);
        }
        else
        {
            hourglass.SetActive(true);
            float fillAmount = (slowdownDuration - currTime) / slowdownDuration; // 0 empty, 1 full

            topMask.padding = new Vector4(0, 0, 0, fillAmount * heightTop);
            bottomMask.padding = new Vector4(0, 0, 0, (1f-fillAmount) * heightBottom);

            currTime -= Time.unscaledDeltaTime;
        }
    }
};
