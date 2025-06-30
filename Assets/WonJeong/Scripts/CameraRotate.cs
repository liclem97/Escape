using UnityEngine;

// 마우스 입력에 따라 카메라를 회전
public class CameraRotate : MonoBehaviour
{
    [Header("Sensivity")]
    [SerializeField] private float mouseSensivity = 200f;

    private Vector3 angle;

    private void Start()
    {
        // 시작할때 현재 카메라의 각도를 적용한다.
        angle.y = -Camera.main.transform.eulerAngles.x;
        angle.x = Camera.main.transform.eulerAngles.y;
        angle.z = Camera.main.transform.eulerAngles.z;
    }

    private void Update()
    {
        // 마우스 입력에 따라 카메라를 회전 시키고 싶다.
        // 1. 사용자의 마우스 입력을 얻어와야 한다.
        // 마우스의 좌우 입력을 받는다.
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");

        // 2. 방향이 필요하다.
        // 이동 공식에 대입하여 각 속성별로 회전 값을 누적 시킨다.
        angle.x += x * mouseSensivity * Time.deltaTime;
        angle.y += y * mouseSensivity * Time.deltaTime;

        angle.y = Mathf.Clamp(angle.y, -90, 90);
        // 3. 회전 시키고 싶다.
        // 카메라의 회전값에 새로 만들어진 회전 값을 할당한다.
        transform.eulerAngles = new Vector3(-angle.y, angle.x, transform.eulerAngles.z);
    }
}
