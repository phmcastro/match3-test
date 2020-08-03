using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class UIManager : MonoBehaviour
{
    public GameObject[] screens;
    private int currentScreen;
    public AudioSource fxSrc;
    public AudioSource musicSrc;
    public static AudioClip[] soundFxs;
    private int roundsCnt;
    public TextMeshProUGUI roundsText;
    public TextMeshProUGUI roundTimerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI goalText;
    private WaitForSeconds transitionDelay;
    public static List<Sprite> gemSprites = new List<Sprite>();
    private float roundTime = 120;
    private bool timerCounting = false;
    public TheGrid myGrid;
    private int score;
    private int goal;
    public TextMeshProUGUI inGameRoundText;
    

    void Start()
    {
        // screen 0 = mainMenu, screen 1 = gameAction, screen 2 = roundTransition, screen 3 = loseScreen


        // start is always at main menu
        currentScreen = 0;


        // making sure the right screens are disabled or enabled
        DisableScreen(1);
        DisableScreen(2);
        DisableScreen(3);

        EnableScreen(currentScreen);

        // setting gameplay values
        roundsCnt = 0;
        transitionDelay = new WaitForSeconds(2.5f);

        // getting medias from project assets
        gemSprites = Resources.LoadAll<Sprite>("Medias/Sprites/Gems").ToList();
        soundFxs = Resources.LoadAll<AudioClip>("Medias/Audio/SFX");

        // debugging to check if the sprites got added in the right order
        int i = 0;
        foreach(var spr in gemSprites)
        {
            Debug.Log($"Index: {i} = Object: {spr.name}");

            i++;
        }
    }

    void Update() 
    {
        if(!timerCounting) { return; }

        roundTime -= Time.deltaTime;
        roundTimerText.text = roundTime > 0 ? roundTime.ToString("###") : "0";

        if(roundTime <= 15)
        {
            roundTimerText.color = Color.red;
            musicSrc.pitch = Mathf.Lerp(musicSrc.pitch, 1.4f, Time.deltaTime);
        }

        if(roundTime <= 0 || score >= goal)
        {
            StopTimer();
            StartCoroutine(ShowTransitionScreen());
        }
    }

    public void AddScore(int pointsToAdd)
    {
        score += pointsToAdd;
        UpdateScore();
    }

    public void UpdateScore()
    {
        scoreText.text = score.ToString();
    }

    public void UpdateGoal()
    {
        goal += 25;
        goalText.text = $"Goal: {goal}pts";
    }

    public void StopTimer()
    {
        timerCounting = false;
    }

    public void RestartTimer()
    {
        roundTime = 120;
        musicSrc.pitch = 1;
        timerCounting = true;
        roundTimerText.color = Color.white;
    }

    public void PlayButtonPressed()
    {
        PlaySoundFX(0);
        StartCoroutine(ShowTransitionScreen());
    }

    public void TryAgain(bool tryAgain)
    {
        if(!tryAgain)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            ReloadRound();
        }
    }

    public void PlaySoundFX(int fxIndex)
    {
        fxSrc.clip = soundFxs[fxIndex];
        fxSrc.Play();
    }

    public void ChangeScreen(int screenIndex)
    {
        DisableScreen(currentScreen);

        // in the transition screen we set new round variables
        if(screenIndex == 2)
        {
            roundsCnt++;
            score = 0;
            UpdateScore();
            PlaySoundFX(0);
            roundsText.text = $"Round {roundsCnt}";
            inGameRoundText.text = roundsCnt.ToString();
        }
        EnableScreen(screenIndex);
        currentScreen = screenIndex;
    }

    public void ReloadRound()
    {
        score = 0;
        UpdateScore();

        RestartTimer();

        myGrid.ShuffleAndCheckGrid();
        myGrid.ResetClicks();

        ChangeScreen(1);
    }

    IEnumerator ShowTransitionScreen()
    {
        if(score >= goal)
        {
            ChangeScreen(2);
            UpdateGoal();
        }
        else
        {
            ChangeScreen(3);
            yield break;
        }

        yield return transitionDelay;

        ChangeScreen(1);
        if(roundsCnt > 1)
        {
            myGrid.ShuffleAndCheckGrid();
        }
        RestartTimer();
    }

    public void DisableScreen(int screenToDisable)
    {
        screens[screenToDisable].SetActive(false);
    }

    public void EnableScreen(int screenToEnable)
    {
        screens[screenToEnable].SetActive(true);
    }
}
