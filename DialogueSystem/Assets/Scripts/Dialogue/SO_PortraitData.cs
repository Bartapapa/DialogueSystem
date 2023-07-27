using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/PortraitData")]
public class SO_PortraitData : ScriptableObject
{
    public List<SO_PortraitSet> PortraitSets = new List<SO_PortraitSet>();
}
