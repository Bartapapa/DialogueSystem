using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualNovelPortrait : MonoBehaviour
{
    [Header("Character")]
    [SerializeField][ReadOnlyInspector] private SO_PortraitSet PortraitSet;
    public string CharacterName = "";

    [Header("Object references")]
    [SerializeField] private Animator Anim;
    [SerializeField] private Image PortraitImage;
    [SerializeField] private RectTransform RTransform;

    [Header("Highlights")]
    [SerializeField] private Color ActiveColor = Color.white;
    [SerializeField] private Color InactiveColor = Color.white;
    private Coroutine _currentHighlightCoroutine = null;
    private bool _highlighted = false;
    private Coroutine _currentHideCoroutine = null;
    private bool _hidden = false;

    [Header("Position")]
    [SerializeField] private float AnimateOverTime;
    private Coroutine _currentPositionCoroutine = null;
    private bool _facingRight = true;
    private Coroutine _currentFlipCoroutine = null;
    private Vector3 _destinationPoint = Vector3.zero;

    [Header("Animation curve")]
    [SerializeField] private AnimationCurve SCurve;

    public void InitializePortrait(SO_PortraitSet portraitSet)
    {
        PortraitSet = portraitSet;
        PortraitImage.sprite = PortraitSet.Neutral;
        CharacterName = portraitSet.CharacterName;
        ForceHide(true);
    }

    public void Emotion(string emotion)
    {
        PortraitImage.sprite = PortraitSet.GetPortraitFromTagValue(emotion);
    }
    public void Enter(bool fromRight)
    {
        //Move character portrait into scene from indicated side, and unhide.
        if (fromRight)
        {
            RTransform.anchoredPosition = DialogueManager.instance.GetPositionFromInterpolationPoint(1f);
            ForceFlip(false);
        }
        else
        {
            RTransform.anchoredPosition = DialogueManager.instance.GetPositionFromInterpolationPoint(0f);
            ForceFlip(true);
        }
        
        ForceHide(true);
        Show();
    }

    public void Exit(bool toRight)
    {
        //Move character portrait out of scene from indicated side, and hide.
        if (toRight)
        {
            MoveTo(1f);
        }
        else
        {
            MoveTo(0f);
        }
        ForceHide(false);
        Hide();
    }

    public void MoveTo(float scenePosition)
    {
        //Move character portrait from their current position to scenePosition.
        if (_currentPositionCoroutine != null)
        {
            StopCoroutine(_currentPositionCoroutine);
            RTransform.anchoredPosition = _destinationPoint;
        }
        _currentPositionCoroutine = StartCoroutine(AnimatePosition(scenePosition, AnimateOverTime));
    }

    public void Height(float newYPosition)
    {
        //Move character portrait from their current Yposition to newYPosition.
    }

    public void Deactivate()
    {
        if (!_highlighted) return;
        //Unhighlight character.
        if (_currentHighlightCoroutine != null)
        {
            StopCoroutine(_currentHighlightCoroutine);
        }
        _currentHighlightCoroutine = StartCoroutine(AnimateHighlight(ActiveColor, InactiveColor, AnimateOverTime));
        _highlighted = false;
    }

    public void Activate()
    {
        if (_highlighted) return;
        //Highlight character.
        if (_currentHighlightCoroutine != null)
        {
            StopCoroutine(_currentHighlightCoroutine);
        }
        _currentHighlightCoroutine = StartCoroutine(AnimateHighlight(InactiveColor, ActiveColor, AnimateOverTime));
        _highlighted = true;
    }

    public void Flip(bool toFaceRight)
    {
        //Flip character orientation, animating the process.
        if (_currentFlipCoroutine != null)
        {
            StopCoroutine(_currentFlipCoroutine);
        }

        if ((toFaceRight && _facingRight) || (!toFaceRight && !_facingRight)) return;

        _currentFlipCoroutine = StartCoroutine(AnimateFlip(toFaceRight, AnimateOverTime));

    }

    public void ForceFlip(bool toFaceRight)
    {
        //Flip character orientation, not animating the process.
        if (_currentFlipCoroutine != null)
        {
            StopCoroutine(_currentFlipCoroutine);
        }

        if (toFaceRight)
        {
            PortraitImage.transform.localScale = new Vector3(1f, PortraitImage.transform.localScale.y, PortraitImage.transform.localScale.z);
            _facingRight = true;
        }
        else
        {
            PortraitImage.transform.localScale = new Vector3(-1f, PortraitImage.transform.localScale.y, PortraitImage.transform.localScale.z);
            _facingRight = false;
        }
    }

    public void Hide()
    {
        if (_hidden) return;

        if (_currentHideCoroutine != null)
        {
            StopCoroutine(_currentHideCoroutine);
        }

        _currentHideCoroutine = StartCoroutine(AnimateAlpha(true, AnimateOverTime));
    }

    public void Show()
    {
        if (!_hidden) return;

        if (_currentHideCoroutine != null)
        {
            StopCoroutine(_currentHideCoroutine);
        }

        _currentHideCoroutine = StartCoroutine(AnimateAlpha(false, AnimateOverTime));
    }

    public void ForceHide(bool hide)
    {
        if (_currentHideCoroutine != null)
        {
            StopCoroutine(_currentHideCoroutine);
        }

        if (hide)
        {
            //Force hide
            PortraitImage.color = new Color(PortraitImage.color.r, PortraitImage.color.g, PortraitImage.color.b, 0f);
            _hidden = true;
        }
        else
        {
            //Force unhide
            PortraitImage.color = new Color(PortraitImage.color.r, PortraitImage.color.g, PortraitImage.color.b, 1f);
            _hidden = false;
        }
    }

    public void PlayAnimation(string animationName)
    {
        //Play Animator's assigned animation.
        Debug.Log(this.name + " played the " + animationName + " animation.");
    }

    private IEnumerator AnimateFlip(bool toFaceRight, float overTime)
    {
        float timer = 0;
        float duration = overTime;
        float from;
        float to;
        if (toFaceRight)
        {
            from = -1f;
            to = 1f;
        }
        else
        {
            from = 1f;
            to = -1f;
        }

        PortraitImage.transform.localScale = new Vector3(from, PortraitImage.transform.localScale.y, PortraitImage.transform.localScale.z);

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float interp = SCurve.Evaluate(timer / duration);
            float value = Mathf.Lerp(from, to, interp);
            PortraitImage.transform.localScale = new Vector3(value, PortraitImage.transform.localScale.y, PortraitImage.transform.localScale.z);

            yield return null;
        }

        PortraitImage.transform.localScale = new Vector3(to, PortraitImage.transform.localScale.y, PortraitImage.transform.localScale.z);
        _currentFlipCoroutine = null;
        _facingRight = toFaceRight;
    }

    private IEnumerator AnimateHighlight(Color from, Color to, float overTime)
    {
        float timer = 0;
        float duration = overTime;
        PortraitImage.color = from;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float interp = SCurve.Evaluate(timer / duration);
            Color value = Color.Lerp(from, to, interp);
            PortraitImage.color = new Color(value.r, value.g, value.b, PortraitImage.color.a);

            yield return null;
        }

        PortraitImage.color = new Color(to.r, to.g, to.b, PortraitImage.color.a);
        _currentHighlightCoroutine = null;
    }

    private IEnumerator AnimatePosition(float to, float overTime)
    {
        float timer = 0;
        float duration = overTime;

        Vector3 from = RTransform.anchoredPosition;

        Vector3 toPosition = DialogueManager.instance.GetPositionFromInterpolationPoint(to);
        _destinationPoint = toPosition;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float interp = SCurve.Evaluate(timer / duration);
            RTransform.anchoredPosition = Vector3.Lerp(from, toPosition, interp);

            yield return null;
        }

        RTransform.anchoredPosition = toPosition;
        _currentPositionCoroutine = null;
    }

    private IEnumerator AnimateAlpha(bool hide, float overTime)
    {
        float timer = 0;
        float duration = overTime;
        float from;
        float to;
        if (hide)
        {
            from = 1f;
            to = 0f;
        }
        else
        {
            from = 0f;
            to = 1f;
        }
        PortraitImage.color = new Color(PortraitImage.color.r, PortraitImage.color.g, PortraitImage.color.b, from);

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float interp = SCurve.Evaluate(timer / duration);
            float value = Mathf.Lerp(from, to, interp);
            PortraitImage.color = new Color(PortraitImage.color.r, PortraitImage.color.g, PortraitImage.color.b, value);

            yield return null;
        }

        PortraitImage.color = new Color(PortraitImage.color.r, PortraitImage.color.g, PortraitImage.color.b, to);
        _hidden = hide;
        _currentHideCoroutine = null;
    }
}
