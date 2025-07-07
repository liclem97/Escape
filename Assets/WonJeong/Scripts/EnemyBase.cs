using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;

public class EnemyBase : MonoBehaviourPunCallbacks, IDamageable
{
    protected enum EnemyState 
    {
        Nothing,
        Idle,
        Search,
        Move,
        Attack,
        Die
    }

    [SerializeField] protected Transform target;
    [SerializeField] protected float moveSpeed = 1f;
    [SerializeField] protected EnemyState enemyState;
    [SerializeField] protected float humanSearchRadius = 20f;
    [SerializeField] protected float attackRange = 1f;
    [SerializeField] protected float attackDamage = 10f;
    [SerializeField] protected float attackDelay;
    [SerializeField] protected float health = 100f;

    protected EnemyState nextState;
    protected Rigidbody rigid;
    protected Animator animator;

    private void Start()
    {
        //yield return new WaitUntil(() => PhotonNetwork.InRoom); // 방에 들어올 때까지 대기
        rigid = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        if (photonView.IsMine)
        {
            ChangeState(EnemyState.Idle);
        }
    }

    private void Update()
    {
        //if (nextState != enemyState)
        //{
        //    StopCoroutine($"Co{enemyState}");
        //    enemyState = nextState;
        //    StartCoroutine($"Co{enemyState}");
        //    Debug.LogWarning($"Enemy start: {enemyState}");
        //}

        //if (target)
        //{
        //    float distance = Vector3.Distance(transform.position, target.position);
        //    Debug.Log($"[EnemyBase] Target Distance: {distance:F2}");
        //}
    }

    protected void ChangeState(EnemyState state)
    {   
        // 다음 상태가 같음 or 게임 매니저가 유효하지 않음 or 게임 오버 상태이면 리턴
        if (nextState == state || GameManager.Instance == null || GameManager.Instance.IsGameOver) return;

        nextState = state;

        // 바로 상태 전환 수행
        if (nextState != enemyState)
        {
            StopCoroutine($"Co{enemyState}");
            enemyState = nextState;
            StartCoroutine($"Co{enemyState}");
            Debug.LogWarning($"[EnemyBase] 상태 전환: {enemyState}");
        }
    }

    [PunRPC]
    protected void RPC_ChangeState(EnemyState state)
    {
        ChangeState(state);
    }

    protected IEnumerator CoIdle()
    {
        //animator.SetTrigger("Idle");
        animator.SetFloat("Speed", 0f);
        yield return new WaitUntil(() => photonView.IsMine);

        rigid.velocity = Vector3.zero;
        ChangeState(EnemyState.Search);
    }

    protected IEnumerator CoSearch()
    {
        yield return new WaitUntil(() => photonView.IsMine);

        while (target == null)
        {
            target = FindNearestHuman();            
            yield return null;
        }

        photonView.RPC(nameof(RPC_ChangeState), RpcTarget.All, EnemyState.Move);
    }

    protected Transform FindNearestHuman()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, humanSearchRadius, LayerMask.GetMask("Human"/*, "Player"*/));
        Transform nearest = null;
        float closestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            float sqrDist = (hit.transform.position - transform.position).sqrMagnitude;
            if (sqrDist < closestDist)
            {
                closestDist = sqrDist;
                nearest = hit.transform;
            }
        }

        return nearest;
    }

    protected IEnumerator CoMove()
    {        
        yield return new WaitUntil(() => photonView.IsMine);

        while (target != null && GameManager.Instance != null && !GameManager.Instance.IsGameOver)
        {
            var dir = target.position - transform.position;
            dir.y = 0f;
            float distToTarget = dir.sqrMagnitude;

            // 공격 거리 이내라면 공격 상태로 전환
            if (distToTarget <= attackRange * attackRange)
            {
                rigid.velocity = Vector3.zero; // 멈춤
                photonView.RPC(nameof(RPC_ChangeState), RpcTarget.All, EnemyState.Attack);
                yield break; // 코루틴 종료
            }

            var velocity = moveSpeed * dir.normalized;
            velocity.y = rigid.velocity.y;
            rigid.velocity = velocity;

            transform.forward = dir.normalized;
            animator.SetFloat("Speed", velocity.magnitude);
            yield return null;
        }

        photonView.RPC(nameof(RPC_ChangeState), RpcTarget.All, EnemyState.Idle);
    }

    protected IEnumerator CoAttack()
    {
        while (target != null && GameManager.Instance != null && !GameManager.Instance.IsGameOver)
        {
            // 공격 애니메이션 실행
            animator.SetTrigger("Attack");
            animator.SetFloat("Speed", 0f);
            // 공격 딜레이 대기
            yield return new WaitForSeconds(attackDelay);

            //// 공격 거리 밖으로 나가면 상태 변경
            //float sqrDist = (target.position - transform.position).sqrMagnitude;
            //if (sqrDist > attackRange * attackRange)
            //{
            //    photonView.RPC(nameof(RPC_ChangeState), RpcTarget.All, EnemyState.Move);
            //    yield break;
            //}
        }

        // 타겟이 사라졌거나 게임이 끝난 경우
        photonView.RPC(nameof(RPC_ChangeState), RpcTarget.All, EnemyState.Idle);
    }

    protected IEnumerator CoDie()
    {
        animator.SetTrigger("Die");
        yield return new WaitUntil(() => photonView.IsMine);

        target = null;
        rigid.velocity = Vector3.zero;
    }

    public void TakeDamage(float amount, int instigatorID)
    {
        if (enemyState == EnemyState.Die) return;

        health -= amount;

        Debug.Log("damage를 받음 " + amount + " 공격자: " + PhotonView.Find(instigatorID).Owner.NickName);
        if (health <= 0f)
        {
            photonView.RPC(nameof(RPC_ChangeState), RpcTarget.All, EnemyState.Die);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Layer가 "Human" 또는 "Player"인지 확인
        int otherLayer = other.gameObject.layer;
        string layerName = LayerMask.LayerToName(otherLayer);
        if (layerName != "Human" && layerName != "Player") return;        

        // IDamageable 인터페이스 구현체인지 확인
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(attackDamage, photonView.ViewID);
            Debug.Log($"[EnemyBase] {other.name}에게 {attackDamage} 데미지를 줌");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, humanSearchRadius);
    }
}
