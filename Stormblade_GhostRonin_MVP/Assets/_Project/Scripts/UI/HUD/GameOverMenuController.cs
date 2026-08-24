using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class GameOverMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private RectTransform selectorIcon;

    [Header("Selector Settings")]
    [SerializeField] private Vector2 selectorOffset = new Vector2(-40f, 0f);

    private void OnEnable()
    {
        StartCoroutine(SelectDefaultOption());
    }

    private IEnumerator SelectDefaultOption()
    {
        yield return null;

        if(continueButton == null)
            yield break;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(continueButton.gameObject);

        UpdateSelector(continueButton.transform as RectTransform);
    }

    public void SelectContinue()
    {
        SetSelectedButton(continueButton);
    }

    public void SelectExit()
    {
        SetSelectedButton(exitButton);
    }

    private void SetSelectedButton(Button button)
    {
        if(button == null)
            return;

        EventSystem.current.SetSelectedGameObject(button.gameObject);

        UpdateSelector(button.transform as RectTransform);
    }

    public void UpdateSelector(RectTransform target)
    {
        if(selectorIcon == null || target == null)
            return;

        selectorIcon.position = (Vector2)target.position + selectorOffset;
    }
}
