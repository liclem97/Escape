using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ���� �ð����� ���� �����ϴ� �Լ� */
public class ZombieSpawner : MonoBehaviourPun
{
    [Header("Zombies to spawn")]
    [SerializeField] private string normalZombiePrefabName = "Zombie";
    [SerializeField] private string fatZombiePrefabName = "FatZombie";
    [SerializeField] private string crawlZombiePrefabName = "CrawlZombie";

    [Header("Spawn time")]
    [SerializeField] private float spawnTimeMin = 1f;   
    [SerializeField] private float spawnTimeMax = 3f;   // ���� ���� �ð�

    [Header("Zombie Count")]
    [SerializeField] private int maxZombie = 5; // ������ �ִ� ����
    [SerializeField] private int minZombie = 2; // ������ �� ���Ϸ� �������� �ٽ� ���� ����

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
    * �ۼ���: �ڿ���
    * �Լ�: CoSpawn
    * ���: ���� �����ϴ� �ڷ�ƾ �Լ�
    ***********************************************************************************/
    private IEnumerator CoSpawn()
    {
        while (true)
        {
            // ����(��Ȱ��ȭ��) ���� ����
            spawnedZombies.RemoveAll(z => z == null || !z.activeSelf);

            // ������ ���� maxZombie �̻��� ���
            if (spawnedZombies.Count >= maxZombie)
            {
                yield return new WaitUntil(() =>
                {
                    spawnedZombies.RemoveAll(z => z == null || !z.activeSelf);   // ����(��Ȱ��ȭ��) ���� ����
                    return spawnedZombies.Count <= minZombie;   // ����Ʈ�� ���� minZombie �����϶����� ���
                });
            }

            float waitTime = Random.Range(spawnTimeMin, spawnTimeMax);
            yield return new WaitForSeconds(waitTime);

            // ���� Ÿ�� ���� (0: Normal, 1: Fat, 2: Crawl)
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
