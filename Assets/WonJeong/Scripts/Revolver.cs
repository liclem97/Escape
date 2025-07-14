using Photon.Pun;
using UnityEngine;

/* 플레이어1의 리볼버 스크립트 */
public class Revolver : Gun
{  
    protected override void Start()
    {
        gunType = GunType.Revolver;
        attackRange = 100f; // 리볼버 사정거리
        base.Start();
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: Fire
    * 기능: 총기를 발사하고 대미지를 입히는 함수
    ***********************************************************************************/
    protected override void Fire()
    {
        if (currentAmmo <= 0) return;    
        ARAVRInput.PlayVibration(ARAVRInput.Controller.RTouch);

        // 머즐 포인트부터 머즐 포인트의 앞 방향으로 레이 생성
        Ray ray = new Ray(muzzlePoint.transform.position, muzzlePoint.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, attackRange, HitRayMask))
        {
            EnemyBase zombie = hit.collider.GetComponentInParent<EnemyBase>();
            int instigatorID = photonView.ViewID;   // 총 소유자의 포톤뷰 id

            // 헤드샷
            if (hit.collider.CompareTag("Head") && zombie)
            {
                zombie.TakeDamage(gunDamage * 1.5f, instigatorID);
            }
            else if (hit.collider.TryGetComponent(out IDamageable damageable))
            {
                if (hit.collider.TryGetComponent(out DestructibleObject dest))
                {
                    // 리볼버는 destructibleObject를 파괴할 수 없음
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

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: Reload
    * 기능: 리볼버의 탄창을 채우고 UI를 갱신함
    ***********************************************************************************/
    public override void Reload()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoText();
    }    
}
