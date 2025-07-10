using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviourPun
{
    [Header("Zombies to spawn")]
    [SerializeField] private string normalZombiePrefabName = "Zombie";
    [SerializeField] private string fatZombiePrefabName = "FatZombie";

    [Header("Spawn time")]
    [SerializeField] private float spawnTimeMin = 1f;
    [SerializeField] private float spawnTimeMax = 3f;

    [Header("Zombie Count")]
    [SerializeField] private int maxZombie = 5;
    [SerializeField] private int minZombie = 2;

    private Coroutine spawnRoutine;
    private List<GameObject> spawnedZombies = new List<GameObject>();


    private void OnEnable()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            spawnRoutine = StartCoroutine(CoSpawn());
        }
    }

    private void OnDisable()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        spawnedZombies.Clear();
    }

    private IEnumerator CoSpawn()
    {
        while (true)
        {
            // 죽은(비활성화된) 좀비 정리
            spawnedZombies.RemoveAll(z => z == null || !z.activeSelf);

            // 좀비 수 확인
            if (spawnedZombies.Count >= maxZombie)
            {
                yield return new WaitUntil(() =>
                {
                    spawnedZombies.RemoveAll(z => z == null || !z.activeSelf);
                    return spawnedZombies.Count <= minZombie;
                });
            }

            float waitTime = Random.Range(spawnTimeMin, spawnTimeMax);
            yield return new WaitForSeconds(waitTime);

            string prefabToSpawn = Random.value < 0.5f ? normalZombiePrefabName : fatZombiePrefabName;
            GameObject zombie = PhotonNetwork.Instantiate(prefabToSpawn, transform.position, transform.rotation);
            spawnedZombies.Add(zombie);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawCube(transform.position, new Vector3(1f, 1f, 1f));
    }
}
