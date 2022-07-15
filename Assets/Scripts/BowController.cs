using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BowController : MonoBehaviour
{
    private enum State
    {
        PLACING_ARROW, // New arrow being placed
        RELAXED,       // Arrow is placed
        PULLING,       // Pulling the bow
        RELEASING      // Releasing, firing the arrow
    };
    // Normally we go 0, 1, 2, 3, 0...
    // But from 2 (PULLING) we could go back to 2 (RELAXED)

    private bool desktopMode = true; // Ignore this, for now this setting is in PlayerController



    // Attached objects:
    public GameObject arrowPrefab, stringPullPoint;
    public TimeManager timeManager; // For slow motion


    private Rigidbody rb;

    // Initial (RELAXED) position of the arrow wrt the bow
    public Vector3 arrowOffset = new Vector3(0.0f, -1.82f, 0.0f);

    // We start placing the arrow (could already start in RELAXED I guess)
    private State state = State.PLACING_ARROW;

    // Current arrow, nothing on PLACING_ARROW
    private GameObject currentArrow = null;

    // Coordinates of the two ends of the pulling movement
    public Transform stringEnd;
    private Vector3 maxPullingOffset, stringOffset;


    public float timeToPlaceArrow = 2f; // seconds to place a new arrow

    // Arrow shooting Force:
    public float minArrowSpeed = 20.0f;
    public float maxArrowSpeed = 50.0f; // Throwing force
    public float maxForceTime = 1.5f; // 1.5 seconds
    //public float releaseTime = 0.1f; // Time to release the arrow

    private float currentSpeed = 0.0f;

    private float timeInState = 0.0f; // Utility counter for the current state

    private float currentPullingSpeed; // useless for now, for SmoothDamp

    // Stats
    private int shotCount = 0; // Number of shots

    // Start is called before the first frame update
    void Start()
    {
        transform.LookAt(transform.position + Camera.main.transform.forward, transform.up);

        // Offset when not pulled:
        stringOffset = stringPullPoint.transform.localPosition;
        // Offset when pulled:
        maxPullingOffset = stringEnd.localPosition - stringOffset;

        rb = arrowPrefab.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.RELAXED)
        {
            if (PullingInput())
            {
                state = State.PULLING;
                timeManager.DoSlowmotion();
                UpdatePulling();
            }
        }
        else if (state == State.PLACING_ARROW)
        {
            timeInState += Time.deltaTime;
            if (timeInState > timeToPlaceArrow)
            {
                timeInState = 0f;
                AttachArrow();
                state = State.RELAXED;
            }
        }
        else if (state == State.PULLING)
        {
            if (ReleasingInput())
            {
                Shoot(); // Goes into RELEASING
            }
            else
            {
                UpdatePulling();
            }
        }
        else if (state == State.RELEASING) // After shooting the arrow, the bow must go back to the initial position
        {
            UpdateReleasing();
        }
        CheckStates(); // TODO: Quitar, solo para debug
    }

    public int GetShotCount ()
    {
        return shotCount;
    }

    // Instantiate a new arrow in its resting position
    private void AttachArrow()
    {
        currentArrow = Instantiate(arrowPrefab);
        currentArrow.transform.parent = stringPullPoint.transform;
        currentArrow.transform.localPosition = arrowOffset;
        currentArrow.transform.localRotation = Quaternion.Euler(new Vector3(-90f,0,0));
        currentArrow.transform.localScale = new Vector3(1f, 1f, 1f);

        // Yup just destroy it, weird bugs
        var body = currentArrow.GetComponent<Rigidbody>();
        Destroy(body);
    }

    // Tutorial https://www.youtube.com/watch?v=Dh7Wwqs-s2c, 1:05:00

    // Debug sanity checks
    private void CheckStates()
    {
        if (state == State.PLACING_ARROW || state == State.RELEASING)
        {
            if (currentArrow != null)
            {
                Debug.Log("PLACING and RELEASING states should not have an arrow!");
            }
        }
        else
        {
            if (currentArrow == null)
            {
                Debug.Log("No arrow and not in PLACING/RELEASING states!");
            }
        }
    }

    private bool PullingInput()
    {
        if (desktopMode)
        {
            if (Input.GetMouseButton(0))
            {
                return true;
            }
        }
        else
        {
            Debug.Log("[BowController] Pulling input for VR not implemented");
        }
        return false;
    }


    private bool ReleasingInput()
    {
        if (desktopMode)
        {
            if (Input.GetMouseButtonUp(0))
            {
                return true;
            }
        }
        else
        {
            Debug.Log("[BowController] Releasing input for VR not implemented");
        }
        return false;
    }

    // ... shoot the arrow
    private void Shoot()
    {
        shotCount++;
        // Current force depends on the pulling time

        currentSpeed = Mathf.SmoothStep(minArrowSpeed, maxArrowSpeed, timeInState/maxForceTime);

        // Deatch the arrow transform:
        Vector3 objectPos = currentArrow.transform.position;
        currentArrow.transform.SetParent(null);
        currentArrow.transform.position = objectPos;

        Rigidbody body = currentArrow.AddComponent<Rigidbody>();

        if (body)
        {
            //Debug.Log("shoot");
            body.mass = rb.mass;
            body.useGravity = rb.useGravity;
            body.isKinematic = rb.isKinematic;
            body.detectCollisions = rb.detectCollisions;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            body.velocity = currentArrow.transform.forward * currentSpeed;
            currentArrow.GetComponent<BoxCollider>().enabled = true;
        }
        currentArrow.GetComponent<ArrowController>().Shoot(); // Notify the arrow it has been shot

        // Change states:
        currentArrow = null;
        state = State.RELEASING;
        timeInState = 0.0f;
    }

    // The string is pulled towards its maximum point
    private void UpdatePulling()
    {
        timeInState = Mathf.Min(timeInState + Time.deltaTime, maxForceTime);
        stringPullPoint.transform.localPosition = stringOffset + maxPullingOffset * timeInState / maxForceTime;
        //currentArrow.transform.localPosition = arrowOffset + maxPullingOffset * timeInState / maxForceTime;
        
    }

    // The string is released with the same speed as the arrow until it reaches its original position
    private void UpdateReleasing()
    {

        float offset = Mathf.Max(maxPullingOffset.magnitude - currentSpeed * Time.deltaTime, 0.0f);

        stringPullPoint.transform.localPosition = stringOffset + maxPullingOffset * offset;

        if (offset <= 0.001f)
        {
            state = State.PLACING_ARROW;
        }
    }

}
