using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] UnityEvent joinedRoomEvent;
    private void Start()
    {
        PhotonNetwork.NickName = "Player" + Random.Range(1, 9999);
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

        joinedRoomEvent.Invoke();
    }

    void TryJoinOrCreateRoom()
    {
        var roomName = "myRoom";
        var options = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
    }
}
