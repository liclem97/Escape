using Photon.Pun;
using UnityEngine;

/* 적 애니메이션에 따른 행동 함수 */
public class EnemyAnimationEvent : MonoBehaviour
{
    [Header("Colliders")]
    [SerializeField] private BoxCollider attackCollider;    // 일반 좀비의 공격 콜라이더
    [SerializeField] private BoxCollider throwCollider1;    // 보스 좀비가 공격 시 활성화되는 공격 콜라이더1
    [SerializeField] private BoxCollider throwCollider2;    // 보스 좀비가 공격 시 활성화되는 공격 콜라이더2

    [Header("Sounds")]
    [SerializeField] private AudioClip idleSound;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip dieSound;
    [SerializeField] private AudioClip hitSound;            // 각 사운드 저장

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


    /***********************************************************************************
    * 작성자: 박원정
    * 함수: OnBossDied
    * 기능: 보스 사망 시 게임 매니저에서 OnGameClear 함수를 호출
    ***********************************************************************************/
    public void OnBossDied()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameClear();
        }
    }
}