using UnityEngine;
using UnityEngine.EventSystems;

public class ColumnClick : MonoBehaviour, IPointerClickHandler
{
    public int columnIndex;
    public GameManager gameManager;

    public void OnPointerClick(PointerEventData eventData)
    {
        gameManager.PlayerMove(columnIndex);
        Debug.Log("Player moved");
    }
}
