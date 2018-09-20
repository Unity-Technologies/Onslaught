using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGameTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            GameManager.instance.StateTransitionTo(GameManager.GameState.Gameplay);
    }
}
