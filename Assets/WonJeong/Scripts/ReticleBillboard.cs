using UnityEngine;

/* ũ�ν��� �׻� ���� ũ��� ���̵��� �ϴ� ��ũ��Ʈ */
public class ReticleBillboard : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;       // �ν����Ϳ��� �Ҵ�
    [SerializeField] private float fixedScale = 3f; // 1���� �Ÿ������� ���� ũ��

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        // ī�޶� �ٶ󺸰� ȸ��
        transform.forward = targetCamera.transform.forward;

        // ī�޶�κ����� �Ÿ� ��� ������ ����
        float distance = Vector3.Distance(transform.position, targetCamera.transform.position);
        float scale = fixedScale * distance;
        transform.localScale = new Vector3(scale, scale, scale);
    }
}
