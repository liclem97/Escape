using UnityEngine;

public class ItemTargetTrigger : MonoBehaviour
{
    [SerializeField] private Transform flyTarget;

    private void OnTriggerEnter(Collider other)
    {
        Item item = other.GetComponent<Item>();
        if (item != null && item.IsHeld)
        {
            Debug.Log($"item entered: {other.gameObject.name}");
            item.SetTargetPosition(flyTarget);
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
