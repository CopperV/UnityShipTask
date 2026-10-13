using UnityEngine;

public class SailBoatVisualsHandler : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField]
    private PlayerControlModel _controlModel;

    [SerializeField]
    private Transform _mastTransform;
    [SerializeField]
    private Transform _sailTransform;
    [SerializeField]
    private Transform _steerTransform;

    private void Update()
    {
        HandleVisuals();
    }

    private void HandleVisuals()
    {
        //Cache
        var opennessValue = _controlModel.SailOpennessNormalized;
        var rotationValue = _controlModel.SailRotation;
        var steerValue = _controlModel.SteerAngle;

        //Openness
        var newScale = new Vector3(opennessValue, opennessValue, _sailTransform.localScale.z);
        _sailTransform.localScale = newScale;

        //Sail
        _mastTransform.localRotation = Quaternion.Euler(0, -rotationValue, 0);

        //Steer
        _steerTransform.localRotation = Quaternion.Euler(0, -steerValue, 0);
    }
}
