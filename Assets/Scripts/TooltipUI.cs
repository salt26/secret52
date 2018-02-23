using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipUI : MonoBehaviour {

    public Image background;
    private Text text;
    public Image border;
    private RectTransform rect;

    private void Awake()
    {
        //background = GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0f);
        text = GetComponentInChildren<Text>();
        text.text = "";
        text.color = new Color(1f, 1f, 1f, 0f);
        //border = GetComponentInChildren<Image>();
        border.color = new Color(1f, 0.643f, 0f, 0f);
        rect = GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.01f, 0.32f);
        rect.anchorMax = new Vector2(0.99f, 0.44f);
    }

    public void SetText(string body)
    {
        text.color = new Color(1f, 1f, 1f, 0f);
        text.text = body;
    }

    public void SetText(string head, Color headColor, string body)
    {
        text.color = new Color(headColor.r, headColor.g, headColor.b, 0f);
        text.text = head;
        text.text += "\n<color=#ffffff>" + body + "</color>";
    }

    public string GetText()
    {
        return text.text;
    }

    public void ClearText()
    {
        text.color = new Color(1f, 1f, 1f, 0f);
        text.text = "";
    }

    public void SetPosition(float left, float down, float right, float up)
    {
        rect.anchorMin = new Vector2(left, down);
        rect.anchorMax = new Vector2(right, up);
    }

    /// <summary>
    /// 툴팁이 서서히 나타나게 하는 함수입니다.
    /// 오브젝트 생성 후에, 툴팁이 보여지려면 이 함수를 호출해야 합니다.
    /// </summary>
    public void Appear()
    {
        StartCoroutine("FadeIn");
    }

    /// <summary>
    /// 툴팁이 희미해지다가 제거되도록 하는 함수입니다.
    /// </summary>
    public void Disappear()
    {
        StopCoroutine("FadeIn");
        StartCoroutine("FadeOut");
    }

    IEnumerator FadeIn()
    {
        int frame = 32;
        for (int i = 0; i < frame; i++)
        {
            background.color = Color.Lerp(new Color(0f, 0f, 0f, 0f), new Color(0f, 0f, 0f, 0.75f), i / (float)frame);
            text.color = Color.Lerp(new Color(text.color.r, text.color.g, text.color.b, 0f),
                new Color(text.color.r, text.color.g, text.color.b, 1f), i / (float)frame);
            border.color = Color.Lerp(new Color(1f, 0.643f, 0f, 0f), new Color(1f, 0.643f, 0f, 1f), i / (float)frame);
            yield return new WaitForFixedUpdate();
        }
        background.color = new Color(0f, 0f, 0f, 0.75f);
        text.color = new Color(text.color.r, text.color.g, text.color.b, 1f);
        border.color = new Color(1f, 0.643f, 0f, 1f);
    }

    IEnumerator FadeOut()
    {
        int frame = 16;
        Color bgc = background.color;
        Color tc = text.color;
        Color bc = border.color;
        for (int i = 0; i < frame; i++)
        {
            background.color = Color.Lerp(bgc, new Color(0f, 0f, 0f, 0f), i / (float)frame);
            text.color = Color.Lerp(tc,
                new Color(tc.r, tc.g, tc.b, 0f), i / (float)frame);
            border.color = Color.Lerp(bc, new Color(1f, 0.643f, 0f, 0f), i / (float)frame);
            yield return new WaitForFixedUpdate();
        }
        Destroy(gameObject);
    }
}
