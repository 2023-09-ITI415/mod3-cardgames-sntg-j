using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ePFSState
{
    idle,
    pre,
    active,
    post
}
public class Poker_FloatingScore : MonoBehaviour
{
    [Header("Set Dynamically")]
    public ePFSState state = ePFSState.idle;

    [SerializeField]
    protected int _score = 0;
    public string scoreString;

    public int score
    {
        get { return (_score); }
        set
        {
            _score = value;
            scoreString = _score.ToString("N0");

            GetComponent<TextMeshProUGUI>().text = scoreString;
        }
    }
    public List<Vector2> bezierPts;
    public List<float> fontSizes;
    public float timeStart = -1f;
    public float timeDuration = 1f;
    public string easingCurve = Easing.InOut;

    public GameObject reportFinishTo = null;
    private RectTransform rectTrans;
    private TextMeshProUGUI txt;

    public void Init(List<Vector2> ePts, float eTimeS = 0, float eTimeD = 1)
    {
        rectTrans = GetComponent<RectTransform>();
        rectTrans.anchoredPosition = Vector2.zero;
        txt = GetComponent<TextMeshProUGUI>();
        bezierPts = new List<Vector2>(ePts);
        if (ePts.Count == 1)
        {
            transform.position = ePts[0];
            return;
        }
        if (eTimeS == 0) eTimeS = Time.time;
        timeStart = eTimeS;
        timeDuration = eTimeD;
        state = ePFSState.pre;
    }
    public void PFSCallback(Poker_FloatingScore fs)
    {
        score += fs.score;
    }

    void Update()
    {
        if (state == ePFSState.idle) return;

        float u = (Time.time - timeStart) / timeDuration;

        float uC = Easing.Ease(u, easingCurve);
        if (u < 0)
        {
            state = ePFSState.pre;
            txt.enabled = false;
        }
        else
        {
            if (u >= 1)
            {
                uC = 1;
                state = ePFSState.post;
                if (reportFinishTo != null)
                {
                    reportFinishTo.SendMessage("PFSCallback", this);
                    Destroy(gameObject);
                }
                else
                {
                    state = ePFSState.idle;
                }
            }
            else
            {
                state = ePFSState.active;
                txt.enabled = true; // Show the score once more
            }

            Vector2 pos = Utils.Bezier(uC, bezierPts);
            rectTrans.anchorMin = rectTrans.anchorMax = pos;
            if (fontSizes != null && fontSizes.Count > 0)
            {
                int size = Mathf.RoundToInt(Utils.Bezier(uC, fontSizes));
                GetComponent<TextMeshProUGUI>().fontSize = size;
            }
        }
    }
}