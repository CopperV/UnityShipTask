using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField]
    private Transform _playerRoot;

    [SerializeField]
    private Transform _sailTransform;

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
    private Vector3 _velocity;

    [SerializeField]
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

        var origin = _playerRoot.position + new Vector3(0, 1.5f, 0);

        ApplyDrag(dt);
        ApplySailTorque(dt);

        if (_velocity.magnitude > _maxForwardSpeed)
            _velocity = _velocity.normalized * _maxForwardSpeed;

        _playerRoot.position += _velocity * dt;
        _playerRoot.Rotate(0f, _angularVelocity * dt, 0f, Space.World);

        Debug.DrawRay(origin, sailForce, Color.red);
        Debug.DrawRay(origin, _velocity, Color.green);
        Debug.DrawRay(origin, windController.GetWindForceAtPoint(_playerRoot.position).normalized * 5f, Color.yellow);
    }

    private Vector3 CalcSailForce()
    {
        var openness = _controlModel.SailOpennessNormalized;
        if (Mathf.Approximately(openness, 0f))
            return Vector3.zero;

        var trueWind = windController.GetWindForceAtPoint(_playerRoot.position);
        var windDir = trueWind.normalized;
        var windForce = trueWind.magnitude;

        var sailAngleWorld = _playerRoot.rotation * Quaternion.Euler(0, -_controlModel.SailRotation, 0);
        var sailNormal = sailAngleWorld * Vector3.forward;

        var windOnSail = Vector3.Dot(windDir, sailNormal);
        var effective = Mathf.Max(0, windOnSail);

        var force = sailNormal * (windForce * windOnSail * openness * _sailForceMultiplier);

        var forwardPush = _playerRoot.forward * (force.magnitude * 0.35f);
        force += forwardPush;

        return force;
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

    private void ApplySailTorque(float dt)
    {
        var openness = _controlModel.SailOpennessNormalized;
        var sailAngle = _controlModel.SailRotation;

        var torque = -sailAngle * openness * _turnTorqueFromSail * 0.02f;
        _angularVelocity += torque * dt;
    }
}
