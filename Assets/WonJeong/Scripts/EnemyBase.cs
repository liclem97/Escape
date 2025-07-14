using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/* 적의 베이스 함수 */
public class EnemyBase : MonoBehaviourPun, IDamageable
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

    [SerializeField] protected Transform target;                // 공격 타깃 저장
    [SerializeField] protected float moveSpeed = 1f;            // 적 이동 속도
    [SerializeField] protected EnemyState enemyState;           // 적의 상태
    [SerializeField] protected float humanSearchRadius = 20f;   // 적 색적 범위
    [SerializeField] protected float attackRange = 1f;          // 공격 거리
    [SerializeField] protected float attackDamage = 10f;        // 공격 대미지
    [SerializeField] protected float attackDelay;               // 공격 딜레이
    [SerializeField] protected float health = 100f;             // 체력

    protected EnemyState nextState;    
    protected Animator animator;
    protected NavMeshAgent agent;

    protected virtual void Start()
    {        
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        AdjustPositionToGround();

        agent.updateRotation = false; // 수평 회전만 수동으로
        agent.updateUpAxis = false;   // Y축 자동 조절 끄기
        agent.speed = moveSpeed;

        if (photonView.IsMine)
        {
            ChangeState(EnemyState.Idle);
        }
    }

    private void OnEnable()
    {
        AdjustPositionToGround();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false; // 수평 회전만 수동으로
        agent.updateUpAxis = false;   // Y축 자동 조절 끄기
        agent.speed = moveSpeed;
    }

    private void Update()
    {
        if (target != null)
        {
            Vector3 lookPos = target.position;
            lookPos.y = transform.position.y; // Y축은 현재 높이로 고정
            transform.LookAt(lookPos);
        }
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: AdjustPositionToGround
    * 기능: 적 활성화 시 공중에 있을 경우 바닥으로 y축 이동
    ***********************************************************************************/
    void AdjustPositionToGround()
    {
        if (TryGetComponent<CapsuleCollider>(out CapsuleCollider col))
        {
            float height = col.height;
            float centerY = col.center.y;
            float bottomOffset = (height / 2f) - centerY;

            transform.position = new Vector3(
                transform.position.x,
                transform.position.y + bottomOffset,
                transform.position.z
            );
        }
    }

    [PunRPC]
    protected void RPC_ChangeState(EnemyState state)
    {
        ChangeState(state);
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: ChangeState
    * 기능: 적 상태 변경 함수
    * 입력:
    *   - state: 적의 다음 상태
    ***********************************************************************************/
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

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: CoIdle
    * 기능: Idle 상태 함수    
    ***********************************************************************************/
    protected IEnumerator CoIdle()
    {
        animator.SetFloat("Speed", 0f);
        yield return new WaitUntil(() => photonView.IsMine);
        
        agent.isStopped = true;         // 에이전트 이동 정지
        ChangeState(EnemyState.Search); // 색적 상태로 변경
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: CoSearch
    * 기능: Search 상태 함수    
    ***********************************************************************************/
    protected IEnumerator CoSearch()
    {
        yield return new WaitUntil(() => photonView.IsMine);

        while (target == null)
        {
            target = FindNearestHuman();    // 타깃을 찾아서 저장함
            yield return null;
        }

        photonView.RPC(nameof(RPC_ChangeState), RpcTarget.All, EnemyState.Move);
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: FindNearestHuman
    * 기능: 설정한 범위의 적을 찾아 저장하는 함수
    * 출력:
        - Transform: 저장한 타깃의 위치
    ***********************************************************************************/
    protected Transform FindNearestHuman()
    {   
        // humanSearchRadius의 범위만큼 Human, Player 레이어를 가진 게임 오브젝트를 찾아 배열에 저장함
        Collider[] hits = Physics.OverlapSphere(transform.position, humanSearchRadius, LayerMask.GetMask("Human", "Player"));
        Transform nearest = null;
        float closestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            // VRPlayerHealth 컴포넌트 확인
            VRPlayerHealth health = hit.GetComponent<VRPlayerHealth>();
            if (health != null && health.photonView != null)
            {
                // 무적 상태(플레이어2)면 무시
                if (health.enabled && health.GetType() == typeof(VRPlayerHealth) && health.IsInvincible)
                    continue;
            }

            // 거리를 계산하여 가까운 타깃을 갱신함
            float sqrDist = (hit.transform.position - transform.position).sqrMagnitude;
            if (sqrDist < closestDist)
            {
                closestDist = sqrDist;
                nearest = hit.transform;
            }
        }

        return nearest;
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: CoMove
    * 기능: 적 이동 함수   
    ***********************************************************************************/
    protected IEnumerator CoMove()
    {
        yield return new WaitUntil(() => photonView.IsMine);

        while (target != null && GameManager.Instance != null && !GameManager.Instance.IsGameOver)
        {
            float distToTarget = (target.position - transform.position).sqrMagnitude;   // 타깃과의 거리 계산

            if (distToTarget <= attackRange * attackRange)  // 타깃이 공격 범위에 있을 시 실행
            {
                agent.isStopped = true; // 에이전트 이동 정지
                photonView.RPC(nameof(RPC_ChangeState), RpcTarget.All, EnemyState.Attack);  // 공격 상태로 전환
                yield break;
            }

            agent.isStopped = false;
            agent.SetDestination(target.position);  // 에이전트 이동 설정

            Vector3 lookTarget = target.position;
            lookTarget.y = transform.position.y; // 수평 회전만 적용
            transform.LookAt(lookTarget);        // 적을 바라보도록 회전

            animator.SetFloat("Speed", agent.velocity.magnitude);
            yield return null;
        }

        agent.isStopped = true;
        photonView.RPC(nameof(RPC_ChangeState), RpcTarget.All, EnemyState.Idle);
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: CoAttack
    * 기능: 적 공격 함수   
    ***********************************************************************************/
    protected virtual IEnumerator CoAttack()
    {
        while (target != null && GameManager.Instance != null && !GameManager.Instance.IsGameOver)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);  // 최근 애니메이션 상태 저장

            // Attack 애니메이션 상태가 아닐 때만 트리거 발동
            if (!stateInfo.IsName("Attack"))
            {
                photonView.RPC(nameof(RPC_TriggerAttack), RpcTarget.All);
            }

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
    * 함수: RPC_TriggerAttack
    * 기능: 적 공격 애니메이션 동기화 함수   
    ***********************************************************************************/
    [PunRPC]
    private void RPC_TriggerAttack()
    {
        animator.SetTrigger("Attack");
        animator.SetFloat("Speed", 0f);
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: CoDie
    * 기능: 적 사망 함수   
    ***********************************************************************************/
    protected IEnumerator CoDie()
    {
        animator.SetBool("bDied", true);
        animator.SetTrigger("Die");
        yield return new WaitUntil(() => photonView.IsMine);

        target = null;
     
        agent.isStopped = true;
        if (TryGetComponent<CapsuleCollider>(out var collider))
        {
            collider.enabled = false;
        }
    }

    // IDamageable 인터페이스 상속 함수
    public void TakeDamage(float amount, int instigatorID)
    {
        if (enemyState == EnemyState.Die) return;

        health -= amount;

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

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, attackRange);
    }
}
