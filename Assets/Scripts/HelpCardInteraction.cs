using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpCardInteraction : MonoBehaviour {

    public Image cardL;
    public Image cardR;
    public Text info;
    public Button[] buttons = new Button[10];
    public Button resetButton;

    private static CardDatabase cd;
    private int cardcodeL;
    private int cardcodeR;
    
	void Start () {
        cardcodeL = -1;
        cardcodeR = -1;
        cd = CardDatabase.cardDatabase;
	}
	
	void FixedUpdate () {
		switch (cardcodeL)
        {
            case 0:
                cardL.sprite = Resources.Load("Cards/Fire card", typeof(Sprite)) as Sprite;
                break;
            case 1:
                cardL.sprite = Resources.Load("Cards/Water card", typeof(Sprite)) as Sprite;
                break;
            case 2:
                cardL.sprite = Resources.Load("Cards/Electricity card", typeof(Sprite)) as Sprite;
                break;
            case 3:
                cardL.sprite = Resources.Load("Cards/Wind card", typeof(Sprite)) as Sprite;
                break;
            case 4:
                cardL.sprite = Resources.Load("Cards/Poison card", typeof(Sprite)) as Sprite;
                break;
            case 5:
                cardL.sprite = Resources.Load("Cards/Life card", typeof(Sprite)) as Sprite;
                break;
            case 6:
                cardL.sprite = Resources.Load("Cards/Light card", typeof(Sprite)) as Sprite;
                break;
            case 7:
                cardL.sprite = Resources.Load("Cards/Dark card", typeof(Sprite)) as Sprite;
                break;
            case 8:
                cardL.sprite = Resources.Load("Cards/Time card", typeof(Sprite)) as Sprite;
                break;
            case 9:
                cardL.sprite = Resources.Load("Cards/Corruption card2", typeof(Sprite)) as Sprite;
                break;
            default:
                cardL.sprite = Resources.Load("Back sticker", typeof(Sprite)) as Sprite;
                break;
        }
        switch (cardcodeR)
        {
            case 0:
                cardR.sprite = Resources.Load("Cards/Fire card", typeof(Sprite)) as Sprite;
                break;
            case 1:
                cardR.sprite = Resources.Load("Cards/Water card", typeof(Sprite)) as Sprite;
                break;
            case 2:
                cardR.sprite = Resources.Load("Cards/Electricity card", typeof(Sprite)) as Sprite;
                break;
            case 3:
                cardR.sprite = Resources.Load("Cards/Wind card", typeof(Sprite)) as Sprite;
                break;
            case 4:
                cardR.sprite = Resources.Load("Cards/Poison card", typeof(Sprite)) as Sprite;
                break;
            case 5:
                cardR.sprite = Resources.Load("Cards/Life card", typeof(Sprite)) as Sprite;
                break;
            case 6:
                cardR.sprite = Resources.Load("Cards/Light card", typeof(Sprite)) as Sprite;
                break;
            case 7:
                cardR.sprite = Resources.Load("Cards/Dark card", typeof(Sprite)) as Sprite;
                break;
            case 8:
                cardR.sprite = Resources.Load("Cards/Time card", typeof(Sprite)) as Sprite;
                break;
            case 9:
                cardR.sprite = Resources.Load("Cards/Corruption card2", typeof(Sprite)) as Sprite;
                break;
            default:
                cardR.sprite = Resources.Load("Back sticker", typeof(Sprite)) as Sprite;
                break;
        }
        if (cardcodeL == -1)
        {
            info.text = "";
        }
        else if (cardcodeL >= 0 && cardcodeR == -1)
        {
            CardInfo ci = cd.GetCardInfo(GetCardName(cardcodeL));
            if (ci != null)
            {
                info.text = "<color=#DB40A6>" + ci.GetNameText();
                if (cardcodeL < 5) info.text += "(공격)";
                info.text += "</color>\n" + ci.GetDetailText().Replace(". ", ".\n");
            }
        }
        else if (cardcodeL >= 0 && cardcodeR >= 0)
        {
            CardInfo ci = cd.GetCardInfo(GetCardName(cardcodeL));
            if (ci != null)
            {
                info.text = "<color=#DB40A6>" + ci.GetNameText();
                if (cardcodeL < 5) info.text += "(공격)";
            }
            ci = cd.GetCardInfo(GetCardName(cardcodeR));
            if (ci != null)
            {
                info.text += " <=> " + ci.GetNameText();
                if (cardcodeR < 5) info.text += "(공격)";
                info.text += "</color>\n";
            }

            if (cardcodeL < 5 && cardcodeR < 5) // 공격+공격
            {
                info.text += "서로 카드를 교환하여 상대가 낸 카드를 받을 때 상대의 공격력만큼 자신의 체력이 감소합니다. 단, 자신과 같은 속성의 공격 카드를 받으면 체력이 대신 2 감소하며 상대에게 자신의 속성이 공개됩니다.";
            }
            else if ((cardcodeL < 5 && cardcodeR == 5) || (cardcodeL == 5 && cardcodeR < 5))    // 공격+생명
            {
                info.text += "공격을 낸 플레이어는 생명을 받아 체력을 5 회복합니다.\n\n";
                info.text += "생명을 낸 플레이어는 공격을 받아 (5 - 상대의 공격력)만큼 체력이 변화합니다. 단, 생명을 낸 플레이어가 자신과 같은 속성의 공격 카드를 받으면 체력이 대신 3 증가하며 상대에게 자신의 속성이 공개됩니다. 이때 최대 체력은 52입니다. ";
                info.text += "예외적으로 상대의 공격력이 낮고 자신의 체력이 52에 가까워서 자신이 받은 공격 카드가 자신의 속성 카드이든 아니든 교환 후 체력이 52가 되는 경우, 상대에게 자신의 속성이 공개되지 않습니다.";
            }
            else if ((cardcodeL < 5 && cardcodeR == 6) || (cardcodeL == 6 && cardcodeR < 5))    // 공격+빛
            {
                info.text += "공격을 낸 플레이어는 빛을 받아 권력이 1 증가하고 상대에게 자신의 속성이 공개됩니다.\n\n";
                info.text += "빛을 낸 플레이어는 권력이 1 증가하고 상대의 속성을 확인하지만 공격을 받아 상대의 공격력만큼 자신의 체력이 감소합니다. 단, 자신과 같은 속성의 공격 카드를 받으면 체력이 대신 2 감소하며 상대에게 자신의 속성이 공개됩니다.";
            }
            else if ((cardcodeL < 5 && cardcodeR == 7) || (cardcodeL == 7 && cardcodeR < 5))    // 공격+어둠
            {
                info.text += "서로 카드만 교환하고 아무 일도 일어나지 않습니다. 어둠을 낸 플레이어가 자신과 같은 속성의 공격 카드를 받아도 상대에게 자신의 속성이 공개되지 않습니다.";
            }
            else if ((cardcodeL < 5 && cardcodeR == 8) || (cardcodeL == 8 && cardcodeR < 5))    // 공격+시간
            {
                info.text += "공격을 내려던 플레이어는 그 카드 대신 자신이 들고 있던 다른 카드를 내야 합니다. 그리고 실제로 낸 카드의 낼 때 효과(생명, 빛)를 받습니다.\n\n";
                info.text += "시간을 낸 플레이어는 상대가 원래 내려고 하지 않았던 카드를 받게 되며, 실제로 받은 카드의 받을 때 효과(";
                for (int i = 0; i < 5; i++)
                {
                    if (cardcodeL == i || cardcodeR == i) continue;
                    info.text += cd.GetCardInfo(GetCardName(i)).GetNameText() + ", ";
                }
                info.text += "생명, 빛)를 받습니다.";
            }
            else if ((cardcodeL < 5 && cardcodeR == 9) || (cardcodeL == 9 && cardcodeR < 5))    // 공격+타락
            {
                info.text += "공격을 낸 플레이어는 타락을 받습니다. 만약 그 턴이 공격을 낸 플레이어의 턴이었다면 공격 효과 처리가 끝난 후에 타락 효과를 받아, 공격력이 1 증가하고 정신력이 2 감소합니다. 최소 정신력은 1이며 정신력이 감소하지 않아도 공격력은 증가합니다.\n\n";
                info.text += "타락을 낸 플레이어는 공격을 받아 상대의 교환 직전 공격력만큼 자신의 체력이 감소합니다. 단, 자신과 같은 속성의 공격 카드를 받으면 체력이 대신 2 감소하며 상대에게 자신의 속성이 공개됩니다.";
            }
            else if ((cardcodeL == 5 && cardcodeR == 6) || (cardcodeL == 6 && cardcodeR == 5))    // 생명+빛
            {
                info.text += "생명을 낸 플레이어는 체력을 5 회복하고, 빛을 받아 권력이 1 증가하며 상대에게 자신의 속성이 공개됩니다.\n\n";
                info.text += "빛을 낸 플레이어는 권력이 1 증가하고 상대의 속성을 확인하며, 생명을 받아 체력을 5 회복합니다. 최대 체력은 52입니다.";
            }
            else if ((cardcodeL == 5 && cardcodeR == 7) || (cardcodeL == 7 && cardcodeR == 5))    // 생명+어둠
            {
                info.text += "생명을 낸 플레이어는 체력을 5 회복하고 어둠을 받습니다. 최대 체력은 52입니다.\n\n";
                info.text += "어둠을 낸 플레이어는 생명을 받고 아무 일도 일어나지 않습니다.";
            }
            else if ((cardcodeL == 5 && cardcodeR == 8) || (cardcodeL == 8 && cardcodeR == 5))    // 생명+시간
            {
                info.text += "생명을 내려던 플레이어는 생명 대신 자신이 들고 있던 다른 카드를 내야 합니다. 그리고 실제로 낸 카드의 낼 때 효과(빛)를 받습니다.\n\n";
                info.text += "시간을 낸 플레이어는 상대가 원래 내려고 하지 않았던 카드를 받게 되며, 실제로 받은 카드의 받을 때 효과(불, 물, 전기, 바람, 독, 빛)를 받습니다.";
            }
            else if ((cardcodeL == 5 && cardcodeR == 9) || (cardcodeL == 9 && cardcodeR == 5))    // 생명+타락
            {
                info.text += "생명을 낸 플레이어는 체력을 5 회복하고 타락을 받습니다. 만약 그 턴이 생명을 낸 플레이어의 턴이었다면 타락 효과를 받아, 공격력이 1 증가하고 정신력이 2 감소합니다. 최소 정신력은 1이며 정신력이 감소하지 않아도 공격력은 증가합니다.\n\n";
                info.text += "타락을 낸 플레이어는 생명을 받아 체력을 5 회복합니다. 최대 체력은 52입니다.";
            }
            else if ((cardcodeL == 6 && cardcodeR == 7) || (cardcodeL == 7 && cardcodeR == 6))    // 빛+어둠
            {
                info.text += "빛을 낸 플레이어는 권력이 1 증가하고 상대의 속성을 확인하며 어둠을 받습니다.\n\n";
                info.text += "어둠을 낸 플레이어는 빛을 받아도 권력이 증가하지 않지만, 상대에게 자신의 속성이 공개됩니다.";
            }
            else if ((cardcodeL == 6 && cardcodeR == 8) || (cardcodeL == 8 && cardcodeR == 6))    // 빛+시간
            {
                info.text += "빛을 내려던 플레이어는 빛 대신 자신이 들고 있던 다른 카드를 내야 합니다. 그리고 실제로 낸 카드의 낼 때 효과(생명)를 받습니다.\n\n";
                info.text += "시간을 낸 플레이어는 상대가 원래 내려고 하지 않았던 카드를 받게 되며, 실제로 받은 카드의 받을 때 효과(불, 물, 전기, 바람, 독, 생명)를 받습니다.";
            }
            else if ((cardcodeL == 6 && cardcodeR == 9) || (cardcodeL == 9 && cardcodeR == 6))    // 빛+타락
            {
                info.text += "빛을 낸 플레이어는 권력이 1 증가하고 상대의 속성을 확인하며 타락을 받습니다. 만약 그 턴이 빛을 낸 플레이어의 턴이었다면 타락 효과를 받아, 공격력이 1 증가하고 정신력이 2 감소합니다. 최소 정신력은 1이며 정신력이 감소하지 않아도 공격력은 증가합니다.\n\n";
                info.text += "타락을 낸 플레이어는 빛을 받아 권력이 1 증가하고 상대에게 자신의 속성이 공개됩니다.";
            }
            else if ((cardcodeL == 7 && cardcodeR == 8) || (cardcodeL == 8 && cardcodeR == 7))    // 어둠+시간
            {
                info.text += "어둠을 내려던 플레이어는 어둠 대신 자신이 들고 있던 다른 카드를 내야 합니다. 그리고 실제로 낸 카드의 낼 때 효과(생명, 빛)를 받습니다. 어둠으로는 시간의 낼 때 효과를 무시할 수 없습니다.\n\n";
                info.text += "시간을 낸 플레이어는 상대가 원래 내려고 하지 않았던 카드를 받게 되며, 실제로 받은 카드의 받을 때 효과(불, 물, 전기, 바람, 독, 생명)를 받습니다.";
            }
            else if ((cardcodeL == 7 && cardcodeR == 9) || (cardcodeL == 9 && cardcodeR == 7))    // 어둠+타락
            {
                info.text += "어둠을 낸 플레이어는 타락을 받습니다. 만약 그 턴이 어둠을 낸 플레이어의 턴이었다면 타락 효과를 받아, 공격력이 1 증가하고 정신력이 2 감소합니다. 최소 정신력은 1이며 정신력이 감소하지 않아도 공격력은 증가합니다. 어둠으로는 타락의 효과를 무시할 수 없습니다.\n\n";
                info.text += "타락을 낸 플레이어는 어둠을 받습니다.";
            }
            else if ((cardcodeL == 8 && cardcodeR == 9) || (cardcodeL == 9 && cardcodeR == 8))    // 시간+타락
            {
                info.text += "타락을 내려던 플레이어는 타락 대신 자신이 들고 있던 다른 카드를 내야 합니다. 그리고 실제로 낸 카드의 낼 때 효과(생명, 빛)를 받습니다. 만약 그 턴이 타락을 내려던 플레이어의 턴이었다면 타락을 내지 못했으므로 타락 효과를 받아, 공격력이 1 증가하고 정신력이 2 감소합니다. 최소 정신력은 1이며 정신력이 감소하지 않아도 공격력은 증가합니다.\n\n";
                info.text += "시간을 낸 플레이어는 상대가 원래 내려고 하지 않았던 카드를 받게 되며, 실제로 받은 카드의 받을 때 효과(불, 물, 전기, 바람, 독, 생명, 빛)를 받습니다.";
            }
            else
            {
                Debug.LogWarning("Unmatched case!");
            }
        }
    }

    public void ClickCardButton(int cardcode)
    {
        if (cardcode < 0 || cardcode >= 10) return;

        if (cardcodeL == -1)
        {
            cardcodeL = cardcode;
            buttons[cardcode].interactable = false;
            resetButton.interactable = true;
        }
        else if (cardcodeR == -1 && cardcodeL != cardcode)
        {
            cardcodeR = cardcode;
            for (int i = 0; i < 10; i++)
            {
                buttons[i].interactable = false;
            }
        }
    }

    public void ClickResetButton()
    {
        cardcodeL = -1;
        cardcodeR = -1;
        for (int i = 0; i < 10; i++)
        {
            buttons[i].interactable = true;
        }
        resetButton.interactable = false;
    }

    private string GetCardName(int cardcode)
    {
        switch (cardcode)
        {
            case 0:
                return "Fire";
            case 1:
                return "Water";
            case 2:
                return "Electricity";
            case 3:
                return "Wind";
            case 4:
                return "Poison";
            case 5:
                return "Life";
            case 6:
                return "Light";
            case 7:
                return "Dark";
            case 8:
                return "Time";
            case 9:
                return "Corruption";
            default:
                return "?";
        }
    }
}
