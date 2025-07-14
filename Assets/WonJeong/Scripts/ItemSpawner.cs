using Photon.Pun;
using UnityEngine;
using System.Collections;

/* �������� �����ϴ� ������ ������ */
public class ItemSpawner : MonoBehaviourPun
{
    [SerializeField] protected string spawnItemPrefabName = "";     // ������ ������ ������
    [SerializeField] private Transform spawnPoint;                  // ������ ���� ��ġ
    [SerializeField] private float itemSpawnDelay = 5f;             // ������ ���� ������

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
        if (PhotonNetwork.IsMasterClient) // �������� ������ ����
        {
            Debug.Log("�������� ������ ���� ����");
            StartCoroutine(SpawnRoutine(itemSpawnDelay));
        }
    }

    private IEnumerator SpawnRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        SpawnItem();
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: SpawnItem
    * ���: �������� �����ϴ� �Լ�
    ***********************************************************************************/
    private void SpawnItem()
    {
        if (currentItem != null) return;

        GameObject spawned = PhotonNetwork.Instantiate(spawnItemPrefabName, spawnPoint.position, spawnPoint.rotation);
        currentItem = spawned;

        // ������ �����ۿ��� �� Spawner�� ViewID�� ����
        int itemViewID = spawned.GetComponent<PhotonView>().ViewID;
        int spawnerViewID = GetComponent<PhotonView>().ViewID;     

        photonView.RPC(nameof(RPC_SetItemSpawner), RpcTarget.AllBuffered, itemViewID, spawnerViewID);

        StartCoroutine(WatchAndRespawn());
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: RPC_SetItemSpawner
    * ���: ������ �������� �θ� ������ �����ʷ� ������
    * �Է�: 
    *   - itemViewID: ������ �������� ��ID
    *   - spawnerViewID: �������� ��ID
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

                // �����ʸ� �θ�� ���� (����)
                item.transform.SetParent(spawner.transform, worldPositionStays: true);
            }
        }
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: WatchAndRespawn
    * ���: ������ �������� �ı��Ǿ����� �˻��ϰ�, �ı��Ǿ����� spawnDelay ��ŭ ��� �� �ٻ� �������� ������
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
