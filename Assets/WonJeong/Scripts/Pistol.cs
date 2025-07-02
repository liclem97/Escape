using UnityEngine;

public class Pistol : Gun
{
    protected override void Start()
    {
        gunType = GunType.Pistol;
        attackRange = 100f; // 피스톨 사정거리
        base.Start();
    }

    protected override void Fire()
    {
        base.Fire();

        Ray ray = new Ray(rayVisualizer.transform.position, rayVisualizer.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, attackRange))
        {
            // 데미지 처리
            if (hit.collider.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(gunDamage);
            }

            // 이펙트 및 사운드
            SpawnBulletFX(hit);
            PlayGunFireSound();            
        }
    }
}
