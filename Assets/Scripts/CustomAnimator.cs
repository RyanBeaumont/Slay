using UnityEngine;
using System.Collections.Generic;

public class CustomAnimator : MonoBehaviour
{
    [System.Serializable]
    public class AnimationEntry
    {
        public string state;
        public AnimationClip clip;
    }
    public List<AnimationEntry> overrides = new List<AnimationEntry>();
    Animator animator;
    [Tooltip("Animator state to sample (must be looping).")]
    public string stateName = "Skeleton_Idle";
    [Tooltip("How many discrete poses the cycle has.")]
    public int frameCount = 4;
    [Tooltip("How many frames per second you want to show.")]
    public float displayFps = 3f;

    float timer, frameTime;
    public bool loop = true;
    int frame;
    public bool autoUpdate = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.updateMode  = AnimatorUpdateMode.Normal;
        animator.speed       = 0f; // we will drive time manually
        GotoFrame(0);
    }

    void Update()
{
    frameTime = 1f / Mathf.Max(0.01f, displayFps);

    if (autoUpdate)
    {
        timer += Time.deltaTime;
        while (timer >= frameTime)
        {
            if (loop)
            {
                frame = (frame + 1) % frameCount;
                GotoFrame(frame);
            }
            else
            {
                if (frame < frameCount - 1)
                {
                    frame++;
                    GotoFrame(frame);
                }
                else
                {
                    // Animation has reached the last frame, stop updating
                    autoUpdate = false;
                }
            }

            timer -= frameTime;
        }
    }
}

public void Play(string state, int frame, bool canLoop = true, float fps=4f, bool canAutoUpdate = true)
    {
        GetComponent<Animator>().enabled = true;
        AnimationEntry entry = overrides.Find(e => e.state == state && e.clip != null);

        if (entry != null) stateName = entry.clip.name;
        else stateName = state;
        autoUpdate = canAutoUpdate;
        loop = canLoop;
        displayFps = fps;

        timer = 0f;
        this.frame = frame;

        // refresh frameCount for new clip
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == stateName)
            {
                frameCount = Mathf.RoundToInt(clip.length * clip.frameRate);
                break;
            }
        }

        GotoFrame(frame);
    }

    void GotoFrame(int f)
    {
        // sample the state at an exact normalized frame boundary
        // tiny epsilon avoids precision landing exactly on 0 and retriggering
        float normalized = (f + 0.0001f) / frameCount;
        animator.Play(stateName, 0, normalized);
        animator.Update(0f); // apply immediately
    }
}
