using UnityEngine;

/* 아이템의 타겟을 설정하는 트리거 */
public class ItemTargetTrigger : MonoBehaviour
{
    [SerializeField] private Transform flyTarget;   // 아이템이 날라가는 위치(플레이어1)
    
    public Transform FlyTarget
    {
        get => flyTarget;
        set => flyTarget = value;
    }    

    private void OnTriggerEnter(Collider other)
    {
        Item item = other.GetComponent<Item>();
        if (item != null && item.IsHeld)
        {
            item.SetTargetPosition(FlyTarget);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Item item = other.GetComponent<Item>();
        if (item != null)
        {
            item.ClearTargetPosition();
        }
    }
}
