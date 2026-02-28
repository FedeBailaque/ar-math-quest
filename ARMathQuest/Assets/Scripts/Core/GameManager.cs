using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private QuestionGenerator questionGenerator;

    private void Start()
    {
        questionGenerator = new QuestionGenerator();
        GenerateNewQuestion();
    }

    public void GenerateNewQuestion()
    {
        var questionData = questionGenerator.GenerateQuestion();

        Debug.Log(questionData.questionText);

        for (int i = 0; i < questionData.answers.Count; i++)
        {
            Debug.Log($"Option {i}: {questionData.answers[i]}");
        }

        CheckAnswer(0, questionData.correctIndex);
    }

    public void CheckAnswer(int selectedIndex, int correctIndex)
    {
        if (selectedIndex == correctIndex)
        {
            ScoreManager.Instance.RegisterCorrect();
        }
        else
        {
            ScoreManager.Instance.RegisterIncorrect();
        }
    }
}