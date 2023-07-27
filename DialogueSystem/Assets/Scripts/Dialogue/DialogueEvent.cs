using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialogueEvent
{
    public UnityEvent Event;
    [Tooltip("By default, this event fires when the line starts. If set to true, it will only fire when the line ends.")]
    public bool FireEventOnLineEnd = false;
}
