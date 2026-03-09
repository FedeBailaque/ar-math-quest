using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    public enum GameState { Start, Scanning, Placed, Answering, End }
    public GameState State { get; private set; } = GameState.Start;

    [Header("Config")]
    public int questionsPerRound = 5;
    public bool autoIncreaseDifficulty = true;
    public MathQuestionGenerator.Difficulty difficulty = MathQuestionGenerator.Difficulty.Easy;

    [Header("References")]
    public MathQuestionGenerator generator;
    public ARAnswerSpawner spawner;

    [Header("UI")]
    public GameObject startPanel;
    public GameObject endPanel;
    public TMP_Text questionText;
    public TMP_Text scoreText;
    public TMP_Text endScoreText;

    [Header("Audio (optional)")]
    public AudioSource audioSource;
    public AudioClip correctClip;
    public AudioClip incorrectClip;
    public AudioClip clickClip;

    private int score = 0;
    private int asked = 0;
    private MathQuestionGenerator.Question currentQuestion;
    private bool inputLocked = false;

    void Start()
    {
        ShowStart();
    }

    public void ShowStart()
    {
        State = GameState.Start;

        if (startPanel) startPanel.SetActive(true);
        if (endPanel) endPanel.SetActive(false);

        score = 0;
        asked = 0;
        inputLocked = false;

        SetHUD("", 0, 0);
        spawner.SetPlacementEnabled(false);
    }

    public void OnPressStart()
    {
        Play(clickClip);

        if (startPanel) startPanel.SetActive(false);
        if (endPanel) endPanel.SetActive(false);

        State = GameState.Scanning;
        SetHUD("Find a plane and tap to place", score, asked);

        spawner.SetPlacementEnabled(true);
    }

    public void OnPlaced()
    {
        State = GameState.Placed;
        NextQuestion();
    }

    private void NextQuestion()
    {
        if (asked >= questionsPerRound)
        {
            EndRound();
            return;
        }

        asked++;
        inputLocked = false;

        // Optional auto difficulty scaling
        if (autoIncreaseDifficulty)
        {
            if (asked > questionsPerRound * 0.6f)
                difficulty = MathQuestionGenerator.Difficulty.Hard;
            else if (asked > questionsPerRound * 0.3f)
                difficulty = MathQuestionGenerator.Difficulty.Medium;
        }

        currentQuestion = generator.GenerateQuestion(difficulty);
        SetHUD(currentQuestion.ToString(), score, asked);

        int correct = currentQuestion.correct;
        int[] dis = generator.GenerateDistractors(correct);

        int[] values = new[] { correct, dis[0], dis[1] };
        Shuffle(values);

        State = GameState.Answering;

        spawner.ClearAnswers();   // Prevent stacking bugs
        spawner.SpawnAnswers(values, correct);
    }

    public void OnAnswerSelected(AnswerController answer)
    {
        if (State != GameState.Answering) return;
        if (inputLocked) return;

        inputLocked = true;

        if (answer.IsCorrect)
        {
            score++;
            Play(correctClip);
            spawner.PlayCorrectFeedback(answer);
        }
        else
        {
            Play(incorrectClip);
            spawner.PlayIncorrectFeedback(answer);
        }

        Invoke(nameof(NextQuestion), 1.0f);
    }

    private void EndRound()
    {
        State = GameState.End;

        spawner.ClearAnswers();
        spawner.SetPlacementEnabled(false);

        if (endPanel) endPanel.SetActive(true);
        if (endScoreText) endScoreText.text = $"Score: {score}/{questionsPerRound}";

        SetHUD("", score, asked);
    }

    public void OnPressPlayAgain()
    {
        Play(clickClip);
        ShowStart();
    }

    public void OnPressSaveAndExit()
    {
        Play(clickClip);
        ShowStart();
    }

    private void SetHUD(string q, int s, int numAsked)
    {
        if (questionText) questionText.text = q;
        if (scoreText) scoreText.text = $"Score: {s}  Q: {numAsked}/{questionsPerRound}";
    }

    private void Play(AudioClip clip)
    {
        if (!audioSource || !clip) return;
        audioSource.PlayOneShot(clip);
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