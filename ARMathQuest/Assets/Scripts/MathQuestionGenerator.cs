using UnityEngine;
using System.Collections.Generic;

public class MathQuestionGenerator : MonoBehaviour
{
    public enum Difficulty { Easy, Medium, Hard }

    [System.Serializable]
    public struct Question
    {
        public int a;
        public int b;
        public char op;   // '+', '-', '×', '÷'
        public int correct;

        public override string ToString()
        {
            return $"{a} {op} {b} = ?";
        }
    }

    [Header("Difficulty Settings")]
    public int easyMax = 5;
    public int mediumMax = 12;
    public int hardMax = 20;

    public Question GenerateQuestion(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                return GenerateEasyQuestion();

            case Difficulty.Medium:
                return GenerateMediumQuestion();

            case Difficulty.Hard:
                return GenerateHardQuestion();

            default:
                return GenerateEasyQuestion();
        }
    }

    private Question GenerateEasyQuestion()
    {
        int a = Random.Range(1, easyMax + 1);
        int b = Random.Range(1, easyMax + 1);

        char op = Random.value < 0.7f ? '+' : '-';

        if (op == '-' && a < b)
        {
            int temp = a;
            a = b;
            b = temp;
        }

        int correct = op == '+' ? a + b : a - b;

        return new Question
        {
            a = a,
            b = b,
            op = op,
            correct = correct
        };
    }

    private Question GenerateMediumQuestion()
    {
        char op;

        int choice = Random.Range(0, 3);

        if (choice == 0)
            op = '+';
        else if (choice == 1)
            op = '-';
        else
            op = '×';

        int a;
        int b;
        int correct;

        if (op == '×')
        {
            // Medium multiplication should feel harder than Easy
            a = Random.Range(3, mediumMax + 1);
            b = Random.Range(2, 10);
            correct = a * b;
        }
        else
        {
            // Medium addition/subtraction uses larger numbers than Easy
            a = Random.Range(easyMax + 1, mediumMax + 1);
            b = Random.Range(2, mediumMax + 1);

            if (op == '-' && a < b)
            {
                int temp = a;
                a = b;
                b = temp;
            }

            correct = op == '+' ? a + b : a - b;
        }

        return new Question
        {
            a = a,
            b = b,
            op = op,
            correct = correct
        };
    }

    private Question GenerateHardQuestion()
    {
        char op = Random.value < 0.5f ? '×' : '÷';

        int a;
        int b;
        int correct;

        if (op == '×')
        {
            // Hard multiplication uses larger values
            a = Random.Range(6, hardMax + 1);
            b = Random.Range(3, 13);
            correct = a * b;
        }
        else
        {
            // Hard division always gives a clean whole-number answer
            b = Random.Range(2, 13);
            correct = Random.Range(3, hardMax + 1);
            a = b * correct;
        }

        return new Question
        {
            a = a,
            b = b,
            op = op,
            correct = correct
        };
    }

    public int[] GenerateDistractors(int correct)
    {
        HashSet<int> distractors = new HashSet<int>();

        int range = Mathf.Max(4, Mathf.Abs(correct / 3));

        while (distractors.Count < 2)
        {
            int candidate = correct + Random.Range(-range, range + 1);

            if (candidate > 0 && candidate != correct)
            {
                distractors.Add(candidate);
            }
        }

        int[] result = new int[2];
        distractors.CopyTo(result);
        return result;
    }
}