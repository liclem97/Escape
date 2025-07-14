using System.Collections;
using UnityEngine;
using Photon.Pun;

/* 플레이어가 진입 시 스테이지를 활성화하는 스크립트 */
public class StageVolume : MonoBehaviourPun
{
    [Header("Enemy Control")]
    [SerializeField] protected GameObject[] enemiesInVolume;        // 스테이지에 설정된 적

    [Header("Shortcut Control")]
    [SerializeField] private GameObject shortcutGate;               // 숏컷의 게이트

    [Header("Next Stage Destinations")]
    [SerializeField] private Transform player1DefaultMovePoint;     // 플레이어1의 기본 이동 위치
    [SerializeField] private Transform player1ShortcutMovePoint;    // 플레이어1의 숏컷 이동 위치
    [SerializeField] private Transform player2DefaultMovePoint;     // 플레이어2의 기본 이동 위치
    [SerializeField] private Transform player2ShortcutMovePoint;    // 플레이어2의 숏컷 이동 위치

    [SerializeField] private float moveTime = 1f;                   // 기본 이동 시간
    [SerializeField] private float shortCutMoveTime = 0f;           // 숏컷 이동 시간

    protected bool isActivated = false;                             // 기본 값은 비활성화

    protected virtual void Awake()
    {
        SetEnemiesActive(false);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (isActivated) return;

        // 플레이어 진입 시 활성화
        if (other.CompareTag("Player") && other.GetComponent<PhotonView>().IsMine)
        {
            photonView.RPC(nameof(ActivateVolume), RpcTarget.AllBuffered);
        }
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: ActivateVolume
    * 기능: 플레이어 진입 시 볼륨을 활성화
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
    * 작성자: 박원정
    * 함수: SetEnemiesActive
    * 기능: 배열에 저장된 적들을 모두 활성화한다
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
    * 작성자: 박원정
    * 함수: CheckEnemiesAndTransition
    * 기능: 활성화 한 적의 상태를 감시한다
    ***********************************************************************************/
    private IEnumerator CheckEnemiesAndTransition()
    {
        while (true)
        {
            if (GameManager.Instance.IsGameOver) yield break;
            yield return new WaitForSeconds(2f);    // 감시 주기는 2초

            bool anyAlive = false;
            foreach (var enemy in enemiesInVolume)
            {
                if (enemy != null)
                {
                    anyAlive = true; 
                    break;
                }
            }

            // 스테이지의 적이 모두 죽은 경우 다음 위치로 플레이어 이동
            if (!anyAlive)
            {
                photonView.RPC(nameof(MovePlayersToNextStage), RpcTarget.AllBuffered);
                yield break;
            }
        }
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: MovePlayersToNextStage
    * 기능: 플레이어를 다음 스테이지로 이동시킨다
    ***********************************************************************************/
    [PunRPC]
    private void MovePlayersToNextStage()
    {
        if (GameManager.Instance == null || CameraMove.Instance == null) return;

        GameObject player1 = GameManager.Instance.GetPlayer1();
        GameObject player2 = CameraMove.Instance.Vehicle;

        // 숏컷 활성화 확인
        bool useShortcut = (shortcutGate != null && !shortcutGate.activeInHierarchy);
        float moveNextStageTime = moveTime;
        if (useShortcut)
        {
            moveNextStageTime = shortCutMoveTime;
        }

        // 숏컷 여부에 따른 다음 위치 설정
        Transform dest1 = useShortcut ? player1ShortcutMovePoint : player1DefaultMovePoint;
        Transform dest2 = useShortcut ? player2ShortcutMovePoint : player2DefaultMovePoint;

        // 플레이어 이동
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