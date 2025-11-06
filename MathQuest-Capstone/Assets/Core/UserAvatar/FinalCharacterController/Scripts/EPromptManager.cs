using System.Collections;
using UnityEngine;

public class EPromptManager : MonoBehaviour
{
    public static EPromptManager Instance { get; private set; }

    [Header("Prefab & Sizing")]
    public GameObject promptPrefab;
    [Tooltip("Final world scale for the prompt root (on top of the 0.001 canvas scale).")]
    public float worldScale = 1f;

    [Header("Pop Animation")]
    public float popInDuration = 0.08f;
    public float popOutDuration = 0.06f;
    public float popOvershoot = 1.15f; // scale overshoot on pop-in

    GameObject _prompt;
    Transform _followAnchor;
    Coroutine _tween;
    bool _visible;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (promptPrefab)
        {
            _prompt = Instantiate(promptPrefab, transform); // Parent to this manager
            _prompt.SetActive(false);
            _prompt.transform.localScale = Vector3.zero; // Start at zero scale for animation
        }
    }

    void LateUpdate()
    {
        if (!_prompt || !_visible || !_followAnchor) return;
        // Follow anchor
        _prompt.transform.position = _followAnchor.position;
    }

    public void Show(Transform anchor)
    {
        if (!_prompt || anchor == null) return;
        _followAnchor = anchor;

        if (!_visible)
        {
            _visible = true;
            _prompt.SetActive(true);
            StartTween(true);
        }
    }

    public void Hide()
    {
        if (!_prompt || !_visible) return;
        _visible = false;
        StartTween(false);
    }

    void StartTween(bool show)
    {
        if (_tween != null) StopCoroutine(_tween);
        _tween = StartCoroutine(TweenScale(show));
    }

    IEnumerator TweenScale(bool show)
    {
        float t = 0f;
        float dur = show ? popInDuration : popOutDuration;

        Vector3 start, end;
        if (show)
        {
            start = Vector3.zero; // Start from zero
            end   = Vector3.one * worldScale;
            // Quick overshoot feel
            _prompt.transform.localScale = start;
        }
        else
        {
            start = _prompt.transform.localScale;
            end   = Vector3.zero; // End at zero
        }

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / dur);

            if (show)
            {
                // Ease out + overshoot
                float ease = 1f - Mathf.Pow(1f - u, 3f);
                float s = Mathf.Lerp(0f, 1f, ease);
                float over = Mathf.SmoothStep(1f, popOvershoot, Mathf.Clamp01(u * 1.2f));
                _prompt.transform.localScale = Vector3.LerpUnclamped(start, end * over, s);
            }
            else
            {
                // Ease in
                float ease = u * u;
                _prompt.transform.localScale = Vector3.Lerp(start, end, ease);
            }
            yield return null;
        }

        _prompt.transform.localScale = show ? Vector3.one * worldScale : Vector3.zero;
        if (!show) _prompt.SetActive(false);
        _tween = null;
    }
}
