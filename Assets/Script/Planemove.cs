using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Planemove : MonoBehaviour
{
    [Header("Collectible")]
    [SerializeField] private List<Sprite> CollectibleSprite = new List<Sprite>();

    [Header("GamePlay")]
    [SerializeField] private GameObject FireballObj;
    [SerializeField] GameObject LeftPlane, RightPlane;
    [SerializeField] private Transform CanvasMain;
    [SerializeField] private Transform CenterTop;
    [SerializeField] private Transform RoundCircle;
    [SerializeField] private GameObject ParticleSkull;

    [Header("Speed Plane")]
    [SerializeField] private int rotationSpeed;
    [SerializeField] private int bulletSpeed;

    [Header("Score && LevelNumber")]
    [SerializeField] private int score;
    [SerializeField] private int levelNumber;

    [Header("Bool For GameStart ,Move ,Next round check")]
    bool isMoving = true;
    bool isGamestart = false;
    bool isFirstTap = true;

    bool isNext;

    [Header("Start Position of Plane")]
    Vector2 startpositionPlane;
    float _direction;
    BoxCollider2D boxCollider;

    int completeNumber = 0;

    public void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        UIManager.Instance.ShowBestScore(PlayerPrefs.GetInt("BestScore"));
        startpositionPlane = transform.position;
        score = PlayerPrefs.GetInt("Score", 0);
        levelNumber = PlayerPrefs.GetInt("Level", 1);

        UIManager.Instance.UpdateScore(score);
        UpdateSpeedByLevel();
        CollectibleInit();
    }

    #region
    void CollectibleInit()
    {
        RoundCircle.gameObject.SetActive(true);
        StartCoroutine(ActivateObjectsWithDelay());
    }
    #endregion

    #region Coin Animation when game would start
    IEnumerator ActivateObjectsWithDelay()
    {
        yield return new WaitForSeconds(1.0f);
        int randomvalue;
        randomvalue = Random.Range(0, 10);
        for (int i = 0; i < RoundCircle.childCount - 1; i++)
        {
            RoundCircle.GetChild(i).GetComponent<Image>().sprite = CollectibleSprite[randomvalue];
            RoundCircle.GetChild(i).gameObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }
        CenterTop.gameObject.SetActive(true);
        UIManager.Instance.startButton.gameObject.SetActive(true);
        UIManager.Instance.shopPanel.SetActive(true);
        UpdateSpeedByLevel();
    }
    #endregion

    public void GameStart()
    {
        UIManager.Instance.StartGameUI();
        InvokeRepeating("Fireball", 0.5f, 1);
        isGamestart = true;
        boxCollider.enabled = true; // Disable collider to prevent further triggers
    }

    public void GameRestart()
    {
        isMoving = true;
        transform.position = startpositionPlane;
        transform.rotation = Quaternion.Euler(0, 0, 0);

        SelectPlane(LeftPlane);

        UIManager.Instance.StartGameUI();
        UIManager.Instance.UpdateScore(score);
        CollectibleInit();
    }

    void Update()
    {
        if (!isGamestart)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (isFirstTap)
            {
                isFirstTap = false;
                isMoving = false;
                return;
            }
            isMoving = !isMoving;
        }

        if (isMoving)
            MovePlane(Vector3.back, LeftPlane);
        else
            MovePlane(Vector3.forward, RightPlane);
    }

    void MovePlane(Vector3 direction, GameObject planeselect)
    {
        CenterTop.eulerAngles += Vector3.forward * rotationSpeed * Time.deltaTime;
        transform.RotateAround(RoundCircle.position, direction, rotationSpeed * Time.deltaTime);
        SelectPlane(planeselect);
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("coin"))
        {
            GameObject gm = Instantiate(ParticleSkull, CanvasMain);
            gm.transform.position = col.transform.position;
            Destroy(gm, 2.0f);

            score++;
            completeNumber++;
            UIManager.Instance.UpdateScore(score);
            col.gameObject.SetActive(false);
        }

        if (col.CompareTag("ball"))
        {
            isGamestart = false;
            Destroy(col.gameObject);
            Invoke("GameoverResetdata", 1);
            CancelInvoke("Fireball");
        }
        if (completeNumber == 28)
        {
            boxCollider.enabled = false; // Disable collider to prevent further triggers
            isGamestart = false;
            PlayerPrefs.SetInt("Level", levelNumber + 1);
            Invoke("GameoverResetdata", 1);
            CancelInvoke("Fireball");
        }
    }

    void GameoverResetdata()
    {
        if (completeNumber >= 28)
            isNext = true;
        else
            isNext = false;

        isFirstTap = true;
        UIManager.Instance.GameOver(score, isNext);
        if (isNext)
        {
            levelNumber = PlayerPrefs.GetInt("Level");
            UpdateSpeedByLevel();
            UIManager.Instance.UpdateLevel(levelNumber);
        }
        else
        {
            score = 0;
            for (int i = 0; i < RoundCircle.childCount - 1; i++)
            {
                RoundCircle.GetChild(i).gameObject.SetActive(false);
            }
        }
        completeNumber = 0;
    }

    void Fireball()
    {
        GameObject gm = Instantiate(FireballObj, CenterTop);
        gm.transform.localPosition = Vector3.zero;

        RectTransform fireballRect = gm.GetComponent<RectTransform>();
        Vector2 direction = isMoving ? Vector2.left : Vector2.right;  // FIXED direction based on movement

        StartCoroutine(MoveBullet(fireballRect, direction));
        Destroy(gm, 2f);
    }
    IEnumerator MoveBullet(RectTransform rect, Vector2 dir)
    {
        while (rect != null)
        {
            rect.anchoredPosition += dir * bulletSpeed * Time.deltaTime;
            yield return null;
        }
    }
    public void SelectPlane(GameObject plane)
    {
        LeftPlane.SetActive(false);
        RightPlane.SetActive(false);
        plane.SetActive(true);
    }

    void UpdateSpeedByLevel()
    {
        rotationSpeed = 75 + (levelNumber * 3);     // plane speed
        bulletSpeed = 1000 + (levelNumber * 20);    // fireball speed (slightly faster scaling)
    }
}