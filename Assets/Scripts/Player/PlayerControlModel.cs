using UnityEngine;

public class PlayerControlModel : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField]
    private PlayerInputHandler _inputHandler;

    [Header("Sail Openness")]
    [SerializeField]
    private float _initialSailOpenness = 0f;
    [SerializeField]
    private float _maxSailOpenness = 100f;
    [SerializeField]
    private float _opennessChangeSpeed = 45f;

    [Header("Sail Rotation")]
    [SerializeField]
    private float _initialSailRotation = 0f;
    [SerializeField]
    private float _maxSailRotation = 45f;
    [SerializeField]
    private float _rotationChangeSpeed = 15f;

    [Header("Steering")]
    [SerializeField]
    private float _initialSteerAngle = 0f;
    [SerializeField]
    private float _maxSteerAngle = 35f;
    [SerializeField]
    private float _steerChangeSpeed = 25f;

    public float SailOpenness { get; private set; }
    public float SailRotation { get; private set; }
    public float SteerAngle { get; private set; }

    public float SailOpennessNormalized => SailOpenness / _maxSailOpenness;
    public float SailRotationNormalized => SailRotation / _maxSailRotation;
    public float SteerAngleNormalized => SteerAngle / _maxSteerAngle;

    private void Awake()
    {
        SailOpenness = _initialSailOpenness;
        SailRotation = _initialSailRotation;
        SteerAngle = _initialSteerAngle;
    }

    private void Update()
    {
        HandleInputs(Time.deltaTime);
    }

    private void HandleInputs(float dt)
    {
        if (_inputHandler == null)
            return;

        SailOpenness += _inputHandler.SailOpennessInput * _opennessChangeSpeed * dt;
        SailOpenness = Mathf.Clamp(SailOpenness, 0f, _maxSailOpenness);

        SailRotation += _inputHandler.SailRotationInput * _rotationChangeSpeed * dt;
        SailRotation = Mathf.Clamp(SailRotation, -_maxSailRotation, _maxSailRotation);

        SteerAngle += _inputHandler.SteerInput * _steerChangeSpeed * dt;
        SteerAngle = Mathf.Clamp(SteerAngle, -_maxSteerAngle, _maxSteerAngle);
    }
}
