using UnityEngine;

public class MovePointGizmo : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        // 현재 오브젝트의 위치와 방향
        Vector3 position = transform.position;
        Vector3 forward = transform.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(position, 1f);

        // 화살표 색상
        Gizmos.color = Color.cyan;

        // 길이 지정
        float arrowLength = 2f;

        // 선 그리기
        Gizmos.DrawLine(position, position + forward * arrowLength);

        // 끝에 작은 삼각형처럼 화살촉 표시
        Vector3 right = Quaternion.LookRotation(forward) * Quaternion.Euler(0, 150, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(forward) * Quaternion.Euler(0, -150, 0) * Vector3.forward;

        Gizmos.DrawLine(position + forward * arrowLength, position + forward * arrowLength + right * 0.3f);
        Gizmos.DrawLine(position + forward * arrowLength, position + forward * arrowLength + left * 0.3f);
    }
}
