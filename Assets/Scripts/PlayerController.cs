using System.Collections;
using System.Collections.Generic;
using GraphVisualizer;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayerController : MonoBehaviour
{
    public Animator animator;
    public AnimationClip idleClip;
    public AnimationClip walkClip;
    public AnimationClip runClip;
    public AnimationSystem animationSystem;

    [Range(0, 1)]
    public float Weight;

    void Awake()
    {
        animator = GetComponent<Animator>();
        animationSystem = new AnimationSystem(animator, idleClip, walkClip, runClip);
    }

    void Update()
    {
        animationSystem.UpdateTopLevelMixer(Weight);
    }
}


public class AnimationSystem
{
    private PlayableGraph graph;
    private AnimationMixerPlayable topLevelMixer;

    public AnimationSystem(Animator animator, AnimationClip idleClip, AnimationClip walkClip, AnimationClip runClip)
    {
        graph = PlayableGraph.Create("Animation System");

        var animationOutput = AnimationPlayableOutput.Create(graph, "Animation System Output", animator);

        topLevelMixer = AnimationMixerPlayable.Create(graph, 3);

        var idleClipPlayable = AnimationClipPlayable.Create(graph, idleClip);
        var walkClipPlayable = AnimationClipPlayable.Create(graph, walkClip);
        var runClipPlayable = AnimationClipPlayable.Create(graph, runClip);

        topLevelMixer.ConnectInput(0, idleClipPlayable,  0);
        topLevelMixer.ConnectInput(1, walkClipPlayable,  0);
        topLevelMixer.ConnectInput(2, runClipPlayable,  0);

        topLevelMixer.SetInputWeight(0, .5f);
        topLevelMixer.SetInputWeight(1, .5f);
        topLevelMixer.SetInputWeight(2, .5f);

        animationOutput.SetSourcePlayable(topLevelMixer);

        graph.Play();
    }

    public void UpdateTopLevelMixer(float weight)
    {
        topLevelMixer.SetInputWeight(0, 1 - weight / 3.0f);
        topLevelMixer.SetInputWeight(1, weight / 3.0f);
        topLevelMixer.SetInputWeight(2, 2.0f * weight / 3.0f);
    }

    public void Destroy()
    {
        if(graph.IsValid()) graph.Destroy();
    }
}