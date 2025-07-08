using Photon.Pun;
using System.Collections;
using UnityEngine;

public class EnemyThrower : EnemyBase
{
    [Header("SpawnPoint")]
    [SerializeField] private Transform ballAttachPoint;

    protected override void Start()
    {
        base.Start();
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

        GameObject ball = PhotonNetwork.Instantiate("ThrowBall", ballAttachPoint.position, ballAttachPoint.rotation);
        ball.transform.SetParent(ballAttachPoint); // 손에 붙임

        // 필요시 공에게 던지는 방향이나 타겟 설정 가능
        // 예: ball.GetComponent<ThrowBall>().SetTarget(target.position);
    }

    public void ThrowCancle()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName("Throw")) return;

        animator.SetTrigger("Hit");
    }
}
