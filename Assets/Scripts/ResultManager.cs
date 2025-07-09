using MoreMountains.Feedbacks;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResultManager : MonoBehaviour
{
    // ___ PRIVATE FIELDS ___
    [Header("Refrences")]
    [SerializeField] private MMF_Player CompletedFeedback;
    [SerializeField] private MMF_Player FailedFeedback;
    [SerializeField] private MMF_Player SequenceOverFeedback;
    [SerializeField] private GameObject SequenceOverButton; // Button to enable after the sequence is over
    [Space(2)]
    [SerializeField] private GameObject[] starImages; // 3 stars
    [SerializeField] private GameObject[] conditions; // 3 conditions w text
    [Space(2)]

    private bool[] results = new bool[3];


    private const float Delay = 0.4f;

    /* ___ NOTE ___
    * Completed Feedback sequence:
    * MMF_SetActive,
    * MMF_SquashAndStretch
    * MMF_SquashAndStretch
    * MMF_TMPColor
    * 
    * Failed Feedback sequence:
    * MMF_SquashAndStretch,
    * MMF_TMPColor
    */

    // ___ PRIVATE METHODS ___
    private IEnumerator CompleteStar(int index, float delay)
    {
        // Wait for the specified delay before feedback
        yield return new WaitForSeconds(delay);

        // Find out if the star should be completed or not
        bool isCompleted = results[index];

        if (isCompleted)
        {
            // Activate Star
            CompletedFeedback.GetFeedbackOfType<MMF_SetActive>().TargetGameObject = starImages[index];

            // Squash
            var squash = CompletedFeedback.GetFeedbacksOfType<MMF_SquashAndStretch>();
            squash[0].SquashAndStretchTarget = starImages[index].transform;
            squash[1].SquashAndStretchTarget = conditions[index].transform;

            // Text Color
            CompletedFeedback.GetFeedbackOfType<MMF_TMPColor>().TargetTMPText = conditions[index].GetComponent<TMP_Text>();

            // Init and Play
            CompletedFeedback.Initialization();
            CompletedFeedback.PlayFeedbacks();
        }
        else
        {
            FailedFeedback.GetFeedbackOfType<MMF_SquashAndStretch>().SquashAndStretchTarget = conditions[index].transform;
            FailedFeedback.GetFeedbackOfType<MMF_TMPColor>().TargetTMPText = conditions[index].GetComponent<TMP_Text>();
            FailedFeedback.Initialization();
            FailedFeedback.PlayFeedbacks();
        }

    }

    private IEnumerator SequenceOver(float delay)
    {
        // Plays a final Feedback with sound and visual effects, 
        // After a small delay, buttons will be enabled again
        yield return new WaitForSeconds(delay);
        // Play the sequence over feedback
        SequenceOverFeedback.PlayFeedbacks();
        // Enable the button to continue
        EventSystem.current.SetSelectedGameObject(SequenceOverButton);
    }

    // ___ PUBLIC METHODS ___
    public void SetStars(bool first, bool second, bool third)
    {
        results[0] = first;
        results[1] = second;
        results[2] = third;
    }

    public void StartSequence()
    {
        StartCoroutine(CompleteStar(0, 0f));
        StartCoroutine(CompleteStar(1, Delay));
        StartCoroutine(CompleteStar(2, Delay*2));
        StartCoroutine(SequenceOver(Delay*3));
    }
}
