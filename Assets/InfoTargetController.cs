using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoTargetController : MonoBehaviour
{
    public List<string> tips;
    public string arrowTag = "arrow";
    private TextMesh text;
    private int lastTip = -1;

    // Start is called before the first frame update
    void Start()
    {
        text = transform.GetChild(0).GetChild(0).GetChild(0).gameObject.GetComponent<TextMesh>();
        //text.text = "aaaaaaaa";
        SetRandomTip();
    }

    void OnCollisionEnter(Collision obj)
    {
        if (obj.gameObject.tag == arrowTag)
        {
            SetRandomTip();
        }
    }

    private void SetRandomTip()
    {

        int tipIdx = (lastTip+1) % tips.Count; //(Random.Range(0, tips.Count - 2) + lastTip) % tips.Count;

        text.text = tips[tipIdx];

        lastTip = tipIdx;
    }
}
