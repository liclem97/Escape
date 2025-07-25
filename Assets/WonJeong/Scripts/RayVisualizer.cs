﻿using Photon.Pun;
using System.Collections;
using UnityEngine;

/* 총의 레이와 크로스헤어를 그리는 스크립트 */
public class RayVisualizer : MonoBehaviour
{
    [Header("Ray")]
    [SerializeField] private LineRenderer Ray;          // 라인 렌더러
    [SerializeField] private LayerMask HitRayMask;      // 레이가 부딪히는 마스크
    [SerializeField] private float Distance = 100f;

    [Header("Reticle Point")]
    [SerializeField] private GameObject ReticlePoint;   // 레이의 끝에 그리는 포인트
    [SerializeField] private bool ShowReticle = true;

    [Header("Transform")]
    [SerializeField] private Transform muzzlePoint;     // 레이의 시작점

    private PhotonView rootPhotonView;

    private void Start()
    {
        // 루트에 있는 PhotonView 찾기
        rootPhotonView = GetComponentInParent<PhotonView>();
        if (muzzlePoint)
        {
            transform.SetParent(muzzlePoint);
        }
        if (rootPhotonView != null && rootPhotonView.IsMine)
        {
            ReticlePoint.SetActive(true); // 내 것만 보이게            
        }
        else
        {
            ReticlePoint.SetActive(false); // 다른 사람은 크로스헤어 안 보임
        }
    }

    private void OnEnable()
    {
        On();
    }

    public void On()
    {
        StopAllCoroutines();
        StartCoroutine(Process());
    }

    public void Off()
    {
        StopAllCoroutines();
        Ray.enabled = false;
        ReticlePoint.SetActive(false);
    }

    private IEnumerator Process()
    {
        while (true)
        {   
            // LineTrace: 시작 위치, 방향, HitResult, 거리, Layer
            if (Physics.Raycast(muzzlePoint.position, muzzlePoint.forward, out RaycastHit HitInfo, Distance, HitRayMask))
            {
                Ray.SetPosition(0, muzzlePoint.position);
                Ray.SetPosition(1, HitInfo.point); // 월드 좌표 그대로 사용
                Ray.enabled = true;

                ReticlePoint.transform.position = HitInfo.point;
                ReticlePoint.SetActive(ShowReticle); // 닿은 지점에 빨간 네모를 그림
                ReticlePoint.transform.LookAt(transform.position);
            }
            else
            {
                Ray.enabled = false;
                ReticlePoint.SetActive(false);
            }

            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        if (Ray.enabled)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(muzzlePoint.position, 0.05f);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(Ray.GetPosition(1), 0.2f);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(Ray.GetPosition(0), Ray.GetPosition(1));
        }
    }
}
