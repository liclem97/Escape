using UnityEngine;

public class Pistol : Gun
{
    protected override void Start()
    {
        gunType = GunType.Pistol;
        attackRange = 100f; // �ǽ��� �����Ÿ�
        base.Start();
    }

    protected override void Fire()
    {
        base.Fire();

        Ray ray = new Ray(rayVisualizer.transform.position, rayVisualizer.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, attackRange))
        {
            // ������ ó��
            if (hit.collider.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(gunDamage);
            }

            // ����Ʈ �� ����
            SpawnBulletFX(hit);
            PlayGunFireSound();            
        }
    }
}
