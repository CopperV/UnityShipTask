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
    private Vector2 _gustStrengthRange = new(0.4f, 0.5f);

    [SerializeField]
    private float _gustFrequency = 0.08f;

    [SerializeField]
    private Vector2 _gustDuration = new(1.2f, 3.5f);

    [SerializeField]
    private float _gustAttackTime = 0.4f;

    [SerializeField]
    private float _gustDecayTime = 1.8f;

    [SerializeField, Range(0f, 1f)]
    private float _gustDirectionInfluence = .35f;

    private float _gustTimer;
    private float _gustValue;
    private float _gustTarget;
    private float _gustStrength;
    private float _gustDirectionOffset;

    [SerializeField]
    private Vector3 _currentDirection;
    [SerializeField]
    private float _currentForce;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
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
            if(_gustTarget <= 0f && Random.value < _gustFrequency * dt)
            {
                _gustTarget = 1f;
                _gustStrength = Random.Range(_gustStrengthRange.x, _gustStrengthRange.y);
                _gustTimer = Random.Range(_gustDuration.x, _gustDuration.y);
                _gustDirectionOffset = Random.Range(-_maxDirectionVariation * 0.6f, _maxDirectionVariation * 0.6f) * _gustDirectionInfluence;
            }

            if(_gustTarget > 0f)
            {
                _gustTimer -= dt;
                if (_gustTimer <= 0)
                {
                    _gustTarget = 0f;
                }
            }

            var speed = _gustTarget > 0f ? (1f / _gustAttackTime) : (1f / _gustDecayTime);
            _gustValue = Mathf.MoveTowards(_gustValue, _gustTarget, speed * dt);

            _currentForce += _initialForce * _gustValue * _gustStrength;

            if(_gustValue > 0.01f)
            {
                _currentDirection = Quaternion.Euler(0f, _gustDirectionOffset * _gustValue, 0f) * _currentDirection;
            }
        }
    }

    public Vector3 GetWindForceAtPoint(Vector3 point)
    {
        return _currentDirection * _currentForce;
    }
}
