using Photon.Pun;
using UnityEngine;

/* �÷��̾� ������ ��ũ��Ʈ */
public class VRPlayer : MonoBehaviourPun
{
    [Header("Character Reference")]
    [SerializeField] private GameObject characterHead;      // �÷��̾��� �Ӹ� ��
    [SerializeField] private GameObject characterLeftHand;  // �÷��̾��� �޼� ��

    [Header("Item")]
    [SerializeField] private float itemGrabDistance;        // �������� ���� �� �ִ� ����
    [SerializeField] private ItemSpawner ammoSpanwer;       // ������ źâ ������

    private Transform rightHand;
    private Transform leftHand;
    private Transform hmd;
    private OVRCameraRig rig;

    private Item heldItem = null;

    private GameObject pistol;      // �ǽ���
    private GameObject revolver;    // ������
    private GameObject sniperRifle; // �������� ������

    private void Start()
    {
        rig = GetComponentInChildren<OVRCameraRig>();

        if (!photonView.IsMine)
        {
            if (rig != null && rig.centerEyeAnchor != null)
            {
                // �� ���� �ƴ� �÷��̾��� centerEyeAnchor ��Ȱ��ȭ
                rig.centerEyeAnchor.gameObject.SetActive(false);
            }

            return;
        }

        if (rig != null)
        {
            rightHand = rig.rightHandAnchor;
            leftHand = rig.leftHandAnchor;
            hmd = rig.centerEyeAnchor;
        }

        StartAmmoSpawn();
    }

    private void Update()
    {   
        // ������ �ƴ� �÷��̾��� ������Ʈ�� ����
        if (!photonView.IsMine) return;
        if (GameManager.Instance.IsGameOver)
        {
            photonView.RPC(nameof(HideAllMeshes), RpcTarget.AllBuffered);            
            return;
        }

        // ĳ���� �Ӹ� ���� ��ġ�� hmd�� ����
        if (hmd != null && characterHead != null)
        {
            characterHead.transform.position = hmd.position;
            characterHead.transform.rotation = hmd.rotation;
        }

        // ĳ���� �޼� ���� ��ġ�� leftHand�� ����
        if (leftHand != null && characterLeftHand != null)
        {
            characterLeftHand.transform.position = leftHand.position;
            
            Quaternion offsetRotation = Quaternion.Euler(0f, 0f, 0f);
            characterLeftHand.transform.rotation = leftHand.rotation * offsetRotation;
        }

        // �޼� �ڵ�Ʈ���Ÿ� ������ ��� �������� ������ ��
        if (ARAVRInput.GetDown(ARAVRInput.Button.HandTrigger, ARAVRInput.Controller.LTouch))
        {
            TryPickupItem();
        }

        // �޼� �ڵ�Ʈ���Ÿ� ���� ��� �������� ���
        if (ARAVRInput.GetUp(ARAVRInput.Button.HandTrigger, ARAVRInput.Controller.LTouch))
        {
            TryDropItem();
        }

        // ������ 1�� ��ư�� ���� ��� �ѱ� ���� �õ�
        if (ARAVRInput.GetDown(ARAVRInput.Button.One, ARAVRInput.Controller.RTouch))
        {
            SwapWeapon();
        }
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: SwapWeapon
    * ���: �÷��̾�1�� �ѱ⸦ �����ϰ� ��� Ŭ���̾�Ʈ���� ����ȭ��
    ***********************************************************************************/
    [PunRPC]
    private void SwapWeapon()
    {
        // �������� �������� Ȱ��ȭ ���� ��� (2P)
        if (sniperRifle != null && sniperRifle.activeSelf) return;
        
        // �ѱ� ����
        if (pistol != null && revolver != null)
        {
            if (pistol.activeSelf && !revolver.activeSelf)
            {
                pistol.SetActive(false);
                revolver.SetActive(true);
            }
            else if (revolver.activeSelf && !pistol.activeSelf)
            {
                revolver.SetActive(false);
                pistol.SetActive(true);
            }
        }
    }

    // �÷��̾�1�� �ʱ�ȭ �Լ�
    public void InitPlayer1()
    {
        // ���� ��Ʈ��ũ�� ����
        // �ǽ���, �������� �����ϰ� �����տ� ���δ�.
        if (photonView.IsMine && rightHand != null)
        {
            revolver = PhotonNetwork.Instantiate("Revolver", rightHand.position, rightHand.rotation);
            pistol = PhotonNetwork.Instantiate("Pistol", rightHand.position, rightHand.rotation);
            photonView.RPC(nameof(AttachGunToHand), RpcTarget.AllBuffered, revolver.GetComponent<PhotonView>().ViewID);
            photonView.RPC(nameof(AttachGunToHand), RpcTarget.AllBuffered, pistol.GetComponent<PhotonView>().ViewID);

            revolver.SetActive(false); // �������� ó�� ���� �� ��Ȱ��ȭ
        }
    }

    // �÷��̾�2�� �ʱ�ȭ �Լ�
    public void InitPlayer2()
    {
        // ���� ��Ʈ��ũ�� ����
        if (photonView.IsMine && rightHand != null)
        {
            sniperRifle = PhotonNetwork.Instantiate("SniperRifle", rightHand.position, rightHand.rotation);
            photonView.RPC(nameof(AttachGunToHand), RpcTarget.AllBuffered, sniperRifle.GetComponent<PhotonView>().ViewID);
        }
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: AttachGunToHand
    * ���: ������ �ѱ⸦ �����տ� ���̴� �Լ�
    * �Է�:
    *   - gunViewID: ������ ���� ViewId
    ***********************************************************************************/
    [PunRPC]
    private void AttachGunToHand(int gunViewID)
    {
        PhotonView gunView = PhotonView.Find(gunViewID);
        if (gunView != null)
        {
            Transform hand = GetComponentInChildren<OVRCameraRig>()?.rightHandAnchor;
            if (hand != null)
            {
                // �θ� ���� ��� ��ġ/ȸ���� ���󰡵��� ����
                Gun gunScript = gunView.GetComponent<Gun>();
                if (gunScript != null)
                {
                    gunScript.SetTargetHand(hand);
                }
            }
        }
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: TryPickupItem
    * ���: �������� ���� �޼տ� ���̴� �Լ�   
    ***********************************************************************************/
    private void TryPickupItem()
    {
        if (heldItem != null) return; // �̹� ��� ����

        // ���� ������ �ݶ��̴��� Ž��
        Collider[] colliders = Physics.OverlapSphere(leftHand.position, itemGrabDistance);
        foreach (var col in colliders)
        {
            Item item = col.GetComponent<Item>();
            if (item != null)
            {
                heldItem = item;    // ���� ������ ����
                item.AttachToLeftHand(photonView.ViewID); // �� ViewID�� �ѱ�

                if (characterLeftHand != null)
                {
                    characterLeftHand.SetActive(false); // �޼� �� ��� ��Ȱ��ȭ
                }

                break; // ù ��° �����۸� ����
            }
        }
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: TryDropItem
    * ���: �������� ����� �õ��ϴ� �Լ�
    ***********************************************************************************/
    private void TryDropItem()
    {
        if (heldItem == null) return;

        heldItem.DetachFromHand(); // ���� �θ� ����
        heldItem = null;

        if (characterLeftHand != null)
        {
            characterLeftHand.SetActive(true);
        }
    }

    public void StartAmmoSpawn()
    {
        if (ammoSpanwer != null)
        {
            ammoSpanwer.ItemSpawnStart();
        }
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: HideAllMeshes
    * ���: ���� ���� �� ������ �޽��� ����� �Լ�
    ***********************************************************************************/
    [PunRPC]
    private void HideAllMeshes()
    {
        if (characterLeftHand) characterLeftHand.SetActive(false);
        if (characterHead) characterHead.SetActive(false);
        if (pistol) pistol.SetActive(false);
        if (revolver) revolver.SetActive(false);
        if (sniperRifle) sniperRifle.SetActive(false);
        if (ammoSpanwer)
        {
            ammoSpanwer.StopAllCoroutines();
            if (ammoSpanwer.CurrentItem)
            {
                ammoSpanwer.CurrentItem.SetActive(false);
            }            
        }
        if (TryGetComponent<Collider>(out Collider collider))
        {
            collider.enabled = false;
        }
    }

    private void OnDrawGizmos()
    {
        //    if (rightHand == null || leftHand == null || hmd == null)
        //    {
        //        var rig = GetComponentInChildren<OVRCameraRig>();
        //        if (rig != null)
        //        {
        //            rightHand = rig.rightHandAnchor;
        //            leftHand = rig.leftHandAnchor;
        //            hmd = rig.centerEyeAnchor;
        //        }
        //    }

        //    if (rightHand != null)
        //    {
        //        Gizmos.color = Color.blue;
        //        Gizmos.DrawSphere(rightHand.position, 0.05f);
        //    }

        //    if (leftHand != null)
        //    {
        //        Gizmos.color = Color.red;
        //        Gizmos.DrawSphere(leftHand.position, itemGrabDistance);
        //    }

        //    if (hmd != null)
        //    {
        //        Gizmos.color = Color.yellow;
        //        Gizmos.DrawSphere(hmd.position, 0.05f);
        //    }
    }
}