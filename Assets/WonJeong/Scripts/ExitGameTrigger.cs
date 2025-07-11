using UnityEngine;
using Photon.Pun;

public class ExitGameTrigger : MonoBehaviourPun
{
    [SerializeField] private float exitDelay = 5f; // ������� ��� �ð� (��)

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

        Debug.Log("[ExitGameTrigger] ���� ����");

        // ���⿡�� ���ϴ� ������� ���� ����
        // ��: �κ�� �� �̵�
        //PhotonNetwork.LoadLevel("LobbyScene"); // �� �̸��� ��Ȳ�� �°� ����
        Application.Quit();
    }
}