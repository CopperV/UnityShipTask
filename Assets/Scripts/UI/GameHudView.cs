using System;
using UnityEngine;
using UnityEngine.UIElements;

public class GameHudView : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField]
    private PlayerControlModel _controlModel;

    [SerializeField]
    private PanelRenderer _panelRenderer;

    private Label _opennessValueLabel;
    private Label _rotationValueLabel;
    private int _uiVersion = -1;

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
    }

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement rootElement, int version)
    {
        if (_uiVersion == version)
            return;
        _uiVersion = version;

        _opennessValueLabel = rootElement?.Q<VisualElement>("SailOpenness")?.Q<Label>("Value");
        _rotationValueLabel = rootElement?.Q<VisualElement>("SailRotation")?.Q<Label>("Value");
    }
}
