using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/PortraitSet")]
public class SO_PortraitSet : ScriptableObject
{
    public string CharacterName = "";
    public Sprite Neutral;
    public Sprite Happy;
    public Sprite Fear;
    public Sprite Angry;

    public Sprite GetPortraitFromTagValue(string tag)
    {
        Sprite portrait = null;
        if (tag == "neutral")
        {
            portrait = Neutral;
        }
        else if (tag == "happy")
        {
            portrait = Happy;
        }
        else if (tag == "fear")
        {
            portrait = Fear;
        }
        else if (tag == "angry")
        {
            portrait = Angry;
        }
        else
        {
            Debug.LogWarning(CharacterName + " has no portrait for tag: " + tag);
        }
        return portrait;
    }
}
