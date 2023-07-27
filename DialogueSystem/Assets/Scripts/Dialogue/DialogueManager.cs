using Ink.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    private enum DialogueTypeEnum
    {
        Classic,
        VisualNovel,
        SpeechBubbles,
    }

    public static DialogueManager instance;

    [Header("Portrait data")]
    [SerializeField] private SO_PortraitData PortraitData;

    [Header("Dialogue type")]
    [SerializeField] private DialogueTypeEnum DialogueType = DialogueTypeEnum.Classic;

    [Header("Classic dialogue")]
    [Header("Dialogue UI")]
    [SerializeField] private GameObject C_DialogueUI;
    [SerializeField] private TMP_Text C_DialogueTextBox;
    [SerializeField] private TextMeshProUGUI C_DialogueText;
    [SerializeField] private GameObject C_ContinueIcon;
    [SerializeField] private TextMeshProUGUI C_DisplayNameText;
    [SerializeField] private Image C_PortraitImage;
    [Header("Choices UI")]
    [SerializeField] private GameObject ChoiceUIPrefab;
    [SerializeField] private Transform ChoiceHolderParent;
    private List<GameObject> _currentChoices = new List<GameObject>();

    [Header("Visual novel dialogue")]
    [Header("Dialogue UI")]
    [SerializeField] private VisualNovelPortrait VN_VNPPrefab;
    [SerializeField] private Transform VN_PortraitParent;
    [SerializeField] private GameObject VN_DialogueUI;
    [SerializeField] private TMP_Text VN_DialogueTextBox;
    [SerializeField] private TextMeshProUGUI VN_DialogueText;
    [SerializeField] private GameObject VN_ContinueIcon;
    [SerializeField] private TextMeshProUGUI VN_DisplayNameText;
    [SerializeField] private List<VisualNovelPortrait> VN_Characters = new List<VisualNovelPortrait>();
    [SerializeField] private Transform SceneStart, SceneEnd;
    private VisualNovelPortrait _activeCharacter = null;
    private VisualNovelPortrait ActiveCharacter
    {
        get
        {
            return _activeCharacter;
        }
        set
        {
            if (_activeCharacter == value) return;

            foreach (VisualNovelPortrait Character in VN_Characters)
            {
                Character.Deactivate();
            }

            _activeCharacter = value;
            _activeCharacter.Activate();
        }
    }
    [Header("Choices UI")]
    [SerializeField] private GameObject VN_ChoiceUIPrefab;
    [SerializeField] private Transform VN_ChoiceHolderParent;

    [Header("Speech bubble dialogue")]
    [Header("Dialogue UI")]
    [Header("Choices UI")]

    [Header("Dialogue paramaters")]
    [SerializeField] private float ContinueDialogueInputGraceTime = .1f;
    private float _continueDialogueInputGraceTimer = 0f;

    private Story _currentStory;
    private Dialogue _currentDialogue;
    private DialogueVertexAnimator _dialogueVertexAnimator;
    public bool _dialogueIsPlaying { get; private set; }
    private Coroutine _typeRoutine;
    private bool _canContinueToNextLine = false;

    private SO_PortraitSet _currentCharacterPortraits = null;

    private Action _endOfLineEvent = null;

    //General ink tags.
    private const string SPEAKER_TAG = "speaker";
    private const string AKA_TAG = "aka";
    private const string EMOTION_TAG = "emotion";
    private const string EVENT_TAG = "event";

    //Classic-specific ink tags.
    private const string SIDE_TAG = "side";

    //VisualNovel-specific ink tags.
    private const string START_TAG = "start";
    private const string ACTIVE_TAG = "active";
    private const string ENTER_TAG = "enter";
    private const string EXIT_TAG = "exit";
    private const string MOVE_TAG = "move";
    private const string SHOW_TAG = "show";
    private const string FLIP_TAG = "flip";
    private const string PORTRAIT_ANIM_TAG = "panim";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("WARNING! Multiple instances of DialogueManager found.");
        }

        switch (DialogueType)
        {
            case DialogueTypeEnum.Classic:
                _dialogueVertexAnimator = new DialogueVertexAnimator(C_DialogueTextBox);
                break;
            case DialogueTypeEnum.VisualNovel:
                _dialogueVertexAnimator = new DialogueVertexAnimator(VN_DialogueTextBox);
                break;
            case DialogueTypeEnum.SpeechBubbles:
                break;
        }



        if (PortraitData == null)
        {
            Debug.Log("DialogueManager has no current PortraitData. Portraits will always be set to default sprite.");
        }
    }

    private void Start()
    {
        _dialogueIsPlaying = false;

        C_DialogueUI.SetActive(false);
        ChoiceHolderParent.gameObject.SetActive(false);
        C_ContinueIcon.SetActive(false);

        VN_DialogueUI.SetActive(false);
        VN_ContinueIcon.SetActive(false);
        VN_ChoiceHolderParent.gameObject.SetActive(false);
    }

    public void InitializeDialogue(Dialogue dialogue)
    {
        _currentDialogue = dialogue;
        _currentStory = new Story(_currentDialogue.inkJSON.text);
        _dialogueIsPlaying = true;

        switch (DialogueType)
        {
            case DialogueTypeEnum.Classic:
                C_DialogueUI.SetActive(true);

                C_DisplayNameText.text = "???";
                _currentCharacterPortraits = null;
                C_PortraitImage.sprite = null;
                break;
            case DialogueTypeEnum.VisualNovel:
                VN_DialogueUI.SetActive(true);
                VN_DisplayNameText.text = "???";
                _activeCharacter = null;
                foreach(VisualNovelPortrait Character in VN_Characters)
                {
                    Destroy(Character.gameObject);
                }
                VN_Characters.Clear();
                break;
            case DialogueTypeEnum.SpeechBubbles:
                break;
        }

        //Should have SetUp() function that deals with the initializing animation, and then ContinueDialogue() once said animation is done.
        ContinueDialogue();
    }

    public void ContinueDialogue()
    {
        if (_currentStory.canContinue)
        {
            PlayDialogue(_currentStory.Continue());
            //Handle tags
            HandleTags(_currentStory.currentTags);
        }
        else
        {
            ExitDialogue();
        }
    }

    private void HandleTags(List<string> currentTags)
    {
        foreach(string tag in currentTags)
        {
            string[] splitTag = tag.Split(':');
            if (splitTag.Length != 2)
            {
                Debug.LogError("Tag could not be appropriately parsed: " + tag);
            }

            string tagKey = splitTag[0].Trim();
            string tagValue = splitTag[1].Trim();


            switch (DialogueType)
            {
                case DialogueTypeEnum.Classic:
                    switch (tagKey)
                    {
                        case SPEAKER_TAG:
                            //Display character's name.
                            C_DisplayNameText.text = tagValue;

                            //Acquire that given character's portrait data.
                            if (PortraitData)
                            {
                                SO_PortraitSet characterPortraits = null;
                                foreach (SO_PortraitSet portraitSet in PortraitData.PortraitSets)
                                {
                                    if (tagValue == portraitSet.CharacterName) characterPortraits = portraitSet;
                                }

                                if (characterPortraits == null)
                                {
                                    //Establish default character portraitSet.
                                    characterPortraits = PortraitData.PortraitSets[0];
                                }
                                _currentCharacterPortraits = characterPortraits;
                                //By default, the character's portrait will be neutral.
                                C_PortraitImage.sprite = characterPortraits.Neutral;
                            }

                            //A special case should be InitializingCharacter - which links to the portraits of the character who started the dialogue, gotten as a reference when initializing the dialogue.
                            break;
                        case AKA_TAG:
                            C_DisplayNameText.text = tagValue;
                            //Use the above in order to make characters have a certain name, and not their 'real one', such as renaming George to The Forbidden One.
                            break;
                        case EMOTION_TAG:
                            //Will change the portrait to the current speaker's listed portrait based on the emotion.
                            if (PortraitData)
                            {
                                if (_currentCharacterPortraits == null)
                                {
                                    Debug.LogWarning("There is no current character assigned to this line of dialogue! Check the inkJSON file.");
                                }
                                else
                                {
                                    C_PortraitImage.sprite = _currentCharacterPortraits.GetPortraitFromTagValue(tagValue);
                                }
                            }
                            break;
                        case SIDE_TAG:
                            Debug.Log("side=" + tagValue);
                            //Will change the base layout of the dialogue.
                            //In a more visual-novel style dialogue, this wouldn't be necessary - the side on which a character appears would be determined at initialization of the dialogue.
                            break;
                        case EVENT_TAG:
                            int eventIndex = int.Parse(tagValue);
                            if (eventIndex + 1 > _currentDialogue.DialogueEvents.Count)
                            {
                                Debug.LogWarning("There is no event assigned for event " + eventIndex + " in the current dialogue.");
                            }
                            else
                            {
                                if (_currentDialogue.DialogueEvents[eventIndex].FireEventOnLineEnd)
                                {
                                    _endOfLineEvent = () =>
                                    {
                                        _currentDialogue.DialogueEvents[eventIndex].Event?.Invoke();
                                    };
                                }
                                else
                                {
                                    _currentDialogue.DialogueEvents[eventIndex].Event?.Invoke();
                                }
                            }
                            break;

                        default:
                            Debug.LogWarning("Tag came in but not currently handled: " + tag);
                            break;
                    }
                    break;
                case DialogueTypeEnum.VisualNovel:
                    switch (tagKey)
                    {
                        case START_TAG:
                            SO_PortraitSet characterPortraits = null;
                            foreach (SO_PortraitSet portraitSet in PortraitData.PortraitSets)
                            {
                                if (tagValue == portraitSet.CharacterName) characterPortraits = portraitSet;
                            }

                            if (characterPortraits == null)
                            {
                                //Establish default character portraitSet.
                                characterPortraits = PortraitData.PortraitSets[0];
                            }

                            ActiveCharacter = AddNewCharacter(characterPortraits);
                            ActiveCharacter.Show();
                            //ActiveCharacter.Deactivate();
                            break;
                        case SPEAKER_TAG:
                            VN_DisplayNameText.text = tagValue;
                            //Acquire that given character's portrait data.
                            if (PortraitData)
                            {
                                CheckNewCharacter(tagValue);
                            }
                            break;
                        case ACTIVE_TAG:
                            VisualNovelPortrait activatedCharacter = null;
                            foreach (VisualNovelPortrait characterInScene in VN_Characters)
                            {
                                if (tagValue == characterInScene.CharacterName)
                                {
                                    activatedCharacter = characterInScene;
                                }
                            }

                            if (activatedCharacter == null)
                            {
                                Debug.LogWarning("There is no: " + tagValue + " character in this scene yet. Did you forget to add them through the START or SPEAKER tags?");
                            }
                            ActiveCharacter = activatedCharacter;
                            break;
                        case AKA_TAG:
                            VN_DisplayNameText.text = tagValue;
                            break;
                        case EMOTION_TAG:
                            if (PortraitData)
                            {
                                if (ActiveCharacter == null)
                                {
                                    Debug.LogWarning("There is no active character yet! Check the inkJSON file.");
                                }
                                else
                                {
                                    ActiveCharacter.Emotion(tagValue);
                                }
                            }
                            break;
                        case EVENT_TAG:
                            int eventIndex = int.Parse(tagValue);
                            if (eventIndex + 1 > _currentDialogue.DialogueEvents.Count)
                            {
                                Debug.LogWarning("There is no event assigned for event " + eventIndex + " in the current dialogue.");
                            }
                            else
                            {
                                if (_currentDialogue.DialogueEvents[eventIndex].FireEventOnLineEnd)
                                {
                                    _endOfLineEvent = () =>
                                    {
                                        _currentDialogue.DialogueEvents[eventIndex].Event?.Invoke();
                                    };
                                }
                                else
                                {
                                    _currentDialogue.DialogueEvents[eventIndex].Event?.Invoke();
                                }
                            }
                            break;
                        case ENTER_TAG:
                            if (ActiveCharacter == null)
                            {
                                Debug.LogWarning("There is no active character set! Check the inkJSON file.");
                            }
                            else
                            {
                                if (tagValue == "right")
                                {
                                    ActiveCharacter.Enter(true);
                                }
                                else if (tagValue == "left")
                                {
                                    ActiveCharacter.Enter(false);
                                }
                                else
                                {
                                    Debug.LogWarning("tagValue: " + tagValue + " not recognized for ENTER.");
                                }                            
                            }
                            break;
                        case EXIT_TAG:
                            if (ActiveCharacter == null)
                            {
                                Debug.LogWarning("There is no active character set! Check the inkJSON file.");
                            }
                            else
                            {
                                if (tagValue == "right")
                                {
                                    ActiveCharacter.Exit(true);
                                }
                                else if (tagValue == "left")
                                {
                                    ActiveCharacter.Exit(false);
                                }
                                else
                                {
                                    Debug.LogWarning("tagValue: " + tagValue + " not recognized for EXIT.");
                                }
                            }
                            break;
                        case MOVE_TAG:
                            if (ActiveCharacter == null)
                            {
                                Debug.LogWarning("There is no active character set! Check the inkJSON file.");
                            }
                            else
                            {
                                float scenePosition = float.Parse(tagValue);
                                ActiveCharacter.MoveTo(scenePosition);
                            }
                            break;
                        case SHOW_TAG:
                            if (ActiveCharacter == null)
                            {
                                Debug.LogWarning("There is no active character set! Check the inkJSON file.");
                            }
                            else
                            {
                                if (tagValue == "show")
                                {
                                    ActiveCharacter.Show();
                                }
                                else if (tagValue == "hide")
                                {
                                    ActiveCharacter.Hide();
                                }
                                else
                                {
                                    Debug.LogWarning("tagValue: " + tagValue + " not recognized for SHOW.");
                                }                          
                            }
                            break;
                        case FLIP_TAG:
                            if (ActiveCharacter == null)
                            {
                                Debug.LogWarning("There is no active character set! Check the inkJSON file.");
                            }
                            else
                            {
                                if (tagValue == "right")
                                {
                                    ActiveCharacter.Flip(true);
                                }
                                else if (tagValue == "left")
                                {
                                    ActiveCharacter.Flip(false);
                                }
                                else
                                {
                                    Debug.LogWarning("tagValue: " + tagValue + " not recognized for FLIP.");
                                }
                            }
                            break;
                        case PORTRAIT_ANIM_TAG:
                            if (ActiveCharacter == null)
                            {
                                Debug.LogWarning("There is no active character set! Check the inkJSON file.");
                            }
                            else
                            {
                                ActiveCharacter.PlayAnimation(tagValue);
                            }
                            break;

                        default:
                            Debug.LogWarning("Tag came in but not currently handled: " + tag);
                            break;
                    }
                    break;
                case DialogueTypeEnum.SpeechBubbles:
                    break;
            }
        }
    }

    private void CheckNewCharacter(string characterName)
    {
        SO_PortraitSet characterPortraits = null;
        foreach (SO_PortraitSet portraitSet in PortraitData.PortraitSets)
        {
            if (characterName == portraitSet.CharacterName) characterPortraits = portraitSet;
        }

        if (characterPortraits == null)
        {
            //Establish default character portraitSet.
            characterPortraits = PortraitData.PortraitSets[0];
        }

        if (VN_Characters.Count == 0)
        {
            AddNewCharacter(characterPortraits);
        }
        else
        {
            bool isInList = false;
            VisualNovelPortrait character = null;
            foreach (VisualNovelPortrait VNPortrait in VN_Characters)
            {
                if (VNPortrait.CharacterName == characterPortraits.CharacterName)
                {
                    isInList = true;
                    character = VNPortrait;
                }
            }

            if (isInList)
            {
                //Do nothing, just change active speaker to this character.
                ActiveCharacter = character;
            }
            else
            {
                AddNewCharacter(characterPortraits);
            }
        }
    }

    private VisualNovelPortrait AddNewCharacter(SO_PortraitSet characterPortraits)
    {
        VisualNovelPortrait newCharacter = Instantiate<VisualNovelPortrait>(VN_VNPPrefab, VN_PortraitParent);
        newCharacter.InitializePortrait(characterPortraits);
        VN_Characters.Add(newCharacter);
        return newCharacter;
    } 

    private void OnLineEnd()
    {
        _canContinueToNextLine = true;

        switch (DialogueType)
        {
            case DialogueTypeEnum.Classic:
                C_ContinueIcon.SetActive(true);
                break;
            case DialogueTypeEnum.VisualNovel:
                VN_ContinueIcon.SetActive(true);
                break;
            case DialogueTypeEnum.SpeechBubbles:
                break;
        }

        if (_currentStory.currentChoices.Count > 0)
        {
            DisplayChoices();
        }

        if (_endOfLineEvent != null)
        {
            _endOfLineEvent();
            _endOfLineEvent = null;
        }
    }

    public void ExitDialogue()
    {
        _currentDialogue.EndDialogue();
        _currentDialogue = null;
        _dialogueIsPlaying = false;

        //Should have SetDown() function that deals with the exiting animation, and then calls the rest of this anim once said animation is done.

        switch (DialogueType)
        {
            case DialogueTypeEnum.Classic:
                C_DialogueUI.SetActive(false);
                C_DialogueText.text = "";
                break;
            case DialogueTypeEnum.VisualNovel:
                VN_DialogueUI.SetActive(false);
                VN_DialogueText.text = "";
                break;
            case DialogueTypeEnum.SpeechBubbles:
                break;
        }
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = _currentStory.currentChoices;

        switch (DialogueType)
        {
            case DialogueTypeEnum.Classic:
                ChoiceHolderParent.gameObject.SetActive(true);

                for (int i = 0; i < currentChoices.Count; i++)
                {
                    GameObject choiceObject = Instantiate<GameObject>(ChoiceUIPrefab, ChoiceHolderParent);
                    _currentChoices.Add(choiceObject);

                    Button choiceButton = choiceObject.GetComponent<Button>();
                    int tempInt = i;
                    choiceButton.onClick.AddListener(delegate { MakeChoice(tempInt); });

                    TextMeshProUGUI choiceText = choiceObject.GetComponentInChildren<TextMeshProUGUI>();
                    choiceText.text = currentChoices[i].text;
                }

                EventSystem.current.SetSelectedGameObject(_currentChoices[0]);
                break;
            case DialogueTypeEnum.VisualNovel:
                VN_ChoiceHolderParent.gameObject.SetActive(true);

                for (int i = 0; i < currentChoices.Count; i++)
                {
                    GameObject choiceObject = Instantiate<GameObject>(VN_ChoiceUIPrefab, VN_ChoiceHolderParent);
                    _currentChoices.Add(choiceObject);

                    Button choiceButton = choiceObject.GetComponent<Button>();
                    int tempInt = i;
                    choiceButton.onClick.AddListener(delegate { MakeChoice(tempInt); });

                    TextMeshProUGUI choiceText = choiceObject.GetComponentInChildren<TextMeshProUGUI>();
                    choiceText.text = currentChoices[i].text;
                }

                EventSystem.current.SetSelectedGameObject(_currentChoices[0]);
                break;
            case DialogueTypeEnum.SpeechBubbles:
                break;
        }
    }

    private void DestroyChoices()
    {
        for (int i = _currentChoices.Count-1; i >= 0; i--)
        {
            Button choiceButton = _currentChoices[i].GetComponent<Button>();

            int tempInt = i;
            choiceButton.onClick.RemoveListener(() => { MakeChoice(tempInt); });

            Destroy(_currentChoices[i]);
        }

        _currentChoices.Clear();

        switch (DialogueType)
        {
            case DialogueTypeEnum.Classic:
                ChoiceHolderParent.gameObject.SetActive(false);
                break;
            case DialogueTypeEnum.VisualNovel:
                VN_ChoiceHolderParent.gameObject.SetActive(false);
                break;
            case DialogueTypeEnum.SpeechBubbles:
                break;
        }

    }

    public void MakeChoice(int choiceIndex)
    {
        if (_canContinueToNextLine)
        {
            _continueDialogueInputGraceTimer = 0f;
            _currentStory.ChooseChoiceIndex(choiceIndex);
            DestroyChoices();
            ContinueDialogue();
        }

    }

    private IEnumerator SelectFirstChoice()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(_currentChoices[0]);
    }

    void PlayDialogue(string message)
    {
        this.EnsureCoroutineStopped(ref _typeRoutine);
        _dialogueVertexAnimator.textAnimating = false;

        _canContinueToNextLine = false;

        switch (DialogueType)
        {
            case DialogueTypeEnum.Classic:
                C_ContinueIcon.SetActive(false);
                break;
            case DialogueTypeEnum.VisualNovel:
                VN_ContinueIcon.SetActive(false);
                break;
            case DialogueTypeEnum.SpeechBubbles:
                break;
        }

        List<DialogueCommand> commands = DialogueUtility.ProcessInputString(message, out string totalTextMessage);
        _typeRoutine = StartCoroutine(_dialogueVertexAnimator.AnimateTextIn(commands, totalTextMessage, () =>
        {
            OnLineEnd();
        }));
    }

    private void ForceEndLine()
    {
        _dialogueVertexAnimator.SkipToEndOfCurrentMessage();

        switch (DialogueType)
        {
            case DialogueTypeEnum.Classic:
                C_DialogueTextBox.ForceMeshUpdate();
                break;
            case DialogueTypeEnum.VisualNovel:
                VN_DialogueTextBox.ForceMeshUpdate();
                break;
            case DialogueTypeEnum.SpeechBubbles:
                break;
        }
    }

    public Vector3 GetPositionFromInterpolationPoint(float interpolationPoint)
    {
        Vector3 sceneStartRectPos = SceneStart.GetComponent<RectTransform>().anchoredPosition;
        Vector3 sceneEndRectPos = SceneEnd.GetComponent<RectTransform>().anchoredPosition;


        Vector3 scenePosition = Vector3.Lerp(sceneStartRectPos, sceneEndRectPos, interpolationPoint);
        return scenePosition;
    }

    private void Update()
    {
        if (!_dialogueIsPlaying) return;
        else
        {
            if (_continueDialogueInputGraceTimer < ContinueDialogueInputGraceTime)
            {
                _continueDialogueInputGraceTimer += Time.deltaTime;
            }
            else
            {
                if (_typeRoutine != null
                    && _dialogueVertexAnimator.textAnimating
                    && Input.GetKeyDown(KeyCode.E))
                {
                    ForceEndLine();
                    _continueDialogueInputGraceTimer = 0f;
                }
                else if (_canContinueToNextLine
                    && _currentStory.currentChoices.Count == 0
                    && Input.GetKeyDown(KeyCode.E))
                {
                    _continueDialogueInputGraceTimer = 0f;
                    ContinueDialogue();
                    return;
                }
            }
        }
    }
}
