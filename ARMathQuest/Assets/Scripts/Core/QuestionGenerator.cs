using System;
using System.Collections.Generic;

public class QuestionGenerator
{
    private System.Random random = new System.Random();

    public (string questionText, List<int> answers, int correctIndex) GenerateQuestion()
    {
        int num1 = random.Next(0, 11);
        int num2 = random.Next(0, 11);

        int correctAnswer = num1 + num2;

        List<int> answers = new List<int>();
        answers.Add(correctAnswer);

        while (answers.Count < 3)
        {
            int wrongAnswer = correctAnswer + random.Next(-3, 4);
            if (!answers.Contains(wrongAnswer) && wrongAnswer >= 0)
            {
                answers.Add(wrongAnswer);
            }
        }

        Shuffle(answers);

        int correctIndex = answers.IndexOf(correctAnswer);
        string question = $"{num1} + {num2} = ?";

        return (question, answers, correctIndex);
    }

    private void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = random.Next(i, list.Count);
            int temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}