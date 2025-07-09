using Photon.Pun;
using UnityEngine;

public class SniperRifle : Gun
{
    protected override void Start()
    {
        gunType = GunType.SniperRifle;
        attackRange = 1000f;
        base.Start();       
    }

    protected override void Fire()
    {
        ARAVRInput.PlayVibration(ARAVRInput.Controller.RTouch);

        Ray ray = new Ray(muzzlePoint.transform.position, muzzlePoint.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, attackRange, HitRayMask))
        {
            EnemyBase zombie = hit.collider.GetComponentInParent<EnemyBase>();
            EnemyThrower thrower = zombie as EnemyThrower;
            int instigatorID = photonView.ViewID;   // ÃÑ ¼ÒÀ¯ÀÚÀÇ Æ÷Åæºä id

            // Çìµå¼¦
            if (hit.collider.CompareTag("Head") && zombie)
            {
                zombie.TakeDamage(gunDamage * 1.5f, instigatorID);
            }
            else if (hit.collider.CompareTag("HitCancel") && thrower)
            {
                Debug.Log("throw canceld");
                thrower.ThrowCancle();
                thrower.TakeDamage(thrower.maxHealth * 0.4f, instigatorID);
            }
            else if (hit.collider.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(gunDamage, instigatorID);
            }

            photonView.RPC(nameof(RPC_SpawnBulletFX), RpcTarget.All, hit.point, hit.normal, hit.collider.gameObject.layer);
            PlayGunFireSound();
        }
        base.Fire();
    }

    [PunRPC]
    private void RPC_SpawnBulletFX(Vector3 position, Vector3 normal, int hitLayer)
    {
        SpawnBulletFX(position, normal, hitLayer);
    }
}
