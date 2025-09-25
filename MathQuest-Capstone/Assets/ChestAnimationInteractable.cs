using UnityEngine;

[RequireComponent(typeof(Animation)), RequireComponent(typeof(Collider))]
public class ChestAnimationInteractable : MonoBehaviour, IInteractable
{
    [Header("Animation (Legacy)")]
    [SerializeField] Animation anim;
    [SerializeField] AnimationClip openClip;
    [SerializeField] AnimationClip closeClip;

    [Header("Behavior")]
    [SerializeField] bool toggle = true;
    [SerializeField] bool onlyOnce = false;
    [SerializeField] float cooldown = 0.25f;
    [SerializeField] float crossFade = 0.1f;

    bool opened;
    float lastUse;

    void Awake()
    {
        if (!anim) anim = GetComponent<Animation>();
        anim.playAutomatically = false;
        anim.enabled = true;

        AddClipIfNeeded(openClip);
        AddClipIfNeeded(closeClip);

        SetWrap(openClip);
        SetWrap(closeClip);
    }

    void AddClipIfNeeded(AnimationClip clip)
    {
        if (clip && anim.GetClip(clip.name) == null)
            anim.AddClip(clip, clip.name);
    }

    void SetWrap(AnimationClip clip)
    {
        if (clip) anim[clip.name].wrapMode = WrapMode.ClampForever;
    }

    public void Interact()
    {
        if (Time.time < lastUse + cooldown) return;
        lastUse = Time.time;

        if (onlyOnce && opened) return;
        if (!openClip) return;

        if (toggle && opened && closeClip)
        {
            Play(closeClip.name);
            opened = false;
        }
        else
        {
            Play(openClip.name);
            opened = true;
        }
    }

    void Play(string name)
    {
        if (anim.GetClip(name) == null) return;
        SetWrap(openClip);
        SetWrap(closeClip);
        if (crossFade > 0f && anim.isPlaying) anim.CrossFade(name, crossFade);
        else anim.Play(name);
    }
}
