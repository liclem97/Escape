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

            // �����͸� ����, ��� Ŭ���̾�Ʈ���� Throw Ʈ���� ���� ��û
            photonView.RPC(nameof(RPC_TriggerThrow), RpcTarget.All);

            // �� ������ �����͸�
            SpawnThrowBall();

            // ���� ������ ���
            yield return new WaitForSeconds(attackDelay);

            if (target == null || GameManager.Instance.IsGameOver)
            {
                photonView.RPC(nameof(RPC_ChangeState), RpcTarget.All, EnemyState.Idle);
            }
        }
    }

    [PunRPC]
    private void RPC_TriggerThrow()
    {
        animator.SetTrigger("Throw");
        animator.SetFloat("Speed", 0f);
    }

    public void SpawnThrowBall()
    {
        if (!photonView.IsMine) return;

        ball = PhotonNetwork.Instantiate("ThrowBall", ballAttachPoint.position, ballAttachPoint.rotation);

        if (ball.TryGetComponent<ThrowBall>(out var ballref))
        {
            ballref.ballDamage = attackDamage;
        }

        photonView.RPC(nameof(RPC_AttachBallToHand), RpcTarget.All, ball.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    private void RPC_AttachBallToHand(int ballViewID)
    {
        GameObject obj = PhotonView.Find(ballViewID)?.gameObject;
        if (obj != null)
        {
            obj.transform.SetParent(ballAttachPoint);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;

            if (obj.TryGetComponent<ThrowBall>(out var ballScript))
            {
                ballScript.StartGrow();
            }
        }
    }

    public void ThrowBall()
    {
        if (target != null && gameObject)
        {
            Vector3 targetPos = target.position + Vector3.up * 1.5f; // �ణ ������ ����
            ball.GetComponent<ThrowBall>().photonView.RPC("ThrowToTarget", RpcTarget.All, targetPos);
        }        
    }

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
