using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : MonoBehaviour {

    public static CardDatabase cardDatabase;

    private static List<CardInfo> cardInfo = new List<CardInfo>();
    private static BattleManager bm;

    private void Awake()
    {
        bm = gameObject.GetComponent<BattleManager>();
        cardDatabase = this;
        CardInfo ci;

        /*
        // 공격 카드
        ci = new CardInfo("Attack", "공격", "이 카드를 받을 때 피해를 1 받습니다.");
        cardInfo.Add(ci);

        // 치유 카드
        ci = new CardInfo("Heal", "치유", "이 카드를 받을 때 체력을 1 회복합니다. 이 카드로 체력이 6을 넘을 수 없습니다.");
        cardInfo.Add(ci);

        // 폭탄 카드
        ci = new CardInfo("Bomb", "폭탄", "이 카드를 들고 있는 동안 내 턴이 끝날 때마다 피해를 1 받습니다.");
        cardInfo.Add(ci);

        // 회피 카드
        ci = new CardInfo("Avoid", "회피", "이 카드를 내면 교환 상대에게서 받는 카드의 받을 때 효과를 무효화합니다. 카드는 그대로 교환합니다.");
        cardInfo.Add(ci);

        // 속임 카드
        ci = new CardInfo("Deceive", "속임", "이 카드를 내면 교환 상대에게서 받는 카드를 확인하지 않고 돌려줍니다. 대신 교환 상대가 들고 있던 카드를 가져오며, 가져온 카드의 받을 때 효과를 받습니다.");
        cardInfo.Add(ci);

        // 빙결 카드
        ci = new CardInfo("Freeze", "빙결", "이 카드를 받을 때 다음 한 번의 내 턴이 교환 없이 끝납니다. 이 카드의 효과를 받은 사실은 다음 내 턴이 시작될 때 모두에게 공개됩니다.");
        cardInfo.Add(ci);
        */

        // 불 카드
        ci = new CardInfo("Fire", "불", "받을 때 자신이 불의 마법사이면 피해를 2 받고, 아니면 상대의 공격력만큼 피해를 받습니다.", new Color(1f, 0.231f, 0.357f));
        cardInfo.Add(ci);

        // 물 카드
        ci = new CardInfo("Water", "물", "받을 때 자신이 물의 마법사이면 피해를 2 받고, 아니면 상대의 공격력만큼 피해를 받습니다.", new Color(0.2f, 0.404f, 0.992f));
        cardInfo.Add(ci);

        // 전기 카드
        ci = new CardInfo("Electricity", "전기", "받을 때 자신이 전기의 마법사이면 피해를 2 받고, 아니면 상대의 공격력만큼 피해를 받습니다.", new Color(0.792f, 0.522f, 1f));
        cardInfo.Add(ci);

        // 바람 카드
        ci = new CardInfo("Wind", "바람", "받을 때 자신이 바람의 마법사이면 피해를 2 받고, 아니면 상대의 공격력만큼 피해를 받습니다.", new Color(0.22f, 0.659f, 1f));
        cardInfo.Add(ci);

        // 독 카드
        ci = new CardInfo("Poison", "독", "받을 때 자신이 독의 마법사이면 피해를 2 받고, 아니면 상대의 공격력만큼 피해를 받습니다.", new Color(0.004f, 0.58f, 0.18f));
        cardInfo.Add(ci);

        // 생명 카드
        ci = new CardInfo("Life", "생명", "낼 때 체력을 5 회복합니다. 받을 때 체력을 5 회복합니다. 최대 체력은 52입니다.", new Color(0.357f, 0.867f, 0.22f));
        cardInfo.Add(ci);

        // 빛 카드
        ci = new CardInfo("Light", "빛", "낼 때 권력을 1 얻고 상대 마법사의 속성을 확인합니다. 받을 때 권력을 1 얻습니다.", new Color(1f, 1f, 0.184f));
        cardInfo.Add(ci);
        
        // 어둠 카드
        ci = new CardInfo("Dark", "어둠", "낼 때 상대에게서 받는 카드의 받을 때 효과를 무시합니다.", new Color(0.329f, 0.329f, 0.329f));
        cardInfo.Add(ci);

        // 시간 카드
        ci = new CardInfo("Time", "시간", "낼 때 상대가 내려고 했던 카드를 내지 못하게 하는 대신 들고 있던 카드를 내도록 하여 자신이 받습니다.", new Color(1f, 0.514f, 0.365f));
        cardInfo.Add(ci);

        // 타락 카드
        ci = new CardInfo("Corruption", "타락", "들고 있는 동안 자신의 턴이 끝나면 공격력을 1 얻고 정신력을 1 잃습니다. 최소 정신력은 1입니다.", new Color(0.475f, 0.208f, 0.871f));
        cardInfo.Add(ci);

    }


    /// <summary>
    /// 인자로 주어진 카드의 영어 이름이 데이터베이스에 존재하는지 확인하는 함수입니다.
    /// 존재하면 true를 반환합니다.
    /// </summary>
    /// <param name="cardName">카드의 영어 이름</param>
    /// <returns></returns>
    public bool VerifyCard(string cardName)
    {
        foreach (CardInfo card in cardInfo)
        {
            if (card.GetName() == cardName) return true;
        }
        return false;
    }

    public bool VerifyCard(Card card)
    {
        if (card == null) return false;
        foreach (CardInfo ci in cardInfo)
        {
            if (ci.GetName() == card.GetCardName()) return true;
        }
        return false;
    }

    /// <summary>
    /// 인자로 주어진 카드의 영어 이름으로 그 카드의 정보를 열람하는 함수입니다.
    /// </summary>
    /// <param name="cardName">카드의 영어 이름</param>
    /// <returns></returns>
    public CardInfo GetCardInfo(string cardName)
    {
        foreach (CardInfo card in cardInfo)
        {
            if (card.GetName() == cardName) return card;
        }
        return null;
    }

    /// <summary>
    /// 인자로 주어진 카드의 정보를 열람하는 함수입니다.
    /// </summary>
    /// <param name="card">카드</param>
    /// <returns></returns>
    public CardInfo GetCardInfo(Card card)
    {
        if (card == null) return null;
        return GetCardInfo(card.GetCardName());
    }
}

public class CardInfo
{
    private string cardName;        // 카드의 영어 이름(내부적으로 사용하는 이름)
    private string cardNameText;    // 카드의 한글 이름
    private string cardDetailText;  // 카드의 효과 설명 텍스트
    private Color cardColor;        // 카드의 대표색

    public CardInfo(string name, string nameText, string detailText/*, int effectType*/)
    {
        cardName = name;
        cardNameText = nameText;
        cardDetailText = detailText;
        cardColor = Color.grey;
        //cardEffectType = effectType;
    }

    public CardInfo(string name, string nameText, string detailText, Color color)
    {
        cardName = name;
        cardNameText = nameText;
        cardDetailText = detailText;
        cardColor = color;
    }

    /// <summary>
    /// 카드의 영어 이름을 반환합니다.
    /// </summary>
    /// <returns></returns>
    public string GetName()
    {
        return cardName;
    }

    /// <summary>
    /// 카드의 한글 이름을 반환합니다.
    /// </summary>
    /// <returns></returns>
    public string GetNameText()
    {
        return cardNameText;
    }

    /// <summary>
    /// 카드의 효과 설명을 반환합니다.
    /// </summary>
    /// <returns></returns>
    public string GetDetailText()
    {
        return cardDetailText;
    }

    /// <summary>
    /// 카드의 대표색을 반환합니다.
    /// </summary>
    /// <returns></returns>
    public Color GetColor()
    {
        return cardColor;
    }
}