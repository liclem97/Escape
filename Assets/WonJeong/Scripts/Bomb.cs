using Photon.Pun;
using UnityEngine;

/*  씬에서 사용하는 폭탄 스크립트    */
public class Bomb : MonoBehaviourPun, IDamageable
{
    [Header("Bomb Stats")]
    [SerializeField] private float bombDamage = 100f;           // 폭발 대미지
    [SerializeField] private float bombRange = 5f;              // 폭발 범위
    [SerializeField] private float bombHP = 100f;               // 폭탄의 HP

    [Header("FX")]   
    [SerializeField] private GameObject bombExplosionEffect;
    [SerializeField] private AudioClip bombSound;               // 폭탄의 폭발 이펙트 및 소리

    private float currentHP;
    private bool isExploded = false;
    private AudioSource audioSource;

    private void Awake()
    {
        currentHP = bombHP;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    // IDamageable 인터페이스 함수
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
        photonView.RPC(nameof(RPC_Explode), RpcTarget.AllBuffered);
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: RPC_Explode
    * 기능: 모든 클라이언트에서 폭탄을 폭발시킴    
    ***********************************************************************************/
    [PunRPC]
    private void RPC_Explode()
    {
        isExploded = true;

        // 폭발 로직을 서버에서만 실행
        if (PhotonNetwork.IsMasterClient)
        {   
            // 폭발 범위 내의 모든 콜라이더 수집
            Collider[] hits = Physics.OverlapSphere(transform.position, bombRange);
            foreach (var hit in hits)
            {   
                // 수집한 콜라이더가 IDamageable를 상속 받는지 확인
                if (hit.TryGetComponent(out IDamageable target))
                {
                    float distance = Vector3.Distance(transform.position, hit.transform.position);
                    float damageRatio = Mathf.Clamp01(1f - distance / bombRange); // 가까울수록 1, 멀수록 0
                    float finalDamage = bombDamage * damageRatio;   // 폭탄의 거리에 따른 대미지 증감률

                    target.TakeDamage(finalDamage, photonView.ViewID);
                }
            }
        }

        // 폭발 효과는 모두에게 공유
        photonView.RPC(nameof(RPC_PlayExplosionEffect), RpcTarget.All);
        
        // 모든 클라이언트에서 폭탄 제거
        photonView.RPC(nameof(RPC_DestroyObject), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPC_DestroyObject()
    {
        Destroy(gameObject);
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: RPC_PlayExplosionEffect
    * 기능: 모든 클라이언트에서 폭발 이펙트와 소리를 재생함  
    ***********************************************************************************/
    [PunRPC]
    private void RPC_PlayExplosionEffect()
    {
        if (bombExplosionEffect != null)
        {   
            // 폭발 이펙트 프리팹 생성
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
