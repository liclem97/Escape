using System.Collections;
using UnityEngine;
using Photon.Pun;

/* �÷��̾ ���� �� ���������� Ȱ��ȭ�ϴ� ��ũ��Ʈ */
public class StageVolume : MonoBehaviourPun
{
    [Header("Enemy Control")]
    [SerializeField] protected GameObject[] enemiesInVolume;        // ���������� ������ ��

    [Header("Shortcut Control")]
    [SerializeField] private GameObject shortcutGate;               // ������ ����Ʈ

    [Header("Next Stage Destinations")]
    [SerializeField] private Transform player1DefaultMovePoint;     // �÷��̾�1�� �⺻ �̵� ��ġ
    [SerializeField] private Transform player1ShortcutMovePoint;    // �÷��̾�1�� ���� �̵� ��ġ
    [SerializeField] private Transform player2DefaultMovePoint;     // �÷��̾�2�� �⺻ �̵� ��ġ
    [SerializeField] private Transform player2ShortcutMovePoint;    // �÷��̾�2�� ���� �̵� ��ġ

    [SerializeField] private float moveTime = 1f;                   // �⺻ �̵� �ð�
    [SerializeField] private float shortCutMoveTime = 0f;           // ���� �̵� �ð�

    protected bool isActivated = false;                             // �⺻ ���� ��Ȱ��ȭ

    protected virtual void Awake()
    {
        SetEnemiesActive(false);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (isActivated) return;

        // �÷��̾� ���� �� Ȱ��ȭ
        if (other.CompareTag("Player") && other.GetComponent<PhotonView>().IsMine)
        {
            photonView.RPC(nameof(ActivateVolume), RpcTarget.AllBuffered);
        }
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: ActivateVolume
    * ���: �÷��̾� ���� �� ������ Ȱ��ȭ
    ***********************************************************************************/
    [PunRPC]
    protected virtual void ActivateVolume()
    {
        if (isActivated) return;

        isActivated = true;
        SetEnemiesActive(true);
        StartCoroutine(CheckEnemiesAndTransition());
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: SetEnemiesActive
    * ���: �迭�� ����� ������ ��� Ȱ��ȭ�Ѵ�
    ***********************************************************************************/
    protected virtual void SetEnemiesActive(bool active)
    {
        foreach (var enemy in enemiesInVolume)
        {
            if (enemy != null)
                enemy.SetActive(active);
        }
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: CheckEnemiesAndTransition
    * ���: Ȱ��ȭ �� ���� ���¸� �����Ѵ�
    ***********************************************************************************/
    private IEnumerator CheckEnemiesAndTransition()
    {
        while (true)
        {
            if (GameManager.Instance.IsGameOver) yield break;
            yield return new WaitForSeconds(2f);    // ���� �ֱ�� 2��

            bool anyAlive = false;
            foreach (var enemy in enemiesInVolume)
            {
                if (enemy != null)
                {
                    anyAlive = true; 
                    break;
                }
            }

            // ���������� ���� ��� ���� ��� ���� ��ġ�� �÷��̾� �̵�
            if (!anyAlive)
            {
                photonView.RPC(nameof(MovePlayersToNextStage), RpcTarget.AllBuffered);
                yield break;
            }
        }
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: MovePlayersToNextStage
    * ���: �÷��̾ ���� ���������� �̵���Ų��
    ***********************************************************************************/
    [PunRPC]
    private void MovePlayersToNextStage()
    {
        if (GameManager.Instance == null || CameraMove.Instance == null) return;

        GameObject player1 = GameManager.Instance.GetPlayer1();
        GameObject player2 = CameraMove.Instance.Vehicle;

        // ���� Ȱ��ȭ Ȯ��
        bool useShortcut = (shortcutGate != null && !shortcutGate.activeInHierarchy);
        float moveNextStageTime = moveTime;
        if (useShortcut)
        {
            moveNextStageTime = shortCutMoveTime;
        }

        // ���� ���ο� ���� ���� ��ġ ����
        Transform dest1 = useShortcut ? player1ShortcutMovePoint : player1DefaultMovePoint;
        Transform dest2 = useShortcut ? player2ShortcutMovePoint : player2DefaultMovePoint;

        // �÷��̾� �̵�
        if (player1 != null && dest1 != null)
        {
            CameraMove.Instance.StartCoroutine(CameraMove.Instance.MoveToPosition(player1, dest1, moveNextStageTime));
        }

        if (player2 != null && dest2 != null)
        {
            CameraMove.Instance.StartCoroutine(CameraMove.Instance.MoveToPosition(player2, dest2, moveNextStageTime));
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        Gizmos.DrawWireCube(boxCollider.transform.position, boxCollider.size);
    }
}