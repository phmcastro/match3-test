using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GemController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private TheGrid myGrid;
    private Gem myGem;

    // loaded when creating new grid
    public void LoadGemController(TheGrid grid, Gem myGemInfo)
    {
        myGrid = grid;
        myGem = myGemInfo;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        myGrid.GotClicked(myGem.myRow, myGem.myColumn);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
    }

}
