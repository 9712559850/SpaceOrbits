using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Button")]
    public Button startButton;

    [Header("Restart Button")]
    [SerializeField] private TextMeshProUGUI txt_Score;
    [SerializeField] private TextMeshProUGUI txt_Level;

    [Header("Home Panel")]
    [SerializeField] private GameObject HomePanel;
    [SerializeField] private GameObject PlayerPlane;
    [Header("Game Over")]
    [SerializeField] private GameObject Gameoverpnl;
    [SerializeField] private TextMeshProUGUI gameoverScore;
    [SerializeField] private TextMeshProUGUI gameoverBestScore;
    [SerializeField] private Image ButtonImg;
    [SerializeField] private Sprite restartSprite, nextSprite;
    [SerializeField] public TextMeshProUGUI gameoverMessage;

    [SerializeField] public GameObject shopPanel;

    public static UIManager Instance;

    void Start()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        startButton.onClick.AddListener(() => StartGameUI());
    }

    public void StartGameUI()
    {
        startButton.gameObject.SetActive(false);
        Gameoverpnl.SetActive(false);
        shopPanel.SetActive(false);
    }

    public void UpdateScore(int scorevalue)
    {
        txt_Score.text = scorevalue.ToString();
    }

    public void UpdateLevel(int level_value)
    {
        if (txt_Level.text != level_value.ToString())
        {
            txt_Level.text = level_value.ToString();
        }
    }

    public void GameOver(int bestscore, bool isNext)
    {
        Gameoverpnl.SetActive(true);
        gameoverScore.text = txt_Score.text;

        if (isNext)
        {
            SoundManager.Instance.PlayGameOverSound(1);
            UIManager.Instance.gameoverMessage.text = "WIN";
            ButtonImg.sprite = nextSprite;
        }
        else
        {
            SoundManager.Instance.PlayGameOverSound(0);
            UIManager.Instance.gameoverMessage.text = "LOSE";
            ButtonImg.sprite = restartSprite;
        }

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

    public void Home()
    {
        Gameoverpnl.SetActive(false);
        shopPanel.SetActive(false);
        HomePanel.SetActive(true);
        UIManager.Instance.gameObject.SetActive(false);
        PlayerPlane.SetActive(false);
    }

    public void PlayStart()
    {
        transform.gameObject.SetActive(true);
        StartCoroutine(waitGameStart());
    }
    IEnumerator waitGameStart()
    {
        HomePanel.SetActive(false);
        PlayerPlane.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        PlayerPlane.GetComponent<Planemove>().StartGame();
    }
}