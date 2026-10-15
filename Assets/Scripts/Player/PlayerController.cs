using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField]
    private Transform _playerRoot;

    [SerializeField]
    private PlayerControlModel _controlModel;

    [Header("Settings")]
    [SerializeField]
    private float _sailForceMultiplier = 18f;

    [SerializeField]
    private float _maxForwardSpeed = 14f;

    [SerializeField]
    private float _linearDrag = 1.8f;

    [SerializeField]
    private float _angularDrag = 3.5f;

    [SerializeField]
    private float _sidewaysDragMultiplier = 4f;

    [SerializeField]
    private float _turnTorqueFromSail = 25f;

    [SerializeField]
    private float _liftCoefficient = 1.5f;

    [SerializeField]
    private float _dragCoefficient = 1.1f;

    [SerializeField]
    private float _parasiticDrag = 0.08f;

    private Vector3 _velocity;
    private float _angularVelocity;

    private WindController windController;

    private void Start()
    {
        windController = WindController.Instance;
    }

    private void FixedUpdate()
    {
        HandleMovement(Time.fixedDeltaTime);
    }

    private void HandleMovement(float dt)
    {
        var sailForce = CalcSailForce();
        _velocity += sailForce * dt;

        ApplyDrag(dt);
        ApplySailTorque(sailForce.magnitude, dt);

        if (_velocity.magnitude > _maxForwardSpeed)
            _velocity = _velocity.normalized * _maxForwardSpeed;

        _playerRoot.position += _velocity * dt;
        _playerRoot.Rotate(0f, _angularVelocity * dt, 0f, Space.World);
    }

    private Vector3 CalcSailForce()
    {
        var openness = _controlModel.SailOpennessNormalized;
        if (Mathf.Approximately(openness, 0f))
            return Vector3.zero;

        var trueWind = windController.GetWindForceAtPoint(_playerRoot.position);
        var relativeWind = trueWind - _velocity;
        var windDir = relativeWind.normalized;
        var windForce = relativeWind.magnitude;
        if (windForce < 0.1f)
            return Vector3.zero;

        var sailRot = _playerRoot.rotation * Quaternion.Euler(0f, -_controlModel.SailRotation, 0f);
        var sailNormal = (sailRot * Vector3.forward).normalized;
        var sailChord = Vector3.Cross(Vector3.up, sailNormal).normalized;

        var angleOfAttack = Vector3.SignedAngle(sailChord, -windDir, Vector3.up);
        var aoaRad = angleOfAttack * Mathf.Deg2Rad;
        aoaRad = Mathf.Clamp(aoaRad, -1.1f, 1.1f);

        var sinAoa = Mathf.Sin(aoaRad);
        var cosAoa = Mathf.Cos(aoaRad);

        var cl = _liftCoefficient * Mathf.Sin(2f * aoaRad);
        var cd = _dragCoefficient * (sinAoa * sinAoa) + _parasiticDrag;

        var dynamicPressure = windForce * windForce * openness * _sailForceMultiplier * 0.04f;

        var liftDir = Vector3.Cross(windDir, Vector3.up).normalized;
        if (Vector3.Dot(liftDir, sailNormal) < 0f)
            liftDir = -liftDir;

        var liftForce = liftDir * (cl * dynamicPressure);
        var dragForce = windDir * (cd + dynamicPressure);

        return liftForce + dragForce;
    }

    private void ApplyDrag(float dt)
    {
        var forward = _playerRoot.forward;
        var right = _playerRoot.right;

        var forwardSpeed = Vector3.Dot(_velocity, forward);
        var sideSpeed = Vector3.Dot(_velocity, right);

        var forwardDrag = -forwardSpeed * _linearDrag;
        var sideDrag = -sideSpeed * _linearDrag * _sidewaysDragMultiplier;

        var dragForce = forward * forwardDrag + right * sideDrag;
        _velocity += dragForce * dt;

        _angularVelocity *= (1f - _angularDrag * dt);
        if (Mathf.Abs(_angularVelocity) < 0.1f)
            _angularVelocity = 0f;
    }

    private void ApplySailTorque(float force, float dt)
    {
        var openness = _controlModel.SailOpennessNormalized;
        var sailAngle = _controlModel.SailRotation;

        var torque = -sailAngle * openness * force * _turnTorqueFromSail * 0.015f;
        _angularVelocity += torque * dt;
        _angularVelocity = Mathf.Clamp(_angularVelocity, -90, 90);
    }
}
