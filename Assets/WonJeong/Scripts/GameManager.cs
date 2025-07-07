using Photon.Pun;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }
    public static VRPlayerHealth player1Health; // �÷��̾�1�� ü�� ����
    public static float sharedHealthPercent = 1f;

    [Header("Spawn Points")]
    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;

    [Header("Stage Start Point")]
    [SerializeField] private Transform player1Stage1Point;
    [SerializeField] private Transform player2Stage1Point;

    [Header("Managers")]
    [SerializeField] private CameraMove cameraMoveManager;

    [Header("Players (Runtime Assigned)")]
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;

    [Header("Item Spawner")]
    [SerializeField] private ItemSpawner healPackItemSpawner;

    public GameObject GetPlayer1() => player1;
    public GameObject GetPlayer2() => player2;

    private bool isGameOver = false;
    public bool IsGameOver
    {
        get => isGameOver;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // PhotonView ������Ʈ�� �ʼ�
        if (photonView == null)
        {
            Debug.LogError("[GameManager] PhotonView ������Ʈ�� �����ϴ�.");
        }
    }

    private void Start()
    {
        Debug.Log($"VRPlayer Start() - IsMine: {photonView.IsMine}");
    }

    private void Update()
    {
        if (!isGameOver && player1Health != null && player1Health.currentHealth <= 0f)
        {
            isGameOver = true;
            OnGameOver();
        }
    }

    private void OnGameStart()
    {
        if (player1 == null || player2 == null) return;

        // ���� ���� �� ó�� ����Ʈ�� �ڵ� �̵�
        if (player1Stage1Point != null && player2Stage1Point != null)
        {
            CameraMove.Instance.StartCoroutine(CameraMove.Instance.MoveToPosition(player1, player1Stage1Point, 2f));
            CameraMove.Instance.StartCoroutine(CameraMove.Instance.MoveToPosition(CameraMove.Instance.Vehicle, player2Stage1Point, 2f));
        }        

        // ������ ���� ���� �� ���� ��ǥ �÷��̾� ����
        ItemTargetTrigger trigger = FindFirstObjectByType<ItemTargetTrigger>();        
        if (trigger != null && healPackItemSpawner)
        {
            trigger.FlyTarget = player1.transform;
            healPackItemSpawner.ItemSpawnStart();           
        }

        // �÷��̾�1 ������ źâ ���� ����
        VRPlayer player = player1.GetComponent<VRPlayer>();
        if (player != null)
        {
            player.StartAmmoSpawn();
        }
    }

    private void OnGameOver()
    {
        Debug.Log("[GameManager] ���� ����!");
        // TODO: �� ��ȯ, UI ��� �� �߰�
    }

    public void OnPlayerJoinedRoom()
    {
        string prefabName = "VRPlayer";

        Transform spawn = PhotonNetwork.CurrentRoom.PlayerCount == 1
            ? player1SpawnPoint
            : player2SpawnPoint;

        if (spawn == null)
        {
            Debug.LogError("[GM] Spawn point is null! Cannot instantiate player.");
            return;
        }

        // �÷��̾� ����
        GameObject player = PhotonNetwork.Instantiate(prefabName, spawn.position, spawn.rotation);

        // �г������� �̸� ���� (���ÿ����� �����)   
        player.name = PhotonNetwork.NickName;

        // ��� Ŭ���̾�Ʈ�� �÷��̾� ���Ҵ� ��� ����
        photonView.RPC(nameof(RPC_AssignPlayers), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPC_AssignPlayers()
    {
        StartCoroutine(AssignPlayersAfterDelay());
    }

    private IEnumerator AssignPlayersAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        AssignPlayers();
    }

    private void AssignPlayers()
    {
        foreach (var view in FindObjectsByType<PhotonView>(FindObjectsSortMode.None))
        {
            if (view.CompareTag("Player"))
            {
                int actorNumber = view.Owner.ActorNumber;

                if (actorNumber == 1 && player1 == null)
                {
                    player1 = view.gameObject;

                    // InitPlayer1 ȣ��
                    VRPlayer vrPlayer = player1.GetComponent<VRPlayer>();
                    vrPlayer?.InitPlayer1();
                }
                else if (actorNumber == 2 && player2 == null)
                {
                    player2 = view.gameObject;
                    if (CameraMove.Instance != null)
                    {
                        //cameraMoveManager.Player2 = player2;

                        if (CameraMove.Instance.Vehicle != null)
                        {
                            Transform attachPoint = CameraMove.Instance.Vehicle.transform.Find("AttachPoint");
                            if (attachPoint != null)
                            {
                                player2.transform.SetParent(attachPoint);
                                player2.transform.localPosition = Vector3.zero;
                                player2.transform.localRotation = Quaternion.identity;
                            }
                        }
                    }
                    // InitPlayer2 ȣ��
                    VRPlayer vrPlayer = player2.GetComponent<VRPlayer>();
                    vrPlayer?.InitPlayer2();
                }
            }
        }

        string p1Name = player1 != null ? player1.name : "null";
        string p2Name = player2 != null ? player2.name : "null";
        Debug.Log($"[GameManager] Assigned Player1: {p1Name}, Player2: {p2Name}");

        if (player1 != null)
        {
            player1Health = player1.GetComponent<VRPlayerHealth>();
            if (player1Health != null)
            {
                player1Health.photonView.RPC(nameof(VRPlayerHealth.SetInvincible), RpcTarget.AllBuffered, false); // Player1 ���� ����
            }
        }

        if (player2 != null)
        {
            VRPlayerHealth p2Health = player2.GetComponent<VRPlayerHealth>();
            if (p2Health != null)
            {
                p2Health.photonView.RPC(nameof(VRPlayerHealth.SetInvincible), RpcTarget.AllBuffered, true); // Player2 ���� ����
            }
        }

        if (player1 != null && player2 != null)
        {
            Debug.Log("[GM] �� �÷��̾ �Ҵ��. ���� ����");
            OnGameStart();
        }
    }

    [PunRPC]
    public void RPC_SetSharedHealthPercent(float value)
    {
        sharedHealthPercent = value;
        Debug.Log($"[GameManager] sharedHealthPercent ���ŵ�: {value}");
    }
}