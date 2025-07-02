using Photon.Pun;
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
        Ray ray = new Ray(rayVisualizer.transform.position, rayVisualizer.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, attackRange))
        {
            if (hit.collider.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(gunDamage);
            }

            photonView.RPC(nameof(RPC_SpawnBulletFX), RpcTarget.All, hit.point, hit.normal, hit.collider.gameObject.layer);
            PlayGunFireSound();
            ARAVRInput.PlayVibration(ARAVRInput.Controller.RTouch);
        }
    }

    [PunRPC]
    private void RPC_SpawnBulletFX(Vector3 position, Vector3 normal, int hitLayer)
    {
        SpawnBulletFX(position, normal, hitLayer);
    }
}
