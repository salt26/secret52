using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PushingCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    private static BattleManager bm;
    private static CardDatabase cd;

    private Pusher pusher;

    static public PlayerControl localPlayer = null;

    public GameObject tooltipBox;
    private static TooltipUI tooltip;
    private bool tooltipActive;

    private Vector3 cardx;
    private Vector3 cardOriginal;

    private Card cardL;
    private Card cardR;

    private void Awake()
    {
        bm = BattleManager.bm;
        cd = CardDatabase.cardDatabase;
        tooltip = null;
        tooltipActive = false;
    }

    private void Start()
    {
        pusher = GetComponentInParent<Pusher>();
        cardOriginal = transform.position;
    }

    private void FixedUpdate()
    {
        if (localPlayer == null)
        {
            return;
        }
        else if (bm == null)
        {
            bm = BattleManager.bm;
            return;
        }
        else if (cd == null)
        {
            cd = CardDatabase.cardDatabase;
            return;
        }
        else if (bm.GetTurnStep() <= 0)
        {
            return;
        }
        switch (localPlayer.GetPlayerNum())
        {
            case 1:
                cardL = bm.GetCardInPosition(0);
                cardR = bm.GetCardInPosition(1);
                break;
            case 2:
                cardL = bm.GetCardInPosition(2);
                cardR = bm.GetCardInPosition(3);
                break;
            case 3:
                cardL = bm.GetCardInPosition(4);
                cardR = bm.GetCardInPosition(5);
                break;
            case 4:
                cardL = bm.GetCardInPosition(6);
                cardR = bm.GetCardInPosition(7);
                break;
            case 5:
                cardL = bm.GetCardInPosition(8);
                cardR = bm.GetCardInPosition(9);
                break;
        }
        if (tooltip == null && tooltipActive)
        {
            if (CompareTag("Left") && cardL != null && transform.position.y == cardOriginal.y)
            {
                GameObject t = Instantiate(tooltipBox, GetComponentInParent<Canvas>().gameObject.transform);
                tooltip = t.GetComponent<TooltipUI>();
                tooltip.SetText(cd.GetCardInfo(cardL).GetNameText(),
                    cd.GetCardInfo(cardL).GetColor(), cd.GetCardInfo(cardL).GetDetailText());
                tooltip.Appear();
            }
            else if (CompareTag("Right") && cardR != null && transform.position.y == cardOriginal.y)
            {
                GameObject t = Instantiate(tooltipBox, GetComponentInParent<Canvas>().gameObject.transform);
                tooltip = t.GetComponent<TooltipUI>();
                tooltip.SetText(cd.GetCardInfo(cardR).GetNameText(),
                    cd.GetCardInfo(cardR).GetColor(), cd.GetCardInfo(cardR).GetDetailText());
                tooltip.Appear();
            }
            tooltipActive = false;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        cardx.x = transform.position.x;
        localPlayer.SetCardDragging(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if ((localPlayer.Equals(bm.GetTurnPlayer()) && localPlayer.GetObjectTarget() != null && bm.GetTurnStep() == 2)
            || (localPlayer.Equals(bm.GetObjectPlayer()) && bm.GetTurnStep() == 3) && !localPlayer.GetIsInTutorial())
        {
            if (bm.GetPlayerSelectedCard(localPlayer) == null && Input.touchCount <= 1
                && (StatPanelUI.statPanelUI == null || !StatPanelUI.statPanelUI.GetIsOpen())
                && (LogPanelUI.logPanelUI == null || !LogPanelUI.logPanelUI.GetIsOpen()))
            {
                cardx.y = eventData.position.y;
                if (cardx.y > cardOriginal.y)
                {
                    transform.SetPositionAndRotation(cardx, transform.rotation);
                }
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        localPlayer.SetCardDragging(false);
        if (!pusher.GetEndDragCooltime() && transform.position.y >= Screen.height * 8 / 16)
        {
            pusher.SetStartEndDragCooltime();
            if (CompareTag("Left"))
            {
                pusher.MoveCardUp(cardx, new Vector3(cardOriginal.x, Screen.height * 3 / 2), tag);
                pusher.SetSelectedCard(new SelectedInfo(cardL, tag, cardOriginal));
            }
            else if (CompareTag("Right"))
            {
                pusher.MoveCardUp(cardx, new Vector3(cardOriginal.x, Screen.height * 3 / 2), tag);
                pusher.SetSelectedCard(new SelectedInfo(cardR, tag, cardOriginal));
            }
            else
            {
                Debug.Log("Card is not appropriate.");
                //LogDisplay.AddText("Card is not appropriate.");
            }
        }
        else
        {
            transform.position = cardOriginal;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipActive = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip == null) return;
        tooltip.Disappear();
        tooltipActive = false;
    }
}

public class SelectedInfo
{
    private Card card;
    private string LR;
    private Vector3 OriginalPosition;

    public SelectedInfo(Card cd, string a, Vector3 Original)
    {
        card = cd;
        LR = a;//왼쪽이면 Left, 오른쪽이면 Right
        OriginalPosition = Original;
    }

    public Card GetCard()
    {
        return card;
    }

    public string GetLR()
    {
        return LR;
    }

    public Vector3 GetOriginalPosition()
    {
        return OriginalPosition;
    }
}
