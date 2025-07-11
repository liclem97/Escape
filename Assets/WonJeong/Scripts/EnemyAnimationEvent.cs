using Photon.Pun;
using UnityEngine;

public class EnemyAnimationEvent : MonoBehaviour
{
    [Header("Colliders")]
    [SerializeField] private BoxCollider attackCollider;
    [SerializeField] private BoxCollider throwCollider1;
    [SerializeField] private BoxCollider throwCollider2;

    [Header("Sounds")]
    [SerializeField] private AudioClip idleSound;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip dieSound;
    [SerializeField] private AudioClip hitSound;

    public void attackColliderOn()
    {
        // attackCollider이가 유효하고 꺼져있음
        if (attackCollider != null && attackCollider.enabled == false)
        {
            attackCollider.enabled = true;
        }
    }

    public void attackColliderOff()
    {
        // attackCollider이가 유효하고 켜져있음
        if (attackCollider != null && attackCollider.enabled == true)
        {
            attackCollider.enabled = false;
        }
    }

    public void DestoryEnemy()
    {
        if (gameObject != null && PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    public void throwColliderOn()
    {
        if (throwCollider1 != null && throwCollider2 != null &&
            throwCollider1.enabled == false && throwCollider2.enabled == false)
        {
            throwCollider1.enabled = true;
            throwCollider2.enabled = true;
        }
    }

    public void throwColliderOff()
    {
        if (throwCollider1 != null && throwCollider2 != null &&
            throwCollider1.enabled == true && throwCollider2.enabled == true)
        {
            throwCollider1.enabled = false;
            throwCollider2.enabled = false;
        }
    }

    public void PlayIdleSound()
    {
        if (TryGetComponent<AudioSource>(out AudioSource audioSource) && idleSound)
        {
            audioSource.PlayOneShot(idleSound);
        }
    }

    public void PlayAttackSound()
    {
        if (TryGetComponent<AudioSource>(out AudioSource audioSource) && attackSound)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }

    public void PlayDieSound()
    {
        if (TryGetComponent<AudioSource>(out AudioSource audioSource) && dieSound)
        {
            audioSource.PlayOneShot(dieSound);
        }
    }

    public void PlayHitSound()
    {
        if (TryGetComponent<AudioSource>(out AudioSource audioSource) && hitSound)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }

    public void OnBossDied()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameClear();
        }
    }
}