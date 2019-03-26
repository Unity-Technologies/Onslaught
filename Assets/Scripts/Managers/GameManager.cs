using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool isAR = false;

    public WaveManager waveManager;
    public PickupManager pickupManager;
    public InputAbstraction inputAbstraction;
    private MovementRaycast movementRaycast;

    public Transform scalingParent;
    public Vector3 gameScale { get { return scalingParent.lossyScale; } }

    public NavMeshSurface navMeshSurface;

    public GameObject playerPrefab;
    public Transform playerSpawn;
    public StartGameTrigger startGameTrigger;
    public Text healthDisplayText;

    public GameObject player { get; private set; }

    public enum GameState
    {
        None,
        Menu,
        Gameplay
    }

    public GameState state { get; private set; }

    void Awake()
    {
        pickupManager.Init();
        waveManager.Init();
        
        if (instance == null)
        {
            instance = this;
            StateTransitionTo(GameState.Menu);
            movementRaycast = GameObject.FindGameObjectWithTag("MovementRaycaster").GetComponent<MovementRaycast>();
            movementRaycast.Init();
        }
        else
        {
            Destroy(this);
        }
    }

    public bool StateTransitionTo(GameState targetState)
    {
        if (targetState == state)
            return false;

        switch (targetState)
        {
            case GameState.Menu:
                TransitionToMenuState();
                break;
            case GameState.Gameplay:
                TransitionToGameplayState();
                break;
            default:
                break;
        }

        return true;
    }

    private void TransitionToMenuState()
    {
        WaveManager.instance.Reset();
        state = GameState.Menu;
        startGameTrigger.gameObject.SetActive(true);
        player = Instantiate(playerPrefab);
        player.transform.localScale = GameManager.instance.gameScale;
        player.GetComponent<Player>().SetHealthDisplayText(healthDisplayText);
        player.GetComponent<NavMeshAgent>().speed = gameScale.x;
        
        player.GetComponent<NavMeshAgent>().Warp(playerSpawn.position); // must warp so that the NavMesh is found by NavMeshAgent
    }

    private void TransitionToGameplayState()
    {
        WaveManager.instance.SpawnFirstWave();
        startGameTrigger.gameObject.SetActive(false);
        state = GameState.Gameplay;
    }

    // This can happen when the scene reloads
    public void PlayerIsLost()
    {
        Debug.Log("Player is lost");
        NavMeshHit closestPosition;
        if (!NavMesh.SamplePosition(playerSpawn.position, out closestPosition, 1, ~0))
            Debug.Log("NavMesh.SamplePosition failed to find a position");
        else
            Debug.Log("SamplePosition found position = (" + closestPosition.position.x + ", " + closestPosition.position.y + ", " + closestPosition.position.z + ")");
        player.GetComponent<NavMeshAgent>().Warp(closestPosition.position); // must warp so that the NavMesh is found by NavMeshAgent
        Debug.Log("hopefully now player is found");
    }
}
