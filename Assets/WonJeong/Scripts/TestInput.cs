using Photon.Pun;
using UnityEngine;

// �׽�Ʈ�� ��ũ��Ʈ
public class TestInput : MonoBehaviourPun
{
    [Header("Test")]
    //[SerializeField] PlayerHealth playerHealth;
    [SerializeField] CameraMove cameraMove;

    private void Update()
    {
        // ������ Ŭ���̾�Ʈ�� ���� ����
        if (!PhotonNetwork.IsMasterClient) return;

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (GameManager.player1Health != null)
            {
                GameManager.player1Health.TakeDamage(10f);
                //photonView.RPC(nameof(RPC_PlayerTakeDamage), RpcTarget.AllBuffered, 10f);
                Debug.Log($"[TestInput] ü�� ����: {GameManager.player1Health.currentHealth}");

            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (GameManager.player1Health != null)
            {
                GameManager.player1Health.RestoreHealth(10f);                
                //photonView.RPC(nameof(RPC_PlayerRestoreHealth), RpcTarget.AllBuffered, 10f);
                Debug.Log($"[TestInput] ü�� ȸ��: {GameManager.player1Health.currentHealth}");
            }
        }
    }

    [PunRPC]
    private void RPC_PlayerTakeDamage(float amount)
    {
        GameManager.player1Health.TakeDamage(10f);
    }

    [PunRPC]
    private void RPC_PlayerRestoreHealth(float amount)
    {
        GameManager.player1Health.RestoreHealth(10f);
    }
}
