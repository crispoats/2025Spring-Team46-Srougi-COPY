using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author: Mark Guo
/// Refactor: Jason Nguyen
/// </summary>
public class Alert : MonoBehaviour
{
    [SerializeField] private GameObject alertObject;
    [SerializeField] private TextMeshProUGUI alertText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button dismissButton;

    private Vector3 offset;
    private Coroutine fadeCoroutine;

    private float displayedTime = 0.0f;
    private bool dismissing = false;

    private void Update()
    {
        if (!dismissing && displayedTime > 10.0f)
        {
            dismissing = true;
            Dismiss();
        }
        displayedTime += Time.deltaTime;
    }

    public string GetMessage()
    {
        return alertText.text;
    }
    public Vector3 GetOffset() => offset;

    public void SetTransform(Transform newTransform)
    {
        alertObject.transform.SetParent(newTransform);
    }

    public void SetMessage(string newMessage)
    {
        alertText.text = newMessage;
    }
    public void SetOffset(Vector3 newOffset)
    {
        alertObject.transform.position = alertObject.transform.parent.position + newOffset;
        offset = newOffset;
    }

    public void Display()
    {
        //Debug.Log($"Displaying Alert: {alertText}");
        alertObject.AddComponent<CameraFollower>();

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeInAlert());
    }

    public void Dismiss()
    {
        if (alertObject == null) return;
        dismissButton.interactable = false;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeOutAlert());
    }

    /// Fades in the alert UI.
    private IEnumerator FadeInAlert()
    {
        if (alertObject == null) yield break;

        alertObject.SetActive(true);
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * 2f;
            canvasGroup.alpha = alpha;
            yield return null;
        }
    }


    /// Fades out the alert UI and hides it.
    private IEnumerator FadeOutAlert()
    {
        if (alertObject == null) yield break;

        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * 2f;
            canvasGroup.alpha = alpha;
            yield return null;
        }
        alertObject.SetActive(false);
        Destroy(alertObject);
    }
}
