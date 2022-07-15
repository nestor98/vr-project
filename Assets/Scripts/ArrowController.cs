using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    public List<string> missPhrases;
    public GameObject textPrefab;
    public float missTime = 2f; // Seconds to assume the arrow has missed

    private float flyingTime = 0f;
    private bool flying = false;

    private Rigidbody rb;

    public void Shoot()
    {
        flying = true;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (rb)
        {
            // Arrow must point forward:
            transform.forward = rb.velocity.normalized;
        }

        if (flying)
        {
            // For the "miss" text:
            flyingTime += Time.deltaTime;
            if (flyingTime > missTime)
            {
                SpawnText(GetMissPhrase());
                flying = false; // dont care about this anymore
                Destroy(this, 6f); // Self destruct after 6 seconds
            }
        }
    }

    string GetMissPhrase()
    {
        int idx = Random.Range(0, missPhrases.Count);
        return missPhrases[idx];
    }

    void SpawnText(string text)
    {
        if (textPrefab)
        {
            float rot = Random.Range(-30, 30);
            var textParent = Instantiate(textPrefab, transform.position + new Vector3(0,2,0), transform.rotation);
            //var transform = textParent.transform;
            //rectTransform.position = new Vector3(0, 0, 0);
            //te
            //textParent.transform.rotation += Quaternion.Euler(rot, 0, 0);
            var euler = transform.rotation.eulerAngles;
            textParent.transform.eulerAngles = new Vector3(0, euler.y, 0);
            //textParent.transform.LookAt(transform.forward);
            textParent.transform.localScale = new Vector3(textParent.transform.localScale.x, textParent.transform.localScale.y, 0);
            //textParent.transform.Rotate(new Vector3(rot, 180, 0));
                //.Rotate(new Vector3(rot,0,0));

            textParent.GetComponentInChildren<TextMesh>().text = text;
            Destroy(textParent, 1.5f); // Destroy after 1 second
        }
    }

    // Stick to anything it hits
    void OnCollisionEnter(Collision obj)
    {

        // Stop physics:
        Destroy(GetComponent<Rigidbody>());
        GetComponent<BoxCollider>().enabled = false;

        // Stick:
        // This would be nice but there is a super strange bug when hitting the targets,
        // so no moving targets and no problem
        //transform.parent = obj.gameObject.transform;

        // Destroy trail:
        Destroy(GetComponentInChildren<TrailRenderer>(), 1f);

        // Dont say you missed:
        flying = false;
    }
}
