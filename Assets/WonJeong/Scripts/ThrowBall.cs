using Photon.Pun;
using System.Collections;
using UnityEngine;

public class ThrowBall : MonoBehaviourPun
{
    [Header("Scale Settings")]
    [SerializeField] private float growDuration = 1.0f; // 커지는 데 걸리는 시간

    private Vector3 startScale = Vector3.one * 0.001f;
    private Vector3 targetScale = Vector3.one * 0.01f;

    private void Start()
    {
        if (photonView.IsMine)
        {
            StartCoroutine(CoGrow());
        }
    }

    private IEnumerator CoGrow()
    {
        float elapsed = 0f;
        transform.localScale = startScale;

        while (elapsed < growDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / growDuration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;

        yield return new WaitForSeconds(2f);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
        }
    }
}
