using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    [Header("Spawn Point")]
    [SerializeField] private Transform spawnPoint;

    [Header("Managers")]
    [SerializeField] private CameraMove cameraMoveManager;

    [Header("Players")]
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;

    private void Awake()
    {
        // 싱글톤 초기화
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void OnPlayerJoinedRoom()
    {
        string prefabName = "VRPlayer";
        GameObject player = PhotonNetwork.Instantiate(prefabName, spawnPoint.position, spawnPoint.rotation);

        // PhotonView 필수
        PhotonView view = player.GetComponent<PhotonView>();
        if (view == null) return;

        // 본인에게만 CameraMove 할당
        if (view.IsMine)
        {
            // CameraMove에 로컬 플레이어 연결
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                player1 = player;
                cameraMoveManager.Player1 = player;
            }
            else if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                player2 = player;
                cameraMoveManager.Player2 = player;
            }
        }
    }
}