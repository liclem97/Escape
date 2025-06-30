using UnityEngine;

public class Bomb : MonoBehaviour, IDamageable
{
    [Header("Bomb Stat")]
    [SerializeField] private float bombHealth = 30f;      // Ã¼·ÂÀÌ 0ÀÌ µÇ¸é ÆøÅºÀÌ ÅÍÁü
    [SerializeField] private float bombDamage;      // ÆøÅºÀÇ ´ë¹ÌÁö
    [SerializeField] private float boomRadius;      // Æø¹ß ¹İ°æ

    [Header("Bomb FX")]
    [SerializeField] private AudioClip boomSound;   // Æø¹ß ¼Ò¸®
    [SerializeField] private GameObject boomEffect; // Æø¹ß ÀÌÆåÆ®

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
        Debug.Log("ÆøÅºÀÌ Æø¹ßÇÔ");
    }

    private void SpawnBoomEffect()
    {
        Debug.Log("Æø¹ß ÀÌÆåÆ® Ãâ·Â");
        //Instantiate(boomEffect, transform.position, Quaternion.identity);
    }
}
