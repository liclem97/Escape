using Photon.Pun;
using UnityEngine;

/* 플레이어2의 스나이퍼 라이플 총기 스크립트 */
public class SniperRifle : Gun
{
    protected override void Start()
    {
        gunType = GunType.SniperRifle;
        attackRange = 1000f;
        base.Start();       
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: Fire
    * 기능: 총기를 발사하고 대미지를 입히는 함수
    ***********************************************************************************/
    protected override void Fire()
    {
        ARAVRInput.PlayVibration(ARAVRInput.Controller.RTouch);

        // 머즐 포인트부터 머즐 포인트의 앞 방향으로 레이 생성
        Ray ray = new Ray(muzzlePoint.transform.position, muzzlePoint.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, attackRange, HitRayMask))
        {
            EnemyBase zombie = hit.collider.GetComponentInParent<EnemyBase>();
            EnemyThrower thrower = zombie as EnemyThrower;
            int instigatorID = photonView.ViewID;   // 총 소유자의 포톤뷰 id

            // 헤드샷
            if (hit.collider.CompareTag("Head") && zombie)
            {
                zombie.TakeDamage(gunDamage * 1.5f, instigatorID);
            }
            else if (hit.collider.CompareTag("HitCancel") && thrower) // 보스의 HitCancel 콜라이더를 맞출 경우
            {                                                         // 보스가 던지는 공격을 캔슬시킬 수 있다.
                thrower.ThrowCancel();
                thrower.TakeDamage(thrower.maxHealth * 0.4f, instigatorID);
            }
            else if (hit.collider.TryGetComponent(out IDamageable damageable))
            {
                if (hit.collider.TryGetComponent(out DestructibleObject dest))
                {
                    // 총으론 destructibleObject를 파괴할 수 없음
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
