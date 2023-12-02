using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proto;
public enum eCardStatus
{
    drawpile,
    tableau,
    target,
    discard
}
public class CardProspector2 : Card
{
    [Header("Set Dynamically: CardProspector2")]

    public eCardStatus state = eCardStatus.drawpile;

    public List<CardProspector2> hiddenBy = new List<CardProspector2>();

    public int layoutID;

    public CellDef cellDef;

    override public void OnMouseUpAsButton()
    {

        Poker.S.CardClicked(this);

        base.OnMouseUpAsButton();
    }
}