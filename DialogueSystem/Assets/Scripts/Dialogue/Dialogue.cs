using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Dialogue : MonoBehaviour, I_Interactor
{
    [Header("Dialogue")]
    public TextAsset inkJSON;

    public UnityEvent OnDialogueStart;
    public UnityEvent OnDialogueEnd;
    [Tooltip("To use these events, write #event:X in the ink code, where X is the index of the DialogueEvent in the following list.")]
    public List<DialogueEvent> DialogueEvents = new List<DialogueEvent>();

    [SerializeField] [ReadOnlyInspector] private InteractibleHandler _currentHandler;


    #region I_INTERACTOR
    public void OnInteract(InteractibleHandler handler)
    {
        _currentHandler = handler;
        _currentHandler.CanInteract = false;
        StartDialogue();       
    }
    #endregion

    public void StartDialogue()
    {
        DialogueManager.instance.InitializeDialogue(this);
        OnDialogueStart?.Invoke();
    }

    public void EndDialogue()
    {
        OnDialogueEnd?.Invoke();
        _currentHandler.CanInteract = true;
        _currentHandler = null;
    }
}
