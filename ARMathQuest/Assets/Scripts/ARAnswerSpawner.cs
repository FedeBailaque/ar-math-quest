using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARAnswerSpawner : MonoBehaviour
{
    [Header("AR")]
    public Camera arCamera;
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;
    public ARAnchorManager anchorManager;

    [Header("Prefabs")]
    public GameObject basePrefab;
    public GameObject answerPrefab;

    [Header("Wiring")]
    public GameController gameController;

    [Header("Spawn tuning")]
    public float answerHeight = 0.15f;
    public float answerSpacing = 0.45f;
    public float answerScale = 0.12f;

    [Header("Selection")]
    public LayerMask answerLayerMask = ~0; // set this to Answers layer in Inspector if you use one

    private bool placementEnabled = false;
    private bool placed = false;

    private ARAnchor anchor;
    private GameObject playAreaRoot;
    private GameObject baseInstance;

    private readonly List<GameObject> spawnedAnswers = new();
    private readonly List<ARRaycastHit> hits = new();

    public void SetPlacementEnabled(bool enabled)
    {
        placementEnabled = enabled;

        if (!enabled) return;

        placed = false;
        ClearAll();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 inputPos = Input.mousePosition;

            if (placementEnabled && !placed)
            {
                TryPlace(inputPos);
                return;
            }

            TrySelectAnswer(inputPos);
        }
#else
        if (Input.touchCount == 0) return;

        Touch t = Input.GetTouch(0);
        if (t.phase != TouchPhase.Began) return;

        Vector2 inputPos = t.position;

        if (placementEnabled && !placed)
        {
            TryPlace(inputPos);
            return;
        }

        TrySelectAnswer(inputPos);
#endif
    }

    private void TryPlace(Vector2 screenPos)
    {
        if (raycastManager == null) return;

        if (!raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
            return;

        var hit = hits[0];
        Pose pose = hit.pose;

        // Optional anchor creation: kept for compatibility, but visuals are no longer parented to it
        ARPlane tappedPlane = null;
        if (planeManager != null)
            tappedPlane = planeManager.GetPlane(hit.trackableId);

        if (anchorManager != null && tappedPlane != null)
            anchor = anchorManager.AttachAnchor(tappedPlane, pose);

        if (anchor == null)
        {
            GameObject dummy = new GameObject("PlayAreaAnchor");
            dummy.transform.SetPositionAndRotation(pose.position, pose.rotation);
            anchor = dummy.AddComponent<ARAnchor>();
        }

        // Stable visual root: prevents visible wobble from live anchor updates
        if (playAreaRoot != null)
            Destroy(playAreaRoot);

        playAreaRoot = new GameObject("PlayAreaRoot");
        playAreaRoot.transform.SetPositionAndRotation(pose.position, pose.rotation);

        if (basePrefab != null)
            baseInstance = Instantiate(basePrefab, playAreaRoot.transform);

        placed = true;
        placementEnabled = false;

        if (gameController != null)
            gameController.OnPlaced();
    }

    public void SpawnAnswers(int[] values, int correctValue)
    {
        ClearAnswers();

        if (playAreaRoot == null || answerPrefab == null || gameController == null) return;

        Vector3[] offsets =
        {
            new Vector3(-answerSpacing, answerHeight, 0f),
            new Vector3(0f,           answerHeight, 0f),
            new Vector3(answerSpacing, answerHeight, 0f)
        };

        for (int i = 0; i < 3; i++)
        {
            GameObject go = Instantiate(answerPrefab, playAreaRoot.transform);
            go.transform.localPosition = offsets[i];
            go.transform.localScale = Vector3.one * answerScale;

            FaceCamera(go.transform);

            var billboard = go.GetComponent<BillboardToCamera>();
            if (billboard == null)
                billboard = go.AddComponent<BillboardToCamera>();

            billboard.targetCamera = arCamera;

            var ac = go.GetComponent<AnswerController>();
            if (ac == null)
                ac = go.GetComponentInChildren<AnswerController>();

            if (ac != null)
            {
                bool isCorrect = values[i] == correctValue;
                ac.SetValue(values[i], isCorrect, gameController);
            }

            spawnedAnswers.Add(go);
        }
    }

    public void ClearAnswers()
    {
        foreach (var a in spawnedAnswers)
        {
            if (a) Destroy(a);
        }

        spawnedAnswers.Clear();
    }

    private void ClearAll()
    {
        ClearAnswers();

        if (baseInstance) Destroy(baseInstance);
        baseInstance = null;

        if (playAreaRoot) Destroy(playAreaRoot);
        playAreaRoot = null;

        if (anchor) Destroy(anchor.gameObject);
        anchor = null;
    }

    private void TrySelectAnswer(Vector2 screenPos)
    {
        if (arCamera == null || gameController == null) return;

        Ray ray = arCamera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, answerLayerMask))
        {
            var ac = hit.collider.GetComponentInParent<AnswerController>();
            if (ac != null)
            {
                gameController.OnAnswerSelected(ac);
            }
        }
    }

    private void FaceCamera(Transform t)
    {
        if (!arCamera) return;

        Vector3 toCam = t.position - arCamera.transform.position;
        toCam.y = 0f;

        if (toCam.sqrMagnitude < 0.0001f) return;

        t.rotation = Quaternion.LookRotation(toCam, Vector3.up);
    }

    public void PlayCorrectFeedback(AnswerController ac)
    {
        if (!ac) return;
        StartCoroutine(AnimateFeedback(ac, Color.green, 1.15f));
    }

    public void PlayIncorrectFeedback(AnswerController ac)
    {
        if (!ac) return;
        StartCoroutine(AnimateFeedback(ac, Color.red, 0.9f));
    }

    private IEnumerator AnimateFeedback(AnswerController ac, Color feedbackColor, float scaleMultiplier)
    {
        if (ac == null) yield break;

        Transform t = ac.transform;
        Vector3 originalScale = t.localScale;

        Renderer r = ac.GetComponentInChildren<Renderer>();
        if (r == null) yield break;

        Color originalColor = r.material.color;

        // Apply temporary feedback
        r.material.color = feedbackColor;
        t.localScale = originalScale * scaleMultiplier;

        // Wait briefly
        yield return new WaitForSeconds(0.35f);

        // Reset
        r.material.color = originalColor;
        t.localScale = originalScale;
    }
}