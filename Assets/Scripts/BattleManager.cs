﻿using System.Collections;
using System.Collections.Generic;
using Prototype.NetworkLobby;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class BattleManager : NetworkBehaviour
{

    static public BattleManager bm;

    public GameObject player;
    [SerializeField] private List<GameObject> cards = new List<GameObject>();
    // cards의 i번째 값은 cardCode가 i인 Card 컴포넌트를 포함하는 게임오브젝트입니다.
    // 이제 cards의 순서는 바뀌지 않고 카드 교환 시 cardcode의 순서가 바뀝니다.

    public Pusher pusher;

    public List<PlayerControl> players = new List<PlayerControl>(){
            null, null, null, null, null
        };
    private SyncListInt playerPermutation = new SyncListInt();
    // playerPermutation은 각 플레이어에게 무작위로 속성을 하나씩 부여하기 위해 사용됩니다.
    // 각 PlayerControl의 인덱스 값(int)을 갖는 배열입니다.
    // players[playerPermutation[0]]의 속성은 불(0번) 속성입니다.
    // players[playerPermutation[1]]의 속성은 물(1번) 속성입니다.
    // players[playerPermutation[2]]의 속성은 전기(2번) 속성입니다.
    // players[playerPermutation[3]]의 속성은 바람(3번) 속성입니다.
    // players[playerPermutation[4]]의 속성은 독(4번) 속성입니다.

    private SyncListInt elementPermutation = new SyncListInt();
    // elementPermutation은 각 속성에게 무작위로 목표 속성을 두 개씩 부여하기 위해 사용됩니다.
    // 속성 값(int)을 갖는 배열입니다.
    // 이것의 인덱스는 목표 그래프에서 몇 번째 꼭지점인지를 나타냅니다.
    // players[playerPermutation[elementPermutation[0]]]의 목표는 players[playerPermutation[elementPermutation[1]]] 또는 players[playerPermutation[elementPermutation[2]]]를 잡는 것입니다.
    // 1의 목표는 2 또는 4를 잡는 것입니다. (반복은 생략)
    // 2의 목표는 3 또는 1을 잡는 것입니다.
    // 3의 목표는 4 또는 0을 잡는 것입니다.
    // 4의 목표는 0 또는 3을 잡는 것입니다.

    private SyncListBool isWin = new SyncListBool(); // 인덱스는 players의 인덱스 기준, 그 플레이어가 승리하면 true

    private SyncListBool playerConfirmStat = new SyncListBool();
    // playerConfirmStat은 각 플레이어가 능력치 분배를 확정지었는지 여부를 나타냅니다.
    // 이것의 인덱스는 플레이어 인덱스와 같습니다.
    // 이것의 값이 true이면 그 플레이어는 능력치 패널에서 능력치를 변경할 수 없습니다.
    // 이것의 값이 false일 때만 그 플레이어가 직접 능력치를 변경할 수 있습니다.
    // 값을 바꿀 때는 항상 RpcPlayerConfirmStat(playerIndex, b)를 함께 불러주어야 합니다.

    [SyncVar] private int turnPlayer = 0;   // 현재 자신의 턴을 진행하는 플레이어 번호
    [SyncVar] private int turnStep;         // 턴의 단계 (0: 대전 시작, 1: 턴 시작, 2: 턴 진행자의 교환 상대 선택과 교환할 카드 선택,
                                            //           3: 교환당하는 자의 교환할 카드 선택, 4: 교환 중(카드를 낼 때와 받을 때 효과 발동),
                                            //           5: 빙결 발동, 6: 턴이 끝날 때 효과 발동, 7: 턴 종료,
                                            //           13: 능력치 분배 전 경험치 지급, 14: 능력치 분배 중)
    [SyncVar] private int turnNum = 0;      // 현재 몇 번째 턴인가?
    [SyncVar] private int objectPlayer = -1;              // 교환당하는 플레이어
    [SyncVar] private int turnPlayerCard = -1;            // 턴을 진행한 플레이어가 낸 카드
    [SyncVar] private int objectPlayerCard = -1;          // 교환당하는 플레이어가 낸 카드
    private Exchange exchange;

    private int tpcIndex;
    private int opcIndex;
    private Vector3 tpcPosition;
    private Vector3 opcPosition;
    private Quaternion tpcRotation;
    private Quaternion opcRotation;

    //private static CardDatabase cd;

    private List<int> cardcode = new List<int>();
    // cardcode의 인덱스는 어떤 플레이어의 손패에 카드가 있는지를 나타낸다.
    // 0 ~ 1: players[0]의 손패, 2 ~ 3: players[1]의 손패, 4 ~ 5: players[2]의 손패,
    // 6 ~ 7: players[3]의 손패, 8 ~ 9: players[4]의 손패
    // cardcode[i]의 값은 cardCode가 i인 Card의 cards에서의 인덱스와 일치한다.
    // 다시 말해, cardcode의 인덱스는 카드의 위치, cardcode의 값은 카드의 종류이다.

    private List<int> playerReady = new List<int>();

    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject exchangeLog;
    private List<GameObject> exchanges = new List<GameObject>();

    private void Awake()
    {
        bm = this;
        //Debug.Log("BattleManager Awake.");

        turnStep = 0;
        //cd = GetComponent<CardDatabase>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        //Debug.Log("playerNumber: " + LobbyManager.s_Singleton._playerNumber);
        //Debug.Log("numPlayers: " + LobbyManager.s_Singleton.numPlayers);
        StartCoroutine("StartGame");
    }

    private bool IsPlayerReady()
    {
        if (playerReady.Count != LobbyManager.s_Singleton.numPlayers) return false;
        /*
        for (int i = 0; i < LobbyManager.s_Singleton._playerNumber; i++)
        {
            if (playerReady.IndexOf(i) == -1) return false;
        }
        */
        return true;
    }

    IEnumerator StartGame()
    {
        yield return new WaitWhile(() => !IsPlayerReady());
        /* AUTO 시 주석 해제
        if (LobbyManager.s_Singleton.numPlayers == 1)
        {
            for (int i = 0; i < 5; i++)
            {
                int j;
                if ((j = playerReady.IndexOf(i)) != -1)
                {
                    players[i].GetComponent<PlayerControl>().enabled = false;
                    foreach (Renderer r in players[i].GetComponentsInChildren<Renderer>()) r.enabled = false;
                    foreach (Text t in players[i].GetComponentsInChildren<Text>()) t.enabled = false;
                    bm.players[i] = null;
                    playerReady[j] = -1;
                }
            }
        }
        */

        // add AI players.
        if (playerReady.IndexOf(0) == -1)
        {
            var p1 = Instantiate(player, new Vector3(0f, 0f, 0f), Quaternion.identity);
            p1.GetComponent<PlayerControl>().SetAI(true);
            p1.GetComponent<PlayerControl>().playerName = "Psychia";
            p1.GetComponent<PlayerControl>().color = new Color(0.816f, 0.216f, 0.059f);
            p1.GetComponent<NetworkIdentity>().localPlayerAuthority = false;
            NetworkServer.Spawn(p1);
            p1.GetComponent<PlayerControl>().RpcSetAI(true);
        }
        if (playerReady.IndexOf(1) == -1)
        {
            var p2 = Instantiate(player, new Vector3(3.236f, 0f, 2.351f), Quaternion.Euler(0f, -72f, 0f));
            p2.GetComponent<PlayerControl>().SetAI(true);
            p2.GetComponent<PlayerControl>().playerName = "Xianzhe";
            p2.GetComponent<PlayerControl>().color = new Color(0.875f, 0.867f, 0.529f);
            p2.GetComponent<NetworkIdentity>().localPlayerAuthority = false;
            NetworkServer.Spawn(p2);
            p2.GetComponent<PlayerControl>().RpcSetAI(true);
        }
        if (playerReady.IndexOf(2) == -1)
        {
            var p3 = Instantiate(player, new Vector3(2f, 0f, 6.155f), Quaternion.Euler(0f, -144f, 0f));
            p3.GetComponent<PlayerControl>().SetAI(true);
            p3.GetComponent<PlayerControl>().playerName = "Orion";
            p3.GetComponent<PlayerControl>().color = new Color(0.106f, 0.69f, 0.208f);
            p3.GetComponent<NetworkIdentity>().localPlayerAuthority = false;
            NetworkServer.Spawn(p3);
            p3.GetComponent<PlayerControl>().RpcSetAI(true);
        }
        if (playerReady.IndexOf(3) == -1)
        {
            var p4 = Instantiate(player, new Vector3(-2f, 0f, 6.155f), Quaternion.Euler(0f, 144f, 0f));
            p4.GetComponent<PlayerControl>().SetAI(true);
            p4.GetComponent<PlayerControl>().playerName = "Libertas";
            p4.GetComponent<PlayerControl>().color = new Color(0.424f, 0.376f, 1f);
            p4.GetComponent<NetworkIdentity>().localPlayerAuthority = false;
            NetworkServer.Spawn(p4);
            p4.GetComponent<PlayerControl>().RpcSetAI(true);
        }
        if (playerReady.IndexOf(4) == -1)
        {
            var p5 = Instantiate(player, new Vector3(-3.236f, 0f, 2.351f), Quaternion.Euler(0f, 72f, 0f));
            p5.GetComponent<PlayerControl>().SetAI(true);
            p5.GetComponent<PlayerControl>().playerName = "Lucifer";
            p5.GetComponent<PlayerControl>().color = new Color(0.369f, 0.106f, 0.427f);
            p5.GetComponent<NetworkIdentity>().localPlayerAuthority = false;
            NetworkServer.Spawn(p5);
            p5.GetComponent<PlayerControl>().RpcSetAI(true);
        }

        List<int> temp = RandomListGenerator(5);
        for (int i = 0; i < 5; i++)
        {
            playerPermutation.Add(temp[i]);
            // players[playerPermutation[n]]으로 n번 속성을 갖는 플레이어를 참조할 수 있다.
            // players[n].GetPlayerElement()로 n번째 플레이어의 속성을 참조할 수 있다.
        }

        temp = RandomListGenerator(5);
        for (int i = 0; i < 5; i++)
        {
            RpcPrintLog("Player" + i + "'s element is " + GetPlayerElement(i));
            isWin.Add(false);
            playerConfirmStat.Add(true);
            elementPermutation.Add(temp[i]);
        }

        for (int i = 0; i < 10; i++)
        {
            if (cardcode.Count <= i)
                cardcode.Add(i);
        }
        CardPermutation();

        long cv = 0;
        for (int i = 0; i < 10; i++)
        {
            cv += cardcode[i] * (long)(Mathf.Pow(10, 9 - i));
        }
        //RpcPrintLog("" + cv);
        RpcSetCardIndex(cv);

        yield return new WaitForSeconds(5.4f);

        for (int i = 0; i < 10; i++)
        {
            cards[cardcode[i]].GetComponent<Card>().RpcMoveCard(100 + i);
        }
        turnPlayer = Random.Range(0, 5);
        
        // TODO 임시 코드
        string m = "cardcode";
        for (int i = 0; i < 10; i++)
        {
            m += " " + bm.GetCardCode()[i];
        }
        RpcPrintLog(m);
        
        yield return new WaitForSeconds(3f);

        //RpcPrintLog("Battle starts.");
        //RpcPrintLog("turnStep 1(" + players[turnPlayer].GetName() + " turn starts)");

        turnStep = 13;  // 능력치 분배 먼저
    }

    /*
    // called when a player is removed for a client
    public override void OnServerRemovePlayer(NetworkConnection conn, short playerControllerId)
    {
        PlayerController player;
        if (conn.GetPlayer(playerControllerId, out player))
        {
            if (player.NetworkIdentity != null && player.NetworkIdentity.gameObject != null)
                NetworkServer.Destroy(player.NetworkIdentity.gameObject);
        }
    }
    */

    void FixedUpdate()
    {
        if (!isServer) return;

        for (int i = 0; i < 5; i++)
        {
            if (turnStep > 0 && players[i] == null) {
                // 중간에 클라이언트 한 명의 접속이 끊긴 경우 대전을 종료한다.
                StartCoroutine(ReturnToLobby(5f, false));
                turnStep = 12;
            }
        }

        if (turnStep == 1)
        {
            turnNum++;
            // 빙결된 상태이면 교환을 할 수 없다.
            if (players[turnPlayer].HasFreezed())
            {
                turnStep = 5;
                //RpcPrintLog("turnStep 5(freezed)");
            }
            else
            {
                turnStep = 2;
                //RpcPrintLog("turnStep 2(select a player to exchange with, and select a card to play)");
            }
        }
        else if (turnStep == 2)
        {
            // TODO 플레이어의 응답을 기다리고 제한 시간 측정하기
        }
        else if (turnStep == 3)
        {
            // TODO 플레이어의 응답을 기다리고 제한 시간 측정하기
            if (turnPlayerCard != -1 && objectPlayerCard != -1)
            {
                turnStep = 4;
                //RpcPrintLog("turnStep 4(preprocessing)");
            }
        }
        else if (turnStep == 4)
        {
            /*
            // TODO 임시 코드
            string y = "before cardcode";
            for (int i = 0; i < 10; i++)
            {
                y += " " + bm.GetCardCode()[i];
            }
            RpcPrintLog(y);
            */
            GameObject ex = Instantiate(exchangeLog);
            exchange = ex.GetComponent<Exchange>();
            exchange.SetExchange(players[turnPlayer], players[objectPlayer], GetCard(turnPlayerCard), GetCard(objectPlayerCard));
            NetworkServer.Spawn(ex);
            //exchange = new Exchange(players[turnPlayer], players[objectPlayer], GetCard(turnPlayerCard), GetCard(objectPlayerCard));
            RpcSetOpponentCard(turnPlayer, objectPlayer, turnPlayerCard, objectPlayerCard);
            if (exchange.GetTurnNum() == 0)
            {
                //RpcPrintLog("Exception throwed while making an exchange!");
                turnStep = 10;
                return;
            }
            tpcIndex = cardcode.IndexOf(exchange.GetTurnPlayerCard().GetCardCode());
            opcIndex = cardcode.IndexOf(exchange.GetObjectPlayerCard().GetCardCode());

            cards[cardcode[tpcIndex]].GetComponent<Card>().RpcMoveCard(tpcIndex * 10 + opcIndex);
            cards[cardcode[opcIndex]].GetComponent<Card>().RpcMoveCard(opcIndex * 10 + tpcIndex);

            // 손패 교환
            int temp = cardcode[tpcIndex];
            cardcode[tpcIndex] = cardcode[opcIndex];
            cardcode[opcIndex] = temp;
            if (LobbyManager.s_Singleton.numPlayers != 1)   // Single Play 모드가 아닌 경우에만 클라이언트에게 교환 적용
                RpcExchangeCardIndex(tpcIndex, opcIndex);

            /**/
            // TODO 임시 코드
            string m = "cardcode";
            for (int i = 0; i < 10; i++)
            {
                m += " " + bm.GetCardCode()[i];
            }
            RpcPrintLog(m);
            /**/

            turnStep = 9;

        }
        else if (turnStep == 5)
        {
            // 빙결된 이펙트 보여주고 잠시 딜레이
            GameObject ex = Instantiate(exchangeLog);
            exchange = ex.GetComponent<Exchange>();
            exchange.SetFreezed(players[turnPlayer]);
            NetworkServer.Spawn(ex);

            players[turnPlayer].Freeze();
            players[turnPlayer].RpcFreeze();
            turnStep = 11;
        }
        else if (turnStep == 6)
        {
            List<Card> hand = GetPlayerHand(players[turnPlayer]);
            // 턴을 진행한 플레이어가 타락 카드를 들고 있으면 타락한다!
            if (hand[0].GetCardName() == "Corruption" || hand[1].GetCardName() == "Corruption")
            {
                players[turnPlayer].Corrupted(); // TODO
            }

            turnStep = 7;
            //RpcPrintLog("turnStep 7(turn ends)");
        }
        else if (turnStep == 7)
        {
            bool isEnd = false;
            for (int i = 0; i < 5; i++)
            {
                players[i].UpdateHealthAndStat();
            }
            exchange.SetTurnPlayerHealthVariation(players[exchange.GetTurnPlayer().GetPlayerIndex()].GetHealth());
            if (exchange.GetObjectPlayer() != null)
                exchange.SetObjectPlayerHealthVariation(players[exchange.GetObjectPlayer().GetPlayerIndex()].GetHealth());

            if (exchange.GetIsComplete())
            {
                exchanges.Add(exchange.gameObject);
                RpcAddExchangeLog(exchange.gameObject);
                for (int i = 0; i < 5; i++)
                {
                    GetPlayers()[i].UnveilFromExchangeLog();
                }
            }

            // 대전 규칙: 한 명이라도 사망하면 게임이 끝나고, 게임이 끝나는 시점에 "자신의 목표 중 사망자의 수"가 가장 많은 플레이어들이 승리한다.
            for (int i = 0; i < 5; i++)
            {
                if (!players[i].HasDead())
                {
                    // 두 명이 동시에 사망하였는데 그 두 명을 모두 목표로 하는 플레이어가 있다면 그 플레이어의 단독 승리!
                    if (players[GetTarget(i)[0]].HasDead()
                        && players[GetTarget(i)[1]].HasDead())
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            isWin[j] = false;
                        }
                        isWin[i] = true;
                        RpcPlayerWinIndex(i);
                        isEnd = true;
                        break;
                    }
                    // 한 명만 사망하였거나, 두 명이 동시에 사망하더라도 그 두 명을 모두 목표로 하는 플레이어가 없으면, 사망자들 중 한 명을 목표로 하는 모든 플레이어들의 공동 승리!
                    else if (players[GetTarget(i)[0]].HasDead()
                        || players[GetTarget(i)[1]].HasDead())
                    {
                        isWin[i] = true;
                        RpcPlayerWinIndex(i);
                        isEnd = true;
                    }
                }
            }
            if (isEnd)
            {
                /*
                for (int i = 0; i < 5; i++)
                {
                    if (isWin[i]) RpcPrintLog(players[i].GetName() + " wins!");
                }
                RpcPrintLog("Battle ends.");
                */
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        GetPlayers()[i].Unveil(j);
                    }
                }
                StartCoroutine(ReturnToLobby(13f, true));
                turnStep = 8;
            }
            else
            {
                turnStep = 16;
                StartCoroutine(Delay16(2f));    // 2초 후 turnStep 14가 됨

                //RpcPrintLog("Turn ends.");
                
            }
        }
        else if (turnStep == 8)
        {
            // 승자가 결정되어 대전이 정상적으로 종료되는 경우
        }
        else if (turnStep == 9)
        {
            // 카드 교환 애니메이션이 끝나면 카드의 위치가 바뀌고 turnStep 6이 됩니다.
        }
        else if (turnStep == 10)
        {
            // 교환(turnStep 4) 단계에서 오류가 생긴 경우
        }
        else if (turnStep == 11)
        {
            // 빙결 애니메이션이 끝나면 플레이어가 해동되고 turnStep 6이 됩니다.
        }
        else if (turnStep == 12)
        {
            // 클라이언트 한 명의 접속이 끊겨 대전이 비정상적으로 종료되는 경우
        }
        else if (turnStep == 13)
        {
            for (int i = 0; i < 5; i++)
            {
                GetPlayers()[i].ExperienceUp();
                SetPlayerConfirmStat(i, false);
            }
            turnStep = 15;
            StartCoroutine(Delay15(2f));    // 2초 후 turnStep 14가 됨
        }
        else if (turnStep == 14)
        {
            // TODO 능력치 분배 중
            bool b = true;
            for (int i = 0; i < 5; i++)
            {
                if (!GetPlayerConfirmStat(i))
                {
                    b = false;
                    break;
                }
            }
            if (b)
            {
                for (int i = 0; i < 5; i++)
                {
                    GetPlayers()[i].UpdateStat();
                    GetPlayers()[i].RpcEndStatDistribTime();
                }
                turnStep = 1;
            }
        }
        else if (turnStep == 15)
        {
            // Delay15 코루틴에 있는 동안 turnStep은 15입니다.
            // turnStep 13에서 turnStep 14로 넘어가는 경우에 해당합니다.
        }
        else if (turnStep == 16)
        {
            // Delay16 코루틴에 있는 동안 turnStep은 16입니다.
            // turnStep 7에서 turnStep 1 또는 turnStep 13으로 넘어가는 경우에 해당합니다.
        }
    }

    public void SetObjectPlayer(int objectTargetIndex)
    {
        if (turnStep != 2 || objectTargetIndex < 0 || objectTargetIndex >= 5) return;
        objectPlayer = objectTargetIndex;
        turnStep = 3;
        //RpcPrintLog("turnStep 3(select a card to play)");
    }

    public void SetCardToPlay(int cardCode, int playerIndex)
    {
        if (turnStep != 3 || cardCode < 0 || cardCode >= 10 || playerIndex < 0 || playerIndex >= 5) return;
        if (playerIndex == turnPlayer && turnPlayerCard == -1)
        {
            turnPlayerCard = cardCode;
            //RpcPrintLog(players[playerIndex].GetName() + " sets " + GetCard(cardCode).GetCardName() + " card to play.");
        }
        else if (playerIndex == objectPlayer && objectPlayerCard == -1)
        {
            objectPlayerCard = cardCode;
            //RpcPrintLog(players[playerIndex].GetName() + " sets " + GetCard(cardCode).GetCardName() + " card to play.");
        }
    }

    /// <summary>
    /// player의 손에 있는 카드 두 장의 목록을 받아옵니다.
    /// 정상적인 경우 배열의 크기는 2이고, 그렇지 않은 경우 null이 반환됩니다.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public List<Card> GetPlayerHand(PlayerControl player)
    {
        List<Card> hand = new List<Card>();
        int playerNum = players.IndexOf(player);
        if (playerNum != -1 && turnStep > 0)
        {
            hand.Add(cards[cardcode[playerNum * 2]].GetComponent<Card>());
            hand.Add(cards[cardcode[playerNum * 2 + 1]].GetComponent<Card>());
            return hand;
        }
        else return null;
    }

    /// <summary>
    /// 현재 턴을 진행하는 플레이어를 반환합니다.
    /// </summary>
    /// <returns></returns>
    public PlayerControl GetTurnPlayer()
    {
        return players[turnPlayer];
    }

    /// <summary>
    /// 현재 교환 요청을 받은 플레이어를 반환합니다.
    /// </summary>
    /// <returns></returns>
    public PlayerControl GetObjectPlayer()
    {
        if (objectPlayer == -1) return null;
        return players[objectPlayer];
    }

    public int GetTurnStep()
    {
        return turnStep;
    }

    public SyncListBool GetIsWin()
    {
        return isWin;
    }

    public bool GetPlayerConfirmStat(int playerIndex)
    {
        return playerConfirmStat[playerIndex];
    }

    /// <summary>
    /// 카드들의 목록을 반환합니다. 주의: 이 리스트의 순서는 카드 배치 순서를 반영하지 않습니다! 카드 배치 순서를 얻고 싶다면 GetCardCode()를 사용하십시오.
    /// </summary>
    /// <returns></returns>
    public List<GameObject> GetCardsInHand()
    {
        return cards;
    }


    public Card GetPlayerSelectedCard(PlayerControl player)
    {
        if (turnStep != 3 || player == null) return null;
        else if (player.Equals(players[turnPlayer]))
        {
            return GetCard(turnPlayerCard);
        }
        else if (player.Equals(players[objectPlayer]))
        {
            return GetCard(objectPlayerCard);
        }
        else
        {
            //Debug.Log("a");
            return null;
        }
    }

    /// <summary>
    /// 교환 로그 목록 전체를 반환합니다.
    /// </summary>
    /// <returns></returns>
    public List<Exchange> GetExchanges()
    {
        List<Exchange> exc = new List<Exchange>();
        for (int i = 0; i < exchanges.Count; i++)
        {
            if (exchanges[i] == null) continue;
            exc.Add(exchanges[i].GetComponent<Exchange>());
        }
        return exc;
    }

    /// <summary>
    /// 교환 처리 이후의 단계로 넘어가기 위해 호출되는 함수입니다.
    /// </summary>
    public void AfterExchange()
    {
        if (turnStep != 9) return;

        RpcSetExchangeComplete();
        objectPlayer = -1;
        turnPlayerCard = -1;
        objectPlayerCard = -1;

        turnStep = 6;
        //Debug.Log("turnStep 6(postprocessing)");
    }

    /// <summary>
    /// 빙결 처리 이후의 단계로 넘어가기 위해 호출되는 함수입니다.
    /// </summary>
    public void AfterFreezed()
    {
        if (turnStep != 11) return;
        //RpcPrintLog("AfterFreezed");

        turnStep = 6;

        // 턴을 진행한 플레이어가 빙결된 상태였으면 해동된다.
        players[turnPlayer].Thawed();
        //Debug.Log("turnStep 6(postprocessing)");
    }

    public void PlayerReady(int playerIndex)
    {
        playerReady.Add(playerIndex);
    }

    public void SetPlayerConfirmStat(int playerIndex, bool b)
    {
        playerConfirmStat[playerIndex] = b;
        RpcSetPlayerConfirmStat(playerIndex, b);
    }

    private List<int> RandomListGenerator(int n)
    {
        List<int> rand = new List<int>();
        bool[] check = new bool[n];
        int r;
        for (int i = n; i > 0; i--)
        {
            r = Random.Range(0, i);
            for (int j = 0; j < n; j++)
            {
                if (!check[j])
                {
                    if (r == 0)
                    {
                        rand.Add(j);
                        check[j] = true;
                        break;
                    }
                    r--;
                }
            }
        }
        return rand;
    }

    private void CardPermutation()
    {
        List<int> rand1 = RandomListGenerator(5);
        List<int> rand2 = RandomListGenerator(5);
        List<int> c = new List<int>();
        for (int i = 0; i < 5; i++)
        {
            c.Add(cardcode[rand1[i]]);
            c.Add(cardcode[rand2[i] + 5]);
        }
        cardcode = c;
    }

    /// <summary>
    /// 인자로 주어진 cardCode와 같은 코드를 갖는 카드를 반환합니다.
    /// </summary>
    /// <param name="cardCode"></param>
    /// <returns></returns>
    public Card GetCard(int cardCode)
    {
        if (cardCode < 0 || cardCode >= 10)
            return null;
        return cards[cardCode].GetComponent<Card>();
    }

    public Card GetCardInPosition(int cardPosition)
    {
        if (cardPosition < 0 || cardPosition >= 10 || turnStep <= 0) return null;
        return cards[cardcode[cardPosition]].GetComponent<Card>();
    }

    /// <summary>
    /// 카드의 배치 상태 리스트를 반환합니다.
    /// 만약 i번째 자리에 배치된 카드에 접근하고 싶다면 GetCardsInHand()[GetCardCode()[i]].GetComponent<Card>()를 사용하십시오.
    /// </summary>
    /// <returns></returns>
    public List<int> GetCardCode()
    {
        return cardcode;
    }

    /// <summary>
    /// 인자로 주어진 플레이어가 잡아야 하는 목표의 번호 리스트를 반환합니다.
    /// 잘못된 입력이 주어지지 않는다면, 반환되는 리스트의 크기는 2입니다.
    /// </summary>
    /// <param name="playerIndex">플레이어 번호</param>
    /// <returns></returns>
    public List<int> GetTarget(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= 5 || playerPermutation.Count != 5 || elementPermutation.Count != 5) return null;

        List<int> t = new List<int>();
        int i = elementPermutation.IndexOf(GetPlayerElement(playerIndex));

        switch (i)
        {
            case 0:
                t.Add(playerPermutation[elementPermutation[1]]);
                t.Add(playerPermutation[elementPermutation[2]]);
                break;
            case 1:
                t.Add(playerPermutation[elementPermutation[2]]);
                t.Add(playerPermutation[elementPermutation[4]]);
                break;
            case 2:
                t.Add(playerPermutation[elementPermutation[3]]);
                t.Add(playerPermutation[elementPermutation[1]]);
                break;
            case 3:
                t.Add(playerPermutation[elementPermutation[4]]);
                t.Add(playerPermutation[elementPermutation[0]]);
                break;
            case 4:
                t.Add(playerPermutation[elementPermutation[0]]);
                t.Add(playerPermutation[elementPermutation[3]]);
                break;
        }
        return t;
    }

    public List<PlayerControl> GetPlayers()
    {
        return players;
    }

    public int GetPlayerElement(int playerIndex)
    {
        return playerPermutation.IndexOf(playerIndex);
    }
    
    // TODO 임시 코드
    [ClientRpc]
    public void RpcPrintLog(string msg)
    {
        //ConsoleLogUI.AddText(msg);
        Debug.Log(msg);
    }

    [ClientRpc]
    private void RpcExchangeCardIndex(int tpc, int opc)
    {
        int temp = cardcode[tpc];
        cardcode[tpc] = cardcode[opc];
        cardcode[opc] = temp;
    }

    [ClientRpc]
    private void RpcPlayerWinIndex(int i)
    {
        isWin[i] = true;
    }

    [ClientRpc]
    private void RpcSetPlayerConfirmStat(int playerIndex, bool b)
    {
        playerConfirmStat[playerIndex] = b;
    }

    [ClientRpc]
    private void RpcAddExchangeLog(GameObject ex)
    {
        exchanges.Add(ex);
    }

    [ClientRpc]
    private void RpcSetExchangeComplete()
    {
        pusher.SetExchangeComplete();
    }

    [ClientRpc]
    private void RpcSetOpponentCard(int TP, int OP, int TPCardCode, int OPCardCode)
    {
        pusher.SetOpponentCard(TP, OP, TPCardCode, OPCardCode);
    }

    [ClientRpc]
    private void RpcSetCardIndex(long values)
    {
        for (int i = 0; i < 10; i++)
        {
            if (cardcode.Count <= i) cardcode.Add(-1);
            cardcode[i] = (int)((values % (long)Mathf.Pow(10, 10 - i)) / (long)Mathf.Pow(10, 9 - i));
            //ConsoleLogUI.AddText("cardcode[" + i + "] = " + cardcode[i]);
        }
    }
    /*
    [ClientRpc]
    private void RpcAlert(int i)
    {
        for (int j = 0; j < 5; j++)
        {
            players[j].CAlert(i);
        }
    }
    */

    IEnumerator ReturnToLobby(float timing, bool normal)
    {
        yield return new WaitForSeconds(timing);
        loadingPanel.SetActive(true);
        if (!normal) NetworkServer.DisconnectAll();
        LobbyManager.s_Singleton.ServerReturnToLobby();
    }

    IEnumerator Delay15(float second)
    {
        if (turnStep != 15) yield break;
        yield return new WaitForSeconds(second);

        for (int i = 0; i < 5; i++)
        {
            GetPlayers()[i].RpcOpenStatPanel();
        }
        turnStep = 14;
    }

    IEnumerator Delay16(float second) {
        if (turnStep != 16) yield break;
        yield return new WaitForSeconds(second);

        turnPlayer += 1;
        if (turnPlayer >= 5) turnPlayer = 0;

        if (turnNum % 5 == 0) turnStep = 13;    // 한 사이클이 끝나서 능력치 분배 시간이 됨
        else turnStep = 1;
        //RpcPrintLog("turnStep 1(" + players[turnPlayer].GetName() + " turn starts)");
    }
}
