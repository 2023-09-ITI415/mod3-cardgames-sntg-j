using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Proto;
using System.Linq;
using System;
using System.Collections;
using UnityEngine.UIElements;

public class Poker : MonoBehaviour
{

    static public Poker S;
    private string[,] table;

    [Header("Set in Inspector")]
    public TextAsset deckXML;
    public TextAsset layoutXML;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Vector3 layoutCenter;
    public Vector2 fsPosMid = new Vector2(0.5f, 0.90f);
    public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);
    public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);
    public Vector2 fsPosEnd = new Vector2(0.5f, 0.95f);
    public float reloadDelay = 2f;
    public TextMeshProUGUI gameOverText, roundResultText, highScoreText;
    /*public CardProspector2 prefabholder;*/

    [Header("Set Dynamically")]
    public Deck_Proto deck;
    public Layout_Proto layout;
    public List<CardProspector2> drawPile;
    private List<CardProspector2> holderGrid;
    public Transform layoutAnchor;
    public CardProspector2 target;
    public List<CardProspector2> tableau;
    public List<CardProspector2> discardPile;
    public Poker_FloatingScore fsRun;



    void Awake()
    {
        S = this;
        SetUpUITexts();
    }

    void SetUpUITexts()
    {
        // Set up the HighScore UI Text
        GameObject go = GameObject.Find("HighScore");
        if (go != null)
        {
            highScoreText = go.GetComponent<TextMeshProUGUI>();
        }
        int highScore = ScoreManager.HIGH_SCORE;
        string hScore = "High Score: " + Utils.AddCommasToNumber(highScore);
        go.GetComponent<TextMeshProUGUI>().text = hScore;
        // Set up the UI Texts that show at the end of the round
        go = GameObject.Find("GameOver");
        if (go != null)
        {
            gameOverText = go.GetComponent<TextMeshProUGUI>();
        }
        go = GameObject.Find("RoundResult");
        if (go != null)
        {
            roundResultText = go.GetComponent<TextMeshProUGUI>();
        }
        // Make the end of round texts invisible
        ShowResultsUI(false);
    }
    void ShowResultsUI(bool show)
    {
        gameOverText.gameObject.SetActive(show);
        roundResultText.gameObject.SetActive(show);


    }

    public void TableSet()
    {
        for (int i = 0; i < table.GetLength(0); i++)
        {
            table[i, 0] = "0";

            table[0, i] = "0";

            if (i >= 1)
            {
                for (int j = 1; j < table.GetLength(1); j++)
                {
                    table[i, j] = "";
                }
            }
        }
    }

    public bool RowCheck(CardProspector2 nCard)
    {
        int index = tableau.IndexOf(nCard);

        bool fullRow = true, fullCol = true;
        int rowIndex = Mathf.FloorToInt(index / 5) + 1;
        int colIndex = (index % 5) + 1;
        /*Debug.Log(rowIndex.ToString() +" "+ colIndex.ToString());*/

        table.SetValue((nCard.suit + nCard.rank.ToString()), rowIndex, colIndex); // sets the values of the cards into the table array

        if (table[rowIndex, 0].Equals("0") || table[0, colIndex].Equals("0"))
        {
            for (int i = 1; i < table.GetLength(0); i++)
            {
                if (table[rowIndex, i].Equals("")) { fullRow = false; }

                if (table[i, colIndex].Equals("")) { fullCol = false; }
            }
            if (fullRow == true)
            {
                table[rowIndex, 0] = "1";
                HandCheck(!fullRow, rowIndex, colIndex);
            }

            if (fullCol == true)
            {
                table[0, colIndex] = "1";
                HandCheck(fullCol, rowIndex, colIndex);
            }
        }
            return (fullCol || fullRow);
    }

    void HandCheck(bool colCheck, int rowIndex, int colIndex)
    {
        string[] type = { "Pair",       "Two-Pair",      "3-Kind", 
                          "Full-House", "4-Kind",        "Straight", 
                          "Flush",      "StraightFlush", "RoyalFlush" };
        string result = "";

        bool royalStraight = false;

        bool isOnesuit = isAFlush(colCheck, rowIndex, colIndex);
        /*print("Is one suit: " + isOnesuit.ToString()); // In Working order*/
        bool isStraight = isAStraight(colCheck, rowIndex, colIndex, royalStraight);

        if (isOnesuit && isStraight)
        {
            if (royalStraight == true) { result = type[8]; }
            else { result = type[7]; }
        }
        else if (isOnesuit) { result = type[6]; }
        else if (isStraight) { result = type[5]; }
        else if (!(isOnesuit && isStraight)) { result = isAPair_Or_A_Kind(type, colCheck, rowIndex, colIndex); }
        else { result = "Nothing"; }

        Poker_ScoreManager.EVENT(ePScoreEvent.rowComplete, result);

    }
    private string isAPair_Or_A_Kind(string[] type, bool colCheck, int rowIndex, int colIndex)
    {
        string val = "", suit = "";
        string[] ranks = new string[5];
        Dictionary<string, int> hands = new Dictionary<string, int>();
        for (int i = 1; i < table.GetLength(0); i++)
        {
            if (colCheck == true) { val = table[i, colIndex]; } // Generalize the search, no matter the travesal through columns or rows. 
            else { val = table[rowIndex, i]; }

            if (val.Contains("D")) { suit = "D"; } // checks if the initial color of the hand is a diamond, heart, club, or spade 
            else if (val.Contains("H")) { suit = "H"; }
            else if (val.Contains("C")) { suit = "C"; }
            else { suit = "S"; }

            val = val.Replace(suit, "");
            ranks[i - 1] = val;
            if (hands.TryAdd(val, 1) == false) { hands[val] += 1; }  // tries to add the value to the dictionary and if the try fails, 
                                                                     // then the value exists within the dictionary and its value will be incremented by 1
        }

        foreach (string key in hands.Keys)
        {
            if (hands[key] == 2)
            {
                if (hands.Count == 3) { return type[1]; }
                else if (hands.Count == 2) { return type[3]; }
                else { return type[0]; }

            }
            else if (hands[key] == 3)
            {
                if (hands.Count == 2) { return type[3]; }
                if (hands.Count == 3) { return type[2]; }
            }
            else if (hands[key] == 4) { return type[4]; }
        }

        return "pair";
    }
    private bool isAFlush(bool colCheck, int rowIndex, int colIndex)
    {
        string val = "", suit = "", start = "";

        for (int i = 1; i < table.GetLength(0); i++)
        {
            start = table[rowIndex, colIndex]; // sets the final card placed

            if (colCheck == true) { val = table[i, colIndex]; } // Generalize the search, no matter the travesal through columns or rows. 
            else { val = table[rowIndex, i]; }

            if (start.Contains("D")) { suit = "D"; } // checks if the initial color of the hand is a diamond, heart, club, or spade 
            else if (start.Contains("H")) { suit = "H"; }
            else if (start.Contains("C")) { suit = "C"; }
            else { suit = "S"; }

            // Checks for the instances if the hand is not a flush (Flush: hand has only red or black cards)
            if ((val.Contains("S") && (suit != "S")) || (val.Contains("C") && (suit != "C")) ||
                (val.Contains("H") && (suit != "H")) || (val.Contains("D") && (suit != "D")))
            {
                return false;
            }
        }
        return true;
    }

    private bool isAStraight(bool colCheck, int rowIndex, int colIndex, bool royalStraight)
    {
        string val = "", suit = "";
        int[] ranks = new int[5];
        for (int i = 1; i < table.GetLength(0); i++)
        {
            if (colCheck == true) { val = table[i, colIndex]; } // Generalize the search, no matter the travesal through columns or rows. 
            else { val = table[rowIndex, i]; }

            if (val.Contains("D")) { suit = "D"; } // checks if the initial color of the hand is a diamond, heart, club, or spade 
            else if (val.Contains("H")) { suit = "H"; }
            else if (val.Contains("C")) { suit = "C"; }
            else { suit = "S"; }

            val = val.Replace(suit, "");
            ranks[i - 1] = int.Parse(val);
        }
        Array.Sort(ranks);

        /*Condition 1: if the value of the card adjacent is one value apart*/
        /*Condition 2: if the values ace and king are present in the hand while the every other value is one value apart*/
        if (ranks[0] == 1 && ranks[ranks.Length - 1] == 13)
        {
            for (int i = 2; i < ranks.Length; i++)
            {// Ace card only counts as both the lowest and highest card in each suit, A 2 J Q K is not a straight
                /*print(ranks[i - 1]);*/
                if (ranks[i - 1] + 1 != ranks[i]) { return false; }
            }
            royalStraight = true;
        }
        else
        {
            for (int i = 1; i < ranks.Length; i++)
            {
                if (ranks[i] != ranks[i - 1] + 1) { return false; }
            }
        }
        return true;
    }

    void Start()
    {
        /*Scoreboard.S.score = ScoreManager.SCORE;*/
        deck = GetComponent<Deck_Proto>();
        deck.InitDeck(deckXML.text);
        Deck_Proto.Shuffle(ref deck.cards);
        layout = GetComponent<Layout_Proto>();
        layout.ReadLayout(layoutXML.text);
        drawPile = ConvertListCardsToListCardProspector(deck.cards);
        holderGrid = ConvertListCardsToListCardProspector(deck.holders);
        table = new string[6, 6];
        TableSet();
        LayoutGame();
    }
    List<CardProspector2> ConvertListCardsToListCardProspector(List<Card> lCD)
    {
        List<CardProspector2> lCP = new List<CardProspector2>();
        CardProspector2 tCP;
        foreach (Card tCD in lCD)
        {
            tCP = tCD as CardProspector2;
            lCP.Add(tCP);
        }
        return lCP;
    }

    CardProspector2 Draw() // draws the cards from the deck
    {
        CardProspector2 cd = drawPile[0];
        drawPile.RemoveAt(0);
        cd.faceUp = true;
        return (cd);
    }
    CardProspector2 Set() // draws the placeholders from a separate deck 
    {
        CardProspector2 cd = holderGrid[0];
        holderGrid.RemoveAt(0);
        return (cd);
    }

    void LayoutGame()
    {
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

        CardProspector2 cp;

        foreach (CellDef tSD in layout.cellDefs)
        {/* for each of the slots in the layout */
            cp = Set(); /* call on the draw function to create a grid of place holders. */
            /*cp.faceUp = tSD.faceUp;*/
            cp.transform.parent = layoutAnchor;

            cp.transform.localPosition = new Vector3(
                        layout.multiplier.x * tSD.x,
                        layout.multiplier.y * tSD.y,
                        -tSD.layerID);

            cp.layoutID = tSD.id;
            cp.cellDef = tSD;

            cp.state = eCardStatus.tableau;
            cp.SetSortingLayerName(tSD.layerName);
            tableau.Add(cp);
        }

        MoveToTarget(Draw());
        UpdateDrawPile();
    }

    void MoveToDiscard(CardProspector2 cd)
    {
        cd.state = eCardStatus.discard;
        discardPile.Add(cd);
        cd.transform.parent = layoutAnchor;

        cd.transform.localPosition = new Vector3(
        layout.multiplier.x * layout.discardPile.x,
        layout.multiplier.y * layout.discardPile.y,
        -layout.discardPile.layerID + 0.5f);
        /*cd.faceUp = true;*/

        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    }

    void MoveToTarget(CardProspector2 cd)
    {
        // If there is currently a target card, move it to discardPile
        if (target != null) MoveToDiscard(target);
        target = cd; // cd is the new target
        cd.state = eCardStatus.target;
        cd.transform.parent = layoutAnchor;
        // Move to the target position
        cd.transform.localPosition = new Vector3(
        layout.multiplier.x * layout.discardPile.x,
        layout.multiplier.y * layout.discardPile.y,
        -layout.discardPile.layerID);

        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }
    // Arranges all the cards of the drawPile to show how many are left
    void UpdateDrawPile()
    {
        CardProspector2 cd;
        // Go through all the cards of the drawPile
        for (int i = 0; i < drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;
            // Position it correctly with the layout.drawPile.stagger
            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(
            layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
            layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
            -layout.drawPile.layerID + 0.1f * i);
            cd.faceUp = false; // Make them all face-down
            cd.state = eCardStatus.drawpile;
            // Set depth sorting
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    public void CardClicked(CardProspector2 cd)
    {
        // The reaction is determined by the state of the clicked card
        /*Debug.Log(cd.layoutID);*/
        switch (cd.state)
        {
            case eCardStatus.tableau:
                if (cd.rank.Equals((int)'h'))
                {   /*
                        The idea here is to first remove the clicked card from the tableau array
                        then update the drawpile to to show the next card while the position of the
                        leading card is placed on the clicked position.
                     */
                    int cdIndex = tableau.IndexOf(cd);
                    tableau.RemoveAt(cdIndex);
                    CardProspector2 nCard = target;
                    MoveToTarget(Draw());
                    nCard.transform.localPosition = cd.transform.localPosition;
                    MoveToDiscard(cd);
                    tableau.Insert(cdIndex, nCard);
                    holderGrid.Remove(cd);
                    bool check = RowCheck(nCard);
                    if (check) FloatingScoreHandler(ePScoreEvent.rowComplete);
                }
                break;
        }
        CheckForGameOver();
    }

    void CheckForGameOver()
    {
        if (drawPile.Count == 26) // considering that the target card is still part of the deck...
        {
            GameOver(true);
            return;
        }
        else
        {
            return;
        }
    }
    void GameOver(bool won)
    {
        int score = Poker_ScoreManager.SCORE;
        if (fsRun != null) score += fsRun.score;
        if (won)
        {
            gameOverText.text = "Game Over";
            if (Poker_ScoreManager.HIGH_SCORE <= score)
            {
                string str = "You got the high score!\nHigh score: " + score;
                roundResultText.text = str;
            }
            else
            {
                roundResultText.text = "Your final score was: " + score;
            }
            ShowResultsUI(true);

            Poker_ScoreManager.EVENT(ePScoreEvent.gameWin, "");
            FloatingScoreHandler(ePScoreEvent.gameWin);
        }

        Invoke("ReloadLevel", reloadDelay);
    }

    void ReloadLevel()
    {
        SceneManager.LoadScene("__Prototype");
    }
    public bool AdjacentRank(CardProspector2 c0, CardProspector2 c1)
    {

        if (!c0.faceUp || !c1.faceUp) return (false);

        if (Mathf.Abs(c0.rank - c1.rank) == 1)
        {
            return (true);
        }

        if (c0.rank == 1 && c1.rank == 13) return (true);
        if (c0.rank == 13 && c1.rank == 1) return (true);

        return (false);
    }
    void FloatingScoreHandler(ePScoreEvent evt)
    {
        List<Vector2> fsPts;
        switch (evt)
        {
            // Same things need to happen whether it's a draw, a win, or a loss
            case ePScoreEvent.gameWin:
                if (fsRun != null)
                {
                    // Create points for the Bézier curve1
                    fsPts = new List<Vector2>();
                    fsPts.Add(fsPosRun);
                    fsPts.Add(fsPosMid2);
                    fsPts.Add(fsPosEnd);
                    fsRun.reportFinishTo = Poker_Scoring.S.gameObject;
                    fsRun.Init(fsPts, 0, 1);
                    // Also adjust the fontSize
                    fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
                    fsRun = null; // Clear fsRun so it's created again
                }
                break;
            case ePScoreEvent.rowComplete:
                Poker_FloatingScore fs;
                // Move it from the mousePosition to fsPosRun
                Vector2 p0 = Input.mousePosition;
                p0.x /= Screen.width-100;
                p0.y /= Screen.height;
                fsPts = new List<Vector2>();
                fsPts.Add(p0);
                fsPts.Add(fsPosMid);
                fsPts.Add(fsPosRun);
                fs = Poker_Scoring.S.CreatePoker_FloatingScore(Poker_ScoreManager.CHAIN, fsPts);
                fs.fontSizes = new List<float>(new float[] { 8, 20, 200 });
                if (fsRun == null)
                {
                    // Create points for the Bézier curve1
                    fsRun = fs;
                    fsRun.reportFinishTo = null;
                }
                else
                {
                    fs.reportFinishTo = Poker_Scoring.S.gameObject; ;

                }
                break;
        }
    }
}