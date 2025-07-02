using Photon.Pun;
using UnityEngine;

// 테스트용 스크립트
public class TestInput : MonoBehaviourPun
{
    [Header("Test")]
    //[SerializeField] PlayerHealth playerHealth;
    [SerializeField] CameraMove cameraMove;

    private void Update()
    {
        // 마스터 클라이언트만 조작 가능
        if (!PhotonNetwork.IsMasterClient) return;

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (GameManager.player1Health != null)
            {
                GameManager.player1Health.TakeDamage(10f);
                //photonView.RPC(nameof(RPC_PlayerTakeDamage), RpcTarget.AllBuffered, 10f);
                Debug.Log($"[TestInput] 체력 감소: {GameManager.player1Health.currentHealth}");

            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (GameManager.player1Health != null)
            {
                GameManager.player1Health.RestoreHealth(10f);                
                //photonView.RPC(nameof(RPC_PlayerRestoreHealth), RpcTarget.AllBuffered, 10f);
                Debug.Log($"[TestInput] 체력 회복: {GameManager.player1Health.currentHealth}");
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
