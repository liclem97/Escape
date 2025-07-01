using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Ray")]
    [SerializeField] private RayVisualizer rayVisualizer;

    private void Start()
    {
        rayVisualizer.On();
    }
}
