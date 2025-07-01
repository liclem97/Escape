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
        Debug.Log("마스터 서버 연결됨. 방 연결 시도");
        TryJoinOrCreateRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"방 입장 성공: {PhotonNetwork.CurrentRoom.Name}");

        joinedRoomEvent.Invoke();
    }

    void TryJoinOrCreateRoom()
    {
        var roomName = "myRoom";
        var options = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
    }
}
