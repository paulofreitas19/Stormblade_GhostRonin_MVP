using UnityEngine;
using UnityEngine.EventSystems;

public class GameOverMenuOption : MonoBehaviour, ISelectHandler
{
    [SerializeField] private GameOverMenuController menuController;

    public void OnSelect(BaseEventData eventData)
    {
        if(menuController == null)
            return;

        Debug.Log($"Opção selecionada: {gameObject.name}");

        menuController.UpdateSelector(transform as RectTransform);
    }
}
