using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Planemove : MonoBehaviour
{
    [Header("GamePlay")]
    public Transform CanvasMain;
    public GameObject RoundCircle;
    public Text txt_Score;
    public Text txt_Level;
    public GameObject Start_Button;

    [Header("Speed Plane")]
    public int Speed;
    public int Bullet_Speed;
    bool Move = true;
    Image Plane_Img;
    int score;
    int Level_Var;

    [Header("Game is Start Or Not")]
    bool is_GameStart=false;

    [Header("Game Over")]
    public GameObject Gameoverpnl;
    public Text Gameover_Score;
    public Text Gameover_BestScore;

    [Header("Start Position of Plane")]
    Vector2 startpositionPlane;

    [Header("Start Position of Plane")]
    public GameObject FireballObj;

    float _directionValue = 1;

    public GameObject particlesys;

    bool isnext = false;

    public Image ButtonImg; 
    public Sprite btn_Restart, Next;

    Transform CenterTop;
    GameObject LeftPlane, RightPlane;

    [Header("Collectible")]
    public List<Sprite> Collectible = new List<Sprite>();

    public void Start()
    {
       Gameover_BestScore.text = PlayerPrefs.GetInt("BestScore").ToString();
       startpositionPlane = transform.position;
       CollectibleInit();
       PlayerPrefs.GetInt("Score",0);
       Level_Var = PlayerPrefs.GetInt("Level", 1);
       txt_Level.text= Level_Var.ToString();
       Plane_Img =transform.GetComponent<Image>();

       // Speed will Increase Base on Level
       Speed =75+ (Level_Var * 3);

       LeftPlane = transform.GetChild(0).gameObject;
       RightPlane = transform.GetChild(1).gameObject;
    }

    #region Instantiate circle with Collible
    int randomvalue;
    void CollectibleInit()
    {
        GameObject gm = Instantiate(RoundCircle,CanvasMain);
        gm.transform.SetSiblingIndex(2);
        gm.SetActive(true);
        gm.name = "Circle";
        Target = gm.transform;

        randomvalue = Random.Range(0, 10);
      
        CenterTop = gm.transform.GetChild(gm.transform.childCount - 1);
        StartCoroutine(ActivateObjectsWithDelay());
    }
    #endregion

    public void Game_Start()
    {
        is_GameStart = true;
        Start_Button.SetActive(false);
        txt_Score.text = "00";
        InvokeRepeating("Fireball", 0.5f, 1);
    }

    public void Game_ReStart()
    {
        Move = true;
        Destroy(Target.gameObject);

        if (isnext)
        {
            txt_Score.text = "00";
        }
        transform.position= startpositionPlane;
        transform.rotation=Quaternion.Euler(0, 0, 0);

        RightPlane.SetActive(false);
        LeftPlane.SetActive(true);

        Start_Button.SetActive(false);
        Gameoverpnl.SetActive(false);
        CollectibleInit();
    }

    #region Coin Animation when game would start
    Transform Target;
    IEnumerator ActivateObjectsWithDelay()
    {
        yield return new WaitForSeconds(2.0f);
        Transform t = Target;
        for (int i=0;i< t.childCount-1;i++)
        {
            t.GetChild(i).GetComponent<Image>().sprite = Collectible[randomvalue];
            t.GetChild(i).gameObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }
        t.GetChild(t.childCount-1).gameObject.SetActive(true);
        Start_Button.SetActive(true);
    }
    #endregion

    // Update is called once per frame
    void Update()
    {
        if (!is_GameStart)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (Move)
            {
                Move = false;
            }
            else
            {
                Move = true;
            }
        }

        if (Move)
        {
            CenterTop.eulerAngles += Vector3.forward * 10 * Time.deltaTime;
            transform.RotateAround(RoundCircle.transform.position, Vector3.back, Speed * Time.deltaTime);

            RightPlane.SetActive(false);
            LeftPlane.SetActive(true);
        }
        else
        {
            CenterTop.eulerAngles += Vector3.forward * -10 * Time.deltaTime;
            transform.RotateAround(RoundCircle.transform.position, Vector3.forward, Speed * Time.deltaTime);

            LeftPlane.SetActive(false);
            RightPlane.SetActive(true);
        }
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        if (col.transform.tag.Equals("coin"))
        {
            GameObject gm = Instantiate(particlesys, CanvasMain);
            gm.transform.position = col.transform.position;
            Destroy(gm, 2.0f);

            score++;
            txt_Score.text = score.ToString();
            Destroy(col.gameObject);
        }

        if (col.transform.tag.Equals("ball"))
        {
            Destroy(col.gameObject);
            GameoverResetdata(1);
        }

        if (Target.childCount == 2)
        {
            PlayerPrefs.SetInt("Level", Level_Var + 1);
            GameoverResetdata();
        }
    }

    void GameoverResetdata(int LevelPlus = 0)
    {     
        Gameoverpnl.SetActive(true);
        Gameover_Score.text = txt_Score.text;
        Debug.Log("Complete");
        is_GameStart = false;

        if (score >= int.Parse(Gameover_BestScore.text))
        {
            Gameover_BestScore.text = score.ToString();
            PlayerPrefs.SetInt("BestScore", int.Parse(Gameover_BestScore.text));
        }    
        // speed will Increase Base on Level    
        CancelInvoke("Fireball");
        if (LevelPlus == 1)
        {       
            score = 0;
            isnext = true;
            ButtonImg.sprite = btn_Restart;
        }
        else
        {
            ButtonImg.sprite = Next;          
            isnext = false;
            Level_Var = PlayerPrefs.GetInt("Level");
            Speed = 75 + (Level_Var * 3);
            txt_Score.text = score.ToString();
            txt_Level.text = Level_Var.ToString();
        }
    }

    void Fireball()
    {
        GameObject gm = Instantiate(FireballObj, Target.GetChild(Target.childCount-1));
        gm.transform.localPosition = Vector3.zero;
        Vector3 pos1 = transform.localPosition;    // Player position
        Vector3 pos2 = gm.transform.localPosition; // FireBall position

        if (Move)
            _directionValue = -15;
        else
            _directionValue = 15;

        var direction = (new Vector3(pos1.x, pos1.y, 0) - new Vector3(pos2.x * _directionValue, pos2.y  *_directionValue, 0)).normalized;
        gm.transform.GetComponent<Rigidbody2D>().AddForce(direction * Bullet_Speed);
        Destroy(gm, 2);
    }
}
