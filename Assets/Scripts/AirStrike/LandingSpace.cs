using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingSpace : MonoBehaviour
{
    public float speedRotate = 1f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.forward * speedRotate * Time.deltaTime);
    }
}
