using Photon.Pun;
using UnityEngine;

/*  ������ ����ϴ� ��ź ��ũ��Ʈ    */
public class Bomb : MonoBehaviourPun, IDamageable
{
    [Header("Bomb Stats")]
    [SerializeField] private float bombDamage = 100f;           // ���� �����
    [SerializeField] private float bombRange = 5f;              // ���� ����
    [SerializeField] private float bombHP = 100f;               // ��ź�� HP

    [Header("FX")]   
    [SerializeField] private GameObject bombExplosionEffect;
    [SerializeField] private AudioClip bombSound;               // ��ź�� ���� ����Ʈ �� �Ҹ�

    private float currentHP;
    private bool isExploded = false;
    private AudioSource audioSource;

    private void Awake()
    {
        currentHP = bombHP;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    // IDamageable �������̽� �Լ�
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
    * �ۼ���: �ڿ���
    * �Լ�: RPC_Explode
    * ���: ��� Ŭ���̾�Ʈ���� ��ź�� ���߽�Ŵ    
    ***********************************************************************************/
    [PunRPC]
    private void RPC_Explode()
    {
        isExploded = true;

        // ���� ������ ���������� ����
        if (PhotonNetwork.IsMasterClient)
        {   
            // ���� ���� ���� ��� �ݶ��̴� ����
            Collider[] hits = Physics.OverlapSphere(transform.position, bombRange);
            foreach (var hit in hits)
            {   
                // ������ �ݶ��̴��� IDamageable�� ��� �޴��� Ȯ��
                if (hit.TryGetComponent(out IDamageable target))
                {
                    float distance = Vector3.Distance(transform.position, hit.transform.position);
                    float damageRatio = Mathf.Clamp01(1f - distance / bombRange); // �������� 1, �ּ��� 0
                    float finalDamage = bombDamage * damageRatio;   // ��ź�� �Ÿ��� ���� ����� ������

                    target.TakeDamage(finalDamage, photonView.ViewID);
                }
            }
        }

        // ���� ȿ���� ��ο��� ����
        photonView.RPC(nameof(RPC_PlayExplosionEffect), RpcTarget.All);
        
        // ��� Ŭ���̾�Ʈ���� ��ź ����
        photonView.RPC(nameof(RPC_DestroyObject), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPC_DestroyObject()
    {
        Destroy(gameObject);
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: RPC_PlayExplosionEffect
    * ���: ��� Ŭ���̾�Ʈ���� ���� ����Ʈ�� �Ҹ��� �����  
    ***********************************************************************************/
    [PunRPC]
    private void RPC_PlayExplosionEffect()
    {
        if (bombExplosionEffect != null)
        {   
            // ���� ����Ʈ ������ ����
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
