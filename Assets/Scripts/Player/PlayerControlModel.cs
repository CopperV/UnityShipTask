using UnityEngine;

public class PlayerControlModel : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField]
    private PlayerInputHandler inputHandler;

    [Header("Sail Openness")]
    [SerializeField]
    private float initialSailOpenness = 0f;
    [SerializeField]
    private float maxSailOpenness = 100f;
    [SerializeField]
    private float opennessChangeSpeed = 45f;

    [Header("Sail Rotation")]
    [SerializeField]
    private float initialSailRotation = 0f;
    [SerializeField]
    private float maxSailRotation = 45f;
    [SerializeField]
    private float rotationChangeSpeed = 15f;

    [Header("Steering")]
    [SerializeField]
    private float initialSteerAngle = 0f;
    [SerializeField]
    private float maxSteerAngle = 35f;
    [SerializeField]
    private float steerChangeSpeed = 25f;

    public float SailOpenness { get; private set; }
    public float SailRotation { get; private set; }
    public float SteerAngle { get; private set; }

    public float SailOpennessNormalized => SailOpenness / maxSailOpenness;
    public float SailRotationNormalized => SailRotation / maxSailRotation;
    public float SteerAngleNormalized => SteerAngle / maxSteerAngle;

    private void Awake()
    {
        SailOpenness = initialSailOpenness;
        SailRotation = initialSailRotation;
        SteerAngle = initialSteerAngle;
    }

    private void Update()
    {
        HandleInputs(Time.deltaTime);
    }

    private void HandleInputs(float dt)
    {
        if (inputHandler == null)
            return;

        SailOpenness += inputHandler.SailOpennessInput * opennessChangeSpeed * dt;
        SailOpenness = Mathf.Clamp(SailOpenness, 0f, maxSailOpenness);

        SailRotation += inputHandler.SailRotationInput * rotationChangeSpeed * dt;
        SailRotation = Mathf.Clamp(SailRotation, -maxSailRotation, maxSailRotation);

        SteerAngle += inputHandler.SteerInput * steerChangeSpeed * dt;
        SteerAngle = Mathf.Clamp(SteerAngle, -maxSteerAngle, maxSteerAngle);
    }

    private void LateUpdate()
    {
        if (inputHandler == null)
            return;

        Debug.Log($"Current input values: {inputHandler.SailOpennessInput} {inputHandler.SailRotationInput} {inputHandler.SteerInput}");
        Debug.Log($"Current player values: {SailOpenness:F} {SailRotation:F} {SteerAngle:F}");
    }
}
