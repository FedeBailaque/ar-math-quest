using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    private int currentScore = 0;
    private int totalQuestions = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RegisterCorrect()
    {
        currentScore++;
        totalQuestions++;
        Debug.Log("Correct! Score: " + currentScore);
    }

    public void RegisterIncorrect()
    {
        totalQuestions++;
        Debug.Log("Incorrect. Score: " + currentScore);
    }

    public int GetScore()
    {
        return currentScore;
    }

    public void ResetScore()
    {
        currentScore = 0;
        totalQuestions = 0;
    }
}