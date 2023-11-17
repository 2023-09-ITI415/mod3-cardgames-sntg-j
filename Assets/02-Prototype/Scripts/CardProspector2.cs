using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proto;
public enum eCardState
{
    drawpile,
    tableau,
    target,
    discard
}
public class CardProspector2 : Card
{
    [Header("Set Dynamically: CardProspector")]

    public eCardState state = eCardState.drawpile;

    public List<CardProspector2> hiddenBy = new List<CardProspector2>();

    public int layoutID;

    public SlotDef slotDef;

    override public void OnMouseUpAsButton()
    {

        /*Prospector.S.CardClicked(this);*/

        base.OnMouseUpAsButton();
    }
}