using Photon.Pun;
using UnityEngine;

public class Bomb : MonoBehaviourPun, IDamageable
{
    [Header("Bomb Stats")]
    [SerializeField] private float bombDamage = 100f;
    [SerializeField] private float bombRange = 5f;
    [SerializeField] private float bombHP = 100f;

    [Header("FX")]
   // [SerializeField] private ParticleSystem bombSmokeEffect; 
    [SerializeField] private GameObject bombExplosionEffect;
    [SerializeField] private AudioClip bombSound;

    private float currentHP;
    private bool isExploded = false;
    private AudioSource audioSource;

    private void Awake()
    {
        currentHP = bombHP;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    //IDamageable
    public void TakeDamage(float amount, int instigatorID)
    {
        if (isExploded) return;

        currentHP -= amount;

        if (currentHP <= 0f)
        {
            Explode();
        }
    }

    private void Explode()
    {
        isExploded = true;

        // 폭발 로직을 서버에서만 실행
        if (PhotonNetwork.IsMasterClient)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, bombRange);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out IDamageable target))
                {
                    float distance = Vector3.Distance(transform.position, hit.transform.position);
                    float damageRatio = Mathf.Clamp01(1f - distance / bombRange); // 가까울수록 1, 멀수록 0
                    float finalDamage = bombDamage * damageRatio;

                    target.TakeDamage(finalDamage, 0);
                }
            }
        }

        // 폭발 효과는 모두에게 공유
        photonView.RPC(nameof(RPC_PlayExplosionEffect), RpcTarget.All);

        // 일정 시간 후 제거
        Destroy(gameObject, 0.5f);
    }

    [PunRPC]
    private void RPC_PlayExplosionEffect()
    {
        if (bombExplosionEffect != null)
        {
            GameObject instance = Instantiate(bombExplosionEffect, transform.position, Quaternion.identity);
            var ps = instance.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            Destroy(instance, 4f);
        }

        if (bombSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(bombSound);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, bombRange);
    }
}
