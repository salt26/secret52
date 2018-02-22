using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 교환을 나타내는 객체의 클래스입니다.
/// 교환한 두 플레이어, 교환한 두 카드, 교환 당시 체력 변화 등을 포함합니다.
/// 카드의 효과를 적용할 때와 로그를 표시할 때 쓰입니다.
/// </summary>
public class Exchange : NetworkBehaviour {

    private PlayerControl turnPlayer;    // 턴을 진행하는 플레이어
    private PlayerControl objectPlayer;  // 교환당하는 플레이어
    private Card turnPlayerCard;          // 턴을 진행하는 플레이어가 낸 카드 이름(종류)
    private Card objectPlayerCard;        // 교환당하는 플레이어가 낸 카드 이름(종류)
    private Card turnPlayerVoidCard;      // 턴을 진행하는 플레이어가 냈지만 상대의 속임 카드로 인해 돌려받게 된 카드 이름(종류)
    private Card objectPlayerVoidCard;    // 교환당하는 플레이어가 냈지만 상대의 속임 카드로 인해 돌려받게 된 카드 이름(종류)
    private List<Card> turnPlayerHand;    // 턴을 진행하는 플레이어의 교환 전 손패(외부 공개 불가)
    private List<Card> objectPlayerHand;  // 교환당하는 플레이어의 교환 전 손패(외부 공개 불가)
    private int turnPlayerHealth;           // 턴을 진행하는 플레이어의 교환 전 체력
    private int objectPlayerHealth;         // 교환당하는 플레이어의 교환 전 체력
    private int turnPlayerHealthVariation;  // 턴을 진행하는 플레이어의 교환 후 체력 변화
    private int objectPlayerHealthVariation;// 교환당하는 플레이어의 교환 후 체력 변화
    private int turnNum;                    // 대전 시작부터 지나온 턴 수 (0: 잘못된 교환 정보)
    private bool isComplete;                // 교환이 끝나 로그로 박제될 때 true가 됨
    private bool isFreezed;                 // 빙결되어 교환하지 못한 경우 true가 됨

    private static int count = 1;           // 턴 수를 세기 위한 변수
    private bool isTurnPComplete;
    private bool isObjectPComplete;
    private static CardDatabase cd;

    /*
    public Exchange(PlayerControl turnP, PlayerControl objectP, Card turnPCard, Card objectPCard)
    {
        isComplete = false;
        turnPlayerCard = turnPCard;
        objectPlayerCard = objectPCard;
        cd = GameObject.Find("BattleManager").GetComponent<CardDatabase>();
        if (!cd.VerifyCard(turnPlayerCard) || !cd.VerifyCard(objectPlayerCard))
        {
            Debug.LogError("Exchanging card is invalid");
            turnNum = 0;
            return;
        }
        turnPlayer = turnP;
        objectPlayer = objectP;
        if (turnPlayer != null)
            turnPlayerHealth = turnPlayer.GetHealth();
        else
        {
            Debug.LogError("Exchanging player is null!");
            turnPlayerHealth = 0;
            objectPlayerHealth = 0;
            turnNum = 0;
            return;
        }
        if (objectPlayer != null)
            objectPlayerHealth = objectPlayer.GetHealth();
        else
        {
            Debug.LogError("Exchanging player is null!");
            objectPlayerHealth = 0;
            turnNum = 0;
            return;
        }
        BattleManager bm = GameObject.Find("BattleManager").GetComponent<BattleManager>();
        turnPlayerHand = bm.GetPlayerHand(turnPlayer);
        objectPlayerHand = bm.GetPlayerHand(objectPlayer);
        turnNum = count;
        count++;

        ProcessExchange();
    }
    */

    public void SetFreezed(PlayerControl turnP)
    {
        isComplete = false;
        isFreezed = true;
        turnPlayer = turnP;
        if (turnPlayer != null)
            turnPlayerHealth = turnPlayer.GetHealth();
        else
        {
            Debug.LogError("Exchanging player is null!");
            turnPlayerHealth = 0;
            objectPlayerHealth = 0;
            turnNum = 0;
            return;
        }
        turnPlayerCard = null;
        objectPlayerCard = null;
        objectPlayer = null;
        BattleManager bm = GameObject.Find("BattleManager").GetComponent<BattleManager>();
        turnPlayerHand = bm.GetPlayerHand(turnPlayer);
        turnNum = count;
        count++;
    }

    public void SetExchange(PlayerControl turnP, PlayerControl objectP, Card turnPCard, Card objectPCard)
    {
        isComplete = false;
        isFreezed = false;
        turnPlayerCard = turnPCard;
        objectPlayerCard = objectPCard;
        cd = GameObject.Find("BattleManager").GetComponent<CardDatabase>();
        if (!cd.VerifyCard(turnPlayerCard) || !cd.VerifyCard(objectPlayerCard))
        {
            Debug.LogError("Exchanging card is invalid");
            turnNum = 0;
            return;
        }
        turnPlayer = turnP;
        objectPlayer = objectP;
        if (turnPlayer != null)
            turnPlayerHealth = turnPlayer.GetHealth();
        else
        {
            Debug.LogError("Exchanging player is null!");
            turnPlayerHealth = 0;
            objectPlayerHealth = 0;
            turnNum = 0;
            return;
        }
        if (objectPlayer != null)
            objectPlayerHealth = objectPlayer.GetHealth();
        else
        {
            Debug.LogError("Exchanging player is null!");
            objectPlayerHealth = 0;
            turnNum = 0;
            return;
        }
        BattleManager bm = GameObject.Find("BattleManager").GetComponent<BattleManager>();
        turnPlayerHand = bm.GetPlayerHand(turnPlayer);
        objectPlayerHand = bm.GetPlayerHand(objectPlayer);
        turnNum = count;
        count++;

        ProcessExchange();
    }

    private void ProcessExchange()
    {
        /*
        // 속임 효과 처리
        if (turnPlayerCard.GetCardName() == "Deceive")
        {
            objectPlayerVoidCard = objectPlayerCard;
            objectPlayerCard = AnotherCardInHand(objectPlayerCard, objectPlayerHand);
        }
        else if (objectPlayerCard.GetCardName() == "Deceive")
        {
            turnPlayerVoidCard = turnPlayerCard;
            turnPlayerCard = AnotherCardInHand(turnPlayerCard, turnPlayerHand);
        }

        // 회피 효과 처리
        if (turnPlayerCard.GetCardName() == "Avoid")
        {
            return;
        }
        if (objectPlayerCard.GetCardName() == "Avoid")
        {
            return;
        }

        // 공격 효과 처리
        if (turnPlayerCard.GetCardName() == "Attack")
        {
            objectPlayer.Damaged();
        }
        if (objectPlayerCard.GetCardName() == "Attack")
        {
            turnPlayer.Damaged();
        }

        // 치유 효과 처리
        if (turnPlayerCard.GetCardName() == "Heal")
        {
            objectPlayer.Restored();
        }
        if (objectPlayerCard.GetCardName() == "Heal")
        {
            turnPlayer.Restored();
        }

        // 빙결 효과 처리
        if (turnPlayerCard.GetCardName() == "Freeze")
        {
            objectPlayer.Freezed();
        }
        if (objectPlayerCard.GetCardName() == "Freeze")
        {
            turnPlayer.Freezed();
        }

        // 폭탄은 낼 때, 받을 때 효과가 없습니다.
        */

        // 시간 효과 처리
        if (turnPlayerCard.GetCardName() == "Time")
        {
            objectPlayerVoidCard = objectPlayerCard;
            objectPlayerCard = AnotherCardInHand(objectPlayerCard, objectPlayerHand);
        }
        else if (objectPlayerCard.GetCardName() == "Time")
        {
            turnPlayerVoidCard = turnPlayerCard;
            turnPlayerCard = AnotherCardInHand(turnPlayerCard, turnPlayerHand);
        }

        // 생명 낼 때 효과 처리
        if (turnPlayerCard.GetCardName() == "Life")
        {
            turnPlayer.Restored();
        }
        if (objectPlayerCard.GetCardName() == "Life")
        {
            objectPlayer.Restored();
        }

        // 빛 낼 때 효과 처리
        if (turnPlayerCard.GetCardName() == "Light")
        {
            turnPlayer.Lighted();
            // 오브젝트 플레이어의 속성을 턴 플레이어에게 공개
            // -> PlayerControl.cs의 UnveilFromExchangeLog 함수에서 처리
            //turnPlayer.Unveil(objectPlayer.GetPlayerIndex());
        }
        if (objectPlayerCard.GetCardName() == "Light")
        {
            objectPlayer.Lighted();
            // 턴 플레이어의 속성을 오브젝트 플레이어에게 공개
            // -> PlayerControl.cs의 UnveilFromExchangeLog 함수에서 처리
            //objectPlayer.Unveil(turnPlayer.GetPlayerIndex());
        }

        // 어둠 효과 처리
        if (turnPlayerCard.GetCardName() == "Dark")
        {
            return;
        }
        if (objectPlayerCard.GetCardName() == "Dark")
        {
            return;
        }

        // 생명 받을 때 효과 처리
        if (turnPlayerCard.GetCardName() == "Life")
        {
            objectPlayer.Restored();
        }
        if (objectPlayerCard.GetCardName() == "Life")
        {
            turnPlayer.Restored();
        }

        // 빛 받을 때 효과 처리
        if (turnPlayerCard.GetCardName() == "Light")
        {
            objectPlayer.Lighted();
        }
        if (objectPlayerCard.GetCardName() == "Light")
        {
            turnPlayer.Lighted();
        }

        // 불 효과 처리
        if (turnPlayerCard.GetCardName() == "Fire")
        {
            // 상대가 불의 마법사이면 피해 감소
            if (objectPlayer.GetPlayerElement() == 0)
            {
                objectPlayer.Damaged(2);

                // -> PlayerControl.cs의 UnveilFromExchangeLog 함수에서 처리
                //turnPlayer.Unveil(objectPlayer.GetPlayerIndex());
            }
            else
                objectPlayer.Damaged(turnPlayer.GetStatAttack());
        }
        if (objectPlayerCard.GetCardName() == "Fire")
        {
            // 상대가 불의 마법사이면 피해 감소
            if (turnPlayer.GetPlayerElement() == 0)
            {
                turnPlayer.Damaged(2);

                // -> PlayerControl.cs의 UnveilFromExchangeLog 함수에서 처리
                //objectPlayer.Unveil(turnPlayer.GetPlayerIndex());
            }
            else
                turnPlayer.Damaged(objectPlayer.GetStatAttack());
        }

        // 물 효과 처리
        if (turnPlayerCard.GetCardName() == "Water")
        {
            // 상대가 물의 마법사이면 피해 감소
            if (objectPlayer.GetPlayerElement() == 1)
            {
                objectPlayer.Damaged(2);
                
                // -> PlayerControl.cs의 UnveilFromExchangeLog 함수에서 처리
                //turnPlayer.Unveil(objectPlayer.GetPlayerIndex());
            }
            else
                objectPlayer.Damaged(turnPlayer.GetStatAttack());
        }
        if (objectPlayerCard.GetCardName() == "Water")
        {
            // 상대가 물의 마법사이면 피해 감소
            if (turnPlayer.GetPlayerElement() == 1)
            {
                turnPlayer.Damaged(2);

                // -> PlayerControl.cs의 UnveilFromExchangeLog 함수에서 처리
                //objectPlayer.Unveil(turnPlayer.GetPlayerIndex());
            }
            else
                turnPlayer.Damaged(objectPlayer.GetStatAttack());
        }

        // 전기 효과 처리
        if (turnPlayerCard.GetCardName() == "Electricity")
        {
            // 상대가 전기의 마법사이면 피해 감소
            if (objectPlayer.GetPlayerElement() == 2)
            {
                objectPlayer.Damaged(2);
                
                // -> PlayerControl.cs의 UnveilFromExchangeLog 함수에서 처리
                //turnPlayer.Unveil(objectPlayer.GetPlayerIndex());
            }
            else
                objectPlayer.Damaged(turnPlayer.GetStatAttack());
        }
        if (objectPlayerCard.GetCardName() == "Electricity")
        {
            // 상대가 전기의 마법사이면 피해 감소
            if (turnPlayer.GetPlayerElement() == 2)
            {
                turnPlayer.Damaged(2);

                // -> PlayerControl.cs의 UnveilFromExchangeLog 함수에서 처리
                //objectPlayer.Unveil(turnPlayer.GetPlayerIndex());
            }
            else
                turnPlayer.Damaged(objectPlayer.GetStatAttack());
        }

        // 바람 효과 처리
        if (turnPlayerCard.GetCardName() == "Wind")
        {
            // 상대가 바람의 마법사이면 피해 감소
            if (objectPlayer.GetPlayerElement() == 3)
            {
                objectPlayer.Damaged(2);

                // -> PlayerControl.cs의 UnveilFromExchangeLog 함수에서 처리
                //turnPlayer.Unveil(objectPlayer.GetPlayerIndex());
            }
            else
                objectPlayer.Damaged(turnPlayer.GetStatAttack());
        }
        if (objectPlayerCard.GetCardName() == "Wind")
        {
            // 상대가 바람의 마법사이면 피해 감소
            if (turnPlayer.GetPlayerElement() == 3)
            {
                turnPlayer.Damaged(2);

                // -> PlayerControl.cs의 UnveilFromExchangeLog 함수에서 처리
                //objectPlayer.Unveil(turnPlayer.GetPlayerIndex());
            }
            else
                turnPlayer.Damaged(objectPlayer.GetStatAttack());
        }

        // 독 효과 처리
        if (turnPlayerCard.GetCardName() == "Poison")
        {
            // 상대가 독의 마법사이면 피해 감소
            if (objectPlayer.GetPlayerElement() == 4) { 
                objectPlayer.Damaged(2);

                // -> PlayerControl.cs의 UnveilFromExchangeLog 함수에서 처리
                //turnPlayer.Unveil(objectPlayer.GetPlayerIndex());
            }
            else
                objectPlayer.Damaged(turnPlayer.GetStatAttack());
        }
        if (objectPlayerCard.GetCardName() == "Poison")
        {
            // 상대가 독의 마법사이면 피해 감소
            if (turnPlayer.GetPlayerElement() == 4)
            {
                turnPlayer.Damaged(2);

                // -> PlayerControl.cs의 UnveilFromExchangeLog 함수에서 처리
                //objectPlayer.Unveil(turnPlayer.GetPlayerIndex());
            }
            else
                turnPlayer.Damaged(objectPlayer.GetStatAttack());
        }

        // 타락은 낼 때, 받을 때 효과가 없습니다.
    }

    /// <summary>
    /// 손패에서 인자로 주어진 카드와 다른 카드를 반환합니다.
    /// 속임 효과를 구현할 때 쓰입니다.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="hand"></param>
    /// <returns></returns>
    private Card AnotherCardInHand(Card card, List<Card> hand)
    {
        if (hand == null || hand.Count != 2) return null;
        if (hand[0].Equals(card)) return hand[1];
        else if (hand[1].Equals(card)) return hand[0];
        else return null;
    }
    /// <summary>
    /// 이 교환에 관련된 플레이어이면 true, 아니면 false를 반환합니다.
    /// </summary>
    /// <param name="player">플레이어</param>
    /// <returns></returns>
    public bool IsPlayerConcerned(PlayerControl player)
    {
        if (player == null) return false;
        return (player.Equals(turnPlayer) || player.Equals(objectPlayer));
    }

    /// <summary>
    /// 턴을 진행한 플레이어가 낸 카드의 영어 이름을 반환합니다.
    /// </summary>
    /// <returns></returns>
    public Card GetTurnPlayerCard()
    {
        return turnPlayerCard;
    }

    /// <summary>
    /// 교환당한 플레이어가 낸 카드의 영어 이름을 반환합니다.
    /// </summary>
    /// <returns></returns>
    public Card GetObjectPlayerCard()
    {
        return objectPlayerCard;
    }

    /// <summary>
    /// 턴을 진행한 플레이어를 반환합니다.
    /// </summary>
    /// <returns></returns>
    public PlayerControl GetTurnPlayer()
    {
        return turnPlayer;
    }

    /// <summary>
    /// 교환당한 플레이어를 반환합니다.
    /// </summary>
    /// <returns></returns>
    public PlayerControl GetObjectPlayer()
    {
        return objectPlayer;
    }

    /// <summary>
    /// 턴을 진행한 플레이어의 교환 전 남은 체력을 반환합니다.
    /// </summary>
    /// <returns></returns>
    public int GetTurnPlayerHealth()
    {
        return turnPlayerHealth;
    }

    /// <summary>
    /// 교환당한 플레이어의 교환 전 남은 체력을 반환합니다.
    /// </summary>
    /// <returns></returns>
    public int GetObjectPlayerHealth()
    {
        return objectPlayerHealth;
    }

    /// <summary>
    /// 턴을 진행한 플레이어가 그 턴에 빙결되어 교환을 하지 못했다면 true를 반환합니다.
    /// </summary>
    /// <returns></returns>
    public bool GetIsFreezed()
    {
        return isFreezed;
    }

    /// <summary>
    /// 턴을 진행한 플레이어의 체력 변화량을 설정합니다.
    /// 이 함수는 턴이 끝날 때 호출되어야 합니다.
    /// </summary>
    /// <param name="healthAfterExchange">교환 후 남은 체력</param>
    public void SetTurnPlayerHealthVariation(int healthAfterExchange)
    {
        if (isTurnPComplete) return;
        if (turnNum == 0) turnPlayerHealthVariation = 0;
        turnPlayerHealthVariation = healthAfterExchange - turnPlayerHealth;
        isTurnPComplete = true;
        if (isObjectPComplete || isFreezed) isComplete = true;
    }

    /// <summary>
    /// 교환당한 플레이어의 체력 변화량을 설정합니다.
    /// 이 함수는 턴이 끝날 때 호출되어야 합니다.
    /// </summary>
    /// <param name="healthAfterExchange">교환 후 남은 체력</param>
    public void SetObjectPlayerHealthVariation(int healthAfterExchange)
    {
        if (isObjectPComplete) return;
        if (turnNum == 0) objectPlayerHealthVariation = 0;
        objectPlayerHealthVariation = healthAfterExchange - objectPlayerHealth;
        isObjectPComplete = true;
        if (isTurnPComplete) isComplete = true;
    }

    /// <summary>
    /// 턴을 진행한 플레이어의 교환 후 체력 변화량을 반환합니다.
    /// </summary>
    /// <returns></returns>
    public int GetTurnPlayerHealthVariation()
    {
        return turnPlayerHealthVariation;
    }

    /// <summary>
    /// 교환당한 플레이어의 교환 후 체력 변화량을 반환합니다.
    /// </summary>
    /// <returns></returns>
    public int GetObjectPlayerHealthVariation()
    {
        return objectPlayerHealthVariation;
    }

    /// <summary>
    /// 이 교환이 이루어진 턴 수를 반환합니다.
    /// </summary>
    /// <returns></returns>
    public int GetTurnNum()
    {
        return turnNum;
    }

    /// <summary>
    /// 교환 정보가 완성되었는지 여부를 반환합니다.
    /// 이 교환이 이루어진 턴이 끝나고 체력 변화량이 반영되면 true가 됩니다.
    /// true이면 로그에 이 교환이 표시됩니다.
    /// </summary>
    /// <returns></returns>
    public bool GetIsComplete()
    {
        return isComplete;
    }

    /// <summary>
    /// 현재 몇 번째 턴을 진행하고 있는지, 그 턴 수를 반환합니다.
    /// </summary>
    /// <returns></returns>
    static public int GetTotalTurnNum()
    {
        return count;
    }
}
