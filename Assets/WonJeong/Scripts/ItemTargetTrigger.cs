using UnityEngine;

/* �������� Ÿ���� �����ϴ� Ʈ���� */
public class ItemTargetTrigger : MonoBehaviour
{
    [SerializeField] private Transform flyTarget;   // �������� ���󰡴� ��ġ(�÷��̾�1)
    
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
