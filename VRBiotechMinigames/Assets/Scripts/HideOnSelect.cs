using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HideOnSelect : MonoBehaviour
{
    [SerializeField]
    private XRBaseInteractor[] interactors;

    void Start()
    {
        foreach (XRBaseInteractor interactor in interactors)
        {
            interactor.selectEntered.AddListener(Hide);
            interactor.selectExited.AddListener(Show);
        }
    }

    private void Hide(SelectEnterEventArgs args)
    {
        gameObject.SetActive(false);
    }

    private void Show(SelectExitEventArgs args)
    {
        gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        foreach (XRBaseInteractor interactor in interactors)
        {
            interactor.selectEntered.RemoveListener(Hide);
            interactor.selectExited.RemoveListener(Show);
        }
    }
}
