using System.Collections;
using UnityEngine;

/* ī�޶� ����ũ ��ũ��Ʈ */
/* �̻�� */
public class CameraShake : MonoBehaviour
{
    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;

        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            Vector3 offset = new Vector3(
                Random.Range(-1f, 1f) * magnitude,
                Random.Range(-1f, 1f) * magnitude,
                Random.Range(-1f, 1f) * magnitude
            );

            transform.localPosition += offset;

            yield return null;

            transform.localPosition -= offset; // VR�� ����� �Ǳ� ������ �� ������ offset ����
        }
    }
}
