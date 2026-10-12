using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField]
    private InputActionReference _sailOpennessAction;

    [SerializeField]
    private InputActionReference _sailRotationAction;

    [SerializeField]
    private InputActionReference _steerAction;

    public float SailOpennessInput { get; private set; }
    public float SailRotationInput { get; private set; }
    public float SteerInput { get; private set; }

    private void OnEnable()
    {
        _sailOpennessAction.action.Enable();
        _sailRotationAction.action.Enable();
        _steerAction.action.Enable();
    }

    private void OnDisable()
    {
        _sailOpennessAction.action.Disable();
        _sailRotationAction.action.Disable();
        _steerAction.action.Disable();
    }

    private void Update()
    {
        SailOpennessInput = _sailOpennessAction.action.ReadValue<float>();
        SailRotationInput = _sailRotationAction.action.ReadValue<float>();
        SteerInput = _steerAction.action.ReadValue<float>();
    }
}
