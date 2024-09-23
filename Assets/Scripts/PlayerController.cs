using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayerController : MonoBehaviour
{
    public Animator animator;
    public AnimationClip idleClip;
    public AnimationClip walkClip;
    public AnimationClip runningClip;
    public AnimationSystem animationSystem;
    

    public float a;
    public float b;
    public float c;

    [Range(0, 1)]
    public float Weight;

    void Awake()
    {
        animator = GetComponent<Animator>();
        animationSystem = new AnimationSystem(animator, idleClip, walkClip, runningClip);
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
}


public class AnimationSystem
{
    private PlayableGraph graph;
    private AnimationMixerPlayable topLevelMixer;

    public AnimationSystem(Animator animator, AnimationClip idleClip, AnimationClip walkClip, AnimationClip runningClip)
    {
        graph = PlayableGraph.Create("Animation System");

        var animationOutput = AnimationPlayableOutput.Create(graph, "Animation System Output", animator);

        topLevelMixer = AnimationMixerPlayable.Create(graph, 3);

        var idleClipPlayable = AnimationClipPlayable.Create(graph, idleClip);
        var walkClipPlayable = AnimationClipPlayable.Create(graph, walkClip);
        var runningPlayable = AnimationClipPlayable.Create(graph, runningClip);

        topLevelMixer.ConnectInput(0, idleClipPlayable, 0);
        topLevelMixer.ConnectInput(1, walkClipPlayable, 0);
        topLevelMixer.ConnectInput(2, runningPlayable, 0);

        topLevelMixer.SetInputWeight(0, 1);
        topLevelMixer.SetInputWeight(1, 0);
        topLevelMixer.SetInputWeight(2, 0);

        animationOutput.SetSourcePlayable(topLevelMixer);

        graph.Play();
    }

    public void UpdateTopLevelMixer(float weight)
    {
        topLevelMixer.SetInputWeight(0, Math.Clamp(1 - weight * 2.0f, 0, 1));
        topLevelMixer.SetInputWeight(1, Math.Clamp(weight * 2.0f, 0, 1));
        topLevelMixer.SetInputWeight(1, Math.Clamp(weight * (int)Math.Floor(Math.Clamp(weight * 2.0f, 0, 1)) , 0, 1));
        
        //weight = .5f;
        //0........5........1
        //I        W        R
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

    public void Destroy()
    {
        if(graph.IsValid()) graph.Destroy();
    }
}