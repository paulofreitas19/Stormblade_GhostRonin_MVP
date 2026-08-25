using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameOverUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerLifePoints playerLifePoints;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private GameObject actions;
    [SerializeField] private GameOverBackgroundTransition backgroundTransition;
    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private ScreenFadeController screenFade;

    private bool isTransitioning;
    private bool gameOverPending;
    private bool gameOverPresentationStarted;

    [SerializeField] private float letterDelay = 0.09f;

    [SerializeField] private float fadeToBlackDuration = 0.5f;
    [SerializeField] private float fadeFromBlackDuration = 0.35f;

    private void Awake()
    {
        if(gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if(playerLifePoints != null)
            playerLifePoints.OnGameOver += HandleGameOver;

        if(animationController != null)
            animationController.OnDeathTransitionPoint += BeginGameOverTransition;

        if(backgroundTransition != null)
            backgroundTransition.OnTransitionFinished += RealoadCurrentScene;
    }

    private void OnDisable()
    {
        if(playerLifePoints != null)
            playerLifePoints.OnGameOver -= HandleGameOver;

        if(animationController != null)
            animationController.OnDeathTransitionPoint -= BeginGameOverTransition;

        if(backgroundTransition != null)
            backgroundTransition.OnTransitionFinished -= RealoadCurrentScene;
    }

    public void ContinueGame()
    {
        if(isTransitioning)
            return;

        isTransitioning = true;

        if(actions != null)
            actions.SetActive(false);

        if(backgroundTransition != null)
            backgroundTransition.PlayContinueTransition();
    }

    private void RealoadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("");
    }

    private IEnumerator RevealGameOverText()
    {
        gameOverText.maxVisibleCharacters = 0;

        gameOverText.ForceMeshUpdate();

        int totalCharacters = gameOverText.textInfo.characterCount;

        for(int i = 0; i <= totalCharacters; i++)
        {
            gameOverText.maxVisibleCharacters = i;

            yield return new WaitForSeconds(letterDelay);
        }

        actions.SetActive(true);
    }

    private void HandleGameOver()
    {
        gameOverPending = true;
    }

    private void BeginGameOverTransition()
    {
        if(!gameOverPending)
            return;

        if(gameOverPresentationStarted)
            return;

        gameOverPresentationStarted = true;

        StartCoroutine(GameOverTransitionRoutine());
    }

    private IEnumerator GameOverTransitionRoutine()
    {
        if(screenFade != null)
            yield return screenFade.FadeTo(1f, fadeToBlackDuration);

        if(gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if(actions != null)
            actions.SetActive(false);
        
        if(gameOverText != null)
            gameOverText.maxVisibleCharacters = 0;

        if(screenFade != null)
            yield return screenFade.FadeTo(0f, fadeFromBlackDuration);

        StartCoroutine(RevealGameOverText());
    }
}
