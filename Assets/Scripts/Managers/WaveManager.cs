using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class WaveManager : MonoBehaviour
{
    public static WaveManager instance = null;

    public GameObject spawnerContainer;

    [Header("Information Displays")]
    public Text waveDisplayText;

    [Header("Enemy Prefabs")]
    public GameObject prefabEnemyStandard = null;
    public GameObject prefabEnemyFast = null;
    public GameObject prefabEnemyTough = null;

    private bool m_SpawnEnemies = false;
    private SpawnInCube[] m_Spawners;
    private List<WaveEnemy> m_WaveEnemies;
    private int m_WaveNumber;

    private enum WaveType
    {
        StandardOnly = 0,
        StandardAndFast = 1,
        Mob = 2,

        Count = 3 // KEEP THIS UP TO DATE!
    }

    private WaveType waveType = WaveType.StandardOnly;

    void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
            m_Spawners = spawnerContainer.GetComponentsInChildren<SpawnInCube>();
            m_WaveEnemies = new List<WaveEnemy>();
            Reset();
        }
        else
        {
            Destroy(this);
        }
    }

    public void AddWaveEnemy(WaveEnemy waveEnemyToAdd)
    {
        if (!m_WaveEnemies.Contains(waveEnemyToAdd))
            m_WaveEnemies.Add(waveEnemyToAdd);
    }

    public void RemoveWaveEnemy(WaveEnemy waveEnemyToRemove)
    {
        if (!m_WaveEnemies.Contains(waveEnemyToRemove))
            return;
        else
            m_WaveEnemies.Remove(waveEnemyToRemove);

        if (m_WaveEnemies.Count == 0)
            SpawnWave(); // Go to next wave
    }

    public void SpawnFirstWave()
    {
        Reset();
        m_SpawnEnemies = true;
        SpawnWave();
    }

    public void Reset()
    {
        m_WaveNumber = 0;
        waveType = WaveType.StandardOnly;
        m_SpawnEnemies = false;

        while (m_WaveEnemies.Count != 0)
        {
            m_WaveEnemies[0].SelfDestruct();
        }
    }

    void SpawnWave()
    {
        if (!m_SpawnEnemies)
            return;

        // Update Pickups
        PickupManager.instance.SpawnPickups();

        m_WaveNumber++;
        int cycleNumber = 1 + ((m_WaveNumber - 1) / (int)WaveType.Count);

        // Information display

        if (waveDisplayText != null)
            waveDisplayText.text = m_WaveNumber.ToString();

        // spawn

        foreach (SpawnInCube spawner in m_Spawners)
        {
            switch (waveType)
            {
                case WaveType.StandardOnly:
                    spawner.Spawn((2 * cycleNumber) + 1, prefabEnemyStandard);
                    break;
                case WaveType.StandardAndFast:
                    spawner.Spawn(cycleNumber + 1, prefabEnemyStandard);
                    spawner.Spawn(cycleNumber, prefabEnemyFast);
                    break;
                case WaveType.Mob:
                    spawner.Spawn(cycleNumber + 1, prefabEnemyStandard);
                    spawner.Spawn(cycleNumber - 1, prefabEnemyFast);
                    spawner.Spawn(cycleNumber, prefabEnemyTough);
                    break;
            }
        }

        SetNextWaveType();
    }

    void SetNextWaveType()
    {
        switch (waveType)
        {
            case WaveType.StandardOnly:
                waveType = WaveType.StandardAndFast;
                break;
            case WaveType.StandardAndFast:
                waveType = WaveType.Mob;
                break;
            case WaveType.Mob:
                waveType = WaveType.StandardOnly;
                break;
        }
    }
}
