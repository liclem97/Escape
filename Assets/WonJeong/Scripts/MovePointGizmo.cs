using UnityEngine;

public class MovePointGizmo : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        // ���� ������Ʈ�� ��ġ�� ����
        Vector3 position = transform.position;
        Vector3 forward = transform.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(position, 1f);

        // ȭ��ǥ ����
        Gizmos.color = Color.cyan;

        // ���� ����
        float arrowLength = 2f;

        // �� �׸���
        Gizmos.DrawLine(position, position + forward * arrowLength);

        // ���� ���� �ﰢ��ó�� ȭ���� ǥ��
        Vector3 right = Quaternion.LookRotation(forward) * Quaternion.Euler(0, 150, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(forward) * Quaternion.Euler(0, -150, 0) * Vector3.forward;

        Gizmos.DrawLine(position + forward * arrowLength, position + forward * arrowLength + right * 0.3f);
        Gizmos.DrawLine(position + forward * arrowLength, position + forward * arrowLength + left * 0.3f);
    }
}
