using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSelector : MonoBehaviour
{

    [SerializeField] private GameObject MenuButton;
    [SerializeField] private GameObject LevelHolder;

    public void SelectLevelButton()
    {

    }

    private IEnumerator DelayedSelectLevelButton()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        // Get first child of level holder
        GameObject firstChild = LevelHolder.transform.GetChild(0).gameObject;
        EventSystem.current.SetSelectedGameObject(firstChild.GetComponentInChildren<Button>().gameObject);
    }

}
