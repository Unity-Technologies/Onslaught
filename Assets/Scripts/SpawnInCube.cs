using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

public class SpawnInCube : MonoBehaviour
{
    [Tooltip("This should be the transform of a cube.  The scale of the cube will inform where the spawnables spawn.")]
    public Transform cubeSpawnArea = null;

    // Update is called once per frame
    public void Spawn(int numberToSpawn, GameObject prefabToSpawn)
    {
        if (prefabToSpawn == null || cubeSpawnArea == null || numberToSpawn < 1)
            return;

        for (int i = 0; i < numberToSpawn; i++)
        {
            Vector3 localSpawnPosition = new Vector3(
                Random.Range(-cubeSpawnArea.lossyScale.x, cubeSpawnArea.lossyScale.x),
                Random.Range(-cubeSpawnArea.lossyScale.y, cubeSpawnArea.lossyScale.y),
                Random.Range(-cubeSpawnArea.lossyScale.z, cubeSpawnArea.lossyScale.z)
            );

            // Transform local spawn position to cubeSpawnArea's rotation
            localSpawnPosition = ((((cubeSpawnArea.rotation * Quaternion.LookRotation(localSpawnPosition)) * Vector3.forward).normalized) * localSpawnPosition.magnitude) / 2;

            GameObject newSpawn = Instantiate(prefabToSpawn, cubeSpawnArea.position + localSpawnPosition, cubeSpawnArea.rotation);
            newSpawn.transform.localScale = GameManager.instance.gameScale;
            newSpawn.GetComponent<NavMeshAgent>().speed *= GameManager.instance.gameScale.x;
        }
    }
}
