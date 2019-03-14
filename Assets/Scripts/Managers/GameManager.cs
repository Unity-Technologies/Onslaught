using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public WaveManager waveManager;
    public PickupManager pickupManager;
    public InputAbstraction inputAbstraction;

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
        player.GetComponent<NavMeshAgent>().Warp(playerSpawn.position); // must warp so that the NavMesh is found by NavMeshAgent
        player.GetComponent<Player>().SetHealthDisplayText(healthDisplayText);
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
        player.GetComponent<NavMeshAgent>().Warp(playerSpawn.position); // must warp so that the NavMesh is found by NavMeshAgent
    }
}
