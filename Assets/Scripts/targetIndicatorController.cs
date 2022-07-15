using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;


public class targetIndicatorController : MonoBehaviour
{
    public GameObject target;

    public float showIconMinAngle = 40f, // Minimum angle (deg) to show the icon
                 showIconMaxDistThreshold = 30f; // Maximum distance to show it
    private float showIconMaxCosine;

    // Size multipliers. Final size will be interpolated between this depending
    // on dist
    public float minSize = 0.1f,
                 maxSize = 2f;

    public Vector2 resolutionWowUnityThanksForThisGreatUISystem = new Vector2(1600, 900);

    private Camera cam;
    private RectTransform rt;

    private Vector3 initialScale;

    private CanvasRenderer rend;

    // Start is called before the first frame update
    void Start()
    {
        showIconMaxCosine = Mathf.Cos(Mathf.Deg2Rad * showIconMinAngle);
        rend = GetComponent<CanvasRenderer>();
        cam = Camera.main;
        rt = GetComponent<RectTransform>();

        initialScale = rt.localScale;

    }

    // Update is called once per frame
    void Update()
    {
        // Relative vector from the cam to the target
        Vector3 relative = cam.transform.InverseTransformPoint(target.transform.position);
        // To 2D
        Vector2 relative2D = relative;

        float cosine = Vector3.Dot(cam.transform.forward, (target.transform.position - cam.transform.position).normalized);
        if (cosine > showIconMaxCosine || relative.magnitude > showIconMaxDistThreshold)
        {// If it is too close or to far dont draw it (TODO: didnt find a way to NOT actually draw it, just set scale=0)
            rt.localScale = Vector3.zero;
        }
        else
        {
            // 2D angle in the screen:
            float angle = Mathf.Atan2(relative.x, relative.y) * Mathf.Rad2Deg;
            rt.localRotation = Quaternion.Euler(0, 0, -angle);
            // Scale the icon according to distance:
            float scale = showIconMaxDistThreshold / relative.magnitude;
            rt.localScale = initialScale * Mathf.Lerp(minSize, maxSize, scale);
            // Move the icon to the corresponding edge of the screen:
            relative2D = relative2D.normalized;
            rt.anchoredPosition = relative2D * resolutionWowUnityThanksForThisGreatUISystem / 2f;
        }
    }

    // Deprecated xd
    //relativeDebug = (0.5f * relativeDebug.normalized + new Vector2(0.5f, 0.5f)).normalized;
    void ClampPivot()
    {
        float x = Mathf.Clamp(rt.pivot.x, 0, 1);
        float y = Mathf.Clamp(rt.pivot.y, 0, 1);
        rt.pivot = new Vector2(x, y);
    }
    void ClampPosition()
    {
        float x = Mathf.Clamp(rt.anchoredPosition.x, 0, 1);
        float y = Mathf.Clamp(rt.anchoredPosition.y, 0, 1);
        rt.anchoredPosition = new Vector2(x, y) * resolutionWowUnityThanksForThisGreatUISystem;
    }
}
