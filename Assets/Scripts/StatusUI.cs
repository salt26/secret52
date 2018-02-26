using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusUI : MonoBehaviour {

    static public StatusUI statusUI;

    static public Text text;
    static private bool isHighlighting;

    private void Awake()
    {
        statusUI = this;
        text = GetComponent<Text>();
        isHighlighting = false;
        text.color = Color.white;
    }

    public void SetText(string t)
    {
        if (t.Length > 300) return;
        text.text = t;
    }

    public void ClearText()
    {
        text.text = "";
        PlainText();
    }

    public void HighlightText()
    {
        if (isHighlighting) return;
        isHighlighting = true;
        StartCoroutine("Highlight");
    }

    public void PlainText()
    {
        if (!isHighlighting) return;
        isHighlighting = false;
        StopCoroutine("Highlight");
        text.color = Color.white;
    }

    IEnumerator Highlight()
    {
        int frame = 16, frame2 = 24;
        while (true)
        {
            for (int i = 0; i < frame2; i++)
            {
                text.color = Color.Lerp(Color.red, new Color(1f, 1f, 0f, 1f), i / (float)frame2);
                yield return new WaitForFixedUpdate();
            }
            for (int i = 0; i < frame; i++)
            {
                text.color = Color.Lerp(new Color(1f, 1f, 0f, 1f), Color.green, i / (float)frame);
                yield return new WaitForFixedUpdate();
            }
            for (int i = 0; i < frame2; i++)
            {
                text.color = Color.Lerp(Color.green, Color.cyan, i / (float)frame2);
                yield return new WaitForFixedUpdate();
            }
            for (int i = 0; i < frame; i++)
            {
                text.color = Color.Lerp(Color.cyan, Color.blue, i / (float)frame);
                yield return new WaitForFixedUpdate();
            }
            for (int i = 0; i < frame2; i++)
            {
                text.color = Color.Lerp(Color.blue, Color.magenta, i / (float)frame2);
                yield return new WaitForFixedUpdate();
            }
            for (int i = 0; i < frame; i++)
            {
                text.color = Color.Lerp(Color.magenta, Color.red, i / (float)frame);
                yield return new WaitForFixedUpdate();
            }
        }
    }
}
