using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Planemove : MonoBehaviour
{
    [Header("Collectible")]
    [SerializeField] private List<Sprite> CollectibleSprite = new List<Sprite>();
    [SerializeField] private List<Sprite> FireSprite = new List<Sprite>();

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

    public void StartGame()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        UIManager.Instance.ShowBestScore(PlayerPrefs.GetInt("BestScore"));
        startpositionPlane = transform.position;
        score = PlayerPrefs.GetInt("Score", 0);
        levelNumber = PlayerPrefs.GetInt("Level", 1);

        UIManager.Instance.UpdateScore(score);
        UIManager.Instance.UpdateLevel(levelNumber);
        UpdateSpeedByLevel();
        CollectibleInit();
    }
    void OnDisable()
    {
        transform.position = startpositionPlane;
        transform.rotation = Quaternion.Euler(0, 0, 0);
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
        randomvalue = Random.Range(0, CollectibleSprite.Count);

        int randomvaluefire;
        randomvaluefire = Random.Range(0, FireSprite.Count);
        FireballObj.GetComponent<Image>().sprite = FireSprite[randomvaluefire];

        for (int i = 0; i < RoundCircle.childCount - 1; i++)
        {
            RoundCircle.GetChild(i).GetComponent<Image>().sprite = CollectibleSprite[randomvalue];
            RoundCircle.GetChild(i).gameObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }
        CenterTop.gameObject.SetActive(true);
        UIManager.Instance.startButton.gameObject.SetActive(true);
        UpdateSpeedByLevel();
    }
    #endregion

    public void GameStart()
    {
        ClearAllFireballs();
        UIManager.Instance.StartGameUI();
        InvokeRepeating("Fireball", 0.5f, 1);
        isGamestart = true;
        boxCollider.enabled = true; // Enable collider for gameplay
    }

    public void GameRestart()
    {
        ClearAllFireballs();
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
            SoundManager.Instance.PlayCollectSound();
            col.gameObject.SetActive(false);
        }

        if (col.CompareTag("ball"))
        {
            SoundManager.Instance.PlayPlaneHitSound();
            boxCollider.enabled = false; // Disable collider to prevent further triggers
            isGamestart = false;
            Destroy(col.gameObject);
            Invoke("GameoverResetdata", 1);
            CancelInvoke("Fireball");
        }
        if (completeNumber == 14)
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
        if (completeNumber >= 14)
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

        // Unparent from rotating CenterTop, parenting to CanvasMain to prevent spiraling
        gm.transform.SetParent(CanvasMain, true);

        RectTransform fireballRect = gm.GetComponent<RectTransform>();
        Vector2 localDir = isMoving ? Vector2.left : Vector2.right;  // Direction based on movement

        // Calculate constant direction in CanvasMain space
        Vector2 canvasDir = gm.transform.localRotation * localDir;

        StartCoroutine(MoveBullet(fireballRect, canvasDir));
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
        rotationSpeed = Mathf.Min(75 + (levelNumber * 3), 180);     // plane speed (clamped to 180 max for playability)
        bulletSpeed = Mathf.Min(1000 + (levelNumber * 20), 1800);   // fireball speed (clamped to 1800 max to prevent collision tunneling)
    }

    void ClearAllFireballs()
    {
        GameObject[] fireballs = GameObject.FindGameObjectsWithTag("ball");
        foreach (GameObject fb in fireballs)
        {
            if (fb != null)
            {
                Destroy(fb);
            }
        }
    }
}