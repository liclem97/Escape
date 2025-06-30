using UnityEngine;

public class Bomb : MonoBehaviour, IDamageable
{
    [Header("Bomb Stat")]
    [SerializeField] private float bombHealth = 30f;      // ü���� 0�� �Ǹ� ��ź�� ����
    [SerializeField] private float bombDamage;      // ��ź�� �����
    [SerializeField] private float boomRadius;      // ���� �ݰ�

    [Header("Bomb FX")]
    [SerializeField] private AudioClip boomSound;   // ���� �Ҹ�
    [SerializeField] private GameObject boomEffect; // ���� ����Ʈ

    public void OnDamaged(float damageAmount)
    {
        bombHealth -= damageAmount;

        if (bombHealth <= 0)
        {
            Boom();
            Destroy(gameObject);
        }
    }

    private void Boom()
    {
        SpawnBoomEffect();
        Debug.Log("��ź�� ������");
    }

    private void SpawnBoomEffect()
    {
        Debug.Log("���� ����Ʈ ���");
        //Instantiate(boomEffect, transform.position, Quaternion.identity);
    }
}
