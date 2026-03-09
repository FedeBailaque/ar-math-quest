using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARPlaneManager))]
public class PlaneDetectionLogger : MonoBehaviour
{
    private ARPlaneManager _planeManager;

    void Awake() => _planeManager = GetComponent<ARPlaneManager>();

    void OnEnable() => _planeManager.trackablesChanged.AddListener(OnPlanesChanged);
    void OnDisable() => _planeManager.trackablesChanged.RemoveListener(OnPlanesChanged);

    private void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> args)
    {
        Debug.Log($"Planes: added={args.added.Count}, updated={args.updated.Count}, removed={args.removed.Count}, total={_planeManager.trackables.count}");
    }
}