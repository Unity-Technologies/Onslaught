using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpartImpulse : MonoBehaviour
{
    public Vector3 impulse;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().AddForce(GetComponent<Transform>().TransformVector(impulse), ForceMode.VelocityChange);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
