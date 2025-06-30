using UnityEngine;

// ���콺 �Է¿� ���� ī�޶� ȸ��
public class CameraRotate : MonoBehaviour
{
    [Header("Sensivity")]
    [SerializeField] private float mouseSensivity = 200f;

    private Vector3 angle;

    private void Start()
    {
        // �����Ҷ� ���� ī�޶��� ������ �����Ѵ�.
        angle.y = -Camera.main.transform.eulerAngles.x;
        angle.x = Camera.main.transform.eulerAngles.y;
        angle.z = Camera.main.transform.eulerAngles.z;
    }

    private void Update()
    {
        // ���콺 �Է¿� ���� ī�޶� ȸ�� ��Ű�� �ʹ�.
        // 1. ������� ���콺 �Է��� ���;� �Ѵ�.
        // ���콺�� �¿� �Է��� �޴´�.
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");

        // 2. ������ �ʿ��ϴ�.
        // �̵� ���Ŀ� �����Ͽ� �� �Ӽ����� ȸ�� ���� ���� ��Ų��.
        angle.x += x * mouseSensivity * Time.deltaTime;
        angle.y += y * mouseSensivity * Time.deltaTime;

        angle.y = Mathf.Clamp(angle.y, -90, 90);
        // 3. ȸ�� ��Ű�� �ʹ�.
        // ī�޶��� ȸ������ ���� ������� ȸ�� ���� �Ҵ��Ѵ�.
        transform.eulerAngles = new Vector3(-angle.y, angle.x, transform.eulerAngles.z);
    }
}
