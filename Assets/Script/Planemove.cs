using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Planemove : MonoBehaviour
{
    [Header("Collectible")]
    [SerializeField] private List<Sprite> CollectibleSprite = new List<Sprite>();
    [Header("GamePlay")]
    [SerializeField] private Transform CanvasMain;
    [SerializeField] private Transform RoundCircle;
    [SerializeField] private GameObject FireballObj;
    [SerializeField] private GameObject ParticleSkull;

    [Header("Speed Plane")]
    public int rotationSpeed;
    public int bulletSpeed;

    [Header("Score LevelNumber")]
    int score, levelNumber;

    [Header("Bool For GameStart ,Move check")]
    bool isMoving = true;
    bool isGamestart = false;
    bool isnext = false;

    [Header("Start Position of Plane")]
    Vector2 startpositionPlane;
    float _direction = 1;

    Image planeImg;

    Transform CenterTop;
    GameObject LeftPlane, RightPlane;

    public void Start()
    {
        UIManager.Instance.ShowBestScore(PlayerPrefs.GetInt("BestScore"));
        startpositionPlane = transform.position;
        CollectibleInit();
        score = PlayerPrefs.GetInt("Score", 0);
        levelNumber = PlayerPrefs.GetInt("Level", 1);
        UIManager.Instance.AddScore(score, levelNumber);

        planeImg = transform.GetComponent<Image>();

        // Speed will Increase Base on Level
        rotationSpeed = 75 + (levelNumber * 3);

        LeftPlane = transform.GetChild(0).gameObject;
        RightPlane = transform.GetChild(1).gameObject;
    }

    #region Instantiate circle with Collible
    int randomvalue;
    void CollectibleInit()
    {
        Target = RoundCircle;
        RoundCircle.gameObject.SetActive(true);
        randomvalue = Random.Range(0, 10);

        CenterTop = RoundCircle.GetChild(RoundCircle.childCount - 1);
        StartCoroutine(ActivateObjectsWithDelay());
    }
    #endregion

    #region Coin Animation when game would start
    Transform Target;
    IEnumerator ActivateObjectsWithDelay()
    {
        yield return new WaitForSeconds(1.0f);
        Transform t = RoundCircle;
        for (int i = 0; i < t.childCount - 1; i++)
        {
            t.GetChild(i).GetComponent<Image>().sprite = CollectibleSprite[randomvalue];
            t.GetChild(i).gameObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }
        t.GetChild(t.childCount - 1).gameObject.SetActive(true);
        UIManager.Instance.startButton.gameObject.SetActive(true);
    }
    #endregion

    public void Game_Start()
    {
        isGamestart = true;
        UIManager.Instance.StartGameUI(PlayerPrefs.GetInt("Level"));
        InvokeRepeating("Fireball", 0.5f, 1);
    }

    public void Game_ReStart()
    {
        isMoving = true;
        if (isnext)
        {
            UIManager.Instance.AddScore(00, levelNumber);
        }
        transform.position = startpositionPlane;
        transform.rotation = Quaternion.Euler(0, 0, 0);

        SelectPlane(LeftPlane);

        UIManager.Instance.StartGameUI(levelNumber);
        CollectibleInit();
    }
    void Update()
    {
        if (!isGamestart)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            isMoving = !isMoving;
        }

        if (isMoving)
            MovePlane(10, Vector3.back, LeftPlane);
        else
            MovePlane(-10, Vector3.forward, RightPlane);
    }

    void MovePlane(int multiply, Vector3 direction, GameObject planeselect)
    {
        CenterTop.eulerAngles += Vector3.forward * 10 * Time.deltaTime;
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
            UIManager.Instance.AddScore(score, levelNumber);
            col.gameObject.SetActive(false);
        }

        if (col.CompareTag("ball"))
        {
            Destroy(col.gameObject);
            GameoverResetdata(true);
        }
        Debug.Log("Score : " + score);
        if (score == 28)
        {
            PlayerPrefs.SetInt("Level", levelNumber + 1);
            GameoverResetdata(false);
        }
    }

    void GameoverResetdata(bool isNext = false)
    {
        UIManager.Instance.GameOver(score, isNext);
        Debug.Log("Complete");
        isGamestart = false;

        CancelInvoke("Fireball");
        if (isNext)
        {
            score = 0;
            isnext = true;
        }
        else
        {
            isnext = false;
            levelNumber = PlayerPrefs.GetInt("Level");
            rotationSpeed = 75 + (levelNumber * 3);
            UIManager.Instance.AddScore(score, levelNumber);
        }
    }

    void Fireball()
    {
        GameObject gm = Instantiate(FireballObj, Target.GetChild(Target.childCount - 1));
        gm.transform.localPosition = Vector3.zero;
        Vector3 pos1 = transform.localPosition;    // Player position
        Vector3 pos2 = gm.transform.localPosition; // FireBall position

        if (isMoving)
            _direction = -15;
        else
            _direction = 15;

        var direction = (new Vector3(pos1.x, pos1.y, 0) - new Vector3(pos2.x * _direction, pos2.y * _direction, 0)).normalized;
        gm.transform.GetComponent<Rigidbody2D>().AddForce(direction * bulletSpeed);
        Destroy(gm, 2);
    }
    public void SelectPlane(GameObject plane)
    {
        LeftPlane.SetActive(false);
        RightPlane.SetActive(false);
        plane.SetActive(true);
    }
}