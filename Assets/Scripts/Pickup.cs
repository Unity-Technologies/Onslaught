using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    private void OnDestroy()
    {
        PickupManager.instance.PickupUsed(this);
    }
}
