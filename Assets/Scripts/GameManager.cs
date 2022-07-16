using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Script Reference")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private SpawnManager spawnManager;

    [Header("Game HUD")]
    [SerializeField] private GameObject hudContainer;
    [SerializeField] private Slider progressSilder;
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject musicBackground;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private AudioClip clearSound;

    [Header("Start Game Menu")]
    [SerializeField] private GameObject startMenu;
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;
    [SerializeField] [Range(1, 3)] private int sameEnemySpawnCount = 2;
    [SerializeField] private Camera menuCamera;
    [SerializeField] private Camera mainCamera;

    [Header("Game Over Menu")]
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private TextMeshProUGUI clearTimeText;
    [SerializeField] private TextMeshProUGUI countdownTimeText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Animator transition;

    //stage game setting
    [Header("Stage Game Setting")]
    [SerializeField] private int progressLevel = 1;
    [SerializeField] private int[] progressLevelGoal = {50, 200, 300, 400};
    [SerializeField] [Range(0, 3)] private float maxIncreasedScale = 0.7f;
    [SerializeField] [Range(0, 20)] private int particleValue;
    [SerializeField] [Range(-20, 0)] private int damageHitValue;
    [SerializeField] private TimeSpan playTime;
    [SerializeField] private float elapsedTime;
    public static bool isGameActive = false;

    //spawn game setting
    [Header("Spawn Game Setting")]
    [SerializeField] private int[] maxEnemies;
    [SerializeField] private int[] maxParticles;
    [SerializeField] private int[] maxPowerups;
    [SerializeField] [Range(5, 20)] private float delaySpawn;
    [SerializeField] private Dictionary<int, float[]>  enemyChance = new Dictionary<int, float[]>(){
    {1, new float[]{80.0f,20.0f,0.0f}}, {2, new float[]{70.0f,30.0f,10.0f}},
    {3, new float[]{50.0f,30.0f,20.0f}}, {4, new float[]{20.0f,40.0f,40.0f}}
    };

    //variable
    [Header("Variable")]
    
    [SerializeField] private int countdownTime;
    [SerializeField] private float transitionTime = 1.0f;
    [SerializeField] private ParticleSystem clearParticle;
    private static bool isRestart = false;
    public static bool isInMainMenu = true;

    // Start is called before the first frame update
    void Start()
    {
        //set value to default
        isInMainMenu = true;

        //add listeners
        AddListeners();

        if(isRestart)
        {
            SetupGame();
        }
        else
        {
            StartMainMenu();
        }
    }

    void AddListeners()
    {
        startButton.onClick.AddListener(SetupGame);
        exitButton.onClick.AddListener(ExitGame);
        restartButton.onClick.AddListener(RestartGame);
        mainMenuButton.onClick.AddListener(BackToMainMenu);
    }

    public void PlayClickSoundEffect()
    {
        audioSource.PlayOneShot(clickSound);
    }

    public void StartMainMenu()
    {
        spawnManager.SpawnObjectsInMainMenu();
    }

    public void SetupGame()
    {
        //set to value to default in case of did restart then to go main menu
        isRestart = false;
        
        MenuTransition();
        
        startMenu.SetActive(false);
        progressSilder.maxValue = GetMaxPlayerSize();

        //setup hud & audio listener for player
        playerController.EnablePlayerAudioListener();
        hudContainer.SetActive(true);

        StartCoroutine(CountdownAtStart());
    }

    void MenuTransition()
    {
        //change to gameplay camera
        menuCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);

        //not in main menu
        isInMainMenu = false;

        //delete objects in main menu
        spawnManager.DeleteObjectsInMainMenu();
    }

    IEnumerator CountdownAtStart()
    {
        while(countdownTime > 0)
        {
            countdownTimeText.text = countdownTime.ToString();
            yield return new WaitForSeconds(1);

            countdownTime--;
        }

        countdownTimeText.text = "Start!";

        StartGame();

        yield return new WaitForSeconds(1);
        countdownTimeText.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        isGameActive = true;
        StartCoroutine(spawnManager.SpawnObjects());
        StartCoroutine(UpdateTimer()); 
    }

    public void GameOver()
    {
        StartCoroutine(GameOverTransition());

        audioSource.PlayOneShot(clearSound);
        musicBackground.SetActive(false);
    }

    public void RestartGame()
    {
        isRestart = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void SetProgressSlider(int playerSize)
    {
        progressSilder.value = playerSize;
    }

    public void CheckAndSetProgressLevel(int playerSize)
    {
        for(int i = 0; i < progressLevelGoal.Length; i++)
        {
            if(playerSize <= progressLevelGoal[i])
            {
                progressLevel = i + 1;
                break;
            }
        }
    }

    public int GetMaxPlayerSize()
    {
        return progressLevelGoal[progressLevelGoal.Length-1];
    }

    public float GetMaxIncreasedScale()
    {
        return maxIncreasedScale;
    }

    public int GetParticleValue()
    {
        return particleValue;
    }
    
    public int GetDamageHitValue()
    {
        return damageHitValue;
    }

    public float[] GetEnemyChance()
    {
        return enemyChance[progressLevel];
    }

    public int GetMaxEnemies()
    {
        return maxEnemies[progressLevel-1];
    }

    public int GetMaxParticles()
    {
        return maxParticles[progressLevel-1];
    }

    public int GetMaxPowerups()
    {
        return maxPowerups[progressLevel-1];
    }

    public float GetDelaySpawn()
    {
        return delaySpawn;
    }

    public void PlaySpawnSound()
    {
        audioSource.PlayOneShot(spawnSound);
    }

    public int GetSameEnemySpawnCount()
    {
        return sameEnemySpawnCount;
    }

    private IEnumerator GameOverTransition()
    {
        clearParticle.gameObject.SetActive(true);

        yield return new WaitForSeconds(transitionTime);
        
        transition.SetTrigger("fade_in");

        yield return new WaitForSeconds(transitionTime);

        isGameActive = false;

        clearTimeText.text = "Clear Time: " + playTime.ToString("mm':'ss'.'ff");
        gameOverMenu.SetActive(true);
    }

    public IEnumerator UpdateTimer()
    {
        while(isGameActive)
        {
            elapsedTime += Time.deltaTime;
            playTime = TimeSpan.FromSeconds(elapsedTime);   
            timeText.text = "Time: " + playTime.ToString("mm':'ss'.'ff");

            yield return null;
        }
    }
}
