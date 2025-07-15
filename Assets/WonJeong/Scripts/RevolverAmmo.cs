using Photon.Pun;
using UnityEngine;

/* ������ źâ ������ */
public class RevolverAmmo : Item
{
    [PunRPC]
    protected override void RPC_AttachToLeftHand(int playerViewID)
    {
        base.RPC_AttachToLeftHand(playerViewID);
    }

    [PunRPC]
    protected override void RPC_DetachFromHand()
    {
        base.RPC_DetachFromHand();
    }

    [PunRPC]
    protected override void RPC_FlyTo(Vector3 pos, Quaternion rot, float duration)
    {
        // �������� ��ġ�� �׻� ���� ��ġ�� �ǵ���
        base.RPC_FlyTo(Spawner.SpawnPoint.position, Spawner.SpawnPoint.rotation, 0.5f);
    }

    [PunRPC]
    protected override void UseItem()
    {
        DetachFromHand();
        base.UseItem(); // Destroy
    }

    // �������� �ε��� �� �������� Reload �Լ��� ȣ��
    protected void OnTriggerEnter(Collider other)
    {
        Debug.Log($"revolver ammo on trigger enter:{other.name}");
        Revolver revolver = other.GetComponent<Revolver>();
        if (revolver)
        {
            revolver.Reload();
            photonView.RPC(nameof(UseItem), RpcTarget.AllBuffered);
        }
        else
        {
            return;
        }
    }

    private void OnDrawGizmos()
    {
        SphereCollider sphere = GetComponent<SphereCollider>();
        if (sphere == null) return;

        // ����� ���� ���� (����)
        Gizmos.color = Color.cyan;

        // ���Ǿ� �ݶ��̴��� ���� ��ġ ��� (���� -> ���� ��ȯ ����)
        Vector3 center = transform.position + transform.rotation * Vector3.Scale(sphere.center, transform.lossyScale);
        float radius = sphere.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);

        Gizmos.DrawWireSphere(center, radius);
    }
}
