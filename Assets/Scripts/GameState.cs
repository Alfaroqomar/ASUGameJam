using DG.Tweening;
using NUnit.Framework;
using System.Collections;
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
    private GameObject Canvas;
    private GameObject Person;
    private Image PersonImage;
    private Vector2 PersonOriginalPosition;
    private float PersonStandingPos;
    private float PersonDownPos;
    private GameObject WindowMask;
    private bool DialogLoaded = false;
    public int WalkLoops = 5;
    public float TravelTime = 3f;
    public Ease WalkXEase = Ease.InOutSine;
    public Ease WalkYEase = Ease.InOutBack;
    public Ease ColorEase = Ease.InQuart;
    public bool Debug = true;
    private bool personInFrame = false;

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
        Debug = true;
        if (Debug)
        {
            TravelTime = 0.5f;
        }
        //wait for 1 second before activating the dialog to ensure that the DialogManager has initialized
        Canvas = GameObject.Find("Canvas");
        WindowMask = Canvas.transform.Find("WindowMask").gameObject;
        Person = WindowMask.transform.Find("Person").gameObject;
        PersonImage = Person.GetComponent<Image>();
        PersonOriginalPosition = Person.transform.localPosition;
        PersonStandingPos = PersonOriginalPosition.y;
        PersonDownPos = PersonOriginalPosition.y - 25;
        

    }

    public void NewNPCEnters(string NPCName)
    {

        print(Person.name);
        personInFrame = true;
        PersonImage.color = Color.black;
        PersonImage.DOColor(new Color(1, 1, 1, 1), TravelTime).SetEase(ColorEase); //fade in the person
        Person.transform.localPosition = new Vector2(PersonOriginalPosition.x, PersonDownPos);
        Person.transform.DOLocalMoveY(PersonStandingPos, TravelTime/WalkLoops).SetEase(WalkYEase).SetLoops((int)WalkLoops, LoopType.Yoyo);
        Person.transform.DOLocalMoveX(0, TravelTime).SetEase(WalkXEase).onComplete = () =>
        {
            DialogManager.Instance.ActivateDialog(NPCName);
        };
    }

    public void NPCLeaves()
    {

        PersonImage.color = Color.white;
        PersonImage.DOColor(new Color(0, 0, 0, 1), TravelTime / WalkLoops).SetEase(ColorEase).onComplete = () =>
        {
            PersonImage.color = Color.white;
        };
        Person.transform.localPosition = new Vector2(Person.transform.localPosition.x, PersonStandingPos);
        Person.transform.DOLocalMoveY(PersonDownPos, TravelTime / WalkLoops).SetEase(WalkYEase).SetLoops((int)WalkLoops, LoopType.Yoyo);
        Person.transform.DOLocalMoveX(-PersonOriginalPosition.x, TravelTime).SetEase(WalkXEase).onComplete = () =>
        {
            Person.transform.localPosition = PersonOriginalPosition;
            personInFrame = false;
            //trigger next npc
        };

    }


    public IEnumerator StartingGame()
    {
        while (!Canvas)
        {
            yield return null; // Wait until the next frame
        }

        NewNPCEnters("ZanyCharacter");
        //DialogManager.Instance.ActivateDialog("ZanyCharacter");
    }

}