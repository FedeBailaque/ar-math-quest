using UnityEngine;

public class MathQuestionGenerator : MonoBehaviour
{
    public enum Difficulty { Easy, Medium }

    [System.Serializable]
    public struct Question
    {
        public int a;
        public int b;
        public char op;     // '+' or '-'
        public int correct;
        public override string ToString() => $"{a} {op} {b} = ?";
    }

    public int easyMax = 5;
    public int mediumMax = 10;

    public Question GenerateQuestion(Difficulty d)
    {
        int max = (d == Difficulty.Easy) ? easyMax : mediumMax;

        int a = Random.Range(1, max + 1);
        int b = Random.Range(1, max + 1);

        // 70% addition, 30% subtraction (no negatives)
        if (Random.value < 0.7f)
        {
            return new Question { a = a, b = b, op = '+', correct = a + b };
        }
        else
        {
            if (a < b) { int t = a; a = b; b = t; }
            return new Question { a = a, b = b, op = '-', correct = a - b };
        }
    }

    public int[] GenerateDistractors(int correct)
    {
        // Guarantee different from correct and from each other
        int d1 = correct + Random.Range(1, 4);
        int d2 = correct - Random.Range(1, 4);
        if (d2 == correct) d2 = correct + 4;
        if (d2 < 0) d2 = correct + 2;

        if (d2 == d1) d2 += 1;
        return new[] { d1, d2 };
    }
}