using Photon.Pun;
using UnityEngine;

public class Revolver : Gun
{  
    protected override void Start()
    {
        gunType = GunType.Revolver;
        attackRange = 100f; // ÇÇ½ºÅç »çÁ¤°Å¸®
        base.Start();
    }

    protected override void Fire()
    {
        if (currentAmmo <= 0) return;
        Debug.Log("currentAmmo: " + currentAmmo);
        ARAVRInput.PlayVibration(ARAVRInput.Controller.RTouch);

        Ray ray = new Ray(muzzlePoint.transform.position, muzzlePoint.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, attackRange, HitRayMask))
        {
            EnemyBase zombie = hit.collider.GetComponentInParent<EnemyBase>();
            int instigatorID = photonView.ViewID;   // ÃÑ ¼ÒÀ¯ÀÚÀÇ Æ÷Åæºä id

            // Çìµå¼¦
            if (hit.collider.CompareTag("Head") && zombie)
            {
                zombie.TakeDamage(gunDamage * 1.5f, instigatorID);
            }
            else if (hit.collider.TryGetComponent(out IDamageable damageable))
            {
                if (hit.collider.TryGetComponent(out DestructibleObject dest))
                {
                    // ÃÑÀ¸·Ð destructibleObject¸¦ ÆÄ±«ÇÒ ¼ö ¾øÀ½
                }
                else
                {
                    damageable.TakeDamage(gunDamage, instigatorID);
                }
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

    public override void Reload()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoText();
        // TODO: play reload sound;
    }    
}
