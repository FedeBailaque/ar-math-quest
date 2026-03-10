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
        int max = difficulty switch
        {
            Difficulty.Easy => easyMax,
            Difficulty.Medium => mediumMax,
            Difficulty.Hard => hardMax,
            _ => easyMax
        };

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
                // ensure clean division
                b = Random.Range(1, max + 1);
                correct = Random.Range(1, max + 1);
                a = correct * b; // guarantees no decimals
                break;
        }

        return new Question
        {
            a = a,
            b = b,
            op = op,
            correct = correct
        };
    }

    private char GetOperation(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                return (Random.value < 0.7f) ? '+' : '-';

            case Difficulty.Medium:
                int m = Random.Range(0, 3);
                return m switch
                {
                    0 => '+',
                    1 => '-',
                    _ => '×'
                };

            case Difficulty.Hard:
                int h = Random.Range(0, 4);
                return h switch
                {
                    0 => '+',
                    1 => '-',
                    2 => '×',
                    _ => '÷'
                };

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

    private void Shuffle(int[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
    }
}