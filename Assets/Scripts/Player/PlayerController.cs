using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField]
    private Transform _playerRoot;

    [SerializeField]
    private PlayerControlModel _controlModel;

    [Header("Physical Properties")]
    [SerializeField]
    private float _boatMass = 480f;

    [SerializeField]
    private float _sailArea = 13.5f;

    [SerializeField]
    private float _airDensity = 1.225f;

    [Header("Sail Aerodynamics")]
    [SerializeField]
    private float _liftCoefficient = 1.45f;

    [SerializeField]
    private float _dragCoefficient = 1.05f;

    [SerializeField]
    private float _parasiticDrag = 0.07f;

    [Header("Hull Resistance")]
    [SerializeField]
    private float _linearDrag = 18f;

    [SerializeField]
    private float _sidewaysDragMultiplier = 5.5f;

    [SerializeField]
    private float _angularDrag = 3.5f;

    [Header("Handling")]
    [SerializeField]
    private float _maxSpeed = 7.5f;

    [SerializeField]
    private float _turnTorqueFromSail = 0.85f;

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
        var hullDragForce = CalcHullDragForce();

        var totalForce = sailForce + hullDragForce;
        var acceleration = totalForce / _boatMass;
        _velocity += acceleration * dt;

        if (_velocity.magnitude > _maxSpeed)
            _velocity = _velocity.normalized * _maxSpeed;
        if (_velocity.magnitude < 0.1f && Mathf.Approximately(_controlModel.SailOpennessNormalized, 0f))
            _velocity = Vector3.zero;

        ApplySailTorque(sailForce.magnitude, dt);

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
        if (angleOfAttack > 90f)
            angleOfAttack -= 180f;
        if (angleOfAttack < -90f)
            angleOfAttack += 180f;

        var aoaRad = angleOfAttack * Mathf.Deg2Rad;
        aoaRad = Mathf.Clamp(aoaRad, -1.1f, 1.1f);

        var sinAoa = Mathf.Sin(aoaRad);
        var cosAoa = Mathf.Cos(aoaRad);

        var cl = _liftCoefficient * Mathf.Sin(2f * aoaRad);
        var cd = _dragCoefficient * (sinAoa * sinAoa) + _parasiticDrag;

        var dynamicPressure = 0.5f * _airDensity * windForce * windForce;

        var liftDir = Vector3.Cross(windDir, Vector3.up).normalized;
        if (Vector3.Dot(liftDir, sailNormal) < 0f)
            liftDir = -liftDir;

        var forceFactor = dynamicPressure * _sailArea * openness;
        var liftForce = cl * forceFactor * liftDir;
        var dragForce = cd * forceFactor * windDir;

        return liftForce + dragForce;
    }

    private Vector3 CalcHullDragForce()
    {
        var forward = _playerRoot.forward;
        var right = _playerRoot.right;

        var forwardSpeed = Vector3.Dot(_velocity, forward);
        var sideSpeed = Vector3.Dot(_velocity, right);

        var forwardDrag = -forwardSpeed * _linearDrag;
        var sideDrag = -sideSpeed * _linearDrag * _sidewaysDragMultiplier;

        return forward * forwardDrag + right * sideDrag;
    }

    private void ApplySailTorque(float force, float dt)
    {
        var openness = _controlModel.SailOpennessNormalized;
        var sailAngle = _controlModel.SailRotation;

        var torque = -sailAngle * openness * force * _turnTorqueFromSail * 0.0012f;
        _angularVelocity += torque * dt;

        _angularVelocity *= (1f - _angularDrag * dt);
        if (Mathf.Abs(_angularVelocity) < 0.05f)
            _angularVelocity = 0f;

        _angularVelocity = Mathf.Clamp(_angularVelocity, -75, 75);
    }
}
