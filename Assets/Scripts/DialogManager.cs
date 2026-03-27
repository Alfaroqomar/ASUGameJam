using DG.Tweening;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static DialogManager;
using Image = UnityEngine.UI.Image;

public class DialogManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    List<TextAsset> myJsons = new List<TextAsset>();
    private List<Coroutine> myCoroutines = new List<Coroutine>();
    private GameObject Canvas;
    private GameObject DialogBox;
    private TextMeshProUGUI DialogText;
    private GameObject DialogArrow;

    private GameState gameState;

    public static event Action<string> DialogueDone;
    public static DialogManager Instance { get; private set; }

    [Serializable]
    public class PassageWrapper
    {
        public List<TempPassage> passages;
    }

    [Serializable]
    public class TempPassage
    {
        public string text;
        public List<TempLink> links;
        public string name;
        public List<string> tags;
    }

    [Serializable]
    public class TempLink
    {
        public string name;
        public string link;
    }

    public class Passage
    {
        public string text;
        public List<string> links;
        public string name;
        public List<string> tags;
        public Passage next;
        public Passage previous;
        public List<Passage> OptionPassages;
        public string optionName;
        public bool MultiplePassage;
    }

    List<TempPassage> tempPassages = new List<TempPassage>();
    List<Passage> Passages = new List<Passage>();
    private List<GameObject> optionObjs = new List<GameObject>();
    private List<GameObject> monitorObjs = new List<GameObject>();
    private bool scrolled;
    public float waitTime = 0.025f; //0.03f
    public float defaultWaitTime = 0.025f; //0.03f
    public bool dialogueUp;
    private string currentPassageName;
    private bool AnimatingText;
    private Color defaultArrowColor;
    private RectTransform arrowRect;
    private Vector2 defaultArrowPos;
    private Tween arrowTween;

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
        //read jsons from resources/dialogue
        //load them into a dictionary
        gameState = GameState.Instance;
        myJsons = Resources.LoadAll<TextAsset>("Dialogue/GitDialog").ToList<TextAsset>();

        for (int i = 0; i < myJsons.Count; i++)
        {
            Debug.Log("Loaded JSON: " + myJsons[i].name);
            PassageWrapper myWrapper = JsonUtility.FromJson<PassageWrapper>(myJsons[i].text);
            tempPassages.AddRange(myWrapper.passages);
        }

        //convert temp passages to passages

        foreach (TempPassage myPassage in tempPassages)
        {
            Passage newPassage = new Passage();
            newPassage.text = myPassage.text;
            //print(myPassage.text + " has " + myPassage.links.Count + " links");
            newPassage.links = myPassage.links.Select(l => l.name).ToList();
            newPassage.name = myPassage.name;

            if (Passages.Exists(p => p.name == myPassage.name))
            {
                //print("Passage " + passage.name + "has multiples");
                continue;
            }

            newPassage.tags = myPassage.tags;
            newPassage.next = null;
            newPassage.previous = null;
            newPassage.OptionPassages = new List<Passage>();
            newPassage.optionName = null;
            newPassage.MultiplePassage = false;
            Passages.Add(newPassage);
        }

        //setting the previous and next of each passage here


        foreach (Passage passage in Passages)
        {

            if (passage.tags.Contains("Options"))
            {
                print("Setting option passages for " + passage.name);
            }
            else
            {
                passage.next = Passages.FirstOrDefault(p => p.name == passage.links.FirstOrDefault());
                List<Passage> myPassages = Passages.Where(p => p.links.Contains(passage.name)).ToList(); // Find all passages that link to this one
                if (myPassages.Count > 1)
                {
                    passage.MultiplePassage = true; // Set MultiplePassage to true if there are multiple passages linking to this one
                }
                else
                {
                    passage.previous = Passages.FirstOrDefault(p => p.links.Contains(passage.name)); // Find the previous passage by checking if it links to the current passage
                }

            }
        }

        //remove last line of all text if link isnt null

        foreach (Passage passage in Passages)
        {
            if (passage.links.Count > 0)
            {
                string[] newText = passage.text.Split('\n');
                // add back all strings except last

                //if any empty lines, remove
                newText = newText.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();

                passage.text = string.Join("\n", newText.Take(newText.Length - 1));
            }

        }

        defaultWaitTime = 0.025f;
        waitTime = defaultWaitTime;

        //ASSIGN VARIABLES HERE
        Canvas = GameObject.Find("Canvas");
        DialogBox = Canvas.transform.Find("DialogBox").gameObject;
        GameObject Viewport = DialogBox.transform.Find("Viewport").gameObject;
        DialogText = Viewport.transform.Find("DialogText").GetComponent<TextMeshProUGUI>();
        DialogArrow = Viewport.transform.Find("DialogArrow").gameObject;
        defaultArrowColor = DialogArrow.GetComponent<Image>().color;
        arrowRect = DialogArrow.GetComponent<RectTransform>();
        defaultArrowPos = arrowRect.localPosition;
        StartCoroutine(GameState.Instance.StartingGame());
    }

    void Update()
    {

    }

    public void ActivateDialog(string passageName)
    {
        Passage currentPassage = Passages.FirstOrDefault(p => p.name == passageName);
        print("Activating passage: " + passageName);
        print("Passage text: " + currentPassage.text);
        currentPassageName = passageName;
        DisplayPassage(currentPassage);

    }

    public void DialogBoxClicked()
    {
        print("Dialog box clicked");
        if (currentPassageName == null)
        {
            return;
        }

        if (AnimatingText)
        {
            AnimatingText = false;
            return;
        }

        print("playing next passage");
        Passage currentPassage = Passages.FirstOrDefault(p => p.name == currentPassageName);
        if (currentPassage.next != null)
        {
            ActivateDialog(currentPassage.next.name);
            currentPassageName = currentPassage.next.name;
        }


    }

    private void DisplayPassage(Passage passage)
    {
        DialogText.text = passage.text;
        resetArrowPos();
        Coroutine textAnimation = StartCoroutine(TextAnimationCoro(DialogText));
        myCoroutines.Add(textAnimation);
    }

    private IEnumerator TextAnimationCoro(TextMeshProUGUI textObj, Action? onComplete = null)
    {
        AnimatingText = true;
        DialogArrow.GetComponent<Image>().color = defaultArrowColor;
        string fullText = textObj.text; // Cache the full text
        textObj.text = ""; // Start with empty text
        //get num of tweens on arrow

        //print(DOTween.TweensByTarget(DialogArrow.transform).Count);

        if (arrowTween != null && arrowTween.IsActive())
        {
            arrowTween.Kill();
        }

        for (int i = 0; i < fullText.Length; i++)
        {
            if (!AnimatingText)
            {
                textObj.text = fullText; // If animation was stopped, set full text immediately
                break; // Exit the coroutine
            }
            //could check if character is [ then check if characters after = a keyword like wait, then could do stuff with it
            textObj.text += fullText[i]; // Add one character at a time
            
            yield return new WaitForSeconds(waitTime); //should be 0.05f
        }


        AnimatingText = false;
        resetArrowPos(); // Reset arrow position before starting animation
        arrowTween = arrowRect
        .DOLocalMoveY(defaultArrowPos.y + 5f, 0.5f)
        .SetLoops(-1, LoopType.Yoyo);

        //DialogArrow.transform.DOLocalMoveY(defaultArrowPos.y + 5f, 0.5f).SetLoops(-1, LoopType.Yoyo).OnComplete(() =>
        //{
        //    DialogArrow.transform.localPosition = defaultArrowPos; // Reset position when animation completes
        //});
        DialogArrow.GetComponent<Image>().color = Color.black;
        onComplete?.Invoke(); // Invoke the onComplete action if provided
    }

    private void resetArrowPos()
    {
        DialogArrow.transform.localPosition = defaultArrowPos; // Reset position before starting animation
    }


}