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
    public int mediumMax = 10;
    public int hardMax = 15;

    public Question GenerateQuestion(Difficulty difficulty)
    {
        int max = easyMax;

        switch (difficulty)
        {
            case Difficulty.Easy:
                max = easyMax;
                break;
            case Difficulty.Medium:
                max = mediumMax;
                break;
            case Difficulty.Hard:
                max = hardMax;
                break;
        }

        int a = Random.Range(1, max + 1);
        int b = Random.Range(1, max + 1);

        char op = GetOperation(difficulty);
        int correct = 0;

        switch (op)
        {
            case '+':
                correct = a + b;
                break;

            case '-':
                if (a < b)
                {
                    int temp = a;
                    a = b;
                    b = temp;
                }
                correct = a - b;
                break;

            case '×':
                correct = a * b;
                break;

            case '÷':
                // Force clean division
                b = Random.Range(1, max + 1);
                correct = Random.Range(1, max + 1);
                a = correct * b;
                break;
        }

        Question q = new Question();
        q.a = a;
        q.b = b;
        q.op = op;
        q.correct = correct;

        return q;
    }

    private char GetOperation(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                // Easy: mostly addition, sometimes subtraction
                return (Random.value < 0.7f) ? '+' : '-';

            case Difficulty.Medium:
                // Medium: addition, subtraction, multiplication
                int m = Random.Range(0, 3);
                if (m == 0) return '+';
                if (m == 1) return '-';
                return '×';

            case Difficulty.Hard:
                // Hard: addition, subtraction, multiplication, division
                int h = Random.Range(0, 4);
                if (h == 0) return '+';
                if (h == 1) return '-';
                if (h == 2) return '×';
                return '÷';

            default:
                return '+';
        }
    }

    public int[] GenerateDistractors(int correct)
    {
        HashSet<int> distractors = new HashSet<int>();

        int range = Mathf.Max(3, Mathf.Abs(correct / 2));

        while (distractors.Count < 2)
        {
            int candidate = correct + Random.Range(-range, range + 1);

            if (candidate >= 0 && candidate != correct)
            {
                distractors.Add(candidate);
            }
        }

        int[] result = new int[2];
        distractors.CopyTo(result);
        return result;
    }
}