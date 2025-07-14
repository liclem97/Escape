using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 일정 시간마다 좀비를 스폰하는 함수 */
public class ZombieSpawner : MonoBehaviourPun
{
    [Header("Zombies to spawn")]
    [SerializeField] private string normalZombiePrefabName = "Zombie";
    [SerializeField] private string fatZombiePrefabName = "FatZombie";
    [SerializeField] private string crawlZombiePrefabName = "CrawlZombie";

    [Header("Spawn time")]
    [SerializeField] private float spawnTimeMin = 1f;   
    [SerializeField] private float spawnTimeMax = 3f;   // 랜덤 스폰 시간

    [Header("Zombie Count")]
    [SerializeField] private int maxZombie = 5; // 스폰할 최대 좀비
    [SerializeField] private int minZombie = 2; // 저장한 수 이하로 떨어지면 다시 스폰 시작

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

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: CoSpawn
    * 기능: 좀비를 스폰하는 코루틴 함수
    ***********************************************************************************/
    private IEnumerator CoSpawn()
    {
        while (true)
        {
            // 죽은(비활성화된) 좀비 정리
            spawnedZombies.RemoveAll(z => z == null || !z.activeSelf);

            // 스폰한 좀비가 maxZombie 이상인 경우
            if (spawnedZombies.Count >= maxZombie)
            {
                yield return new WaitUntil(() =>
                {
                    spawnedZombies.RemoveAll(z => z == null || !z.activeSelf);   // 죽은(비활성화된) 좀비 정리
                    return spawnedZombies.Count <= minZombie;   // 리스트의 수가 minZombie 이하일때까지 대기
                });
            }

            float waitTime = Random.Range(spawnTimeMin, spawnTimeMax);
            yield return new WaitForSeconds(waitTime);

            // 좀비 타입 결정 (0: Normal, 1: Fat, 2: Crawl)
            int rand = Random.Range(0, 3);
            string prefabToSpawn;

            switch (rand)
            {
                case 0:
                    prefabToSpawn = normalZombiePrefabName;
                    break;
                case 1:
                    prefabToSpawn = fatZombiePrefabName;
                    break;
                case 2:
                    prefabToSpawn = crawlZombiePrefabName;
                    break;
                default:
                    prefabToSpawn = normalZombiePrefabName;
                    break;
            }

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
