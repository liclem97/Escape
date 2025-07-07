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
        //yield return new WaitUntil(() => PhotonNetwork.InRoom); // �濡 ���� ������ ���
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

            // ���� �Ÿ� �̳���� ���� ���·� ��ȯ
            if (distToTarget <= attackRange * attackRange)
            {
                rigid.velocity = Vector3.zero; // ����
                photonView.RPC(nameof(RPC_ChangeState), RpcTarget.All, EnemyState.Attack);
                yield break; // �ڷ�ƾ ����
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
            // ���� �ִϸ��̼� ����
            animator.SetTrigger("Attack");
            animator.SetFloat("Speed", 0f);
            // ���� ������ ���
            yield return new WaitForSeconds(attackDelay);

            //// ���� �Ÿ� ������ ������ ���� ����
            //float sqrDist = (target.position - transform.position).sqrMagnitude;
            //if (sqrDist > attackRange * attackRange)
            //{
            //    photonView.RPC(nameof(RPC_ChangeState), RpcTarget.All, EnemyState.Move);
            //    yield break;
            //}
        }

        // Ÿ���� ������ų� ������ ���� ���
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

        Debug.Log("damage�� ���� " + amount + " ������: " + PhotonView.Find(instigatorID).Owner.NickName);
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
    }
}
