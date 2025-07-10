using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimationEvent : MonoBehaviour
{
    [SerializeField] private BoxCollider attackCollider;
    [SerializeField] private BoxCollider throwCollider1;
    [SerializeField] private BoxCollider throwCollider2;

    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private NavMeshObstacle obstacle;

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
        Destroy(gameObject);
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

    public void TurnOnAgentAndOffObstacles()
    {
        if (agent && obstacle)
        {
            obstacle.enabled = false;
            agent.enabled = true;            
        }
    }

    public void TurnOffAgentAndOnObstacles()
    {
        if (agent && obstacle)
        {
            agent.enabled = false;
            obstacle.enabled = true;
        }
    }
}