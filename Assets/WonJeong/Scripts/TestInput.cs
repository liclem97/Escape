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
        // ������ Ŭ���̾�Ʈ�� Ű �Է� ����
        if (!PhotonNetwork.IsMasterClient) return;
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            //playerHealth.OnDamaged(10f);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            //playerHealth.RestoreHealth(10f);
        }
    }
}
