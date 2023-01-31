using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Step
{
    [Tooltip("The animations that get played without requiring another keypress")]
    public Anim[] anims;
}
[System.Serializable]
public struct Anim 
{
    [Tooltip("Duration of the animation in seconds")]
    public float time;
    [Tooltip("Animation will start after [delay] seconds")]
    public float delay;
    [Tooltip("The object that the animation will be played on")]
    public GameObject obj;
    [Tooltip("Specifies the easing. \"None\" with \"In\" to always return 1.")]
    public Easing easing;

    [Space]

    [Tooltip("Specifies how the object should be moved")]
    public Translation[] translations;
    [Tooltip("Changes the size (localScale) of the object.\n" +
        "For Camera: Sets orthographicSize to x.")]
    public Vector2[] scales;
    [Tooltip("Changes the z rotation of the object")]
    public float[] rotations;
    [Tooltip("If length > 0: Gets SpriteRenderer and changes its color")]
    public Color[] colors;
    [Tooltip("If not null: Gets SpriteRenderer and sets its sprite")]
    public Sprite sprite;

    [Space]

    [Tooltip("If you want, you can add an AnimationFloat class and its float \"value\" will be modified")]
    public AnimationFloat animationFloat;
    [Tooltip("The float \"value\" of animationFloat will be changed by these keyframes")]
    public float[] animationFloatKeyframes;
}
[System.Serializable]
public struct Translation
{
    public Vector2 vector;
    [Tooltip("Set to null to use world space, otherwise it's related to this object")]
    public GameObject obj;
    [Tooltip("If obj is null: Changes the translation by [vector], otherwise sets it to [vector]\n" +
        "If obj is set: Moves object to obj's relative [vector] position times outer bounds, otherwise inner bounds")]
    public bool relative;
}