using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnedObject : MonoBehaviour
{
    public ObjectGenerator obj;
    public float speed = 0.0f;
    public float stepA;

    private string displayName;
    private string description;
    private Renderer MainRenderer;

    private GameObject spawn;

    public string Name
    {
        get
        {
            return displayName;
        }

        set
        {
            displayName = value;
        }
    }
    public string Discription
    {
        get
        {
            return description;
        }

        set
        {
            description = value;
        }
    }

    public GameObject Spawn { get => spawn; set => spawn = value; }

    public bool test = false;
    float alfa = 1;

    public void Start()
    {
    }
    private void Update()
    {
        if (speed > 0.0f)
        {
            AutoRotate();
        }

        if (test)
        {
            SwipeX();
        }
    }

    public void AutoRotate()
    {
        if (speed > 0.0f)
        {
            // do something
            transform.Rotate(new Vector3(0, 0.5f, 0) * speed);

            Debug.Log("Acceleration " + speed.ToString());

            if (speed - 0.1f <= 0.0f)
            {
                speed = 0.0f;
            }
            else
            {
                speed -= 0.5f;

            }
        }
    }

    public void SwipeX ()
    {
        Debug.Log("alfa " + spawn.GetComponent<Renderer>().material.color.a);
        spawn.GetComponent<Renderer>().material.color -= new Color(0.0f, 0.0f, 0.0f, 0.02f);

        if (spawn.GetComponent<Renderer>().material.color.a <= 0.02f)
        {
            Destroy(gameObject);

        }
    }

    
}
