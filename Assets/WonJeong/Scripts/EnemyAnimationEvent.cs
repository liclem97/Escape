using UnityEngine;

public class EnemyAnimationEvent : MonoBehaviour
{
    [SerializeField] private BoxCollider attackCollider;
    [SerializeField] private BoxCollider throwCollider1;
    [SerializeField] private BoxCollider throwCollider2;

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
}