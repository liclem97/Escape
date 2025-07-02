using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;

public class Item : MonoBehaviourPun
{
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isHeld = false;
    private bool isInTargetTrigger = false;
    private Transform holder = null;
    private Transform targetPosition;
    public bool IsHeld => isHeld;

    private void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    public void AttachToLeftHand(int playerViewID)
    {
        photonView.RPC(nameof(RPC_AttachToLeftHand), RpcTarget.AllBuffered, playerViewID);
    }

    [PunRPC]
    private void RPC_AttachToLeftHand(int playerViewID)
    {
        PhotonView playerView = PhotonView.Find(playerViewID);
        var rig = playerView?.GetComponentInChildren<OVRCameraRig>();
        Transform leftHand = rig?.leftHandAnchor;

        if (leftHand != null)
        {
            transform.SetParent(leftHand);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            if (TryGetComponent<Rigidbody>(out var rb)) Destroy(rb);
            if (TryGetComponent<Collider>(out var col)) Destroy(col);

            isHeld = true;
            holder = leftHand;
        }
    }

    public void DetachFromHand()
    {
        if (!photonView.IsMine) return;
        photonView.RPC(nameof(RPC_DetachFromHand), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPC_DetachFromHand()
    {
        isHeld = false;
        transform.SetParent(null);
        holder = null;

        if (!TryGetComponent<Rigidbody>(out var rb))
            rb = gameObject.AddComponent<Rigidbody>();

        if (!TryGetComponent<Collider>(out var col))
            col = gameObject.AddComponent<BoxCollider>();

        // 트리거 안에 있으면 목표로 날아감, 아니면 원래 위치로 복귀
        if (isInTargetTrigger && targetPosition != null)
        {
            Debug.Log("target position is valid");
            StartCoroutine(FlyTo(targetPosition.position, 1f));
        }
        else
        {
            Debug.Log("target position is not valid");
            StartCoroutine(FlyTo(originalPosition, 0.5f));
        }
    }



    public void SetTargetPosition(Transform target)
    {
        targetPosition = target;
        isInTargetTrigger = true;
    }

    public void ClearTargetPosition()
    {
        isInTargetTrigger = false;
        targetPosition = null;
    }

    private IEnumerator FlyTo(Vector3 target, float duration)
    {
        float time = 0f;
        Vector3 start = transform.position;
        Quaternion startRot = transform.rotation;

        if (TryGetComponent<Rigidbody>(out var rb)) Destroy(rb);
        if (TryGetComponent<Collider>(out var col)) Destroy(col);

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            transform.position = Vector3.Lerp(start, target, t);
            transform.rotation = Quaternion.Slerp(startRot, originalRotation, t);

            yield return null;
        }

        transform.position = target;

        if (!TryGetComponent<Rigidbody>(out var rb2))
        {
            rb2 = gameObject.AddComponent<Rigidbody>();
            rb2.isKinematic = true; // 필요 시 false로
        }

        if (!TryGetComponent<Collider>(out var col2))
        {
            col2 = gameObject.AddComponent<BoxCollider>();
            col2.isTrigger = false;
        }
    }
}