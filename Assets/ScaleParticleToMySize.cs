using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleParticleToMySize : MonoBehaviour
{
    public Transform Particles;

    private void Start()
    {
        Particles.localScale = transform.localScale;
    }
}
