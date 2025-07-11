using UnityEngine;
using Photon.Pun;

public class ExitGameTrigger : MonoBehaviourPun
{
    [SerializeField] private float exitDelay = 5f; // 종료까지 대기 시간 (초)

    private void OnEnable()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPC_TriggerGameExit), RpcTarget.AllBuffered, exitDelay);
        }
    }

    [PunRPC]
    private void RPC_TriggerGameExit(float delay)
    {
        StartCoroutine(ExitAfterDelay(delay));
    }

    private System.Collections.IEnumerator ExitAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Debug.Log("[ExitGameTrigger] 게임 종료");

        // 여기에서 원하는 방식으로 게임 종료
        // 예: 로비로 씬 이동
        //PhotonNetwork.LoadLevel("LobbyScene"); // 씬 이름은 상황에 맞게 수정
        Application.Quit();
    }
}