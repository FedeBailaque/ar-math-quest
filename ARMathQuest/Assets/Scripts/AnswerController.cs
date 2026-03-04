using UnityEngine;
using TMPro;

public class AnswerController : MonoBehaviour
{
    public int value;
    public bool isCorrect;

    [HideInInspector] public GameController gameController;
    [SerializeField] private TMP_Text tmpText;
    [SerializeField] private TextMesh legacyText;

    public void SetValue(int v, bool correct, GameController gc)
    {
        value = v;
        isCorrect = correct;
        gameController = gc;

        if (tmpText) tmpText.text = v.ToString();
        if (legacyText) legacyText.text = v.ToString();
    }
}