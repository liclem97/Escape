using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/* ���� �Ŵ��� ��ũ��Ʈ */
public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }
    public static VRPlayerHealth player1Health; // �÷��̾�1�� ü�� ����
    public static float sharedHealthPercent = 1f; // �÷��̾�1�� ü�� �ۼ�Ʈ
    public static bool isFadingOut = false; // FadeOut ���� ����
    public static bool isFadingIn = false;  // FadeIn ���� ����

    [Header("Spawn Points")]
    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;   // �÷��̾���� ���� ��ġ ����

    [Header("Stage Start Point")]
    [SerializeField] private Transform player1Stage1Point;
    [SerializeField] private Transform player2Stage1Point;  // ���� ���� �� �������� 1�� �̵� ����

    [Header("Managers")]
    [SerializeField] private CameraMove cameraMoveManager;

    [Header("Players (Runtime Assigned)")]
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;            // ��Ÿ�� �� �÷��̾� ���� ����

    [Header("Item Spawner")]
    [SerializeField] private ItemSpawner healPackItemSpawner;   // ���� ������ ������ ���� ����

    [Header("Move Points")]
    [SerializeField] private Transform gameoverMovePoint;   // ���� ������ �̵��� ��ġ
    [SerializeField] private Transform gameclearMovePoint;  // ���� Ŭ����� �̵��� ��ġ

    [Header("Game information UI")]
    [SerializeField] private GameObject beforeStartUI;
    [SerializeField] private Text beforeStartUI_Text;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject gameClearUI;    // �� UI ����

    [Header("Sound")]
    [SerializeField] private AudioClip gamePlayBGM;
    [SerializeField] private AudioClip bossBGM;         // ���� ����, ���� BGM ����

    public GameObject GetPlayer1() => player1;
    public GameObject GetPlayer2() => player2;

    private bool isGameOver = false;
    private bool isGameClear = false;

    public bool IsGameOver
    {
        get => isGameOver;
    }

    public bool IsGameClear
    {
        get => isGameClear;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // PhotonView ������Ʈ�� �ʼ�
        if (photonView == null)
        {
            Debug.LogError("[GameManager] PhotonView ������Ʈ�� �����ϴ�.");
        }
    }

    private void Start()
    {
        Debug.Log($"VRPlayer Start() - IsMine: {photonView.IsMine}");
        if (gameOverUI) gameOverUI.SetActive(false);
        if (gameClearUI) gameClearUI.SetActive(false);
    }

    private void Update()
    {
        // SharedHealthPercent�� 0 ���ϰ� �Ǹ� ���� ����
        if (!isGameOver && player1Health != null && sharedHealthPercent <= 0f)
        {
            isGameOver = true;

            photonView.RPC(nameof(RPC_SetIsFadeOut), RpcTarget.AllBuffered, true);  // ��� Ŭ���̾�Ʈ���� ���̵� �ƿ�

            OnGameOver();
        }
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: OnGameStart
    * ���: ������ �����ϴ� �Լ� 
    ***********************************************************************************/
    private void OnGameStart()
    {
        if (player1 == null || player2 == null) return;

        // ���� ���� �� ó�� ����Ʈ�� �ڵ� �̵�
        if (player1Stage1Point != null && player2Stage1Point != null)
        {
            CameraMove.Instance.StartCoroutine(CameraMove.Instance.MoveToPosition(player1, player1Stage1Point, 2f));
            CameraMove.Instance.StartCoroutine(CameraMove.Instance.MoveToPosition(CameraMove.Instance.Vehicle, player2Stage1Point, 2f));
        }

        // ������ ���� ���� �� ���� ��ǥ �÷��̾� ����
        ItemTargetTrigger trigger = FindFirstObjectByType<ItemTargetTrigger>();
        if (trigger != null && healPackItemSpawner)
        {
            trigger.FlyTarget = player1.transform;
            healPackItemSpawner.ItemSpawnStart();
        }

        // �÷��̾�1 ������ źâ ���� ����
        VRPlayer player = player1.GetComponent<VRPlayer>();
        if (player != null)
        {
            player.StartAmmoSpawn();
        }

        // BGM ����
        if (BGMPlayer.Instance != null && gamePlayBGM != null)
        {
            BGMPlayer.Instance.PlayBGM(gamePlayBGM);
        }
    }

    [PunRPC]
    void RPC_SetIsFadeOut(bool shouldFadeOut)
    {
        isFadingIn = !shouldFadeOut;
        isFadingOut = shouldFadeOut;
    }

    [PunRPC]
    private void RPC_SetActiveGameClearUI()
    {
        if (gameClearUI) gameClearUI.SetActive(true);
    }

    [PunRPC]
    private void RPC_SetActiveGameOverUI()
    {
        if (gameOverUI) gameOverUI.SetActive(true);
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: OnGameOver
    * ���: ���� ���� �� ����Ǵ� �Լ�
    ***********************************************************************************/
    private void OnGameOver()
    {
        Debug.Log("[GameManager] ���� ����!");

        // ���ӿ��� �������� �̵� ����
        photonView.RPC(nameof(RPC_DelayAndMovePlayers), RpcTarget.AllBuffered, 2.5f, gameoverMovePoint.position, gameoverMovePoint.rotation);

        DisableAllZombieSpawners();
        DisableAllEnemyBases();         // ��� ���� ������ �� ����ִ� ���� ��Ȱ��ȭ

        photonView.RPC(nameof(RPC_SetActiveGameOverUI), RpcTarget.AllBuffered);
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: OnGameClear
    * ���: ���� Ŭ���� �� ����Ǵ� �Լ�
    ***********************************************************************************/
    public void OnGameClear()
    {
        Debug.Log("[GameManager] ���� Ŭ����!");

        photonView.RPC(nameof(RPC_SetIsFadeOut), RpcTarget.AllBuffered, true);  // ���̵� �ƿ�
        photonView.RPC(nameof(RPC_DelayAndMovePlayers), RpcTarget.AllBuffered, 3f, gameclearMovePoint.position, gameclearMovePoint.rotation); // Ŭ���� �������� �̵�

        DisableAllZombieSpawners();
        KillAllZombies();   // ��� ���� ������ ��Ȱ��ȭ �� ����ִ� ���� ��� ����

        Debug.Log("gameclear rpc ȣ��");
        photonView.RPC(nameof(RPC_SetActiveGameClearUI), RpcTarget.AllBuffered);
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: DisableAllEnemyBases
    * ���: Ȱ��ȭ �Ǿ��ִ� ��� ���� ��Ȱ��ȭ ��Ŵ
    ***********************************************************************************/
    private void DisableAllEnemyBases()
    {
        var enemyBases = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        foreach (var enemy in enemyBases)
        {
            enemy.gameObject.SetActive(false);
        }
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: DisableAllEnemyBases
    * ���: Ȱ��ȭ �Ǿ��ִ� ��� ���� ����
    ***********************************************************************************/
    private void KillAllZombies()
    {
        // ���� �ִ� ��� EnemyBase ������Ʈ�� ���� ���� ã��
        EnemyBase[] zombies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);

        int myViewID = photonView.ViewID; // GameManager�� PhotonView ID

        foreach (EnemyBase zombie in zombies)
        {
            // ���� ����� ����
            if (zombie == null || zombie.gameObject == null) continue;

            // ������ ū �������� ��
            zombie.TakeDamage(9999f, myViewID);
        }

        Debug.Log("[GameManager] ��� ���� ������");
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: DisableAllZombieSpawners
    * ���: ���� �����ʸ� ���� ������ ����
    ***********************************************************************************/
    private void DisableAllZombieSpawners()
    {
        ZombieSpawner[] zSpawners = FindObjectsByType<ZombieSpawner>(FindObjectsSortMode.None);
        foreach (ZombieSpawner spawner in zSpawners)
        {
            spawner.enabled = false;
        }
    }

    [PunRPC]
    private void RPC_DelayAndMovePlayers(float delay, Vector3 movePos, Quaternion moveRot)
    {
        StartCoroutine(DelayAndMovePlayers(delay, movePos, moveRot));
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: DelayAndMovePlayers
    * ���: ������ �ð� ���� ������ ��ġ�� �÷��̾ �̵���Ŵ
    * �Է�: 
    *   delay: ����ϴ� �ð�
    *   movePos: �̵� ��ġ
    *   moveRot: �̵� ȸ����
    ***********************************************************************************/
    private IEnumerator DelayAndMovePlayers(float delay, Vector3 movePos, Quaternion moveRot)
    {
        if (movePos == null) yield break;
        yield return new WaitForSeconds(delay);

        float moveDuration = 0.5f;

        if (player1 != null)
        {
            yield return StartCoroutine(MoveToPosition(player1.transform, movePos, moveRot, moveDuration));
        }

        if (player2 != null)
        {
            yield return StartCoroutine(MoveToPosition(player2.transform, movePos, moveRot, moveDuration));
        }

        photonView.RPC(nameof(RPC_SetIsFadeOut), RpcTarget.AllBuffered, false);
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: MoveToPosition
    * ���: ������ ��ġ�� �÷��̾ �̵���Ŵ
    * �Է�: 
    *   target: �̵� ��ǥ
    *   endPos: ��ǥ�� ��ġ
    *   endRot: ��ǥ�� ȸ�� ��
    *   duration: �̵� �ð�
    ***********************************************************************************/
    private IEnumerator MoveToPosition(Transform target, Vector3 endPos, Quaternion endRot, float duration)
    {
        Vector3 startPos = target.position;
        Quaternion startRot = target.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            target.position = Vector3.Lerp(startPos, endPos, t);
            target.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        target.position = endPos;
        target.rotation = endRot;

        photonView.RPC(nameof(RPC_SetIsFadeOut), RpcTarget.AllBuffered, false);
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: OnPlayerJoinedRoom
    * ���: �÷��̾� ���� �� ���� Callback �Լ�  
    ***********************************************************************************/
    public void OnPlayerJoinedRoom()
    {
        // ������ �÷��̾� ������
        string prefabName = "VRPlayer";

        // �÷��̾� 1�� 2�� ���� ��ġ ����
        Transform spawn = PhotonNetwork.CurrentRoom.PlayerCount == 1
            ? player1SpawnPoint
            : player2SpawnPoint;

        if (spawn == null)
        {
            Debug.LogError("[GM] Spawn point is null! Cannot instantiate player.");
            return;
        }

        // �÷��̾� ����
        GameObject player = PhotonNetwork.Instantiate(prefabName, spawn.position, spawn.rotation);

        // �г������� �̸� ���� (���ÿ����� �����)   
        player.name = PhotonNetwork.NickName;

        // ��� Ŭ���̾�Ʈ�� �÷��̾� ���Ҵ� ��� ����
        photonView.RPC(nameof(RPC_AssignPlayers), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPC_AssignPlayers()
    {
        StartCoroutine(AssignPlayersAfterDelay());
    }

    private IEnumerator AssignPlayersAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        AssignPlayers();
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: AssignPlayers
    * ���: �÷��̾� ���� �� ���� �Ŵ����� �� �÷��̾ �Ҵ���
    ***********************************************************************************/
    private void AssignPlayers()
    {
        // ��� ���� �並 Ž����
        foreach (var view in FindObjectsByType<PhotonView>(FindObjectsSortMode.None))
        {
            if (view.CompareTag("Player"))  // Player �±װ� �ִ��� Ȯ��
            {
                int actorNumber = view.Owner.ActorNumber; // �÷��̾��� ���� �ѹ� Ȯ��

                if (actorNumber == 1 && player1 == null)    // 1�� �÷��̾��� ���
                {
                    player1 = view.gameObject;

                    // InitPlayer1 ȣ��
                    VRPlayer vrPlayer = player1.GetComponent<VRPlayer>();
                    vrPlayer?.InitPlayer1();
                }
                else if (actorNumber == 2 && player2 == null)   // 2�� �÷��̾��� ���
                {
                    player2 = view.gameObject;
                    if (CameraMove.Instance != null)
                    {                       
                        if (CameraMove.Instance.Vehicle != null)
                        {   
                            // �︮������ AttachPoint�� ã�Ƽ� ����
                            Transform attachPoint = CameraMove.Instance.Vehicle.transform.Find("AttachPoint");
                            if (attachPoint != null)
                            {
                                player2.transform.SetParent(attachPoint);
                                player2.transform.localPosition = Vector3.zero;
                                player2.transform.localRotation = Quaternion.identity;  // �÷��̾�2�� �︮������ AttachPoint�� ����
                            }
                        }
                    }
                    // InitPlayer2 ȣ��
                    VRPlayer vrPlayer = player2.GetComponent<VRPlayer>();
                    vrPlayer?.InitPlayer2();
                }
            }
        }

        string p1Name = player1 != null ? player1.name : "null";
        string p2Name = player2 != null ? player2.name : "null";
        Debug.Log($"[GameManager] Assigned Player1: {p1Name}, Player2: {p2Name}");

        if (player1 != null)
        {
            player1Health = player1.GetComponent<VRPlayerHealth>();
            if (player1Health != null)
            {
                player1Health.photonView.RPC(nameof(VRPlayerHealth.SetInvincible), RpcTarget.AllBuffered, false); // Player1 ���� ����
            }
        }

        if (player2 != null)
        {
            VRPlayerHealth p2Health = player2.GetComponent<VRPlayerHealth>();
            if (p2Health != null)
            {
                p2Health.photonView.RPC(nameof(VRPlayerHealth.SetInvincible), RpcTarget.AllBuffered, true); // Player2 ���� ����
            }
        }

        if (player1 != null && player2 != null)
        {
            photonView.RPC(nameof(RPC_ShowBeforeStartText), RpcTarget.All, "Start Game Soon...");

            // ���� ���� ���� �� ���� ����
            StartCoroutine(DelayedGameStart(2.5f));
        }
    }

    [PunRPC]
    private void RPC_ShowBeforeStartText(string message)
    {
        if (beforeStartUI_Text != null && beforeStartUI != null)
        {
            beforeStartUI_Text.text = message;
            beforeStartUI.SetActive(true);
        }
    }

    private IEnumerator DelayedGameStart(float delay)
    {
        yield return new WaitForSeconds(delay);
        // UI ����
        beforeStartUI.SetActive(false);

        OnGameStart();
    }

    [PunRPC]
    public void RPC_SetSharedHealthPercent(float value)
    {
        sharedHealthPercent = value;
        Debug.Log($"[GameManager] sharedHealthPercent ���ŵ�: {value}");
    }

    public void PlayBossBGM()
    {
        if (BGMPlayer.Instance != null && bossBGM != null)
        {
            BGMPlayer.Instance.PlayBGM(bossBGM);
        }
    }
}