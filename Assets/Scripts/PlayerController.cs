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
    

    public float a;
    public float b;
    public float c;

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
        
        a = Math.Clamp(1 - Weight * 2.0f, 0, 1);
        b = Math.Clamp(Weight * 2.0f, 0, 1);
        
        c = Math.Clamp(Weight * (int)Math.Floor(b) , 0, 1);
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
        //0........5........1
        //I        W        R
        locomotionMixer.SetInputWeight(0, 1 - weight);
        locomotionMixer.SetInputWeight(1, weight);
        weight = .5f;
        // 0. => 1 0 0

        // 0.125 => .75 .25 0
        // 0.25 => .5 .5 0
        // 0.375 => .25 .75 0

        // .5 => 0 1 0
        
        // .75 => 0 .5 .5

        // 1. => 0 0 1

        // topLevelMixer.SetInputWeight(0, 1-weight*2.0f);
        // topLevelMixer.SetInputWeight(1, weight*2.0f);
        // topLevelMixer.SetInputWeight(1, 0);
    }

    public void PlayOneShot(AnimationClip oneShotClip, float v)
    {
        var oneShotPlayable = AnimationClipPlayable.Create(graph, oneShotClip);

        topLevelMixer.ConnectInput(1, oneShotPlayable, 0);
        topLevelMixer.SetInputWeight(0, 0);
        topLevelMixer.SetInputWeight(1, 1);
        
        var blendDuration = 5.0f;
        BlendIn(blendDuration);
        BlendOut(blendDuration, oneShotClip.length - blendDuration);
    }

    private void BlendOut(float blendDuration, float delay)
    {
        Timing.RunCoroutine(BlendCoroutine(blendDuration,
        blend => {
            topLevelMixer.SetInputWeight(0, blend);
            topLevelMixer.SetInputWeight(1, 1-blend);
        }, delay));
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
            normaliseDuration *= step * Time.deltaTime;
            blendCallback(normaliseDuration);
            yield return normaliseDuration;
        }

        finishedAction.Invoke();
    }

    public void Destroy()
    {
        if(graph.IsValid()) graph.Destroy();
    }
}