using UnityEngine;

public class BillboardToCamera : MonoBehaviour
{
    public Camera targetCamera;

    void Update()
    {
        if (targetCamera == null) return;

        Vector3 toCam = transform.position - targetCamera.transform.position;
        toCam.y = 0f;

        if (toCam.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(toCam, Vector3.up);
    }
}