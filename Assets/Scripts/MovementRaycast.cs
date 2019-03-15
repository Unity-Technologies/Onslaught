using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

public class MovementRaycast : MonoBehaviour
{
    public GameObject navigationAidPrefab = null;
    public LayerMask layerMask;
    public AudioClip moveSound = null;

    private NavMeshAgent m_nmAgent = null;
    private GameObject m_NavigationAidPrefab = null;
    private Transform m_Transform = null;
    private AudioSource m_audioSource = null;

    private bool shouldNavigate = false;

    // Start is called before the first frame update
    public void Init()
    {
        m_Transform = transform;
        m_nmAgent = GameManager.instance.player.GetComponent<NavMeshAgent>();

        if (moveSound != null)
        {
            m_audioSource = gameObject.AddComponent<AudioSource>();
            m_audioSource.clip = moveSound;
        }

        m_NavigationAidPrefab = Instantiate(navigationAidPrefab);
        m_NavigationAidPrefab.transform.localScale = GameManager.instance.gameScale;
        m_NavigationAidPrefab.SetActive(false);

        GameManager.instance.inputAbstraction.NavActive += OnNavActive;
    }
    
    private void OnDestroy()
    {
        GameManager.instance.inputAbstraction.NavActive -= OnNavActive;
    }

    void OnNavActive()
    {
        shouldNavigate = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Easy exit for AR before a game instance has been instantiated
        if (GameManager.instance == null)
            return;

        if (m_nmAgent == null)
            m_nmAgent = GameManager.instance.player.GetComponent<NavMeshAgent>();
        
        // Do Raycast
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, layerMask))
        {
            // Show navigation aid
            m_NavigationAidPrefab.SetActive(true);
            m_NavigationAidPrefab.transform.position = hit.point;
            
            if (shouldNavigate)
            {
                // Handle input
                if (m_nmAgent == null)
                    m_nmAgent = GameManager.instance.player.GetComponent<NavMeshAgent>();
                
                if (m_nmAgent != null)
                {
                    if (!m_nmAgent.isOnNavMesh)
                        GameManager.instance.PlayerIsLost();

                    m_nmAgent.destination = hit.point;
                    if (m_audioSource != null)
                        m_audioSource.Play();
                }
            }
        }
        else
        {
            m_NavigationAidPrefab.SetActive(false);
        }

        // Even if we don't have a valid navigation event, we must unset
        // this flag so that we don't have weird behavior
        shouldNavigate = false;
    }
}
