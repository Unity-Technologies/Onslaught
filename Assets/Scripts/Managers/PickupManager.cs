using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupManager : MonoBehaviour
{
    public static PickupManager instance;

    public Transform pickupSpawnContainer;

    [Header("Pickups")]
    public GameObject healthPickup;
    public GameObject weaponPickup;

    private List<Transform> m_SpawnLocations;
    private List<Pickup> m_Pickups;

    public void Init()
    {
        Debug.Log("PickupManager Awake");
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);

            m_SpawnLocations = new List<Transform>();
            foreach (Transform child in pickupSpawnContainer)
                m_SpawnLocations.Add(child);

            m_Pickups = new List<Pickup>();
        }
        else
        {
            Destroy(this);
        }
    }

    private void Reset()
    {
        while (m_Pickups.Count != 0)
        {
            Pickup toDestroy = m_Pickups[0];
            m_Pickups.Remove(toDestroy);
            Destroy(toDestroy.gameObject);
        }
    }

    public void PickupUsed(Pickup usedPickup)
    {
        if (m_Pickups.Contains(usedPickup))
            m_Pickups.Remove(usedPickup);
    }

    public void SpawnPickups()
    {
        Reset();

        Transform weaponPickupLocation = null;
        Transform healthPickupLocation = null;

        if (weaponPickup != null)
        {
            weaponPickupLocation = m_SpawnLocations[Random.Range(0, m_SpawnLocations.Count - 1)];
            GameObject newPickup = Instantiate(weaponPickup, weaponPickupLocation.position, weaponPickupLocation.rotation);
            newPickup.transform.localScale = GameManager.instance.gameScale;
            m_Pickups.Add(newPickup.GetComponent<Pickup>());
        }

        if (healthPickup != null && !GameManager.instance.player.GetComponent<Player>().IsFullHealth() && m_SpawnLocations.Count > 1)
        {
            // Get a new, unique place for the health
            while (healthPickupLocation == null)
            {
                healthPickupLocation = m_SpawnLocations[Random.Range(0, m_SpawnLocations.Count - 1)];
                if (healthPickupLocation == weaponPickupLocation)
                    healthPickupLocation = null;
            }

            GameObject newPickup = Instantiate(healthPickup, healthPickupLocation.position, healthPickupLocation.rotation);
            newPickup.transform.localScale = GameManager.instance.gameScale;
            m_Pickups.Add(newPickup.GetComponent<Pickup>());
        }
    }
}
