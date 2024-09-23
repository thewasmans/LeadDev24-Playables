using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayerController : MonoBehaviour
{
    public Animator animator;
    public AnimationClip idleClip;
    public AnimationClip walkClip;
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

    public void Destroy()
    {
        if(graph.IsValid()) graph.Destroy();
    }
}