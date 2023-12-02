using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ePScoreEvent
{
    //draw,
    rowComplete,
    gameWin,
}
public class Poker_ScoreManager : MonoBehaviour
{
    static private Poker_ScoreManager S;

    static public int SCORE_FROM_PREV_ROUND = 0;
    static public int HIGH_SCORE = 0;

    [Header("Set Dynamically")]
    public int chain = 0;
    public int scoreRun = 0;
    public int score = 0;

    void Awake()
    {
        if (S == null)
        {
            S = this; // Set the private singleton
        }
        else
        {
            Debug.LogError("ERROR: Poker_ScoreManager.Awake(): S is already set!");
        }

        if (PlayerPrefs.HasKey("PokerHighScore"))
        {
            HIGH_SCORE = PlayerPrefs.GetInt("PokerHighScore");
        }

        score += SCORE_FROM_PREV_ROUND;

        SCORE_FROM_PREV_ROUND = 0;
    }
    static public void EVENT(ePScoreEvent evt, string handType)
    {
        try
        {
            S.Event(evt, handType);
        }
        catch (System.NullReferenceException nre)
        {
            Debug.LogError("ScoreManager:EVENT() called while S=null.\n" + nre);
        }
    }

    void Event(ePScoreEvent evt, string handType)
    {
        switch (evt)
        {
            case ePScoreEvent.rowComplete: // Won the round
                switch (handType)
                {
                    case "Pair":
                        chain = 2;
                        break;
                    case "Two-Pair":
                        chain = 5;
                        break;
                    case "3-Kind":
                        chain = 10;
                        break;
                    case "Straight":
                        chain = 15;
                        break;
                    case "Flush":
                        chain = 20;
                        break;
                    case "Full-House":
                        chain = 25;
                        break;
                    case "4-Kind":
                        chain = 50;
                        break;
                    case "StraightFlush":
                        chain = 75;
                        break;
                    case "RoyalFlush":
                        chain = 100;
                        break;
                    default : break;
                }
                score += chain;
                break;
        }
        switch (evt)
        {
            case ePScoreEvent.gameWin:
                if (HIGH_SCORE <= score)
                {
                    print("You got the high score! High score: " + score);
                    HIGH_SCORE = score;
                    PlayerPrefs.SetInt("PokerHighScore", score);
                }
                else
                {
                    print("Your final score for the game was: " + score);
                }
                break;
        }

    }
    static public int CHAIN { get { return S.chain; } }
    static public int SCORE { get { return S.score; } }
    static public int SCORE_RUN { get { return S.scoreRun; } }
}
