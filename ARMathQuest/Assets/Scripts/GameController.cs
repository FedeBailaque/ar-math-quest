using UnityEngine;
using UnityEngine.UI;
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
    public Button startButton;

    [Header("Difficulty Buttons")]
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;

    [Header("End Screen")]
    public Image[] starImages;
    public TMP_Text messageText;

    [Header("Audio (optional)")]
    public AudioSource audioSource;
    public AudioClip correctClip;
    public AudioClip incorrectClip;
    public AudioClip clickClip;

    private static readonly Color StarActive = new Color(1f, 0.78f, 0.27f);
    private static readonly Color StarInactive = new Color(0.82f, 0.82f, 0.82f);

    // Difficulty button colors
    private readonly Color easyColor = new Color(0.20f, 0.70f, 0.30f);
    private readonly Color mediumColor = new Color(0.85f, 0.70f, 0.20f);
    private readonly Color hardColor = new Color(0.80f, 0.30f, 0.30f);
    private readonly Color selectedBoost = new Color(0.15f, 0.15f, 0.15f);

    private int score = 0;
    private int asked = 0;
    private MathQuestionGenerator.Question currentQuestion;
    private bool inputLocked = false;
    private bool difficultySelected = false;

    void Start()
    {
        ShowStart();
    }

    public void ShowStart()
    {
        State = GameState.Start;

        if (startPanel) startPanel.SetActive(true);
        if (endPanel) endPanel.SetActive(false);

        // Hide HUD on start screen
        if (scoreText) scoreText.gameObject.SetActive(false);
        if (questionText) questionText.gameObject.SetActive(false);

        score = 0;
        asked = 0;
        inputLocked = false;
        difficultySelected = false;
        difficulty = MathQuestionGenerator.Difficulty.Easy;

        if (startButton) startButton.interactable = false;

        ResetDifficultyButtonColors();

        if (spawner) spawner.SetPlacementEnabled(false);
    }

    public void SetEasy()
    {
        difficulty = MathQuestionGenerator.Difficulty.Easy;
        difficultySelected = true;

        ResetDifficultyButtonColors();

        if (easyButton)
            easyButton.image.color = ClampColor(easyColor + selectedBoost);

        if (startButton) startButton.interactable = true;

        Play(clickClip);
    }

    public void SetMedium()
    {
        difficulty = MathQuestionGenerator.Difficulty.Medium;
        difficultySelected = true;

        ResetDifficultyButtonColors();

        if (mediumButton)
            mediumButton.image.color = ClampColor(mediumColor + selectedBoost);

        if (startButton) startButton.interactable = true;

        Play(clickClip);
    }

    public void SetHard()
    {
        difficulty = MathQuestionGenerator.Difficulty.Hard;
        difficultySelected = true;

        ResetDifficultyButtonColors();

        if (hardButton)
            hardButton.image.color = ClampColor(hardColor + selectedBoost);

        if (startButton) startButton.interactable = true;

        Play(clickClip);
    }

    private void ResetDifficultyButtonColors()
    {
        if (easyButton) easyButton.image.color = easyColor;
        if (mediumButton) mediumButton.image.color = mediumColor;
        if (hardButton) hardButton.image.color = hardColor;
    }

    private Color ClampColor(Color c)
    {
        return new Color(
            Mathf.Clamp01(c.r),
            Mathf.Clamp01(c.g),
            Mathf.Clamp01(c.b),
            Mathf.Clamp01(c.a <= 0 ? 1f : c.a)
        );
    }

    public void OnPressStart()
    {
        if (!difficultySelected)
        {
            Debug.Log("Please select a difficulty before starting.");
            return;
        }

        Play(clickClip);

        if (startPanel) startPanel.SetActive(false);
        if (endPanel) endPanel.SetActive(false);

        // Show HUD during gameplay
        if (scoreText) scoreText.gameObject.SetActive(true);
        if (questionText) questionText.gameObject.SetActive(true);

        State = GameState.Scanning;
        SetHUD("Find a plane and tap to place", score, asked);

        if (spawner) spawner.SetPlacementEnabled(true);
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

        if (spawner)
        {
            spawner.ClearAnswers();
            spawner.SpawnAnswers(values, correct);
        }
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
            if (spawner) spawner.PlayCorrectFeedback(answer);
        }
        else
        {
            Play(incorrectClip);
            if (spawner) spawner.PlayIncorrectFeedback(answer);
        }

        Invoke(nameof(NextQuestion), 1.0f);
    }

    private void EndRound()
    {
        State = GameState.End;

        if (spawner)
        {
            spawner.ClearAnswers();
            spawner.SetPlacementEnabled(false);
        }

        // Hide HUD on end screen
        if (scoreText) scoreText.gameObject.SetActive(false);
        if (questionText) questionText.gameObject.SetActive(false);

        if (endPanel) endPanel.SetActive(true);
        if (endScoreText) endScoreText.text = $"{score}/{questionsPerRound}";

        int stars = score <= 1 ? 1 : score <= 3 ? 2 : 3;

        if (starImages != null)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] != null)
                    starImages[i].color = i < stars ? StarActive : StarInactive;
            }
        }

        if (messageText != null)
        {
            messageText.text = stars == 3 ? "Amazing job!"
                             : stars == 2 ? "Good effort!"
                             : "Keep practicing!";
        }
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