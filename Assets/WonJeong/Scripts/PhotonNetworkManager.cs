using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] UnityEvent joinedRoomEvent;
    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("������ ���� �����. �� ���� �õ�");
        TryJoinOrCreateRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"�� ���� ����: {PhotonNetwork.CurrentRoom.Name}");

        // ������ Ŭ���̾�Ʈ�� Player1, �� �ܴ� Player2�� �г��� ����
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.NickName = "Player1 (Master)";
        }
        else
        {
            PhotonNetwork.NickName = "Player2";
        }

        joinedRoomEvent?.Invoke();
    }

    void TryJoinOrCreateRoom()
    {
        var roomName = "myRoom";
        var options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
    }
}
