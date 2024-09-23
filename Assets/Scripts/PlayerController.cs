using System;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayerController : MonoBehaviour
{
    public Animator animator;
    public AnimationClip idleClip;
    public AnimationClip walkClip;
    public AnimationClip oneShotClip;
    public AnimationSystem animationSystem;

    [Range(0, 1)]
    public float Weight;

    void Awake()
    {
        animator = GetComponent<Animator>();
        animationSystem = new AnimationSystem(animator, idleClip, walkClip);
    }

    void Update()
    {
        animationSystem.UpdateTopLevelMixer(Weight);
    }

    private void OnDestroy()
    {
        animationSystem.Destroy();
    }

    [ContextMenu("Play One Shot Animaton")]
    public void PlayOneShot()
    {
        animationSystem.PlayOneShot(oneShotClip, 5.0f);
    }
}


public class AnimationSystem
{
    private PlayableGraph graph;
    private AnimationMixerPlayable topLevelMixer;
    private AnimationMixerPlayable locomotionMixer;

    public AnimationSystem(Animator animator, AnimationClip idleClip, AnimationClip walkClip)
    {
        graph = PlayableGraph.Create("Animation System");

        var animationOutput = AnimationPlayableOutput.Create(graph, "Animation System Output", animator);

        topLevelMixer = AnimationMixerPlayable.Create(graph, 2);
        locomotionMixer = AnimationMixerPlayable.Create(graph, 2);

        var idleClipPlayable = AnimationClipPlayable.Create(graph, idleClip);
        var walkClipPlayable = AnimationClipPlayable.Create(graph, walkClip);

        locomotionMixer.ConnectInput(0, idleClipPlayable, 0);
        locomotionMixer.ConnectInput(1, walkClipPlayable, 0);

        locomotionMixer.SetInputWeight(0, 1);
        locomotionMixer.SetInputWeight(1, 0);

        topLevelMixer.ConnectInput(0, locomotionMixer, 0);

        animationOutput.SetSourcePlayable(topLevelMixer);

        graph.Play();
    }

    public void UpdateTopLevelMixer(float weight)
    {
        locomotionMixer.SetInputWeight(0, 1 - weight);
        locomotionMixer.SetInputWeight(1, weight);
    }

    public void PlayOneShot(AnimationClip oneShotClip, float blendDuration)
    {
        var oneShotPlayable = AnimationClipPlayable.Create(graph, oneShotClip);

        topLevelMixer.ConnectInput(1, oneShotPlayable, 0);
        topLevelMixer.SetInputWeight(0, 0);
        topLevelMixer.SetInputWeight(1, 1);

        BlendIn(blendDuration);
        BlendOut(blendDuration, oneShotClip.length - blendDuration, () => topLevelMixer.DisconnectInput(1));
    }

    private void BlendOut(float blendDuration, float delay, Action finishedAction)
    {
        Timing.RunCoroutine(BlendCoroutine(blendDuration,
        blend => {
            topLevelMixer.SetInputWeight(0, blend);
            topLevelMixer.SetInputWeight(1, 1-blend);
        }, delay, finishedAction));
    }

    private void BlendIn(float blendDuration)
    {
        Timing.RunCoroutine(BlendCoroutine(blendDuration,
        blend => {
            topLevelMixer.SetInputWeight(0, 1-blend);
            topLevelMixer.SetInputWeight(1, blend);
        }));
    }

    IEnumerator<float> BlendCoroutine(float duration, Action<float> blendCallback, float delay = 0, Action finishedAction = null)
    {
        if(delay > 0) Timing.WaitForSeconds(delay);

        float step = 1 / duration;
        float normaliseDuration = 0f;

        while(normaliseDuration < 1)
        {
            Debug.Log("normaliseDuration " + normaliseDuration + " " + step);
            normaliseDuration += step * Time.deltaTime;
            blendCallback(normaliseDuration);
            yield return normaliseDuration;
        }

        if(finishedAction != null) finishedAction.Invoke();
    }

    public void Destroy()
    {
        if(graph.IsValid()) graph.Destroy();
    }
}