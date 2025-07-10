using UnityEngine;
using Photon.Pun;

public class StageVolume_Boss : StageVolume
{
    [Header("Boss Enemy")]
    [SerializeField] private GameObject bossPrefabs;

    [Header("Zombie Spawners")]
    [SerializeField] private GameObject[] zSpawners;

    protected override void Awake()
    {   
        bossPrefabs.SetActive(false);
        base.Awake(); // �Ϲ� ����� ��� ��Ȱ��ȭ
    }

    [PunRPC]
    protected override void ActivateVolume()
    {
        if (isActivated) return;

        isActivated = true;
        SetEnemiesActive(true);
    }

    protected override void SetEnemiesActive(bool active)
    {   
        if (bossPrefabs != null)
        {
            bossPrefabs.SetActive(active);
        }

        foreach(var spawner in zSpawners)
        {
            if (spawner != null)
                spawner.SetActive(active);
        }

        base.SetEnemiesActive(active);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }
}
