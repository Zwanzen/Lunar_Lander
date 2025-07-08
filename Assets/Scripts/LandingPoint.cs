using MoreMountains.Feedbacks;
using UnityEngine;

public class LandingPoint : MonoBehaviour
{
    [SerializeField] private MMF_Player StopDisplayFeedback;
    [SerializeField] private MMF_Player StartDisplayFeedback;
    [SerializeField] private MMF_Player FlagFeedback;

    // ___ PUBLIC METHODS ___
    public void SetVisualsState(bool state)
    {
        if (state)
        {
            StartDisplayFeedback?.PlayFeedbacks();
        }
        else
        {
            StopDisplayFeedback?.PlayFeedbacks();
        }
    }

    public void PlayFlagFeedback()
    {
        FlagFeedback?.PlayFeedbacks();
    }
}
