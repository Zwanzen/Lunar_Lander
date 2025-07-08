using FMODUnity;
using MoreMountains.Feedbacks;
using Unity.VisualScripting;
using UnityEngine;

[AddComponentMenu("")]
[FeedbackHelp("Event Feedback")]
[FeedbackPath("FMOD")]
public class FmodEventFeedback : MMF_Feedbacks
{
    public EventReference EventName;
    public StudioEventEmitter Emitter;

    protected override void CustomInitialization(MMF_Player owner)
    {
        // We want to set the emitter's event to the EventReference if there is one.
        if (Emitter != null && !EventName.IsNull)
        {
            Emitter.EventReference = EventName;
        }
    }

    protected override void CustomPlayFeedback(Vector3 position, float intensity = 1.0f)
    {
        if (!EventName.IsNull && Emitter != null)
        {
            Emitter.Play();
        }
    }

    protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
    {
        if (!EventName.IsNull && Emitter != null)
        {
            Emitter.Stop();
        }
    }
}
