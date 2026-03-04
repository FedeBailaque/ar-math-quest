using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARAnswerSpawner : MonoBehaviour
{
    [Header("AR")]
    public Camera arCamera;
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;      // <-- add this
    public ARAnchorManager anchorManager;

    [Header("Prefabs")]
    public GameObject basePrefab;
    public GameObject answerPrefab;

    [Header("Wiring")]
    public GameController gameController;

    [Header("Spawn tuning")]
    public float answerHeight = 0.15f;
    public float answerSpacing = 0.20f;

    private bool placementEnabled = false;
    private bool placed = false;

    private ARAnchor anchor;
    private GameObject baseInstance;

    private readonly List<GameObject> spawnedAnswers = new();
    private readonly List<ARRaycastHit> hits = new();

    public void SetPlacementEnabled(bool enabled)
    {
        placementEnabled = enabled;

        // allow placement again if starting new round
        if (!enabled) return;

        placed = false;
        ClearAll();
    }

    void Update()
    {
        if (Input.touchCount == 0) return;

        Touch t = Input.GetTouch(0);
        if (t.phase != TouchPhase.Began) return;

        // First tap places base/anchor
        if (placementEnabled && !placed)
        {
            TryPlace(t.position);
            return;
        }

        // After placement: tap answers
        TrySelectAnswer(t.position);
    }

    private void TryPlace(Vector2 screenPos)
    {
        if (raycastManager == null) return;

        // Raycast to planes
        if (!raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
            return;

        var hit = hits[0];
        Pose pose = hit.pose;

        // AR Foundation 6: attach anchor to the plane we hit
        ARPlane tappedPlane = null;
        if (planeManager != null)
            tappedPlane = planeManager.GetPlane(hit.trackableId);

        if (anchorManager != null && tappedPlane != null)
            anchor = anchorManager.AttachAnchor(tappedPlane, pose);

        // Fallback: no anchor created (still works for demo)
        if (anchor == null)
        {
            GameObject dummy = new GameObject("PlayAreaAnchor");
            dummy.transform.SetPositionAndRotation(pose.position, pose.rotation);
            anchor = dummy.AddComponent<ARAnchor>();
        }

        if (basePrefab != null)
            baseInstance = Instantiate(basePrefab, anchor.transform);

        placed = true;
        placementEnabled = false;

        if (gameController != null)
            gameController.OnPlaced();
    }

    public void SpawnAnswers(int[] values, int correctValue)
    {
        ClearAnswers();
        if (anchor == null || answerPrefab == null || gameController == null) return;

        Vector3[] offsets =
        {
            new Vector3(-answerSpacing, answerHeight, 0f),
            new Vector3(0f, answerHeight, 0f),
            new Vector3(answerSpacing, answerHeight, 0f)
        };

        for (int i = 0; i < 3; i++)
        {
            GameObject go = Instantiate(answerPrefab, anchor.transform);
            go.transform.localPosition = offsets[i];

            FaceCamera(go.transform);

            var ac = go.GetComponent<AnswerController>();
            if (ac == null) ac = go.AddComponent<AnswerController>();

            bool isCorrect = values[i] == correctValue;
            ac.SetValue(values[i], isCorrect, gameController);

            spawnedAnswers.Add(go);
        }
    }

    public void ClearAnswers()
    {
        foreach (var a in spawnedAnswers)
            if (a) Destroy(a);
        spawnedAnswers.Clear();
    }

    private void ClearAll()
    {
        ClearAnswers();

        if (baseInstance) Destroy(baseInstance);
        baseInstance = null;

        if (anchor) Destroy(anchor.gameObject);
        anchor = null;
    }

    private void TrySelectAnswer(Vector2 screenPos)
    {
        if (arCamera == null || gameController == null) return;

        Ray ray = arCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var ac = hit.collider.GetComponentInParent<AnswerController>();
            if (ac != null)
                gameController.OnAnswerSelected(ac);
        }
    }

    private void FaceCamera(Transform t)
    {
        if (!arCamera) return;
        Vector3 camPos = arCamera.transform.position;
        t.LookAt(camPos);
        t.eulerAngles = new Vector3(0f, t.eulerAngles.y, 0f);
    }

    public void PlayCorrectFeedback(AnswerController ac)
    {
        if (!ac) return;
        var r = ac.GetComponentInChildren<Renderer>();
        if (r) r.material.color = Color.green;
        ac.transform.localScale *= 1.15f;
    }

    public void PlayIncorrectFeedback(AnswerController ac)
    {
        if (!ac) return;
        var r = ac.GetComponentInChildren<Renderer>();
        if (r) r.material.color = Color.red;
        ac.transform.localScale *= 0.95f;
    }
}