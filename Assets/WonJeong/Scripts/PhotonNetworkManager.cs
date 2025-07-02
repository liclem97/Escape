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
        Debug.Log("마스터 서버 연결됨. 방 연결 시도");
        TryJoinOrCreateRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"방 입장 성공: {PhotonNetwork.CurrentRoom.Name}");

        // 마스터 클라이언트는 Player1, 그 외는 Player2로 닉네임 설정
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
