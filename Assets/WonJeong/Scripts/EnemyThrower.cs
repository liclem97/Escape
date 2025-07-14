using Photon.Pun;
using System.Collections;
using UnityEngine;

/* 물건을 던지는 적 스크립트 함수 */
public class EnemyThrower : EnemyBase
{
    [Header("SpawnPoint")]
    [SerializeField] private Transform ballAttachPoint; // 던지는 물체를 붙이는 위치

    private GameObject ball;

    [HideInInspector]
    public float maxHealth;

    protected override void Start()
    {
        base.Start();
        maxHealth = health;
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: CoAttack
    * 기능: 적 공격 함수   
    ***********************************************************************************/
    protected override IEnumerator CoAttack()
    {
        while (target != null && GameManager.Instance != null && !GameManager.Instance.IsGameOver)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // 마스터만 실행, 모든 클라이언트에게 Throw 트리거 실행 요청
            photonView.RPC(nameof(RPC_TriggerThrow), RpcTarget.All);

            // 공 생성은 마스터만
            SpawnThrowBall();

            // 공격 딜레이 대기
            yield return new WaitForSeconds(attackDelay);

            if (target == null || GameManager.Instance.IsGameOver)
            {
                photonView.RPC(nameof(RPC_ChangeState), RpcTarget.All, EnemyState.Idle);
            }
        }
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: RPC_TriggerThrow
    * 기능: 적 던지는 공격 애니메이션 동기화 함수   
    ***********************************************************************************/
    [PunRPC]
    private void RPC_TriggerThrow()
    {
        animator.SetTrigger("Throw");
        animator.SetFloat("Speed", 0f);
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: SpawnThrowBall
    * 기능: 던지는 공을 스폰하는 함수
    ***********************************************************************************/
    public void SpawnThrowBall()
    {
        if (!photonView.IsMine) return;

        // 공 스폰
        ball = PhotonNetwork.Instantiate("ThrowBall", ballAttachPoint.position, ballAttachPoint.rotation);

        // 공 대미지 변경
        if (ball.TryGetComponent<ThrowBall>(out var ballref))
        {
            ballref.ballDamage = attackDamage;
        }

        photonView.RPC(nameof(RPC_AttachBallToHand), RpcTarget.All, ball.GetComponent<PhotonView>().ViewID);
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: RPC_AttachBallToHand
    * 기능: 스폰한 공을 자신의 손에 붙이는 함수
    * 입력:
    *   - ballViewID: 스폰한 공의 photonView ID
    ***********************************************************************************/
    [PunRPC]
    private void RPC_AttachBallToHand(int ballViewID)
    {
        // 공의 ViewID를 씬에서 찾아서 저장
        GameObject obj = PhotonView.Find(ballViewID)?.gameObject;
        if (obj != null)
        {
            obj.transform.SetParent(ballAttachPoint);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;  // 저장한 공의 위치 설정

            if (obj.TryGetComponent<ThrowBall>(out var ballScript))
            {
                ballScript.StartGrow();
            }
        }
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: ThrowBall
    * 기능: 공을 던지는 함수
    ***********************************************************************************/
    public void ThrowBall()
    {
        if (target != null && gameObject)
        {
            Vector3 targetPos = target.position + Vector3.up * 0.75f;
            ball.GetComponent<ThrowBall>().photonView.RPC("ThrowToTarget", RpcTarget.All, targetPos);
        }        
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: RPC_TriggerThrowCancel
    * 기능: 던지는 공격이 캔슬 됐을 시 애니메이션을 동기화하는 함수
    ***********************************************************************************/
    [PunRPC]
    private void RPC_TriggerThrowCancel()
    {
        animator.SetTrigger("Hit");

        if (ball && ball.TryGetComponent<ThrowBall>(out var ballref))
        {
            ballref.Explode();
        }
    }
    public void ThrowCancel()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName("Throw")) return;

        photonView.RPC(nameof(RPC_TriggerThrowCancel), RpcTarget.All);
    }
}
