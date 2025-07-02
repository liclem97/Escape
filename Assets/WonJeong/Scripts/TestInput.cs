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
        // 마스터 클라이언트만 키 입력 받음
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
