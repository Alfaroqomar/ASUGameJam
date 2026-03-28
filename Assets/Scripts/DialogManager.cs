using DG.Tweening;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPEffects.Components;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static DialogManager;
using Image = UnityEngine.UI.Image;
using Toggle = UnityEngine.UI.Toggle;

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

    public class BookGenre
    {
        public string name;
        public Color color;
        public string ColorString;
        public string Animation;
        public string TextString; //string to use in text

        public BookGenre(string name, Color color, string animation = "")
        {
            this.name = name;
            this.color = color;
            if (animation != "")
            {
                this.TextString = "<" + animation + ">" + 
                    "<color=" + color.ToHexString() + ">" + 
                    name + "</color>" + "</>";
            }
            else
            {
                this.TextString = "<color=#" + color.ToHexString() + ">" + name + "</color>";
            }
        }

    }

    public class Book
    {
        public List<BookGenre> genres;
    }

    List<BookGenre> bookGenres;

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
    private TMPWriter Writer;
    private GameObject DropdownObj;
    private TMP_Dropdown Dropdown;

    private List<BookGenre> foundBookGenres;
    private GameObject bookTemplate;
    private GameObject tablet;

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

            if (passage.tags.Contains("Optionasdasds")) //ignoring this for now
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
        Writer = DialogText.GetComponent<TMPWriter>();

        tablet = Canvas.transform.Find("Tablet").gameObject;
        DropdownObj = Canvas.transform.Find("Tablet/Dropdown").gameObject;
        Dropdown = DropdownObj.GetComponent<TMP_Dropdown>();

        foundBookGenres = new List<BookGenre>();
        bookTemplate = tablet.transform.Find("BookContainer/BookTemplate").gameObject;

        bookGenres = new List<BookGenre>()
        {
            new BookGenre("Fantasy", new Color(0.5f, 0f, 1f), "wave"),
            new BookGenre("Science Fiction", new Color(0f, 1f, 1f), "wave"),
            new BookGenre("Mystery", new Color(1f, 0.5f, 0f), "fade"),
            new BookGenre("Romance", new Color(1f, 0f, 0.5f), ""),
            new BookGenre("Horror", new Color(0.5f, 0.5f, 0.5f), "shake")
        };

        Dropdown.options.Clear();
        for (int i = 0; i < bookGenres.Count; i++)
        {
            Dropdown.options.Add(new TMP_Dropdown.OptionData(bookGenres[i].name,null, bookGenres[i].color));
        }

        StartCoroutine(GameState.Instance.StartingGame());
    }

    void Update()
    {
        //print(Writer.IsWriting);
    }

    public void writerStart(TMPWriter theWriter)
    {
        AnimatingText = true;
        resetArrowPos();

    }

    public void writerDone(TMPWriter theWriter)
    {
        print("Writer done");
        StartArrowLoop();
        AnimatingText = false;
        Passage currentPassage = Passages.FirstOrDefault(p => p.name == currentPassageName);
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
            Writer.SkipWriter();
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

    public void DropDownValueChanged()
    {
        foundBookGenres.Clear();
        if (DropdownObj.transform.Find("Dropdown List"))
        {
            print("Dropdown List found");
            //loop thru children of list
            foreach (Transform child in DropdownObj.transform.Find("Dropdown List/Viewport/Content"))
            {
                // if child is "Viewport"
                // if toggle component is on and gameobject name contains a genre name, add to found genres list
                Toggle childToggle = child.GetComponent<Toggle>();
                if (childToggle.isOn)
                {
                    foreach (BookGenre genre in bookGenres)
                    {
                        if (child.name.ToLower().Contains(genre.name.ToLower()) && !foundBookGenres.Contains(genre))
                        {
                            foundBookGenres.Add(genre);
                            print("Added " + genre.name + " to found genres");

                            print("Current found genres: " + string.Join(", ", foundBookGenres.Select(g => g.name)));
                        }
                    }
                }
            }

            GameObject TemplateParent = bookTemplate.transform.parent.gameObject;


        } else
        {
            print("Dropdown List not found");
            return;
        }
    }

    private void DisplayPassage(Passage passage)
    {
        DialogText.text = ProcessText(passage.text);
        resetArrowPos();
        Writer.ResetWriter();
        Writer.StartWriter();
    }

    private String ProcessText(String text)
    {
        //replace all mentions of a genre with a random genre from the list of genres
        //this might ignore lower, should use index
        foreach (BookGenre genre in bookGenres)
        {
            if (text.ToLower().Contains(genre.name.ToLower()))
            {
                text = text.Replace(genre.name, genre.TextString);
            }
        }
        return text;
    }

    private void StartArrowLoop()
    {
        arrowTween = arrowRect
        .DOLocalMoveY(defaultArrowPos.y + 5f, 0.5f)
        .SetLoops(-1, LoopType.Yoyo);
    }

    private void resetArrowPos()
    {
        if (arrowTween != null && arrowTween.IsActive())
        {
            arrowTween.Kill();
        }
        DialogArrow.GetComponent<Image>().color = defaultArrowColor;
        DialogArrow.transform.localPosition = defaultArrowPos; // Reset position before starting animation
    }


}