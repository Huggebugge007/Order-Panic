
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;

    public Image image;
    public float fadeDuration = 2f;

    [Header("Objects to control")]
    public GameObject player;
    public GameObject sceneparent;
    public GameObject camera;

    private bool transitioning;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(
        string sceneName,
        bool disablePlayer = true,
        GameObject parentToDisable = null)
    {
        if (transitioning)
            return;

        StartCoroutine(LoadRoutine(
            sceneName, disablePlayer, parentToDisable));
    }

    public void UnloadScene(
        string sceneName,
        bool enablePlayer = true,
        GameObject parentToEnable = null)
    {
        if (transitioning)
            return;

        StartCoroutine(UnloadRoutine(
            sceneName, enablePlayer, parentToEnable));
    }

    private IEnumerator LoadRoutine(
        string sceneName,
        bool disablePlayer,
        GameObject parentToDisable)
    {
        transitioning = true;

        yield return image.GetComponent<fadescript>()
            .FadeIn(fadeDuration);

        AsyncOperation operation = SceneManager.LoadSceneAsync(
            sceneName, LoadSceneMode.Additive);

        if (operation == null)
        {
            Debug.LogError("Could not load scene: " + sceneName);
            transitioning = false;
            yield break;
        }

        yield return operation;

        if (disablePlayer && player != null)
        {
            player.SetActive(false);

            LineRenderer line = player.GetComponent<LineRenderer>();
            if (line != null)
                line.enabled = false;
        }

        if (parentToDisable != null)
            parentToDisable.SetActive(false);

        if (camera != null)
            camera.SetActive(false);

        yield return image.GetComponent<fadescript>()
            .FadeOut(fadeDuration);

        transitioning = false;
    }

    private IEnumerator UnloadRoutine(
        string sceneName,
        bool enablePlayer,
        GameObject parentToEnable)
    {
        transitioning = true;

        yield return image.GetComponent<fadescript>()
            .FadeIn(fadeDuration);

        if (SceneManager.GetSceneByName(sceneName).isLoaded)
            yield return SceneManager.UnloadSceneAsync(sceneName);
        else
            Debug.LogWarning("Scene is not loaded: " + sceneName);

        if (enablePlayer && player != null)
        {
            player.SetActive(true);

            LineRenderer line = player.GetComponent<LineRenderer>();
            if (line != null)
                line.enabled = true;
        }

        if (parentToEnable != null)
            parentToEnable.SetActive(true);

        if (camera != null)
            camera.SetActive(true);

        yield return image.GetComponent<fadescript>()
            .FadeOut(fadeDuration);

        transitioning = false;
    }
}
