using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LingeringGO : MonoBehaviour
{
    [Tooltip("Time in seconds until this gameobject self destructs")]
    public float timeToLive = 1f;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        gameObject.SetActive(true);
        transform.SetParent(null);
        StartCoroutine(DelayedDestroy());
    }

    IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(timeToLive);
        Destroy(gameObject);
    }
}
