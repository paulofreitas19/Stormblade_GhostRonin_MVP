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

    private bool isTransitioning;

    [SerializeField] private float letterDelay = 0.08f;

    private void Awake()
    {
        if(gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if(playerLifePoints != null)
            playerLifePoints.OnGameOver += ShowGameOver;

        if(backgroundTransition != null)
            backgroundTransition.OnTransitionFinished += RealoadCurrentScene;
    }

    private void OnDisable()
    {
        if(playerLifePoints != null)
            playerLifePoints.OnGameOver -= ShowGameOver;

        if(backgroundTransition != null)
            backgroundTransition.OnTransitionFinished -= RealoadCurrentScene;
    }

    private void ShowGameOver()
    {
        if(gameOverPanel == null)
            return;

        gameOverPanel.SetActive(true);

        actions.SetActive(false);

        StartCoroutine(RevealGameOverText());
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
}
