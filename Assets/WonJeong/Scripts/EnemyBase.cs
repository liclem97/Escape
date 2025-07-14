using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/* ���� ���̽� �Լ� */
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

    [SerializeField] protected Transform target;                // ���� Ÿ�� ����
    [SerializeField] protected float moveSpeed = 1f;            // �� �̵� �ӵ�
    [SerializeField] protected EnemyState enemyState;           // ���� ����
    [SerializeField] protected float humanSearchRadius = 20f;   // �� ���� ����
    [SerializeField] protected float attackRange = 1f;          // ���� �Ÿ�
    [SerializeField] protected float attackDamage = 10f;        // ���� �����
    [SerializeField] protected float attackDelay;               // ���� ������
    [SerializeField] protected float health = 100f;             // ü��

    protected EnemyState nextState;    
    protected Animator animator;
    protected NavMeshAgent agent;

    protected virtual void Start()
    {        
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        AdjustPositionToGround();

        agent.updateRotation = false; // ���� ȸ���� ��������
        agent.updateUpAxis = false;   // Y�� �ڵ� ���� ����
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

        agent.updateRotation = false; // ���� ȸ���� ��������
        agent.updateUpAxis = false;   // Y�� �ڵ� ���� ����
        agent.speed = moveSpeed;
    }

    private void Update()
    {
        if (target != null)
        {
            Vector3 lookPos = target.position;
            lookPos.y = transform.position.y; // Y���� ���� ���̷� ����
            transform.LookAt(lookPos);
        }
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: AdjustPositionToGround
    * ���: �� Ȱ��ȭ �� ���߿� ���� ��� �ٴ����� y�� �̵�
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
    * �ۼ���: �ڿ���
    * �Լ�: ChangeState
    * ���: �� ���� ���� �Լ�
    * �Է�:
    *   - state: ���� ���� ����
    ***********************************************************************************/
    protected void ChangeState(EnemyState state)
    {   
        // ���� ���°� ���� or ���� �Ŵ����� ��ȿ���� ���� or ���� ���� �����̸� ����
        if (nextState == state || GameManager.Instance == null || GameManager.Instance.IsGameOver) return;

        nextState = state;

        // �ٷ� ���� ��ȯ ����
        if (nextState != enemyState)
        {
            StopCoroutine($"Co{enemyState}");
            enemyState = nextState;
            StartCoroutine($"Co{enemyState}");
            Debug.LogWarning($"[EnemyBase] ���� ��ȯ: {enemyState}");
        }
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: CoIdle
    * ���: Idle ���� �Լ�    
    ***********************************************************************************/
    protected IEnumerator CoIdle()
    {
        animator.SetFloat("Speed", 0f);
        yield return new WaitUntil(() => photonView.IsMine);
        
        agent.isStopped = true;         // ������Ʈ �̵� ����
        ChangeState(EnemyState.Search); // ���� ���·� ����
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: CoSearch
    * ���: Search ���� �Լ�    
    ***********************************************************************************/
    protected IEnumerator CoSearch()
    {
        yield return new WaitUntil(() => photonView.IsMine);

        while (target == null)
        {
            target = FindNearestHuman();    // Ÿ���� ã�Ƽ� ������
            yield return null;
        }

        photonView.RPC(nameof(RPC_ChangeState), RpcTarget.All, EnemyState.Move);
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: FindNearestHuman
    * ���: ������ ������ ���� ã�� �����ϴ� �Լ�
    * ���:
        - Transform: ������ Ÿ���� ��ġ
    ***********************************************************************************/
    protected Transform FindNearestHuman()
    {   
        // humanSearchRadius�� ������ŭ Human, Player ���̾ ���� ���� ������Ʈ�� ã�� �迭�� ������
        Collider[] hits = Physics.OverlapSphere(transform.position, humanSearchRadius, LayerMask.GetMask("Human", "Player"));
        Transform nearest = null;
        float closestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            // VRPlayerHealth ������Ʈ Ȯ��
            VRPlayerHealth health = hit.GetComponent<VRPlayerHealth>();
            if (health != null && health.photonView != null)
            {
                // ���� ����(�÷��̾�2)�� ����
                if (health.enabled && health.GetType() == typeof(VRPlayerHealth) && health.IsInvincible)
                    continue;
            }

            // �Ÿ��� ����Ͽ� ����� Ÿ���� ������
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
    * �ۼ���: �ڿ���
    * �Լ�: CoMove
    * ���: �� �̵� �Լ�   
    ***********************************************************************************/
    protected IEnumerator CoMove()
    {
        yield return new WaitUntil(() => photonView.IsMine);

        while (target != null && GameManager.Instance != null && !GameManager.Instance.IsGameOver)
        {
            float distToTarget = (target.position - transform.position).sqrMagnitude;   // Ÿ����� �Ÿ� ���

            if (distToTarget <= attackRange * attackRange)  // Ÿ���� ���� ������ ���� �� ����
            {
                agent.isStopped = true; // ������Ʈ �̵� ����
                photonView.RPC(nameof(RPC_ChangeState), RpcTarget.All, EnemyState.Attack);  // ���� ���·� ��ȯ
                yield break;
            }

            agent.isStopped = false;
            agent.SetDestination(target.position);  // ������Ʈ �̵� ����

            Vector3 lookTarget = target.position;
            lookTarget.y = transform.position.y; // ���� ȸ���� ����
            transform.LookAt(lookTarget);        // ���� �ٶ󺸵��� ȸ��

            animator.SetFloat("Speed", agent.velocity.magnitude);
            yield return null;
        }

        agent.isStopped = true;
        photonView.RPC(nameof(RPC_ChangeState), RpcTarget.All, EnemyState.Idle);
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: CoAttack
    * ���: �� ���� �Լ�   
    ***********************************************************************************/
    protected virtual IEnumerator CoAttack()
    {
        while (target != null && GameManager.Instance != null && !GameManager.Instance.IsGameOver)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);  // �ֱ� �ִϸ��̼� ���� ����

            // Attack �ִϸ��̼� ���°� �ƴ� ���� Ʈ���� �ߵ�
            if (!stateInfo.IsName("Attack"))
            {
                photonView.RPC(nameof(RPC_TriggerAttack), RpcTarget.All);
            }

            // ���� ������ ���
            yield return new WaitForSeconds(attackDelay);

            if (target == null || GameManager.Instance.IsGameOver)
            {
                photonView.RPC(nameof(RPC_ChangeState), RpcTarget.All, EnemyState.Idle);
            }
        }        
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: RPC_TriggerAttack
    * ���: �� ���� �ִϸ��̼� ����ȭ �Լ�   
    ***********************************************************************************/
    [PunRPC]
    private void RPC_TriggerAttack()
    {
        animator.SetTrigger("Attack");
        animator.SetFloat("Speed", 0f);
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: CoDie
    * ���: �� ��� �Լ�   
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

    // IDamageable �������̽� ��� �Լ�
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
        // Layer�� "Human" �Ǵ� "Player"���� Ȯ��
        int otherLayer = other.gameObject.layer;
        string layerName = LayerMask.LayerToName(otherLayer);
        if (layerName != "Human" && layerName != "Player") return;        

        // IDamageable �������̽� ����ü���� Ȯ��
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(attackDamage, photonView.ViewID);
            Debug.Log($"[EnemyBase] {other.name}���� {attackDamage} �������� ��");
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
