using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSelector : MonoBehaviour
{

    [SerializeField] private GameObject MenuButton;
    [SerializeField] private GameObject LevelHolder;

    private void Start()
    {
        SelectMenuButton();
    }

    public void SelectLevelButton()
    {
        StartCoroutine(DelayedSelectLevelButton());
    }

    public void SelectMenuButton()
    {
        StartCoroutine(DelaySelectMenuButton());
    }

    private IEnumerator DelayedSelectLevelButton()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        // Get first child of level holder
        GameObject firstChild = LevelHolder.transform.GetChild(0).gameObject;
        EventSystem.current.SetSelectedGameObject(firstChild.GetComponentInChildren<Button>().gameObject);
    }

    private IEnumerator DelaySelectMenuButton()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        EventSystem.current.SetSelectedGameObject(MenuButton);

    }
}
