using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeToLive : MonoBehaviour
{
    public float timeToLive = 20f;
    private float m_Timer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        m_Timer = timeToLive;
    }

    private void Update()
    {
        m_Timer -= Time.deltaTime;

        if (m_Timer <= 0)
            Destroy(gameObject);
    }
}
