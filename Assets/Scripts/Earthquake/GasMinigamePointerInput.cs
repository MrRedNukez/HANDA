
using UnityEngine;
using UnityEngine.EventSystems;

public class GasMinigamePointerInput : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    public enum InputTarget
    {
        Valve,
        Hose
    }

    [Header("Input Settings")]
    public GasTankMinigame minigame;
    public InputTarget inputTarget;

    private bool dragging;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (minigame == null)
            return;

        if (inputTarget == InputTarget.Valve)
        {
            dragging = minigame.BeginValveTurn(
                eventData.position);
        }
        else
        {
            dragging = minigame.BeginHosePull();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || minigame == null)
            return;

        if (inputTarget == InputTarget.Valve)
        {
            minigame.DragValve(eventData.position);
        }
        else
        {
            minigame.DragHose(eventData.delta.x);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        EndDrag();
    }

    private void EndDrag()
    {
        if (minigame != null && dragging)
        {
            if (inputTarget == InputTarget.Valve)
                minigame.EndValveTurn();
            else
                minigame.EndHosePull();
        }

        dragging = false;
    }

    private void OnDisable()
    {
        EndDrag();
    }
}
