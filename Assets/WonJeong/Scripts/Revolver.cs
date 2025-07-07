using Photon.Pun;
using UnityEngine;

public class Revolver : Gun
{  
    protected override void Start()
    {
        gunType = GunType.Revolver;
        attackRange = 100f; // 피스톨 사정거리
        base.Start();
    }

    protected override void Fire()
    {
        Ray ray = new Ray(muzzlePoint.transform.position, muzzlePoint.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, attackRange, HitRayMask))
        {
            EnemyBase zombie = hit.collider.GetComponentInParent<EnemyBase>();
            // 헤드샷
            if (hit.collider.CompareTag("Head") && zombie)
            {
                //zombie.TakeDamage(gunDamage * 1.5f);
            }
            else if (hit.collider.TryGetComponent(out IDamageable damageable))
            {
               // damageable.TakeDamage(gunDamage);
            }

            photonView.RPC(nameof(RPC_SpawnBulletFX), RpcTarget.All, hit.point, hit.normal, hit.collider.gameObject.layer);
            PlayGunFireSound();
            ARAVRInput.PlayVibration(ARAVRInput.Controller.RTouch);

            Debug.DrawRay(muzzlePoint.transform.position, muzzlePoint.transform.forward, Color.red, 10f);
        }
        base.Fire();
    }

    [PunRPC]
    private void RPC_SpawnBulletFX(Vector3 position, Vector3 normal, int hitLayer)
    {
        SpawnBulletFX(position, normal, hitLayer);
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }

    public override void Reload()
    {
        
    }
}
