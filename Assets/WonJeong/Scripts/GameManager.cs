using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }
    public static VRPlayerHealth player1Health; // 플레이어1의 체력 참조

    [Header("Spawn Points")]
    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;

    [Header("Managers")]
    [SerializeField] private CameraMove cameraMoveManager;

    [Header("Players (Runtime Assigned)")]
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;

    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // PhotonView 컴포넌트가 필수
        if (photonView == null)
        {
            Debug.LogError("[GameManager] PhotonView 컴포넌트가 없습니다.");
        }

        // cameraMoveManager가 없을 경우 자동으로 찾기 (예외 방지)
        if (cameraMoveManager == null)
        {
            cameraMoveManager = FindFirstObjectByType<CameraMove>();
            if (cameraMoveManager == null)
            {
                Debug.LogError("[GameManager] CameraMoveManager를 찾을 수 없습니다.");
            }
        }
    }

    private void Update()
    {
        if (!isGameOver && player1Health != null && player1Health.currentHealth <= 0f)
        {
            isGameOver = true;
            OnGameOver();
        }
    }

    private void OnGameOver()
    {
        Debug.Log("[GameManager] 게임 오버!");
        // TODO: 씬 전환, UI 출력 등 추가
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

        PhotonNetwork.Instantiate(prefabName, spawn.position, spawn.rotation);

        // 모든 클라이언트에 플레이어 재할당 명령 전달
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
                    if (cameraMoveManager != null)
                        cameraMoveManager.Player1 = player1;
                }
                else if (actorNumber == 2 && player2 == null)
                {
                    player2 = view.gameObject;
                    if (cameraMoveManager != null)
                        cameraMoveManager.Player2 = player2;
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
                player1Health.photonView.RPC(nameof(VRPlayerHealth.SetInvincible), RpcTarget.AllBuffered, false); // Player1 무적 해제
            }
        }

        if (player2 != null)
        {
            VRPlayerHealth p2Health = player2.GetComponent<VRPlayerHealth>();
            if (p2Health != null)
            {
                p2Health.photonView.RPC(nameof(VRPlayerHealth.SetInvincible), RpcTarget.AllBuffered, true); // Player2 무적 설정
            }
        }
    }
}
