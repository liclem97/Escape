using Photon.Pun;
using UnityEngine;
using System.Collections;

/* 아이템을 스폰하는 아이템 스포너 */
public class ItemSpawner : MonoBehaviourPun
{
    [SerializeField] protected string spawnItemPrefabName = "";     // 생성할 아이템 프리팹
    [SerializeField] private Transform spawnPoint;                  // 아이템 생성 위치
    [SerializeField] private float itemSpawnDelay = 5f;             // 아이템 스폰 딜레이

    private GameObject currentItem;
    public GameObject CurrentItem
    {
        get => currentItem;
        set => currentItem = value;
    }

    public Transform SpawnPoint
    {
        get => spawnPoint;
    }

    public void ItemSpawnStart()
    {
        if (PhotonNetwork.IsMasterClient) // 서버에서 아이템 스폰
        {
            Debug.Log("서버에서 아이템 스폰 시작");
            StartCoroutine(SpawnRoutine(itemSpawnDelay));
        }
    }

    private IEnumerator SpawnRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        SpawnItem();
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: SpawnItem
    * 기능: 아이템을 스폰하는 함수
    ***********************************************************************************/
    private void SpawnItem()
    {
        if (currentItem != null) return;

        GameObject spawned = PhotonNetwork.Instantiate(spawnItemPrefabName, spawnPoint.position, spawnPoint.rotation);
        currentItem = spawned;

        // 스폰된 아이템에게 이 Spawner의 ViewID를 전달
        int itemViewID = spawned.GetComponent<PhotonView>().ViewID;
        int spawnerViewID = GetComponent<PhotonView>().ViewID;     

        photonView.RPC(nameof(RPC_SetItemSpawner), RpcTarget.AllBuffered, itemViewID, spawnerViewID);

        StartCoroutine(WatchAndRespawn());
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: RPC_SetItemSpawner
    * 기능: 스폰한 아이템의 부모를 아이템 스포너로 지정함
    * 입력: 
    *   - itemViewID: 스폰한 아이템의 뷰ID
    *   - spawnerViewID: 스포너의 뷰ID
    ***********************************************************************************/
    [PunRPC]
    private void RPC_SetItemSpawner(int itemViewID, int spawnerViewID)
    {
        PhotonView itemView = PhotonView.Find(itemViewID);
        PhotonView spawnerView = PhotonView.Find(spawnerViewID);

        if (itemView != null && spawnerView != null)
        {
            Item item = itemView.GetComponent<Item>();
            ItemSpawner spawner = spawnerView.GetComponent<ItemSpawner>();

            if (item != null && spawner != null)
            {
                item.Spawner = spawner;

                // 스포너를 부모로 지정 (선택)
                item.transform.SetParent(spawner.transform, worldPositionStays: true);
            }
        }
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: WatchAndRespawn
    * 기능: 스폰한 아이템이 파괴되었는지 검사하고, 파괴되었으면 spawnDelay 만큼 대기 후 다사 아이템을 스폰함
    ***********************************************************************************/
    private IEnumerator WatchAndRespawn()
    {
        while (currentItem != null)
        {
            yield return null;
        }

        yield return new WaitForSeconds(itemSpawnDelay);

        SpawnItem();
    }
}
