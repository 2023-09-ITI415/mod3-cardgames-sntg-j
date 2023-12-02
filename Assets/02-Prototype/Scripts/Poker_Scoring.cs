using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Poker_Scoring : MonoBehaviour
{
    public static Poker_Scoring S; // The singleton for Scoreboard
    [Header("Set in Inspector")]
    public GameObject prefabFloatingScore_Poker;

    [Header("Set Dynamically")]
    [SerializeField] private int _score = 0;
    [SerializeField] private string _scoreString;

    private Transform canvasTrans;
    // The score property also sets the scoreString
    public int score
    {
        get
        {
            return (_score);
        }
        set
        {
            _score = value;
            scoreString = _score.ToString("N0");
        }
    }
    // The scoreString property also sets the Text.text
    public string scoreString
    {
        get
        {
            return (_scoreString);
        }
        set
        {
            _scoreString = value;
            GetComponent<TextMeshProUGUI>().text = _scoreString;
        }
    }
    void Awake()
    {
        if (S == null)
        {
            S = this; // Set the private singleton
        }
        else
        {
            Debug.LogError("ERROR: Scoring.Awake(): S is already set!");
        }
        canvasTrans = transform.parent;
    }
    // When called by SendMessage, this adds the fs.score to this.score
    public void PFSCallback(Poker_FloatingScore fs)
    {
        score += fs.score;
    }

    public Poker_FloatingScore CreatePoker_FloatingScore(int amt, List<Vector2> pts)
    {
        GameObject go = Instantiate<GameObject>(prefabFloatingScore_Poker);
        go.transform.SetParent(canvasTrans);
        Poker_FloatingScore fs = go.GetComponent<Poker_FloatingScore>();
        fs.score = amt;
        fs.reportFinishTo = this.gameObject; // Set fs to call back to this
        fs.Init(pts);
        return (fs);
    }
}
