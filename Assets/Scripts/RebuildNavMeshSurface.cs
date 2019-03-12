using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NavMeshBuilder = UnityEngine.AI.NavMeshBuilder;

public class RebuildNavMeshSurface : MonoBehaviour
{
    void OnEnable()
    {
        GetComponent<NavMeshSurface>().BuildNavMesh();
    }
}
