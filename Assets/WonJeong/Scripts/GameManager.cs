using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    [Header("Spawn Point")]
    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;

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

        Transform spawn = null;

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            spawn = player1SpawnPoint;
        else if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            spawn = player2SpawnPoint;

        if (spawn == null)
        {
            Debug.LogError("Spawn point is not assigned or PlayerCount is invalid.");
            return;
        }

        GameObject player = PhotonNetwork.Instantiate(prefabName, spawn.position, spawn.rotation);

        PhotonView view = player.GetComponent<PhotonView>();
        if (view == null) return;

        // 로컬 플레이어만 CameraMove에 할당
        if (view.IsMine)
        {
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