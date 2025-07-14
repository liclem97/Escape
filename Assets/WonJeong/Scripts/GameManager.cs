using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/* 게임 매니저 스크립트 */
public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }
    public static VRPlayerHealth player1Health; // 플레이어1의 체력 참조
    public static float sharedHealthPercent = 1f; // 플레이어1의 체력 퍼센트
    public static bool isFadingOut = false; // FadeOut 실행 변수
    public static bool isFadingIn = false;  // FadeIn 실행 변수

    [Header("Spawn Points")]
    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;   // 플레이어들의 스폰 위치 저장

    [Header("Stage Start Point")]
    [SerializeField] private Transform player1Stage1Point;
    [SerializeField] private Transform player2Stage1Point;  // 게임 시작 시 스테이지 1의 이동 지점

    [Header("Managers")]
    [SerializeField] private CameraMove cameraMoveManager;

    [Header("Players (Runtime Assigned)")]
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;            // 런타임 중 플레이어 저장 변수

    [Header("Item Spawner")]
    [SerializeField] private ItemSpawner healPackItemSpawner;   // 힐팩 아이템 스포너 저장 변수

    [Header("Move Points")]
    [SerializeField] private Transform gameoverMovePoint;   // 게임 오버시 이동할 위치
    [SerializeField] private Transform gameclearMovePoint;  // 게임 클리어시 이동할 위치

    [Header("Game information UI")]
    [SerializeField] private GameObject beforeStartUI;
    [SerializeField] private Text beforeStartUI_Text;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject gameClearUI;    // 각 UI 저장

    [Header("Sound")]
    [SerializeField] private AudioClip gamePlayBGM;
    [SerializeField] private AudioClip bossBGM;         // 게임 진행, 보스 BGM 저장

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

        // PhotonView 컴포넌트가 필수
        if (photonView == null)
        {
            Debug.LogError("[GameManager] PhotonView 컴포넌트가 없습니다.");
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
        // SharedHealthPercent가 0 이하가 되면 게임 오버
        if (!isGameOver && player1Health != null && sharedHealthPercent <= 0f)
        {
            isGameOver = true;

            photonView.RPC(nameof(RPC_SetIsFadeOut), RpcTarget.AllBuffered, true);  // 모든 클라이언트에서 페이드 아웃

            OnGameOver();
        }
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: OnGameStart
    * 기능: 게임을 시작하는 함수 
    ***********************************************************************************/
    private void OnGameStart()
    {
        if (player1 == null || player2 == null) return;

        // 게임 시작 후 처음 포인트로 자동 이동
        if (player1Stage1Point != null && player2Stage1Point != null)
        {
            CameraMove.Instance.StartCoroutine(CameraMove.Instance.MoveToPosition(player1, player1Stage1Point, 2f));
            CameraMove.Instance.StartCoroutine(CameraMove.Instance.MoveToPosition(CameraMove.Instance.Vehicle, player2Stage1Point, 2f));
        }

        // 아이템 스폰 시작 및 힐팩 목표 플레이어 지정
        ItemTargetTrigger trigger = FindFirstObjectByType<ItemTargetTrigger>();
        if (trigger != null && healPackItemSpawner)
        {
            trigger.FlyTarget = player1.transform;
            healPackItemSpawner.ItemSpawnStart();
        }

        // 플레이어1 리볼버 탄창 스폰 시작
        VRPlayer player = player1.GetComponent<VRPlayer>();
        if (player != null)
        {
            player.StartAmmoSpawn();
        }

        // BGM 시작
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
    * 작성자: 박원정
    * 함수: OnGameOver
    * 기능: 게임 오버 시 실행되는 함수
    ***********************************************************************************/
    private void OnGameOver()
    {
        Debug.Log("[GameManager] 게임 오버!");

        // 게임오버 지점으로 이동 시작
        photonView.RPC(nameof(RPC_DelayAndMovePlayers), RpcTarget.AllBuffered, 2.5f, gameoverMovePoint.position, gameoverMovePoint.rotation);

        DisableAllZombieSpawners();
        DisableAllEnemyBases();         // 모든 좀비 스포너 및 살아있는 좀비 비활성화

        photonView.RPC(nameof(RPC_SetActiveGameOverUI), RpcTarget.AllBuffered);
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: OnGameClear
    * 기능: 게임 클리어 시 실행되는 함수
    ***********************************************************************************/
    public void OnGameClear()
    {
        Debug.Log("[GameManager] 게임 클리어!");

        photonView.RPC(nameof(RPC_SetIsFadeOut), RpcTarget.AllBuffered, true);  // 페이드 아웃
        photonView.RPC(nameof(RPC_DelayAndMovePlayers), RpcTarget.AllBuffered, 3f, gameclearMovePoint.position, gameclearMovePoint.rotation); // 클리어 지점으로 이동

        DisableAllZombieSpawners();
        KillAllZombies();   // 모든 좀비 스포너 비활성화 및 살아있는 좀비를 모두 죽임

        Debug.Log("gameclear rpc 호출");
        photonView.RPC(nameof(RPC_SetActiveGameClearUI), RpcTarget.AllBuffered);
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: DisableAllEnemyBases
    * 기능: 활성화 되어있는 모든 좀비를 비활성화 시킴
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
    * 작성자: 박원정
    * 함수: DisableAllEnemyBases
    * 기능: 활성화 되어있는 모든 좀비를 죽임
    ***********************************************************************************/
    private void KillAllZombies()
    {
        // 씬에 있는 모든 EnemyBase 컴포넌트를 가진 좀비를 찾음
        EnemyBase[] zombies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);

        int myViewID = photonView.ViewID; // GameManager의 PhotonView ID

        foreach (EnemyBase zombie in zombies)
        {
            // 죽은 좀비는 무시
            if (zombie == null || zombie.gameObject == null) continue;

            // 강제로 큰 데미지를 줌
            zombie.TakeDamage(9999f, myViewID);
        }

        Debug.Log("[GameManager] 모든 좀비를 제거함");
    }

    /***********************************************************************************
    * 작성자: 박원정
    * 함수: DisableAllZombieSpawners
    * 기능: 좀비 스포너를 꺼서 스폰을 막음
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
    * 작성자: 박원정
    * 함수: DelayAndMovePlayers
    * 기능: 설정한 시간 이후 저장한 위치로 플레이어를 이동시킴
    * 입력: 
    *   delay: 대기하는 시간
    *   movePos: 이동 위치
    *   moveRot: 이동 회전값
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
    * 작성자: 박원정
    * 함수: MoveToPosition
    * 기능: 저장한 위치로 플레이어를 이동시킴
    * 입력: 
    *   target: 이동 목표
    *   endPos: 목표의 위치
    *   endRot: 목표의 회전 값
    *   duration: 이동 시간
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
    * 작성자: 박원정
    * 함수: OnPlayerJoinedRoom
    * 기능: 플레이어 입장 시 실행 Callback 함수  
    ***********************************************************************************/
    public void OnPlayerJoinedRoom()
    {
        // 스폰할 플레이어 프리팹
        string prefabName = "VRPlayer";

        // 플레이어 1과 2의 스폰 위치 설정
        Transform spawn = PhotonNetwork.CurrentRoom.PlayerCount == 1
            ? player1SpawnPoint
            : player2SpawnPoint;

        if (spawn == null)
        {
            Debug.LogError("[GM] Spawn point is null! Cannot instantiate player.");
            return;
        }

        // 플레이어 생성
        GameObject player = PhotonNetwork.Instantiate(prefabName, spawn.position, spawn.rotation);

        // 닉네임으로 이름 설정 (로컬에서만 적용됨)   
        player.name = PhotonNetwork.NickName;

        // 모든 클라이언트에 플레이어 재할당 명령 전달
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
    * 작성자: 박원정
    * 함수: AssignPlayers
    * 기능: 플레이어 입장 시 게임 매니저에 각 플레이어를 할당함
    ***********************************************************************************/
    private void AssignPlayers()
    {
        // 모든 포톤 뷰를 탐색함
        foreach (var view in FindObjectsByType<PhotonView>(FindObjectsSortMode.None))
        {
            if (view.CompareTag("Player"))  // Player 태그가 있는지 확인
            {
                int actorNumber = view.Owner.ActorNumber; // 플레이어의 액터 넘버 확인

                if (actorNumber == 1 && player1 == null)    // 1번 플레이어인 경우
                {
                    player1 = view.gameObject;

                    // InitPlayer1 호출
                    VRPlayer vrPlayer = player1.GetComponent<VRPlayer>();
                    vrPlayer?.InitPlayer1();
                }
                else if (actorNumber == 2 && player2 == null)   // 2번 플레이어인 경우
                {
                    player2 = view.gameObject;
                    if (CameraMove.Instance != null)
                    {                       
                        if (CameraMove.Instance.Vehicle != null)
                        {   
                            // 헬리콥터의 AttachPoint를 찾아서 저장
                            Transform attachPoint = CameraMove.Instance.Vehicle.transform.Find("AttachPoint");
                            if (attachPoint != null)
                            {
                                player2.transform.SetParent(attachPoint);
                                player2.transform.localPosition = Vector3.zero;
                                player2.transform.localRotation = Quaternion.identity;  // 플레이어2를 헬리콥터의 AttachPoint에 붙임
                            }
                        }
                    }
                    // InitPlayer2 호출
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
                player1Health.photonView.RPC(nameof(VRPlayerHealth.SetInvincible), RpcTarget.AllBuffered, false); // Player1 무적 해제
            }
        }

        if (player2 != null)
        {
            VRPlayerHealth p2Health = player2.GetComponent<VRPlayerHealth>();
            if (p2Health != null)
            {
                p2Health.photonView.RPC(nameof(VRPlayerHealth.SetInvincible), RpcTarget.AllBuffered, true); // Player2 무적 설정
            }
        }

        if (player1 != null && player2 != null)
        {
            photonView.RPC(nameof(RPC_ShowBeforeStartText), RpcTarget.All, "Start Game Soon...");

            // 게임 시작 지연 후 실제 시작
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
        // UI 숨김
        beforeStartUI.SetActive(false);

        OnGameStart();
    }

    [PunRPC]
    public void RPC_SetSharedHealthPercent(float value)
    {
        sharedHealthPercent = value;
        Debug.Log($"[GameManager] sharedHealthPercent 갱신됨: {value}");
    }

    public void PlayBossBGM()
    {
        if (BGMPlayer.Instance != null && bossBGM != null)
        {
            BGMPlayer.Instance.PlayBGM(bossBGM);
        }
    }
}