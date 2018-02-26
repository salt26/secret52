﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Alert : MonoBehaviour {

    static public Alert alert;

    public RectTransform alertPanel;
    public Image image;

    public Sprite myTurnSprite; // i == 0
    public Sprite exchangeSprite; // i == 1
    public Sprite disconnectSprite; // i == 2
    public Sprite battleWinSprite; // i == 3
    public Sprite battleLoseSprite; // i == 4
    public Sprite statDistribSprite; // i == 5

    private void Awake()
    {
        alert = this;
    }

    public void CreateAlert(int i)
    {
        if (i < 0 || i >= 6) return;
        StartCoroutine(ShowAlert(i));
    }

    IEnumerator ShowAlert(int i)
    {
        alertPanel.gameObject.SetActive(true);
        switch (i)
        {
            case 0:
                image.sprite = myTurnSprite;
                break;
            case 1:
                image.sprite = exchangeSprite;
                break;
            case 2:
                image.sprite = disconnectSprite;
                break;
            case 3:
                image.sprite = battleWinSprite;
                break;
            case 4:
                image.sprite = battleLoseSprite;
                break;
            case 5:
                image.sprite = statDistribSprite;
                break;
        }
        yield return new WaitForSeconds(1.8f);
        alertPanel.gameObject.SetActive(false);
    }
}
