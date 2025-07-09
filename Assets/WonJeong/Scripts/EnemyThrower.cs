using Photon.Pun;
using System.Collections;
using UnityEngine;

public class EnemyThrower : EnemyBase
{
    [Header("SpawnPoint")]
    [SerializeField] private Transform ballAttachPoint;

    private GameObject ball;

    [HideInInspector]
    public float maxHealth;

    protected override void Start()
    {
        base.Start();
        maxHealth = health;
    }

    protected override IEnumerator CoAttack()
    {
        while (target != null && GameManager.Instance != null && !GameManager.Instance.IsGameOver)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // Attack 애니메이션 상태가 아닐 때만 트리거 발동
            if (!stateInfo.IsName("Throw"))
            {                
                animator.SetTrigger("Throw");
                animator.SetFloat("Speed", 0f);

                SpawnThrowBall();
            }

            // 공격 딜레이 대기
            yield return new WaitForSeconds(attackDelay);

            if (target == null || GameManager.Instance.IsGameOver)
            {
                photonView.RPC(nameof(RPC_ChangeState), RpcTarget.All, EnemyState.Idle);
            }
        }
    }

    public void SpawnThrowBall()
    {
        if (!photonView.IsMine) return;

        ball = PhotonNetwork.Instantiate("ThrowBall", ballAttachPoint.position, ballAttachPoint.rotation);
        ball.transform.SetParent(ballAttachPoint); // 손에 붙임
        if (ball.TryGetComponent<ThrowBall>(out var ballref))
        {
            ballref.ballDamage = attackDamage;
        }        
    }

    public void ThrowBall()
    {
        if (target != null)
        {
            Vector3 targetPos = target.position + Vector3.up * 1.5f; // 약간 위쪽을 조준
            ball.GetComponent<ThrowBall>().photonView.RPC("ThrowToTarget", RpcTarget.All, targetPos);
        }        
    }

    public void ThrowCancle()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName("Throw")) return;

        animator.SetTrigger("Hit");

        if (ball.TryGetComponent<ThrowBall>(out var ballref))
        {
            ballref.Explode();
        }
    }
}
