using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARPlaneManager))]
public class PlaneDetectionLogger : MonoBehaviour
{
    private ARPlaneManager _planeManager;

    void Awake() => _planeManager = GetComponent<ARPlaneManager>();

    void OnEnable() => _planeManager.planesChanged += OnPlanesChanged;
    void OnDisable() => _planeManager.planesChanged -= OnPlanesChanged;

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        Debug.Log($"Planes: added={args.added.Count}, updated={args.updated.Count}, removed={args.removed.Count}, total={_planeManager.trackables.count}");
    }
}