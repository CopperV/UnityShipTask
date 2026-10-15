using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class WindController : MonoBehaviour
{
    public static WindController Instance { get; private set; }

    [Header("Initial Settings")]
    [SerializeField]
    private Vector3 _initialDirection = Vector3.forward;

    [SerializeField]
    private float _initialForce = 10f;

    [Header("Direction Variation")]
    [SerializeField]
    private float _maxDirectionVariation = 35f;

    [SerializeField]
    private float _directionChangeSpeed = 0.07f;

    [Header("Force Variation")]
    [SerializeField, Range(0f, 0.8f)]
    private float _forceVariation = 0.35f;

    [SerializeField]
    private float _forceChangeSpeed = 0.12f;

    [Header("Gusts")]
    [SerializeField]
    private bool _enableGusts = true;

    [SerializeField]
    private float _gustStrength = 0.45f;

    [SerializeField]
    private float _gustFrequency = 0.08f;

    [SerializeField]
    private float _gustDuration = 2.5f;

    private float _gustTimer;
    private float _gustValue;

    [SerializeField]
    private Vector3 _currentDirection;
    [SerializeField]
    private float _currentForce;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(Instance.gameObject);
            return;
        }

        Instance = this;

        _currentDirection = _initialDirection.normalized;
        _currentForce = _initialForce;
    }

    private void Update()
    {
        UpdateWind(Time.deltaTime);
    }

    private void UpdateWind(float dt)
    {
        var time = Time.time;

        var dirNoise = Mathf.PerlinNoise(time * _directionChangeSpeed, 0.5f) * 2f - 1f;
        var angle = dirNoise * _maxDirectionVariation;
        _currentDirection = Quaternion.Euler(0f, angle, 0f) * _initialDirection.normalized;

        var forceNoise = Mathf.PerlinNoise(0.3f, time * _forceChangeSpeed) * 2f - 1f;
        var forceMultiplier = 1f + forceNoise * _forceVariation;
        _currentForce = _initialForce * forceMultiplier;

        if(_enableGusts)
        {
            _gustTimer -= dt;

            if(_gustTimer <= 0f)
            {
                if(Random.value < 0.1f)
                {
                    _gustValue = Random.Range(0.6f, 1.2f) * _gustStrength;
                    _gustTimer = _gustDuration;
                }
                else
                {
                    _gustValue = 0f;
                    _gustTimer = Random.Range(1.5f, 4f) / _gustFrequency;
                }
            }

            if(_gustValue > 0f)
            {
                _gustValue = Mathf.MoveTowards(_gustValue, 0f, dt * _gustStrength / _gustDuration);
                _currentForce += _initialForce * _gustValue;
            }
        }
    }

    public Vector3 GetWindForceAtPoint(Vector3 point)
    {
        return _currentDirection * _currentForce;
    }
}
