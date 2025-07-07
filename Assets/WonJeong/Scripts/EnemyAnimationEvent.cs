using UnityEngine;

public class EnemyAnimationEvent : MonoBehaviour
{
    [SerializeField] private BoxCollider attackCollider;

    public void attackColliderOn()
    {   
        // attackCollider�̰� ��ȿ�ϰ� ��������
        if (attackCollider != null && attackCollider.enabled == false)
        {
            attackCollider.enabled = true;
        }
    }

    public void attackColliderOff()
    {
        // attackCollider�̰� ��ȿ�ϰ� ��������
        if (attackCollider != null && attackCollider.enabled == true)
        {
            attackCollider.enabled = false;
        }
    }

    public void SetActivateFalseEnemy()
    {
        gameObject.SetActive(false);
    }
}
