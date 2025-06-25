using UnityEngine;
public class CarEffects : MonoBehaviour
{
    private CarAgent input;
    public TrailRenderer[] tireMarks;
    private bool currentlyMarking;
    public AudioSource skidAudio;
    private void Start()
    {
        input = gameObject.GetComponent<CarAgent>();

    }

    private void Update()
    {
        isDrifting();
    }

    private void isDrifting()
    {
        if (input.isDrifting)
        {
            startEffect();
        }
        else
            stopEffect();
    }

    private void startEffect()
    {
        if (currentlyMarking) return;
        foreach (TrailRenderer i in tireMarks)
        {
            i.emitting = true;
        }
        skidAudio.Play();
        currentlyMarking = true;
    }

    private void stopEffect()
    {
        if (!currentlyMarking) return;
        foreach (TrailRenderer i in tireMarks)
        {
            i.emitting = false;
        }
        skidAudio.Stop();
        currentlyMarking = false;
    }

}
