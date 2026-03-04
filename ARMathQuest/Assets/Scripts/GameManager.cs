using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private QuestionGenerator questionGenerator;
    private int currentCorrectIndex;

    private void Start()
    {
        questionGenerator = new QuestionGenerator();
        GenerateNewQuestion();
    }

    private void Update()
    {
        // Press 0, 1, or 2 to simulate selecting an answer
        if (Input.GetKeyDown(KeyCode.Alpha0))
            CheckAnswer(0);

        if (Input.GetKeyDown(KeyCode.Alpha1))
            CheckAnswer(1);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            CheckAnswer(2);
    }

    public void GenerateNewQuestion()
    {
        var questionData = questionGenerator.GenerateQuestion();

        currentCorrectIndex = questionData.correctIndex;

        Debug.Log("----- NEW QUESTION -----");
        Debug.Log(questionData.questionText);

        for (int i = 0; i < questionData.answers.Count; i++)
        {
            Debug.Log($"Option {i}: {questionData.answers[i]}");
        }
    }

    public void CheckAnswer(int selectedIndex)
    {
        if (selectedIndex == currentCorrectIndex)
        {
            Debug.Log("Player selected correct answer.");
            ScoreManager.Instance.RegisterCorrect();
        }
        else
        {
            Debug.Log("Player selected wrong answer.");
            ScoreManager.Instance.RegisterIncorrect();
        }

        // Generate a new question after answering
        GenerateNewQuestion();
    }
}