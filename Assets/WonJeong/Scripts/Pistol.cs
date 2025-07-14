using Photon.Pun;
using UnityEngine;

/* �÷��̾�1�� �ǽ��� ��ũ��Ʈ */
public class Pistol : Gun
{
    protected override void Start()
    {
        gunType = GunType.Pistol;
        attackRange = 100f; // �ǽ��� �����Ÿ�
        base.Start();
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: Fire
    * ���: �ѱ⸦ �߻��ϰ� ������� ������ �Լ�
    ***********************************************************************************/
    protected override void Fire()
    {
        ARAVRInput.PlayVibration(ARAVRInput.Controller.RTouch);

        // ���� ����Ʈ���� ���� ����Ʈ�� �� �������� ���� ����
        Ray ray = new Ray(muzzlePoint.transform.position, muzzlePoint.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, attackRange, HitRayMask))
        {
            EnemyBase zombie = hit.collider.GetComponentInParent<EnemyBase>();            
            int instigatorID = photonView.ViewID;   // �� �������� ����� id

            // ��弦
            if (hit.collider.CompareTag("Head") && zombie)
            {                
                zombie.TakeDamage(gunDamage * 1.5f, instigatorID);
            }
            else if (hit.collider.TryGetComponent(out IDamageable damageable))
            {
                if (hit.collider.TryGetComponent(out DestructibleObject dest))
                {
                    // �ǽ����� destructibleObject�� �ı��� �� ����
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
}
