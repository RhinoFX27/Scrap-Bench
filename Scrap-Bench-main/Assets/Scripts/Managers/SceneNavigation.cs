
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneNavigation : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string storeSceneName = "0 (1)";
    [SerializeField] private string gameSceneName = "Scene2";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Update()
    {
        // Only handle Escape from the active scene's manager.
        if (SceneManager.GetActiveScene() != gameObject.scene)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMainMenu();
        }
    }

    public void OpenStore()
    {
        Scene store = SceneManager.GetSceneByName(storeSceneName);

        if (!store.isLoaded)
        {
            StartCoroutine(LoadStore());
        }
    }

    private IEnumerator LoadStore()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(
            storeSceneName,
            LoadSceneMode.Additive
        );

        yield return operation;

        Scene gameScene = SceneManager.GetSceneByName(gameSceneName);

        if (gameScene.IsValid() && gameScene.isLoaded)
        {
            SceneManager.SetActiveScene(gameScene);
        }
    }

    public void ReturnToGame()
    {
        Scene gameScene = SceneManager.GetSceneByName(gameSceneName);
        Scene store = SceneManager.GetSceneByName(storeSceneName);

        if (!gameScene.IsValid() || !gameScene.isLoaded)
        {
            Debug.LogError("Game scene is not loaded: " + gameSceneName);
            return;
        }

        SceneManager.SetActiveScene(gameScene);

        if (store.isLoaded)
        {
            SceneManager.UnloadSceneAsync(store);
        }
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(
            mainMenuSceneName,
            LoadSceneMode.Single
        );
    }


    public void PlayGame()
    {
        SceneManager.LoadScene(
            gameSceneName,
            LoadSceneMode.Single
        );
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

}
