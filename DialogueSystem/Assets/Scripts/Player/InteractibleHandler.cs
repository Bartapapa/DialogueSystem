using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractibleHandler : MonoBehaviour
{
    [Header("Interactible")]
    [ReadOnlyInspector] public Interactible CurrentInteractible;
    private bool _canInteract = true;
    public bool CanInteract
    {
        get
        {
            return _canInteract;
        }
        set
        {
            _canInteract = value;
            if (CurrentInteractible)
            {
                CurrentInteractible.CheckShowPrompt();
            }
        }
    }

    public void InteractWithCurrentInteractible()
    {
        if (CurrentInteractible)
        {
            if (_canInteract)
            {
                CurrentInteractible.Interact(this);
                Debug.Log(this.name + " interacted with " + CurrentInteractible.name + "!");
            }
            else
            {
                Debug.Log("Can't interact right now!");
            }
        }
    }

    public void StartInteract()
    {
        CanInteract = false;
    }

    public void EndInteract()
    {
        CanInteract = true;
    }
}
