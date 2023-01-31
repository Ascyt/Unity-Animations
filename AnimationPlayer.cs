using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPlayer : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The step that is played at the start. Set to -1 to start at the beginning")]
    private int _step = -1;
    public int step 
    {
        get => _step;
        set 
        { 
            _step = Mathf.Clamp(value, -1, steps.Length - 1);
            Debug.Log($"Step {step}");
        }
    }

    [Tooltip("Specifies the speed the animations should be played at. 1 for normal speed, 0 to turn animations off.")]
    public float speed;

    [Space]

    [SerializeField]
    [Tooltip("Keys that play next anim without forcing the last one to be finished")]
    private KeyCode[] next;

    [SerializeField]
    [Tooltip("Keys that play next anim and force the last one to be finished")]
    private KeyCode[] forceNext;

    [SerializeField]
    [Tooltip("Keys that play previous anim without forcing the current one to be finished")]
    private KeyCode[] previous;

    [SerializeField]
    [Tooltip("Keys that play previous anim and force the current one to be finished")]
    private KeyCode[] forcePrevious;

    [Space]

    [SerializeField]
    [Tooltip("YouTube showcase coming soon!")]
    private Step[] steps;

    /// <summary>
    /// Set to true to stop all currently running animations.
    /// </summary>
    [HideInInspector]
    public bool stopAnim;

    private static List<Value> lastValues = new List<Value>();

    void Update()
    {
        stopAnim = false;

        if (AnyKeyDown(next))
            Next(false);

        if (AnyKeyDown(forceNext))
            Next(true);

        if (AnyKeyDown(previous))
            Previous(false);

        if (AnyKeyDown(forcePrevious))
            Previous(true);
    }

    /// <summary>
    /// Returns true if any of the keys are pressed.
    /// </summary>
    private static bool AnyKeyDown(KeyCode[] keys)
    {
        for (int i = 0; i < keys.Length; i++)
        {
            if (Input.GetKeyDown(keys[i]))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Plays the next animation.
    /// </summary>
    /// <param name="force">Should the last animation be forced to finish?</param>
    public void Next(bool force)
    {
        step++;

        stopAnim |= force;

        PlayStep(steps[step], speed);
    }

    /// <summary>
    /// Plays the previous animation.
    /// </summary>
    /// <param name="force">Should the current animation be forced to finish?</param>
    public void Previous(bool force)
    {
        stopAnim |= force;

        PlayStep(steps[step], -speed);

        step--;
    }

    /// <summary>
    /// Keyframes for the values.
    /// </summary>
    private class Values
    {
        public List<Vector3> translations = new List<Vector3>();
        public List<Vector3> scales = new List<Vector3>();
        public List<float> rotations = new List<float>();
        public List<Color> colors = new List<Color>();
        public List<float> animFloats = new List<float>();
    }
    private class Value
    {
        public Vector3 translation;
        public Vector3 scale;
        public float rotation;
        public Color color;
        public float animFloat;

        public Value(Values v)
        {
            translation = GetLast(v.translations);
            scale = GetLast(v.scales);
            rotation = GetLast(v.rotations);
            if (v.colors.Count > 0)
                color = GetLast(v.colors);
            if (v.animFloats.Count > 0)
                animFloat = GetLast(v.animFloats);
        }
    }

    /// <summary>
    /// Plays the specified step. Does not change "step" variable and 
    /// does not assume all steps before have executed properly.
    /// </summary>
    private void PlayStep(Step step, float speed)
    {
        if (speed >= 0)
        {
            foreach (Anim anim in step.anims)
            {
                StartCoroutine(PlayAnim(anim, speed));
            }
        }
        else 
        {
            float longestTime = 0;
            foreach (Anim anim in step.anims)
            {
                if (anim.delay + anim.time > longestTime)
                    longestTime = anim.delay + anim.time;
            }

            for (int i = 0; i < step.anims.Length; i++)
            {
                step.anims[i].delay = longestTime - (step.anims[i].delay + step.anims[i].time);

                StartCoroutine(PlayAnim(step.anims[i], speed));
            }
        }
    }

    private IEnumerator PlayAnim(Anim anim, float speed)
    {
        yield return null;

        Values values = new Values();

        if (speed >= 0)
        {
            values.translations.Add(anim.obj.transform.position);

            Camera camera;
            values.scales.Add(anim.obj.TryGetComponent(out camera) ? (Vector3)(Vector2.one * camera.orthographicSize) : anim.obj.transform.localScale);

            values.rotations.Add(anim.obj.transform.eulerAngles.z);
            if (anim.colors.Length > 0)
                values.colors.Add(anim.obj.GetComponent<SpriteRenderer>().color);

            lastValues.Insert(0, new Value(values));
        }
        else
        {
            values.translations.Add(lastValues[0].translation);
            values.scales.Add(lastValues[0].scale);
            values.rotations.Add(lastValues[0].rotation);
            if (lastValues[0].color != null)
                values.colors.Add(lastValues[0].color);
            values.animFloats.Add(lastValues[0].animFloat);

            lastValues.RemoveAt(0);
        }

        for (int i = 0; i < anim.translations.Length; i++)
        {
            Translation t = anim.translations[i];

            values.translations.Add(t.obj == null ?
            (t.relative ? anim.obj.transform.position + (Vector3)t.vector : ((Vector3)t.vector + new Vector3(0, 0, anim.obj.transform.position.z))) :
            (MultiplyVector((Vector3)t.vector + new Vector3(0, 0, 1), (Vector3)(Vector2)((t.obj.transform.localScale / 2f) + (anim.obj.transform.localScale / 2f * (t.relative ? -1 : 1)))
            + new Vector3(0, 0, anim.obj.transform.position.z)) + t.obj.transform.position));
        }
        AddToList(values.scales, ToVector3Array(anim.scales, anim.obj.transform.localScale.z));
        AddToList(values.rotations, anim.rotations);
        AddToList(values.colors, anim.colors);
        AddToList(values.animFloats, anim.animationFloatKeyframes);

        if (speed != 0 && anim.time > 0)
        {
            float timePassed = 0;

            while (!stopAnim)
            {
                if (timePassed > anim.delay + anim.time)
                    break;

                if (timePassed > anim.delay)
                {
                    float t = anim.easing.Get((timePassed - anim.delay) / anim.time);
                    ChangeObj((speed > 0) ? t : (1 - t));
                }

                timePassed += Time.deltaTime * Mathf.Abs(speed);

                yield return new WaitForEndOfFrame();
            }
        }

        ChangeObj(speed >= 0 ? 1 : 0);

        void ChangeObj(float t)
        {
            if (anim.translations.Length > 0)
                anim.obj.transform.position = LerpBetweenClosest(values.translations, t);

            if (anim.scales.Length > 0)
            {
                Camera camera;
                if (anim.obj.TryGetComponent(out camera))
                    camera.orthographicSize = LerpBetweenClosest(values.scales, t).x;
                else 
                    anim.obj.transform.localScale = LerpBetweenClosest(values.scales, t);
            }

            if (anim.rotations.Length > 0)
                anim.obj.transform.rotation = Quaternion.Euler(anim.obj.transform.eulerAngles.x, anim.obj.transform.eulerAngles.y, LerpBetweenClosest(values.rotations, t));

            if (anim.colors.Length > 0)
                anim.obj.GetComponent<SpriteRenderer>().color = LerpBetweenClosest(values.colors, t);

            if (anim.animationFloat != null)
                anim.animationFloat.value = LerpBetweenClosest(values.animFloats, t);
        }
    }

    /// <summary>
    /// Converts a Vector2[] to a Vector3[]
    /// </summary>
    private static Vector3[] ToVector3Array(Vector2[] v, float z)
    {
        Vector3[] result = new Vector3[v.Length];
        for (int i = 0; i < v.Length; i++)
            result[i] = new Vector3(v[i].x, v[i].y, z);
        return result;
    }

    /// <summary>
    /// Adds an array to a list
    /// </summary>
    private static void AddToList<T>(List<T> list, T[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            list.Add(array[i]);
        }
    }
    private static Vector3 MultiplyVector(Vector3 a, Vector3 b)
        => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    private static T GetLast<T>(List<T> list)
        => list[list.Count - 1];

    // I don't think there is an easier way to do this. Feel free to prove me wrong, though.
    // Ugly code ahead!
    private delegate T Multiplication<T>(T a, float b);
    private delegate T Addition<T>(T a, T b);
    private static Vector3 LerpBetweenClosest(List<Vector3> list, float t)
    {
        Vector3 Mult(Vector3 a, float b) => a * b;
        Vector3 Add(Vector3 a, Vector3 b) => a + b;
        Multiplication<Vector3> mult = Mult;
        Addition<Vector3> add = Add;
        return LerpBetweenClosest(list, t, mult, add);   
    }
    private static float LerpBetweenClosest(List<float> list, float t)
    {
        float Mult(float a, float b) => a * b;
        float Add(float a, float b) => a + b;
        Multiplication<float> mult = Mult;
        Addition<float> add = Add;
        return LerpBetweenClosest(list, t, mult, add);   
    }
    private static Color LerpBetweenClosest(List<Color> list, float t)
    {
        Color Mult(Color a, float b) => a * b;
        Color Add(Color a, Color b) => a + b;
        Multiplication<Color> mult = Mult;
        Addition<Color> add = Add;
        return LerpBetweenClosest(list, t, mult, add);   
    }

    /// <summary>
    /// Linear interpolation between the two closest values of the array.
    /// </summary>
    private static T LerpBetweenClosest<T>(List<T> list, float t, Multiplication<T> mult, Addition<T> add)
    {
        if (t <= 0)
            return list[0];
        if (t >= 1)
            return list[list.Count - 1];

        int from = (int)((list.Count - 1) * t);
        int to = from + 1;

        t %= 1f / (list.Count - 1);
        t *= list.Count - 1;

        return add(mult(list[from], 1 - t), mult(list[to], t));
    }
}
