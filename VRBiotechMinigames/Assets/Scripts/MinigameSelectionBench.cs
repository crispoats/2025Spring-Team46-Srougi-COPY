using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MinigameSelectionBench : MonoBehaviour
{

    [SerializeField]
    private string minigameName;
    [SerializeField]
    private string sceneName;
    [SerializeField]
    private TMP_Text minigameText;


    private void Start()
    {
        minigameText.text = minigameName + "\nMinigame";
    }

    public void SwitchToMinigame()
    {
        if (sceneName != null || sceneName.Length != 0)
        {
            GameManager.getInstance().getTransitionmanager().GoToScene(sceneName);
        }
    }

}