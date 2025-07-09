using UnityEngine;

public class DestructibleObject : MonoBehaviour, IDamageable
{
    public void TakeDamage(float amount, int instigatorID)
    {
        Destroy(gameObject);
    }
}
