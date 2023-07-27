using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactible : MonoBehaviour
{
    public UnityEvent<InteractibleHandler> OnInteract;

    [SerializeField] private bool ShowPromptOnAvailableInteraction = true;
    [SerializeField] private GameObject InteractionPrompt;

    private List<InteractibleHandler> _handlers = new List<InteractibleHandler>();
    private bool _showingPrompt = false;

    private void Awake()
    {
        if (InteractionPrompt) InteractionPrompt.SetActive(false);
    }

    public virtual void Interact(InteractibleHandler handler)
    {
        //Base interact logic.
        OnInteract?.Invoke(handler);
        //handler.StartInteract();
        if (ShowPromptOnAvailableInteraction) CheckShowPrompt();
    }

    public virtual void StopInteract(InteractibleHandler handler)
    {
        //handler.EndInteract();
    }

    public void RegisterInteractible(InteractibleHandler handler)
    {
        if (!_handlers.Contains(handler))
        {
            _handlers.Add(handler);
        }

        if (handler.CurrentInteractible == null)
        {
            handler.CurrentInteractible = this;
        }

        if (ShowPromptOnAvailableInteraction) CheckShowPrompt();
    }

    public void UnregisterInteractible(InteractibleHandler handler)
    {
        if (!_handlers.Contains(handler)) return;
        else
        {
            if (handler.CurrentInteractible == this)
            {
                handler.CurrentInteractible = null;
            }

            _handlers.Remove(handler);
        }

        if (ShowPromptOnAvailableInteraction) CheckShowPrompt();
    }

    public void CheckShowPrompt()
    {
        if (_handlers.Count > 0)
        {
            bool showPrompt = false;
            foreach (InteractibleHandler interactibleHandler in _handlers)
            {
                if (interactibleHandler.CanInteract)
                {
                    showPrompt = true;
                }
            }
            ShowInteractionPrompt(showPrompt);
        }
        else
        {
            ShowInteractionPrompt(false);
        }
    }

    private void ShowInteractionPrompt(bool show)
    {
        if ((show == true && _showingPrompt) || show == false && !_showingPrompt) return;
        _showingPrompt = show;
        InteractionPrompt.SetActive(show);
    }

    //private void OnDisable()
    //{
    //    foreach(InteractibleHandler handler in _handlers)
    //    {
    //        UnregisterInteractible(handler);
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        InteractibleHandler handler = other.GetComponent<InteractibleHandler>();
        if (handler)
        {
            RegisterInteractible(handler);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        InteractibleHandler handler = other.GetComponent<InteractibleHandler>();
        if (handler)
        {
            UnregisterInteractible(handler);
        }
    }
}
