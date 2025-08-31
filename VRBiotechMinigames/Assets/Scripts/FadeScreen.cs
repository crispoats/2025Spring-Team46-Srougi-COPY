using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Script used for the Screen Fade Object. Used to turn on and off the screen fade when transitioning between scenes.
/// 
/// @author: Jason Nguyen
/// </summary>
public class FadeScreen : MonoBehaviour
{
    /// <summary>
    /// Start the FadeScreen only on Game Launch
    /// </summary>
    public bool fadeOnStart = true;

    /// <summary>
    /// The duration of fading
    /// </summary>
    [SerializeField]
    private float fadeDuration = 2.0f;

    /// <summary>
    /// The BaseColor for the FadeScreen
    /// </summary>
    [SerializeField]
    private Color fadeColor;

    /// <summary>
    /// Render material for the FadeScreen Object
    /// </summary>
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (fadeOnStart)
        {
            FadeToNormal();
        }
        fadeOnStart = false;
    }

    /// <summary>
    /// Get the fade duration
    /// </summary>
    /// <returns>Fade Duration</returns>
    public float GetFadeDuration()
    {
        return fadeDuration;
    }


    /// <summary>
    /// Fade the screen to black
    /// Opacity = 1
    /// </summary>
    public void FadeToBlack()
    {

        Fade(0.0f, 1.0f);
    }

    /// <summary>
    /// Fade the screen to normal
    /// Opacity = 0
    /// </summary>
    public void FadeToNormal()
    {
        Fade(1.0f, 0.0f);
    }


    /// <summary>
    /// Cause the Screen to fade from one alpha value to another alpha value
    /// </summary>
    /// <param name="alphaIn">The starting alpha value</param>
    /// <param name="alphaOut">The ending alpha value</param>
    private void Fade(float alphaIn, float alphaOut)
    {
        StartCoroutine(FadeRoutine(alphaIn, alphaOut));
    }

    /// <summary>
    /// The Coroutine for fading the screen
    /// </summary>
    /// <param name="alphaIn">The starting alpha value</param>
    /// <param name="alphaOut">The ending alpha value</param>
    /// <returns></returns>
    private IEnumerator FadeRoutine(float alphaIn, float alphaOut)
    {
        float timer = 0;
        while (timer < fadeDuration)
        {
            Color newColor = fadeColor;
            // Lerp the new color alpha between 0-1
            newColor.a = Mathf.Lerp(alphaIn, alphaOut, timer / fadeDuration);


            rend.material.SetColor("_BaseColor", newColor);

            timer += Time.deltaTime;
            yield return null;
        }

        // On Final call, make sure the color is set to the alphaOut
        Color finalColor = fadeColor;
        // Lerp the new color alpha between 0-1
        finalColor.a = alphaOut;

        rend.material.SetColor("_BaseColor", finalColor);
    }
}
