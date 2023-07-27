using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterTest : MonoBehaviour
{
    public Color CharacterColor = Color.white;
    public Renderer Renderer;

    private void Start()
    {
        Renderer.material.color = CharacterColor;
    }
}
