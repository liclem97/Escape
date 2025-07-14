using UnityEngine;
using Photon.Pun;

/* 보스 스테이지 볼륨 스크립트 */
public class StageVolume_Boss : StageVolume
{
    [Header("Boss Enemy")]
    [SerializeField] private GameObject bossPrefabs;    // 보스 프리팹

    [Header("Zombie Spawners")]
    [SerializeField] private GameObject[] zSpawners;    // 좀비 스포너 프리팹

    protected override void Awake()
    {   
        bossPrefabs.SetActive(false);
        base.Awake(); // 일반 좀비들 모두 비활성화
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
        // 보스와 좀비 스포너를 추가로 활성화 한다
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

    // 트리거에 플레이어 진입 시 보스 BGM 재생
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        GameManager.Instance.PlayBossBGM();
    }
}
