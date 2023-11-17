using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proto;

    public enum eCardState2
    {
        drawpile,
        tableau,
        target,
        discard
    }
    public class CardProspector : Card
    {
        [Header("Set Dynamically: CardProspector")]

        public eCardState state = eCardState.drawpile;

        public List<CardProspector> hiddenBy = new List<CardProspector>();

        public int layoutID;

        public SlotDef slotDef;
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        override public void OnMouseUpAsButton()
        {

            Prototype.S.CardClicked(this);

            base.OnMouseUpAsButton();
        }
    }