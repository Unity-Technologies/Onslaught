using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetachLingeringGOsOnTrigger : MonoBehaviour
{
    public bool onlyHitPlayer = false;

    private bool m_HasHit = false;

    private void OnTriggerEnter(Collider other)
    {
        if (m_HasHit || other.gameObject.layer == LayerMask.NameToLayer("TransparentFX"))
            return;

        if (onlyHitPlayer && other.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;

        m_HasHit = true;

        foreach (LingeringGO go in GetComponentsInChildren<LingeringGO>(true))
        {
            go.Activate();
        }

        if (GetComponent<Health>() != null)
            GetComponent<Health>().SelfDestruct();
        else
            Destroy(gameObject);
    }
}
