using Photon.Pun;
using System.Collections;
using UnityEngine;

/*  �÷��̾�1�� �÷��̾�2�� ���� �������� �̵���Ű�� ��ũ��Ʈ    */
public class CameraMove : MonoBehaviourPun
{
    public static CameraMove Instance { get; private set; }
    public enum PlayerType { Player1, Player2 }

    [SerializeField] private GameObject vehicle;    // �÷��̾�2�� Ÿ��ٴϴ� �︮����

    public GameObject Vehicle
    {
        get => vehicle;
        set => vehicle = value;
    }

    private void Awake()
    {
        // �̱��� �ʱ�ȭ
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /***********************************************************************************
    * �ۼ���: �ڿ���
    * �Լ�: MoveToPosition
    * ���: �÷��̾ ���� ����Ʈ�� ������ �̵���Ŵ
    ***********************************************************************************/
    public IEnumerator MoveToPosition(GameObject player, Transform movePoint, float moveTime)
    {
        // �̵� �� ��� RayVisualizer ��Ȱ��ȭ
        RayVisualizer[] rayVisualizers = player.GetComponentsInChildren<RayVisualizer>();
        foreach (var ray in rayVisualizers)
        {
            ray.Off();
        }
        // ���� ����
        Vector3 startPos = player.transform.position;
        Quaternion startRot = player.transform.rotation;

        // ���� ����
        Vector3 targetPos = movePoint.position;
        Quaternion targetRot = movePoint.rotation;

        float elapsed = 0f;
        float duration = moveTime;

        // lerp�� ������ �̵�
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            player.transform.position = Vector3.Lerp(startPos, targetPos, t);
            player.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        player.transform.position = targetPos;
        player.transform.rotation = targetRot;

        //�̵��� ���� �� RayVisualizer �ٽ� Ȱ��ȭ
        foreach (var ray in rayVisualizers)
        {
            ray.On();
        }
    }
}