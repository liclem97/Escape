using System.Collections;
using UnityEngine;

/* 카메라 셰이크 스크립트 */
/* 미사용 */
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

            transform.localPosition -= offset; // VR은 덮어쓰기 되기 때문에 매 프레임 offset 제거
        }
    }
}
