using UnityEngine;

/* 크로스헤어가 항상 일정 크기로 보이도록 하는 스크립트 */
public class ReticleBillboard : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;       // 인스펙터에서 할당
    [SerializeField] private float fixedScale = 3f; // 1미터 거리에서의 기준 크기

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        // 카메라를 바라보게 회전
        transform.forward = targetCamera.transform.forward;

        // 카메라로부터의 거리 기반 스케일 유지
        float distance = Vector3.Distance(transform.position, targetCamera.transform.position);
        float scale = fixedScale * distance;
        transform.localScale = new Vector3(scale, scale, scale);
    }
}
