using Photon.Pun;
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
        ARAVRInput.PlayVibration(ARAVRInput.Controller.RTouch);

        Ray ray = new Ray(muzzlePoint.transform.position, muzzlePoint.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, attackRange, HitRayMask))
        {
            EnemyBase zombie = hit.collider.GetComponentInParent<EnemyBase>();
            //EnemyThrower thrower = zombie as EnemyThrower;
            int instigatorID = photonView.ViewID;   // �� �������� ����� id

            // ��弦
            if (hit.collider.CompareTag("Head") && zombie)
            {
                //Debug.Log("HeadShot");
                zombie.TakeDamage(gunDamage * 1.5f, instigatorID);
            }
            else if (hit.collider.TryGetComponent(out IDamageable damageable))
            {
                if (hit.collider.TryGetComponent(out DestructibleObject dest))
                {
                    // ������ destructibleObject�� �ı��� �� ����
                }
                else
                {
                    damageable.TakeDamage(gunDamage, instigatorID);
                }                    
            }

            photonView.RPC(nameof(RPC_SpawnBulletFX), RpcTarget.All, hit.point, hit.normal, hit.collider.gameObject.layer);
            PlayGunFireSound();

            //Debug.DrawRay(muzzlePoint.transform.position, muzzlePoint.transform.forward, Color.red, 10f);
        }
        base.Fire();
    }

    [PunRPC]
    private void RPC_SpawnBulletFX(Vector3 position, Vector3 normal, int hitLayer)
    {
        SpawnBulletFX(position, normal, hitLayer);
    }
}
