using UnityEngine;

public class WindController : MonoBehaviour
{
    public static WindController Instance { get; private set; }

    [SerializeField]
    private Vector3 _initialDirection = Vector3.forward;

    [SerializeField]
    private float _initialForce = 10f;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(Instance.gameObject);
            return;
        }

        Instance = this;
    }

    public Vector3 GetWindForceAtPoint(Vector3 point)
    {
        return _initialDirection.normalized * _initialForce;
    }
}
