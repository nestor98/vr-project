using UnityEngine;

public class Billboard : MonoBehaviour
{

    private float startY;
    private Transform camTransform;

    void Start()
    {
        camTransform = Camera.main.transform;
        startY = transform.position.y;
    }

    // Sun follows the player up 
    void Update()
    {
        transform.position = new Vector3(transform.position.x, 
                                         Mathf.Max(camTransform.position.y, startY), 
                                         transform.position.z);
    }
}
