using System.Collections;
using UnityEngine;

// ������ ��Ʈ�ѷ����� ������ ����
public class RayVisualizer : MonoBehaviour
{
    [Header("Ray")]
    [SerializeField] private LineRenderer Ray;            // ���� ������
    [SerializeField] private LayerMask HitRayMask;        // ������ ���̾� ����ũ
    [SerializeField] private float Distance = 100f;       // ���� �ִ� �Ÿ�

    [Header("Reticle Point")]
    [SerializeField] private GameObject ReticlePoint;     // ��Ʈ�� �׸� ����Ʈ
    [SerializeField] private bool ShowReticle = true;
    [SerializeField] private float baseScale = 0.05f;     // ũ�ν������ ���� ������ (���ϴ� ũ��)

    private void Awake()
    {
        On();
    }

    public void On()
    {
        StopAllCoroutines();            // ��� �ڷ�ƾ�� ���߰�,
        StartCoroutine(Process());      // Process �ڷ�ƾ�� �����Ѵ�.
    }

    public void Off()
    {
        StopAllCoroutines();            // �ڷ�ƾ, ����, ReticlePoint�� ��� ��Ȱ��ȭ
        Ray.enabled = false;
        ReticlePoint.SetActive(false);
    }

    /*********************************************************************************************
    �ۼ���: �ڿ���
    �Լ�: Process
    ���: �����տ��� �����ϴ� ���̸� �׸�
    �ۼ�����: 2025-06-30
    *********************************************************************************************/
    private IEnumerator Process()
    {
        while (true)
        {
            // LineTrace: ���� ��ġ, ����, HitResult, �Ÿ�, Layer
            if (Physics.Raycast(ARAVRInput.RHandPosition, ARAVRInput.RHandDirection, out RaycastHit HitInfo, Distance, HitRayMask))
            {
                Ray.SetPosition(1, transform.InverseTransformPoint(HitInfo.point)); // ������ �� ���� HitInfo.Point�� �����ϵ� ���� ��ǥ -> ���� ��ǥ��
                Ray.enabled = true;

                ReticlePoint.transform.position = HitInfo.point;
                ReticlePoint.SetActive(ShowReticle);                    // ���� ������ ũ�ν��� �׸�
                ReticlePoint.transform.LookAt(Camera.main.transform);   // ReticlePoint == ũ�ν��� �׻� ī�޶� �ٶ󺸵��� ��

                // ũ�ν���� ũ�� ����
                float distanceToCamera = Vector3.Distance(Camera.main.transform.position, HitInfo.point);
               

                ReticlePoint.transform.localScale = Vector3.one * (baseScale * distanceToCamera);
            }
            else
            {
                Ray.enabled = false;
                ReticlePoint.SetActive(false);
            }

            yield return null;
        }
    }
}
