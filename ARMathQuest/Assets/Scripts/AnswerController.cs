using UnityEngine;

public class AnswerController : MonoBehaviour
{
    [SerializeField] private TextMesh label;

    private int value;
    private bool isCorrect;
    private GameController gameController;

    private void Awake()
    {
        if (label == null)
            label = GetComponentInChildren<TextMesh>(true);
    }

    public void SetValue(int v, bool correct, GameController gc)
    {
        value = v;
        isCorrect = correct;
        gameController = gc;

        if (label != null)
            label.text = v.ToString();
        else
            Debug.LogWarning("AnswerController: No TextMesh found on AnswerPrefab.");
    }

    public int Value => value;
    public bool IsCorrect => isCorrect;
}