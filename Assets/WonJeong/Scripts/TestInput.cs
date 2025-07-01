using UnityEngine;

// 테스트용 스크립트
public class TestInput : MonoBehaviour
{
    [Header("Test")]
    //[SerializeField] PlayerHealth playerHealth;
    [SerializeField] CameraMove cameraMove;

    private void Update()
    {
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
