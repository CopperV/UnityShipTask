using System;
using UnityEngine;
using UnityEngine.UIElements;

public class GameHudView : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField]
    private Transform _playerRoot;

    [SerializeField]
    private PlayerControlModel _controlModel;

    [SerializeField]
    private PanelRenderer _panelRenderer;

    private Label _opennessValueLabel;
    private Label _rotationValueLabel;
    private VisualElement _playerCompassMarker;
    private VisualElement _windArrowPointer;
    private Label _windForceLabel;

    private WindController windController;
    private int _uiVersion = -1;

    private void Start()
    {
        windController = WindController.Instance;
    }

    private void OnEnable()
    {
        _panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDisable()
    {
        _panelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }

    private void Update()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (_opennessValueLabel != null)
            _opennessValueLabel.text = $"{_controlModel.SailOpenness:F0}%";

        if (_rotationValueLabel != null)
            _rotationValueLabel.text = $"{_controlModel.SailRotation:F0}°";

        if (_playerCompassMarker != null)
        {
            var playerRot = _playerRoot.eulerAngles.y;
            _playerCompassMarker.style.rotate = Quaternion.Euler(0, 0, playerRot);
        }

        if(_windArrowPointer != null && _windForceLabel != null)
        {
            var wind = windController.GetWindForceAtPoint(_playerRoot.position);
            var force = wind.magnitude;
            var dir = wind.normalized;

            var angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            _windArrowPointer.style.rotate = Quaternion.Euler(0, 0, -angle);

            _windForceLabel.text = $"{force:0.#}";
        }
    }

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement rootElement, int version)
    {
        if (_uiVersion == version)
            return;
        _uiVersion = version;

        _opennessValueLabel = rootElement?.Q<VisualElement>("SailOpenness")?.Q<Label>("Value");
        _rotationValueLabel = rootElement?.Q<VisualElement>("SailRotation")?.Q<Label>("Value");
        _playerCompassMarker = rootElement?.Q<VisualElement>("PlayerMarker");
        _windArrowPointer = rootElement?.Q<VisualElement>("WindForce")?.Q<VisualElement>("Arrow");
        _windForceLabel = rootElement?.Q<VisualElement>("WindForce")?.Q<Label>("Value");
    }
}
