using Photon.Pun;
using System.Collections;
using UnityEngine;

public class ThrowBall : MonoBehaviourPun
{
    [Header("Ball Stats")]
    [SerializeField] private float growDuration = 1.0f; // 커지는 데 걸리는 시간
    [SerializeField] private float ballRange = 2f;

    [Header("FX")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private AudioClip explosionSound;    

    private AudioSource audioSource;

    private Vector3 startScale = Vector3.one * 0.001f;
    private Vector3 targetScale = Vector3.one * 0.02f;

    private bool isExploded = false;

    public float ballDamage;

    private void Start()
    {   
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();       
    }

    public void StartGrow()
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

        //yield return new WaitForSeconds(2f);
    }

    [PunRPC]
    public void ThrowToTarget(Vector3 targetPosition)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        transform.SetParent(null); // 손에서 분리

        Vector3 direction = (targetPosition - transform.position).normalized;
        float throwForce = 15f; // 힘의 세기

        rb.isKinematic = false;
        rb.AddForce(direction * throwForce, ForceMode.VelocityChange);
    }

    public void Explode()
    {
        photonView.RPC(nameof(RPC_Explode), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPC_Explode()
    {
        if (isExploded) return;
        isExploded = true;

        // 폭발 로직을 서버에서만 실행
        if (PhotonNetwork.IsMasterClient)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, ballRange);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out IDamageable target))
                {
                    float distance = Vector3.Distance(transform.position, hit.transform.position);
                    float damageRatio = Mathf.Clamp01(1f - distance / ballRange); // 가까울수록 1, 멀수록 0
                    float finalDamage = ballRange * damageRatio;

                    Debug.Log($"target: {hit.name}");

                    target.TakeDamage(finalDamage, photonView.ViewID);
                }
            }
        }

        // 폭발 효과는 모두에게 공유
        photonView.RPC(nameof(SpawnExplodeFX), RpcTarget.All);

        // 일정 시간 후 제거
        photonView.RPC(nameof(RPC_DestroySelf), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_DestroySelf()
    {
        if (gameObject && PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    private void SpawnExplodeFX()
    {
        if (explosionEffect != null)
        {
            GameObject instance = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            var ps = instance.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            Destroy(instance, 4f);
        }

        if (explosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<VRPlayer>())
        {
            Explode();
        }
    }
}
