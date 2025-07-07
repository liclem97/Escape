using UnityEngine;

public class EnemyAnimationEvent : MonoBehaviour
{
    [SerializeField] private BoxCollider attackCollider;

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

    public void SetActivateFalseEnemy()
    {
        gameObject.SetActive(false);
    }
}
