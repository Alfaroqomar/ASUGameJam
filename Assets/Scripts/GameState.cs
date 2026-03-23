using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
//using static System.Net.Mime.MediaTypeNames;

public class GameState : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public static GameState Instance { get; private set; }

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;

        }
        else
        {
            Destroy(gameObject);
        }

    }

    void Start()
    {
        //wait for 1 second before activating the dialog to ensure that the DialogManager has initialized


    }

    public void StartingGame()
    {
        DialogManager.Instance.ActivateDialog("ZanyCharacter");
    }

}