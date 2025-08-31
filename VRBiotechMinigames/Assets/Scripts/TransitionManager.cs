
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manager class used for switching between scenes.
/// Becomes a child of the GameManager.
/// @author: Jason Nguyen
/// </summary>
public class TransitionManager : MonoBehaviour
{

    /// <summary>
    /// FadeScreen for playing the effects 
    /// </summary>
    [SerializeField]
    private FadeScreen fadeScreen;

    /// <summary>
    /// Destory the FadeScreen when TransitionManager is Destroyed
    /// </summary>
    void OnDestroy()
    {

        if (fadeScreen != null)
        {
            Destroy(fadeScreen.gameObject);
        }

    }

    /// <summary>
    /// Perform initial setups for the FadeScreen
    /// </summary>
    /// <param name="camera">The Camera to attach the FadeScreen to </param>
    /// <param name="fadeScreenObj">The FadeScreenObject to set up</param>
    public void Setup(GameObject camera, GameObject fadeScreenObj)
    {
        fadeScreenObj.transform.SetParent(camera.transform);
        FadeScreen fadeScreenScript = fadeScreenObj.GetComponent<FadeScreen>();
        fadeScreenObj.transform.localPosition = new Vector3(0.0f, 0.0f, 0.15f);
        fadeScreenObj.transform.localRotation = Quaternion.identity;
        this.SetFadeScreen(fadeScreenScript);
    }

    /// <summary>
    /// Set the FadeScreen Object
    /// </summary>
    /// <param name="screen"></param>
    public void SetFadeScreen(FadeScreen screen)
    {
        if (screen != null)
        {
            // If there is already a fadeScreen, destroy it
            if (fadeScreen != null)
            {
                Destroy(fadeScreen.gameObject);
            }
            fadeScreen = screen;
        }
    }

    /// <summary>
    /// Change the GameScene based on the Scene name
    /// </summary>
    /// <param name="sceneName">The name of the scene to change to</param>
    public void GoToScene(string sceneName)
    {
        StartCoroutine(GoToSceneRoutine(sceneName));
    }

    /// <summary>
    /// CoRoutine for fading the scene
    /// </summary>
    /// <param name="sceneName">The name of the scene to change to</param>
    /// <returns></returns>
    IEnumerator GoToSceneRoutine(string sceneName)
    {
        fadeScreen.FadeToBlack();
        // Wait for the FadeRoutine to finish, and some additional time just in case
        yield return new WaitForSeconds(fadeScreen.GetFadeDuration() + 0.05f);

        // Launch the new scene
        // SceneManager is something Unity already owns
        SceneManager.LoadScene(sceneName);

    }
}
