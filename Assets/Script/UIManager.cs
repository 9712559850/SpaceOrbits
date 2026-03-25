using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Button")]
    public Button startButton;

    [Header("Restart Button")]
    [SerializeField] private Image ButtonImg;
    [SerializeField] private Sprite restartSprite, nextSprite;
    [SerializeField] private TextMeshProUGUI txt_Score;
    [SerializeField] private TextMeshProUGUI txt_Level;

    [Header("Game Over")]
    [SerializeField] private GameObject Gameoverpnl;
    [SerializeField] private TextMeshProUGUI gameoverScore;
    [SerializeField] private TextMeshProUGUI gameoverBestScore;

    public static UIManager Instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
        }

        startButton.onClick.AddListener(() => StartGameUI(0));
    }

    public void StartGameUI(int levelvalue)
    {
        startButton.gameObject.SetActive(false);
        AddScore(0, levelvalue);
        Gameoverpnl.SetActive(false);
    }

    public void AddScore(int scorevalue, int level_value)
    {
        txt_Score.text = scorevalue.ToString();
        txt_Level.text = level_value.ToString();
    }

    public void GameOver(int bestscore, bool isNext)
    {
        Gameoverpnl.SetActive(true);
        gameoverScore.text = txt_Score.text;
        //gameoverBestScore.text = bestscore.ToString();
        if (isNext)
            ButtonImg.sprite = restartSprite;
        else
            ButtonImg.sprite = nextSprite;


        if (bestscore >= int.Parse(gameoverBestScore.text))
        {
            gameoverBestScore.text = bestscore.ToString();
            PlayerPrefs.SetInt("BestScore", bestscore);
        }
    }
    public void ShowBestScore(int score)
    {
        gameoverBestScore.text = score.ToString();
    }
}