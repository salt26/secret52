using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Prototype.NetworkLobby;

public class PlayerControl : NetworkBehaviour
{

    [SyncVar] private int currentHealth;    // 현재 남은 체력(실시간으로 변화, 외부 열람 불가)
    [SyncVar] private int currentAttack;    // 현재 공격력(실시간으로 변화, 외부 열람 불가)
    [SyncVar] private int currentAuthority;    // 현재 권력(실시간으로 변화, 외부 열람 불가)
    [SyncVar] private int currentMentality;    // 현재 권력(실시간으로 변화, 외부 열람 불가)
    [SyncVar] private int currentExperience;   // 현재 남은 경험치(실시간으로 변화, 외부 열람 불가)
    [SerializeField] [SyncVar] private int maxHealth = 52;      // 최대 체력(초기 체력)
    [SyncVar] private GameObject character; // 캐릭터 모델
    [SyncVar] private bool isDead = false;  // 사망 여부(true이면 사망)
    [SyncVar] public string playerName;     // 플레이어 이름
    [SyncVar] public int playerNum;         // 대전에서 부여된 플레이어 번호 (1 ~ 5)
    [SyncVar] public Color color = Color.white;

    [SyncVar] private int displayedHealth;                      // 현재 남은 체력(턴이 끝날 때만 변화, 외부 열람 가능)
    [SerializeField] [SyncVar] private int statAttack;      // 현재 공격력(외부 열람 가능)
    [SerializeField] [SyncVar] private int statAuthority;   // 현재 권력(외부 열람 가능)
    [SerializeField] [SyncVar] private int statMentality;   // 현재 정신력(외부 열람 가능)
    [SerializeField] [SyncVar] private int experience = 0;      // 현재 남은 경험치
    [SyncVar] private bool isFreezed = false;                   // 빙결 여부(true이면 다음 한 번의 내 턴에 교환 불가)

    private List<bool> unveiled = new List<bool>();
    // unveiled의 인덱스는 (플레이어 번호 - 1)이고, 그 값은 해당 플레이어의 속성이 이 플레이어에게 공개되었는지 여부이다.
    // 자기 자신의 속성은 항상 공개되어 있는 것으로 취급한다.

    private bool isAI = false;                      // 인공지능 플레이어 여부(true이면 인공지능, false이면 사람)
    private bool hasDecidedObjectPlayer = false;    // 내 턴에 교환 상대를 선택했는지 여부
    private bool hasDecidedPlayCard = false;        // 교환 시 교환할 카드를 선택했는지 여부
    private PlayerControl objectTarget;             // 내가 선택한 교환 대상
    private Card playCardAI;                        // 인공지능이 낼 카드

    private RectTransform HealthBar;                // HP UI
    [SerializeField] private GameObject playerCamera;

    private static BattleManager bm;
    private static CardDatabase cd;
    //private static Alert alert;

    private GameObject Border;
    private SpriteRenderer Face;
    private GameObject targetElementImage1;
    private GameObject targetElementImage2;
    private GameObject targetElementText;
    private GameObject targetElementBackground;
    private GameObject cannotRequestTextB;
    private GameObject cannotRequestTextW1;
    private GameObject cannotRequestTextW2;
    private SpriteRenderer elementSprite;
    private TooltipUI tooltip;

    public GameObject Ice;
    public GameObject targetMark;
    public GameObject healthText;
    public GameObject attackUIText;
    public GameObject authorityUIText;
    public GameObject tooltipBox;
    private bool isMarked; //마크가 되었는지 여부
    private bool isPlayingCannotRequest;

    private bool isAlerted0;
    private bool isAlerted1;
    private bool isAlerted2;
    private bool isAlerted3;

    private bool isStart;
    private bool isThinking;    // 인공지능의 생각 전 딜레이 동안 true가 됨
    private bool isCardDragging;  // 큰 카드를 드래그하는 동안 true가 됨

    void Awake () {
        // bm은 Awake에서 아직 로딩되지 않았을 수 있음. 즉, BattleManager.Awake가 PlayerControl.Awake보다 늦게 실행될 수 있음. 
        //alert = Alert.alert;
        HealthBar = GetComponentInChildren<Finder>().GetComponent<Image>().rectTransform;
        Border = GetComponentsInChildren<SpriteRenderer>()[1].gameObject;
        Face = GetComponentsInChildren<SpriteRenderer>()[0];
        targetElementImage1 = GameObject.Find("TargetElementImage1");
        targetElementImage2 = GameObject.Find("TargetElementImage2");
        targetElementText = GameObject.Find("TargetElementText");
        targetElementBackground = GameObject.Find("TargetElementBackground");
        cannotRequestTextB = GameObject.Find("CannotRequestTextB");
        cannotRequestTextW1 = GameObject.Find("CannotRequestTextW1");
        cannotRequestTextW2 = GameObject.Find("CannotRequestTextW2");
        elementSprite = GetComponentsInChildren<SpriteRenderer>()[2];   // Player 프리팹의 하이어러키에서 스프라이트 순서 중요!
        tooltip = null;
        Border.SetActive(false);
        currentHealth = maxHealth;
        displayedHealth = currentHealth;
        isPlayingCannotRequest = false;
        isAlerted0 = false;
        isAlerted1 = false;
        isAlerted2 = false;
        isAlerted3 = false;
        isStart = false;
        isThinking = false;
        isCardDragging = false;
        statAttack = 13;     // 초기값 4로 설정
        statAuthority = Random.Range(1, 6);  // 초기값 1로 설정
        statMentality = 6;  // 초기값 6으로 설정
        currentAttack = statAttack;
        currentAuthority = statAuthority;
        currentMentality = statMentality;
        currentExperience = experience;
        experience = 0;
        for (int i = 0; i < 5; i++)
        {
            unveiled.Add(false);
        }
        if (transform.position.z < 1f)
        {
            playerNum = 1;
            unveiled[0] = true;
        }
        else if (transform.position.z < 4f)
        {
            if (transform.position.x > 0f)
            {
                playerNum = 2;
                unveiled[1] = true;
            }
            else
            {
                playerNum = 5;
                unveiled[4] = true;
            }
        }
        else
        {
            if (transform.position.x > 0f)
            {
                playerNum = 3;
                unveiled[2] = true;
            }
            else
            {
                playerNum = 4;
                unveiled[3] = true;
            }
        }
        //Log("Awake " + playerName);
    }

    void Start()
    {   
        // bm은 Start에서 아직 로딩되지 않았을 수 있음. 즉, BattleManager.Awake가 PlayerControl.Start보다 늦게 실행될 수 있음. 
        Renderer[] rends = GetComponentsInChildren<Renderer>();
        rends[0].material.color = color;
        //Log("Start " + playerName);
        /*
        if (GetPlayerIndex() != -1)
            bm.SetCameraVisible(GetPlayerIndex());
            */
    }

    // 개별 클라이언트에서만 보여져야 하는 것들을 이 함수 안에 넣습니다.
    // 서버와 일치되어야 하는 변수나 함수는 여기에서 빼야 합니다.
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        // bm은 Start에서 아직 로딩되지 않았을 수 있음. 즉, BattleManager.Awake가 PlayerControl.Start보다 늦게 실행될 수 있음. 
        //Log("OnStartLocalPlayer");
        playerCamera.SetActive(true);
        PushingCard.localPlayer = this;
        Pusher.localPlayer = this;
        Card.localPlayer = this;
        ObjectiveHighlight();
    }
    

    void FixedUpdate()
    {
        if (bm == null)
        {
            bm = BattleManager.bm;
            return;
        }
        if (!isStart)
        {
            bm.players[playerNum - 1] = this;
            isStart = true;
            //Log("FixedUpdate " + playerName);
            if (isLocalPlayer)
                CmdReady();
        }
        if (cd == null)
        {
            cd = CardDatabase.cardDatabase;
        }
        /*
        if (isLocalPlayer && Input.GetMouseButtonDown(1))
        {
            ConsoleLogUI.ClearText(); // TODO 임시 코드
            string m = "cardcode";
            for (int i = 0; i < 10; i++)
            {
                m += " " + bm.GetCardCode()[i];
            }
            Log(m);
        }
        */
        if (isLocalPlayer)
        {
            StatusUpdate();
        }
        if (isLocalPlayer && Input.GetMouseButton(0) && Input.touchCount <= 1 && !isCardDragging)
        {
            /*
            if (bm.GetObjectPlayer() != null)
                Log("Mouse Clicked. bm.GetTurnStep(): " + bm.GetTurnStep() + ", bm.GetTurnPlayer(): " + bm.GetTurnPlayer().GetName() + ", bm.GetObjectPlayer(): " + bm.GetObjectPlayer().GetName());
            else Log("Mouse Clicked. bm.GetTurnStep(): " + bm.GetTurnStep() + ", bm.GetTurnPlayer(): " + bm.GetTurnPlayer().GetName());
            *//*
            if (bm.GetTurnStep() == 2 && objectTarget != null && bm.GetTurnPlayer().Equals(this))
                PlayerToSelectCard();
            if (bm.GetTurnStep() == 3 && bm.GetObjectPlayer() != null && bm.GetObjectPlayer().Equals(this))
                PlayerToSelectCard();
                */
            if (bm.GetTurnStep() == 2 && bm.GetTurnPlayer().Equals(this))
            {
                PlayerToSelectTarget();
            }
        }

        if (bm.GetTurnStep() == 3 && isMarked == true)
            Destroy(GameObject.Find("TargetMark(Clone)"));

        HealthBar.sizeDelta = new Vector2(displayedHealth * 100f / maxHealth, HealthBar.sizeDelta.y); // HealthBar 변경 -> displayedHealth 기준으로 계산하도록 수정
        healthText.GetComponent<Text>().text = displayedHealth.ToString();
        attackUIText.GetComponent<Text>().text = statAttack.ToString();
        authorityUIText.GetComponent<Text>().text = statAuthority.ToString();

        /* 작은 카드의 툴팁을 보여주기 위한 코드입니다. */
        // 앞면인 작은 카드를 클릭하고 있는 동안에 카드 이름과 설명을 포함한 툴팁이 나타납니다.
        // 뒷면인 작은 카드를 클릭하고 있는 동안에 비공개 공격 카드 설명을 포함한 툴팁이 나타납니다.
        if (isLocalPlayer && Input.GetMouseButton(0) && Input.touchCount <= 1 && !isCardDragging)
        {
            List<Card> hand = bm.GetPlayerHand(this);
            Ray ray = GetComponentInChildren<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, (1 << 9)))
            {
                //Log("Click " + hit.collider.name + ".");
                Debug.DrawLine(ray.origin, hit.point, Color.yellow, 3f);
                if (hit.collider.gameObject.GetComponentInParent<Card>() != null
                    && (hit.collider.gameObject.GetComponentInParent<Card>().Equals(hand[0])
                    || hit.collider.gameObject.GetComponentInParent<Card>().Equals(hand[1])
                    || hit.collider.gameObject.GetComponentInParent<Card>().GetCardCode() >= 5)
                    && Alert.alert != null && cd != null && tooltip == null)
                {
                    Card c = hit.collider.gameObject.GetComponentInParent<Card>();
                    GameObject t = Instantiate(tooltipBox, Alert.alert.gameObject.transform);   // Alert.alert.gameObject는 메인 Canvas
                    tooltip = t.GetComponent<TooltipUI>();
                    tooltip.SetText(cd.GetCardInfo(c).GetNameText(),
                        cd.GetCardInfo(c).GetColor(), cd.GetCardInfo(c).GetDetailText());
                    tooltip.Appear();
                }
                else if (hit.collider.gameObject.GetComponentInParent<Card>() != null
                    && Alert.alert != null && cd != null && tooltip == null)
                {
                    GameObject t = Instantiate(tooltipBox, Alert.alert.gameObject.transform);   // Alert.alert.gameObject는 메인 Canvas
                    tooltip = t.GetComponent<TooltipUI>();
                    tooltip.SetText("받을 때 피해를 받는 공격 카드입니다. 상대 손에 있을 때는 공개되지 않습니다.");
                    tooltip.Appear();
                }
                else if ((hit.collider.gameObject.GetComponentInParent<Card>() == null
                    /*|| !(hit.collider.gameObject.GetComponentInParent<Card>().Equals(hand[0])
                    || hit.collider.gameObject.GetComponentInParent<Card>().Equals(hand[1])
                    || hit.collider.gameObject.GetComponentInParent<Card>().GetCardCode() >= 5)*/) && tooltip != null)
                {
                    // 마우스로 클릭하여 닿은 것이 작은 카드가 아닌 경우, 상대가 들고 있는 공격 카드인 경우
                    tooltip.Disappear();
                }
            }
            else if (tooltip != null)   // 마우스를 클릭했지만 클릭한 지점이 아무 것과도 닿지 않은 경우
            {
                tooltip.Disappear();
            }
        }
        else if (isLocalPlayer && tooltip != null)  // 클릭하지 않고 있는 경우, 큰 카드를 드래그중인 경우, 두 곳 이상을 동시 터치한 경우
        {
            tooltip.Disappear();
        }

        if (isServer && isAI && bm.GetTurnStep() == 2 && bm.GetTurnPlayer().Equals(this) && !isThinking)
        {
            isThinking = true;
            StartCoroutine("AITurnDelay");
        }
        if (isServer && isAI && bm.GetTurnStep() == 3 && bm.GetObjectPlayer() != null && bm.GetObjectPlayer().Equals(this) && !isThinking)
        {
            isThinking = true;
            StartCoroutine("AIExchangeDelay");
        }
    }


    public void Damaged(int amount)
    {
        if (!isServer) return;
        if (amount > 0 && currentHealth > 0)
        {
            currentHealth -= amount;
        }
    }

    public void Restored()
    {
        if (!isServer) return;
        if (currentHealth > 0 && currentHealth <= 47)
        {
            currentHealth += 5;
        }
        else if (currentHealth <= 48)
        {
            currentHealth = 52;
        }
    }

    public void Freezed()
    {
        if (!isServer) return;
        if (!isDead)
        {
            isFreezed = true;
            //Log(playerName + " is freezed.");
        }
    }

    public void Thawed()
    {
        if (!isServer) return;
        if (!isDead && isFreezed)
        {
            isFreezed = false;
        }
    }

    public void Lighted()
    {
        if (!isServer) return;
        if (currentAuthority < 99) currentAuthority++;
        else currentAuthority = 99;
    }

    public void Corrupted()
    {
        if (!isServer) return;
        if (currentAttack < 99) currentAttack++;
        else currentAttack = 99;
        if (currentMentality > 1) currentMentality--;
        else currentMentality = 1;
    }

    public void Unveil(int playerIndex)
    {
        if (!isServer || playerIndex < 0 || playerIndex >= 5 || unveiled[playerIndex]) return;
        unveiled[playerIndex] = true;
        RpcUnveil(playerIndex);
    }

    [ClientRpc]
    public void RpcUnveil(int playerIndex)
    {
        if (!isLocalPlayer) return;
        unveiled[playerIndex] = true;
        if (bm == null) return;
        bm.GetPlayers()[playerIndex].SetElementSprite();
        // Highlight
        if (bm.GetTarget(GetPlayerIndex()).IndexOf(playerIndex) != -1)
        {
            bm.GetPlayers()[playerIndex].SetHighlight(true);
        }
    }

    public void UpdateHealthAndStat()
    {
        if (!isServer) return;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
        }
        int HealthChange = displayedHealth - currentHealth;

        if (HealthChange < 0)
        {
            //bm.RpcPrintLog(playerName + " is Healed.");
            RpcHealed(); //힐을 받음
        }
        else if (HealthChange > 0 && isDead == false)
        {
            //bm.RpcPrintLog(playerName + " is Damaged.");
            RpcDamaged(); //데미지를 받음
        }
        else if (isDead == true)
        {
            //bm.RpcPrintLog(playerName + " is Dead.");
            RpcDead(); //뒤짐
        }
        displayedHealth = currentHealth;
        statAttack = currentAttack;
        statAuthority = currentAuthority;
        statMentality = currentMentality;
    }

    public void UnveilFromExchangeLog()
    {
        if (!isServer || bm == null) return;
        List<Exchange> exchanges = bm.GetExchanges();
        List<int> unknowns = GetUnknownElements();
        List<List<bool>> table = new List<List<bool>>();
        bool again = false; // 간접적으로 누군가의 속성이 밝혀지면, 한 번 더 이 함수를 실행해서 새로운 누군가의 속성이 밝혀질 수 있다.
        for (int i = 0; i < 5; i++)
        {
            List<bool> row = new List<bool>();
            for (int j = 0; j < 5; j++)
            {
                row.Add(false);
            }
            table.Add(row);
        }
        if (exchanges.Count > 0)
        {
            // 마지막 교환에서 직접적으로 상대 속성을 알 수 있는 경우 (빛 카드를 사용하거나 상대 속성의 카드로 2의 피해를 준 경우)
            Exchange exc = exchanges[exchanges.Count - 1];
            if (exc.GetTurnPlayer() == this && exc.GetTurnPlayerCard().GetCardName().Equals("Light"))
            {
                Unveil(exc.GetObjectPlayer().GetPlayerIndex());
            }
            else if (exc.GetObjectPlayer() == this && exc.GetObjectPlayerCard().GetCardName().Equals("Light"))
            {
                Unveil(exc.GetTurnPlayer().GetPlayerIndex());
            }
            else if (exc.GetTurnPlayer() == this && exc.GetTurnPlayerCard().GetCardCode() == exc.GetObjectPlayer().GetPlayerElement()
                && !exc.GetObjectPlayerCard().GetCardName().Equals("Dark"))
            {
                Unveil(exc.GetObjectPlayer().GetPlayerIndex());
            }
            else if (exc.GetObjectPlayer() == this && exc.GetObjectPlayerCard().GetCardCode() == exc.GetTurnPlayer().GetPlayerElement()
                && !exc.GetTurnPlayerCard().GetCardName().Equals("Dark"))
            {
                Unveil(exc.GetTurnPlayer().GetPlayerIndex());
            }
        }
        foreach (Exchange exc in exchanges)
        {
            if (exc.GetTurnPlayer() == this && !GetUnveiled(exc.GetObjectPlayer().GetPlayerIndex())) {
                if (exc.GetTurnPlayerCard().GetCardName().Equals("Fire") && !exc.GetObjectPlayerCard().GetCardName().Equals("Dark")) {
                    table[exc.GetObjectPlayer().GetPlayerIndex()][0] = true;
                }
                else if (exc.GetTurnPlayerCard().GetCardName().Equals("Water") && !exc.GetObjectPlayerCard().GetCardName().Equals("Dark"))
                {
                    table[exc.GetObjectPlayer().GetPlayerIndex()][1] = true;
                }
                else if (exc.GetTurnPlayerCard().GetCardName().Equals("Electricity") && !exc.GetObjectPlayerCard().GetCardName().Equals("Dark"))
                {
                    table[exc.GetObjectPlayer().GetPlayerIndex()][2] = true;
                }
                else if (exc.GetTurnPlayerCard().GetCardName().Equals("Wind") && !exc.GetObjectPlayerCard().GetCardName().Equals("Dark"))
                {
                    table[exc.GetObjectPlayer().GetPlayerIndex()][3] = true;
                }
                else if (exc.GetTurnPlayerCard().GetCardName().Equals("Poison") && !exc.GetObjectPlayerCard().GetCardName().Equals("Dark"))
                {
                    table[exc.GetObjectPlayer().GetPlayerIndex()][4] = true;
                }

            }
            if (exc.GetObjectPlayer() == this && !GetUnveiled(exc.GetTurnPlayer().GetPlayerIndex()))
            {
                if (exc.GetObjectPlayerCard().GetCardName().Equals("Fire") && !exc.GetTurnPlayerCard().GetCardName().Equals("Dark"))
                {
                    table[exc.GetTurnPlayer().GetPlayerIndex()][0] = true;
                }
                else if (exc.GetObjectPlayerCard().GetCardName().Equals("Water") && !exc.GetTurnPlayerCard().GetCardName().Equals("Dark"))
                {
                    table[exc.GetTurnPlayer().GetPlayerIndex()][1] = true;
                }
                else if (exc.GetObjectPlayerCard().GetCardName().Equals("Electricity") && !exc.GetTurnPlayerCard().GetCardName().Equals("Dark"))
                {
                    table[exc.GetTurnPlayer().GetPlayerIndex()][2] = true;
                }
                else if (exc.GetObjectPlayerCard().GetCardName().Equals("Wind") && !exc.GetTurnPlayerCard().GetCardName().Equals("Dark"))
                {
                    table[exc.GetTurnPlayer().GetPlayerIndex()][3] = true;
                }
                else if (exc.GetObjectPlayerCard().GetCardName().Equals("Poison") && !exc.GetTurnPlayerCard().GetCardName().Equals("Dark"))
                {
                    table[exc.GetTurnPlayer().GetPlayerIndex()][4] = true;
                }

            }
        }
        for (int i = 0; i < 5; i++)
        {
            if (GetUnveiled(i)) continue;
            int count = 0;
            foreach (int j in unknowns)
            {
                if (table[i][j]) count++;
            }
            // 속성을 모르는 같은 상대에게 (모르는 속성 개수 - 1)번의 서로 다른 속성 공격을 한 경우
            // 상대의 속성을 확실히 알 수 있다.
            if (count >= unknowns.Count - 1)
            {
                Unveil(i);
                again = true;
                // TODO 잘 동작하는지 확인해 볼 것.
            }
        }

        // 속성을 모르는 다른 모든 상대에게 같은 속성 공격을 한 경우
        // 나머지 한 명의 속성을 확실히 알 수 있다.
        foreach (int j in unknowns)
        {
            int count1 = 0, count2 = 0, pi = -1;
            for (int i = 0; i < 5; i++)
            {
                if (GetUnveiled(i)) continue;
                count1++;
                if (table[i][j]) count2++;
                else pi = i;
            }

            if (count2 >= count1 - 1 && pi != -1)
            {
                Unveil(pi);
                again = true;
                // TODO 잘 동작하는지 확인해 볼 것.
            }
        }

        if (again) UnveilFromExchangeLog();
    }

    /// <summary>
    /// 능력치 분배 시에 경험치를 5 소모하여 권력을 1 올리는 함수입니다.
    /// 클라이언트에서만 호출 가능합니다.
    /// TODO 네트워크 상에서 잘 작동하는지 확인하기
    /// </summary>
    public void StatAuthorityUp()
    {
        if (!isLocalPlayer || bm == null || bm.GetPlayerConfirmStat(GetPlayerIndex())) return;
        if (currentAuthority < 99 && currentExperience >= 5)
        {
            currentAuthority++;
            currentExperience -= 5;
        }
        // TODO 경험치가 부족할 때 경고 메시지 띄우기
        // TODO 권력이 99 이상일 때 경고 메시지 띄우기
    }

    /// <summary>
    /// 능력치 분배 시에 경험치를 5 소모하여 공격력을 1 올리는 함수입니다.
    /// 클라이언트에서만 호출 가능합니다.
    /// TODO 네트워크 상에서 잘 작동하는지 확인하기
    /// </summary>
    public void StatAttackUp()
    {
        if (!isLocalPlayer || bm == null || bm.GetPlayerConfirmStat(GetPlayerIndex())) return;
        if (currentAttack < 99 && currentExperience >= 5)
        {
            currentAttack++;
            currentExperience -= 5;
        }
        // TODO 경험치가 부족할 때 경고 메시지 띄우기
        // TODO 공격력이 99 이상일 때 경고 메시지 띄우기
    }

    /// <summary>
    /// 능력치 분배 시에 경험치를 (현재 정신력 + 1) 소모하여 정신력을 1 올리는 함수입니다.
    /// 클라이언트에서만 호출 가능합니다.
    /// TODO 네트워크 상에서 잘 작동하는지 확인하기
    /// </summary>
    public void StatMentalityUp()
    {
        if (!isLocalPlayer || bm == null || bm.GetPlayerConfirmStat(GetPlayerIndex())) return;
        if (currentMentality < 99 && currentExperience >= currentMentality + 1)
        {
            currentMentality++;
            currentExperience -= currentMentality;
        }
        // TODO 경험치가 부족할 때 경고 메시지 띄우기
        // TODO 정신력이 99 이상일 때 경고 메시지 띄우기
    }

    /// <summary>
    /// 능력치 분배 시에 경험치와 공격력, 권력, 정신력을 분배 전으로 되돌리는 함수입니다.
    /// 클라이언트에서만 호출 가능합니다.
    /// TODO 네트워크 상에서 잘 작동하는지 확인하기
    /// </summary>
    public void StatRedo()
    {
        if (!isLocalPlayer || bm == null || bm.GetPlayerConfirmStat(GetPlayerIndex())) return;
        currentExperience = experience;
        currentAttack = statAttack;
        currentAuthority = statAuthority;
        currentMentality = statMentality;
    }

    /// <summary>
    /// 능력치 분배 시에 분배한 능력치를 확정짓는 함수입니다.
    /// 클라이언트에서만 호출 가능합니다.
    /// TODO 네트워크 상에서 잘 작동하는지 확인하기
    /// </summary>
    public void StatConfirm()
    {
        if (!isLocalPlayer || bm == null || bm.GetPlayerConfirmStat(GetPlayerIndex())) return;
        experience = currentExperience;
        statAttack = currentAttack;
        statAuthority = currentAuthority;
        statMentality = currentMentality;
        // BattleManager에게 능력치를 확정했다는 신호 보내기
        CmdConfirmStat();
    }

    /// <summary>
    /// 경험치를 현재 정신력만큼 상승시키는 함수입니다.
    /// </summary>
    [ClientRpc]
    public void RpcExperienceUp()
    {
        if (!isLocalPlayer) return;
        currentExperience += statMentality;
        if (currentExperience > 9999) currentExperience = 9999;
        experience = currentExperience;
    }

    [ClientCallback]
    public void PlayerToSelectTarget()
    {
        if (!isLocalPlayer) return;

        // 내 턴이 아니면 패스
        if (!bm.GetTurnPlayer().Equals(this)) return;
        Ray ray = GetComponentInChildren<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, (1 << 8)))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.blue, 3f);
            if (hit.collider.gameObject.GetComponentInParent<PlayerControl>() != null
                && !hit.collider.gameObject.GetComponentInParent<PlayerControl>().Equals(this))
            {
                if (objectTarget == null) 
                {
                    objectTarget = hit.collider.gameObject.GetComponentInParent<PlayerControl>();
                    if (CanRequestExchange(objectTarget.GetPlayerIndex()))
                    {
                        Instantiate(targetMark, hit.collider.gameObject.GetComponentInParent<PlayerControl>().transform);
                        isMarked = true;
                        //Log("Set " + hit.collider.gameObject.GetComponentInParent<PlayerControl>().GetName() + " to a target.");
                    }
                    else
                    {
                        // 교환 요청이 불가능하면 "권력 때문에 교환을 요청할 수 없습니다." 메시지 띄우기
                        StartCoroutine("CannotRequestExchange");
                        objectTarget = null;
                    }
                }
                else if (!objectTarget.Equals(hit.collider.gameObject.GetComponentInParent<PlayerControl>()))
                {
                    Destroy(GameObject.Find("TargetMark(Clone)"));
                    objectTarget = hit.collider.gameObject.GetComponentInParent<PlayerControl>();
                    if (CanRequestExchange(objectTarget.GetPlayerIndex()))
                    {
                        Instantiate(targetMark, hit.collider.gameObject.GetComponentInParent<PlayerControl>().transform);
                        isMarked = true;
                        //Log("Set " + hit.collider.gameObject.GetComponentInParent<PlayerControl>().GetName() + " to a target.");
                    }
                    else
                    {
                        // 교환 요청이 불가능하면 "권력 때문에 교환을 요청할 수 없습니다." 메시지 띄우기
                        StartCoroutine("CannotRequestExchange");
                        objectTarget = null;
                    }

                }
            }
        }
    }

    IEnumerator CannotRequestExchange()
    {
        if (isPlayingCannotRequest)
        {
            yield break;
        }
        isPlayingCannotRequest = true;
        Color cw, cb;
        int frame = 24;
        for (int i = 1; i <= frame; i++)
        {
            cw = Color.Lerp(new Color(1f, 1f, 1f, 0f), new Color(1f, 1f, 1f, 1f), i / (float)frame);
            cb = Color.Lerp(new Color(0f, 0f, 0f, 0f), new Color(0f, 0f, 0f, 1f), i / (float)frame);
            cannotRequestTextB.GetComponent<Text>().color = cb;
            cannotRequestTextW1.GetComponent<Text>().color = cw;
            cannotRequestTextW2.GetComponent<Text>().color = cw;
            yield return new WaitForFixedUpdate();
        }
        for (int i = (frame / 2) - 1; i >= 0; i--)
        {
            cw = Color.Lerp(new Color(1f, 1f, 1f, 0f), new Color(1f, 1f, 1f, 1f), i / (float)(frame / 2));
            cannotRequestTextW1.GetComponent<Text>().color = cw;
            cannotRequestTextW2.GetComponent<Text>().color = cw;
            yield return new WaitForFixedUpdate();
        }
        for (int i = 1; i <= frame / 2; i++)
        {
            cw = Color.Lerp(new Color(1f, 1f, 1f, 0f), new Color(1f, 1f, 1f, 1f), i / (float)(frame / 2));
            cannotRequestTextW1.GetComponent<Text>().color = cw;
            cannotRequestTextW2.GetComponent<Text>().color = cw;
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(0.5f);
        for (int i = frame - 1; i >= 0; i--)
        {
            cw = Color.Lerp(new Color(1f, 1f, 1f, 0f), new Color(1f, 1f, 1f, 1f), i / (float)frame);
            cb = Color.Lerp(new Color(0f, 0f, 0f, 0f), new Color(0f, 0f, 0f, 1f), i / (float)frame);
            cannotRequestTextB.GetComponent<Text>().color = cb;
            cannotRequestTextW1.GetComponent<Text>().color = cw;
            cannotRequestTextW2.GetComponent<Text>().color = cw;
            yield return new WaitForFixedUpdate();
        }
        isPlayingCannotRequest = false;
    }

    // 임시 코드
    /*
    [ClientCallback]
    public void PlayerToSelectCard()
    {
        if (!isLocalPlayer) return;

        // 내가 교환에 참여한 플레이어가 아니면 패스
        if (!bm.GetTurnPlayer().Equals(this) && !bm.GetObjectPlayer().Equals(this))
        {
            return;
        }
        //Debug.Log("PlayerToSelectCard");
        List<Card> hand = bm.GetPlayerHand(this);
        Ray ray = GetComponentInChildren<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, (1 << 9)))
        {
            //Log("Click " + hit.collider.name + ".");
            Debug.DrawLine(ray.origin, hit.point, Color.red, 3f);
            if (hit.collider.gameObject.GetComponentInParent<Card>() != null
                && (hit.collider.gameObject.GetComponentInParent<Card>().Equals(hand[0])
                || hit.collider.gameObject.GetComponentInParent<Card>().Equals(hand[1])))
            {
                /*
                Log("Set " + hit.collider.gameObject.GetComponentInParent<Card>().GetCardName() + " card to play.");
                DecideClicked();
                CmdSetCardToPlay(hit.collider.gameObject.GetComponentInParent<Card>().GetCardCode(), GetPlayerIndex());
            }
        }
    }
    */

                /// <summary>
                /// 선택한 대상에게 교환 요청을 거는 것을 확정짓는 함수입니다. turnStep이 2일 때만 작동합니다.
                /// </summary>
    [ClientCallback]
    public void DecideClicked()
    {
        if (bm.GetTurnStep() != 2)
        {
            return;
        }
        if (!isLocalPlayer) return;
        // 내 턴이 아니면 패스
        else if (!bm.GetTurnPlayer().Equals(this))
        {
            //Debug.Log("It isn't your turn! turn Player: " + (bm.GetTurnPlayer().GetPlayerNum()) + ", this Player: " + bm.GetCameraPlayer().GetPlayerNum());
            return;
        }
        else if (objectTarget == null)
        {
            //Debug.Log("turn Player: " + (bm.GetTurnPlayer().GetPlayerNum()) + ", this Player: " + bm.GetCameraPlayer().GetPlayerNum());
            Debug.LogWarning("Please select a player that you wants to exchange with.");
            return;
        }
        else
        {
            int i = bm.GetPlayers().IndexOf(objectTarget);
            CmdSetObjectPlayer(i);
            objectTarget = null;
        }
    }

    /// <summary>
    /// 교환할 카드가 결정되어 서버에 이를 적용할 때 호출되는 함수입니다.
    /// </summary>
    /// <param name="cardCode"></param>
    /// <param name="playerIndex"></param>
    [Command]
    public void CmdSetCardToPlay(int cardCode, int playerIndex)
    {
        bm.SetCardToPlay(cardCode, playerIndex);
    }

    /// <summary>
    /// 교환 대상이 결정되어 서버에 이를 적용할 때 호출하는 함수입니다. DecideClicked() 에서 호출됩니다.
    /// </summary>
    /// <param name="objectTargetIndex"></param>
    [Command]
    private void CmdSetObjectPlayer(int objectTargetIndex)
    {
        bm.SetObjectPlayer(objectTargetIndex);
    }

    [Command]
    public void CmdAfterExchange()
    {
        bm.AfterExchange();
    }

    [Command]
    public void CmdAfterFreezed()
    {
        bm.AfterFreezed();
    }

    [Command]
    public void CmdReady()
    {
        bm.PlayerReady(GetPlayerIndex());
    }

    [Command]
    public void CmdConfirmStat()
    {
        bm.SetPlayerConfirmStat(GetPlayerIndex(), true);
    }

    public void SetName(string name)
    {
        playerName = name;
    }

    public string GetName()
    {
        return playerName;
    }

    public void SetPlayerNum(int num)
    {
        playerNum = num;
    }

    public int GetPlayerNum()
    {
        return playerNum;
    }

    /// <summary>
    /// 대전에서 부여된 플레이어 속성을 반환합니다.(0: 불, 1: 물, 2: 전기, 3: 바람, 4: 독)
    /// bm이 null이면 -1을 반환합니다.
    /// </summary>
    public int GetPlayerElement()
    {
        if (bm == null) return -1;
        return bm.GetPlayerElement(GetPlayerNum() - 1);
    }

    /// <summary>
    /// 이 플레이어가 playerIndex 플레이어의 속성을 알아냈는지 여부를 반환합니다.
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <returns></returns>
    public bool GetUnveiled(int playerIndex)
    {
        return unveiled[playerIndex];
    }

    /// <summary>
    /// 이 플레이어가 알아내지 못한 속성들의 목록을 반환합니다.
    /// 모든 속성의 플레이어를 알고 있으면 빈 목록을 반환합니다.
    /// bm이 null이면 null을 반환합니다.
    /// </summary>
    /// <returns></returns>
    public List<int> GetUnknownElements()
    {
        if (bm == null) return null;
        List<int> l = new List<int>();
        for (int i = 0; i < 5; i++)
        {
            if (!unveiled[i])
            {
                l.Add(bm.GetPlayers()[i].GetPlayerElement());
            }
        }
        return l;
    }

    public void SetAI(bool AI)
    {
        isAI = AI;
    }

    public void SetCardDragging(bool drag)
    {
        isCardDragging = drag;
    }

    public int GetHealth()
    {
        return displayedHealth;
    }

    public int GetStatAttack()
    {
        return statAttack;
    }

    public int GetStatAuthority()
    {
        return statAuthority;
    }

    public int GetStatMentality()
    {
        return statMentality;
    }

    public int GetExperience()
    {
        return experience;
    }

    public bool HasFreezed()
    {
        return isFreezed;
    }

    public bool HasDead()
    {
        return isDead;
    }

    public bool HasDecidedObjectPlayer()
    {
        return hasDecidedObjectPlayer;
    }

    public bool HasDecidedPlayCard()
    {
        return hasDecidedPlayCard;
    }

    public PlayerControl GetObjectTarget()
    {
        return objectTarget;
    }

    /// <summary>
    /// 상대와의 권력을 비교하여 내가 교환을 요청할 수 있는지 알려주는 함수입니다.
    /// 교환을 요청할 수 있으면 true를 반환합니다.
    /// 교환을 요청할 수 없거나 bm이 null이면 false를 반환합니다.
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <returns></returns>
    public bool CanRequestExchange(int playerIndex)
    {
        if (bm == null || playerIndex == GetPlayerIndex()) return false;
        int opponentAuthority = bm.GetPlayers()[playerIndex].GetStatAuthority();
        // 내가 상대보다 권력이 더 높으면 교환 요청 가능
        if (GetStatAuthority() >= opponentAuthority) return true;

        // 내가 권력 꼴지이고 상대가 권력 1등이면 교환 요청 가능
        int maxAuthority = 0;
        int minAuthority = 100;
        for (int i = 0; i < 5; i++)
        {
            int a = bm.GetPlayers()[i].GetStatAuthority();
            if (a > maxAuthority) maxAuthority = a;
            if (a < minAuthority) minAuthority = a;
        }
        if (GetStatAuthority() == minAuthority && opponentAuthority == maxAuthority) return true;
        else return false;
    }

    public int GetPlayerIndex()
    {
        return playerNum - 1;
    }

    /// <summary>
    /// 이 플레이어의 실제 속성에 따라 해당 속성 스프라이트를 표시하는 함수입니다.
    /// 이 플레이어의 속성을 알게 되었을 때만 호출하십시오.
    /// </summary>
    public void SetElementSprite()
    {
        //Log("SetElementSprite " + GetPlayerElement());
        switch (GetPlayerElement())
        {
            case 0:
                elementSprite.sprite = Resources.Load("Elements/Fire element2", typeof(Sprite)) as Sprite;
                break;
            case 1:
                elementSprite.sprite = Resources.Load("Elements/Water element2", typeof(Sprite)) as Sprite;
                break;
            case 2:
                elementSprite.sprite = Resources.Load("Elements/Electricity element2", typeof(Sprite)) as Sprite;
                break;
            case 3:
                elementSprite.sprite = Resources.Load("Elements/Wind element2", typeof(Sprite)) as Sprite;
                break;
            case 4:
                elementSprite.sprite = Resources.Load("Elements/Poison element2", typeof(Sprite)) as Sprite;
                break;
            default:
                elementSprite.sprite = Resources.Load("Elements/Unknown element", typeof(Sprite)) as Sprite;
                break;
        }
    }

    public void SetHighlight(bool TF)
    {
        Border.SetActive(TF);
    }
    
    private void ObjectiveHighlight()
    {
        StartCoroutine("Highlight");
    }

    IEnumerator Highlight()
    {
        while (bm == null)
        {
            bm = BattleManager.bm;
            yield return null;
        }
        //Debug.Log("bm is set in Highlight.");
        List<int> t = null;
        while (t == null)
        {
            t = bm.GetTarget(GetPlayerIndex());
            yield return null;
        }
        bool b = true;  // bm.players가 모두 잘 채워져 있을 때까지 대기
        do
        {
            b = true;
            for (int i = 0; i < 5; i++)
            {
                if (bm.GetPlayers()[i] == null)
                {
                    b = false;
                    break;
                }
            }
            yield return null;
        } while (!b);

        SetElementSprite(); // 여기서 자신의 속성을 표시하게 함

        switch (bm.GetPlayerElement(t[0]))
        {
            case 0:
                targetElementImage1.GetComponent<RawImage>().texture = Resources.Load("Elements/Fire", typeof(Texture)) as Texture;
                break;
            case 1:
                targetElementImage1.GetComponent<RawImage>().texture = Resources.Load("Elements/Water", typeof(Texture)) as Texture;
                break;
            case 2:
                targetElementImage1.GetComponent<RawImage>().texture = Resources.Load("Elements/Electricity", typeof(Texture)) as Texture;
                break;
            case 3:
                targetElementImage1.GetComponent<RawImage>().texture = Resources.Load("Elements/Wind", typeof(Texture)) as Texture;
                break;
            case 4:
                targetElementImage1.GetComponent<RawImage>().texture = Resources.Load("Elements/Poison", typeof(Texture)) as Texture;
                break;
        }

        switch (bm.GetPlayerElement(t[1]))
        {
            case 0:
                targetElementImage2.GetComponent<RawImage>().texture = Resources.Load("Elements/Fire", typeof(Texture)) as Texture;
                break;
            case 1:
                targetElementImage2.GetComponent<RawImage>().texture = Resources.Load("Elements/Water", typeof(Texture)) as Texture;
                break;
            case 2:
                targetElementImage2.GetComponent<RawImage>().texture = Resources.Load("Elements/Electricity", typeof(Texture)) as Texture;
                break;
            case 3:
                targetElementImage2.GetComponent<RawImage>().texture = Resources.Load("Elements/Wind", typeof(Texture)) as Texture;
                break;
            case 4:
                targetElementImage2.GetComponent<RawImage>().texture = Resources.Load("Elements/Poison", typeof(Texture)) as Texture;
                break;
        }

        yield return new WaitForSeconds(2f);
        int frame = 64;
        for (int i = 0; i < frame; i++)
        {
            targetElementImage1.GetComponent<RectTransform>().anchorMin = Vector2.Lerp(new Vector2(0.22f, 0.52f), new Vector2(0.02f, 0.94f), i / (float)frame);
            targetElementImage1.GetComponent<RectTransform>().anchorMax = Vector2.Lerp(new Vector2(0.46f, 0.64f), new Vector2(0.12f, 0.99f), i / (float)frame);
            
            targetElementImage2.GetComponent<RectTransform>().anchorMin = Vector2.Lerp(new Vector2(0.54f, 0.52f), new Vector2(0.14f, 0.94f), i / (float)frame);
            targetElementImage2.GetComponent<RectTransform>().anchorMax = Vector2.Lerp(new Vector2(0.78f, 0.64f), new Vector2(0.24f, 0.99f), i / (float)frame);

            targetElementText.GetComponent<RectTransform>().anchorMin = Vector2.Lerp(new Vector2(0.26f, 0.46f), new Vector2(0.26f, 0.94f), i / (float)frame);
            targetElementText.GetComponent<RectTransform>().anchorMax = Vector2.Lerp(new Vector2(0.74f, 0.51f), new Vector2(0.74f, 0.99f), i / (float)frame);

            targetElementBackground.GetComponent<RectTransform>().anchorMin = Vector2.Lerp(new Vector2(0.2f, 0.46f), new Vector2(0f, 0.93f), i / (float)frame);
            targetElementBackground.GetComponent<RectTransform>().anchorMax = Vector2.Lerp(new Vector2(0.8f, 0.65f), new Vector2(0.77f, 1f), i / (float)frame);
            yield return new WaitForFixedUpdate();
        }

        targetElementImage1.GetComponent<RectTransform>().anchorMin = new Vector2(0.02f, 0.94f);
        targetElementImage1.GetComponent<RectTransform>().anchorMax = new Vector2(0.12f, 0.99f);

        targetElementImage2.GetComponent<RectTransform>().anchorMin = new Vector2(0.14f, 0.94f);
        targetElementImage2.GetComponent<RectTransform>().anchorMax = new Vector2(0.24f, 0.99f);

        targetElementText.GetComponent<RectTransform>().anchorMin = new Vector2(0.26f, 0.94f);
        targetElementText.GetComponent<RectTransform>().anchorMax = new Vector2(0.74f, 0.99f);
        
        targetElementBackground.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0.93f);
        targetElementBackground.GetComponent<RectTransform>().anchorMax = new Vector2(0.77f, 1f);

        /*
        // TODO 만약 상대 플레이어의 속성을 모르는 상태이면 그 상대가 목표임을 공개하면 안 된다.
        while (!unveiled[t[0]] || !unveiled[t[1]])
        {
            //Log(bm.GetPlayers()[t[0]].GetName() + " is my objective.");
            if (unveiled[t[0]]) bm.GetPlayers()[t[0]].SetHighlight(true);
            //Log(bm.GetPlayers()[t[1]].GetName() + " is my objective, too.");
            if (unveiled[t[1]]) bm.GetPlayers()[t[1]].SetHighlight(true);
            yield return null;
        }
        */
        // 목표인 상대에게 주황색 테두리를 씌우는 것은 RpcUnveil 함수에서 하도록 했다.
    }
    
    private void Log(string msg)
    {
        Debug.Log(msg);
        //ConsoleLogUI.AddText(msg);
    }

    /*
    public void CAlert(int i)
    {
        if (!isLocalPlayer) return;
        if (i == 0 && Equals(bm.GetTurnPlayer())) alert.CreateAlert(i);
        else if (i == 1 && Equals(bm.GetObjectPlayer())) alert.CreateAlert(i);
        else if (i == 2 || i == 3) alert.CreateAlert(i);
    }
    */

    private void StatusUpdate()
    {
        int ts = bm.GetTurnStep();
        bool isTP = (Equals(bm.GetTurnPlayer()));
        bool isOP = (Equals(bm.GetObjectPlayer()));
        string s = "";

        if (ts == 0)
        {
            StatusUI.SetText("대전 시작");
        }
        else if (ts == 2 && isTP && objectTarget == null)
        {
            if (!isAlerted0)
            {
                Alert.alert.CreateAlert(0);
                isAlerted0 = true;
            }
            StatusUI.SetText("교환하고 싶은 상대의 캐릭터를 누르세요.");
        }
        else if (ts == 2 && isTP && objectTarget != null)
        {
            StatusUI.SetText("교환하고 싶은, 하단의 카드 하나를 위로 드래그해서 내세요.");
        }
        else if (ts == 2)
        {
            StatusUI.SetText(bm.GetTurnPlayer().GetName() + "의 턴");
            isAlerted1 = false;
        }
        else if (ts == 3 && isTP)
        {
            StatusUI.SetText("상대에게 교환 요청을 보냈습니다. 기다리세요.");
        }
        else if (ts == 3 && isOP)
        {
            if (!isAlerted1)
            {
                Alert.alert.CreateAlert(1);
                isAlerted1 = true;
            }
            StatusUI.SetText("교환 요청을 받았습니다. 교환하고 싶은, 하단의 카드 하나를 위로 드래그해서 내세요.");
        }
        else if (ts == 3)
        {
            StatusUI.SetText(bm.GetTurnPlayer().GetName() + "이(가) " + bm.GetObjectPlayer().GetName() + "에게 교환을 요청했습니다.");
        }
        else if (ts == 4 || ts == 9)
        {
            StatusUI.SetText("교환중...");
            isAlerted0 = false;
        }
        else if ((ts == 5 || ts == 11))
        {
            StatusUI.SetText(bm.GetTurnPlayer().GetName() + "이(가) 빙결되어 이번 턴에 교환할 수 없습니다.");
        }
        else if (ts == 8)
        {
            for (int j = 0; j < 5; j++)
            {
                if (bm.GetIsWin()[j])
                {
                    s += bm.GetPlayers()[j].GetName() + " ";
                }
            }
            StatusUI.SetText("대전 종료!\n" + s + "승리!");
            if (!isAlerted3 && bm.GetIsWin()[GetPlayerIndex()])
            {
                Alert.alert.CreateAlert(3);
                isAlerted3 = true;
            }
            else if (!isAlerted3 && !bm.GetIsWin()[GetPlayerIndex()])
            {
                Alert.alert.CreateAlert(4);
                isAlerted3 = true;
            }
        }
        else if (ts == 12)
        {
            if (!isAlerted2)
            {
                Alert.alert.CreateAlert(2);
                isAlerted2 = true;
            }
            StatusUI.SetText("누군가가 게임을 나갔습니다. 대전을 진행할 수 없으므로 종료합니다.");
        }
        else
        {
            StatusUI.ClearText();
        }
    }

    [ClientRpc]
    public void RpcSetAI(bool AI)
    {
        isAI = AI;
        GetComponent<NetworkIdentity>().localPlayerAuthority = false;
    }

    [ClientRpc]
    public void RpcFreeze()
    {
        StartCoroutine("FreezeAnimation");
    }

    [ClientRpc]
    public void RpcHealed()
    {
        StartCoroutine("HealedAnimation");
    }

    [ClientRpc]
    public void RpcDamaged()
    {
        StartCoroutine("DamagedAnimation");
    }

    [ClientRpc]
    public void RpcDead()
    {
        StartCoroutine("DeadAnimation");
    }

    public void Freeze()
    {
        StartCoroutine("FreezeAnimation");
    }

    IEnumerator HealedAnimation()
    {
        //Log("HealedAnimation");
        Face.sprite = Resources.Load("캐릭터/치유받은_캐릭터", typeof(Sprite)) as Sprite;
        Quaternion Original = Face.transform.localRotation;

        float t = Time.time;
        while (Time.time - t < (20f / 60f))
        {
            Face.transform.localRotation = Quaternion.Lerp(Original, Quaternion.Euler(0f, 181f, 0f), (Time.time - t) / (20f / 60f));
            yield return null;
        }

        t = Time.time;
        while (Time.time - t < (20f / 60f))
        {
            Face.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(0f, 181f, 0f), Original, (Time.time - t) / (20f / 60f));
            yield return null;
        }

        Face.transform.localRotation = Original;
        yield return new WaitForSeconds(40f / 60f);
        Face.sprite = Resources.Load("캐릭터/디폴트_캐릭터", typeof(Sprite)) as Sprite;
    }

    IEnumerator DamagedAnimation()
    {
        //Log("DamagedAnimation");
        Face.sprite = Resources.Load("캐릭터/데미지받은_캐릭터", typeof(Sprite)) as Sprite;
        Vector3 Original = Face.transform.localPosition;
        for (int i = 0; i < 5; i++)
        {
            Face.transform.localPosition = Original + new Vector3(0.2f, 0f, 0f);
            yield return new WaitForSeconds(5f / 60f);
            Face.transform.localPosition = Original + new Vector3(-0.2f, 0f, 0f);
            yield return new WaitForSeconds(5f / 60f);
        }
        Face.transform.localPosition = Original;
        yield return new WaitForSeconds(40f / 60f);
        Face.sprite = Resources.Load("캐릭터/디폴트_캐릭터", typeof(Sprite)) as Sprite;
    }

    IEnumerator DeadAnimation()
    {
        //Log("DeadAnimation");
        Face.sprite = Resources.Load("캐릭터/죽은_캐릭터", typeof(Sprite)) as Sprite;
        yield return new WaitForSeconds(30f / 30f);
        //폭발애니메이...
        
        
    }

    //대전화면에서 빙결이 일어나는 애니메이션
    IEnumerator FreezeAnimation()
    {
        //Log("FreezeAnimation");
        Ice.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        Ice.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.75f);
        Ice.GetComponent<SpriteRenderer>().sprite = Resources.Load("이펙트/대전화면_빙결/얼음0", typeof(Sprite)) as Sprite;
        yield return new WaitForSeconds(4f / 3f);
        Ice.GetComponent<SpriteRenderer>().sprite = Resources.Load("이펙트/대전화면_빙결/얼음2", typeof(Sprite)) as Sprite;
        yield return new WaitForSeconds(4f / 3f);
        Ice.GetComponent<SpriteRenderer>().sprite = Resources.Load("이펙트/대전화면_빙결/얼음4", typeof(Sprite)) as Sprite;
        yield return new WaitForSeconds(4f / 3f);
        float t = Time.time;
        while (Time.time - t < (90f / 60f))
        {
            Ice.GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(1f, 1f, 1f, 0.75f), new Color(1f, 1f, 1f, 0f), (Time.time - t) / (90f / 60f));
            yield return null;
        }
        Ice.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
        //Log("In client, " + GetName() + " isAI? " + isAI + ", then why not thawed?");
        //if (isServer) bm.RpcPrintLog("In server, " + GetName() + " isAI? " + isAI + ", then why not thawed?");
        if (isAI && isServer) bm.AfterFreezed();
        else if (isLocalPlayer) CmdAfterFreezed();
    }

    //조작화면에서 빙결이 일어나는 애니메이션
    IEnumerator IceAnimation()
    {
        yield return null;
    }

    /*
     * 여기서부터는 AI(인공지능) 코드입니다.
     */

    IEnumerator AITurnDelay()
    {
        yield return new WaitForSeconds(Random.Range(1.5f, 3f));
        AIThinking(null);   // 여기서 objectTarget과 playCardAI를 설정함.
        int i = bm.GetPlayers().IndexOf(objectTarget);
        bm.SetObjectPlayer(i);
        objectTarget = null;
        bm.SetCardToPlay(playCardAI.GetCardCode(), GetPlayerIndex());
        playCardAI = null;
        yield return null;
        isThinking = false;
    }

    IEnumerator AIExchangeDelay()
    {
        yield return new WaitForSeconds(Random.Range(1.5f, 3f)); /* AUTO 시 주석처리 */
        AIThinking(bm.GetTurnPlayer());
        bm.SetCardToPlay(playCardAI.GetCardCode(), GetPlayerIndex());
        playCardAI = null;
        yield return null;
        isThinking = false;
    }

    /// <summary>
    /// 인공지능이 생각하여 교환할 대상과 교환할 카드를 결정하게 하는 함수입니다. 인자로 null이 아닌 값을 주면 교환할 카드만 정합니다.
    /// </summary>
    /// <param name="opponent">교환을 요청해온 상대</param>
    private void AIThinking(PlayerControl opponent)
    {
        List<int> playerClass = AIObjectRelation();

        /* TODO 임시 코드 */
        string m = "";
        for (int i = 0; i < 5; i++)
        {
            m += playerClass[i] + " ";
        }
        bm.RpcPrintLog(m);
        

        List<string> hand = AIHandEstimation();
        // TODO 손패 상황 목록을 받으면 특정 상대와 교환할 때 어떤 카드를 받게 될지 추정하기
        // TODO 특정 상대에게 특정 카드를 줄 때의 결과를 생각하여 행동 점수를 매기고, 점수에 해당하는 수만큼 상자에 제비뽑기를 넣어 랜덤으로 하나 뽑기
        List<Card> myHand = bm.GetPlayerHand(this);
        List<string> decisionBox = new List<string>(); // 이 목록에 제비뽑기를 넣고 나중에 하나 뽑아 나온 행동을 한다.
        if (opponent == null) {
            /*
            for (int i = 0; i < 5; i++)
            {
                if (i == GetPlayerIndex()) continue;
                string opponentCard = AIOpponentPlay(playerClass[i], hand[2 * i], hand[2 * i + 1], bm.GetPlayers()[i].GetHealth());
                bm.RpcPrintLog("opponentCard is " + opponentCard + ".");
                AIScoreBehavior(myHand[0].GetCardName(), opponentCard, hand, i, playerClass[i], decisionBox);
                AIScoreBehavior(myHand[1].GetCardName(), opponentCard, hand, i, playerClass[i], decisionBox);
            }
            */
            // TODO 랜덤 말고 인공지능으로 고치기
            int r;
            do
            {
                r = Random.Range(0, 5);
                objectTarget = bm.GetPlayers()[r];
            } while (objectTarget == null || objectTarget.Equals(this) || !CanRequestExchange(r));
        }
        /*
        else
        {
            int i = opponent.GetPlayerIndex();
            string opponentCard = AIOpponentPlay(playerClass[i], hand[2 * i], hand[2 * i + 1], bm.GetPlayers()[i].GetHealth());
            bm.RpcPrintLog("opponentCard is " + opponentCard + ".");
            AIScoreBehavior(myHand[0].GetCardName(), opponentCard, hand, i, playerClass[i], decisionBox);
            AIScoreBehavior(myHand[1].GetCardName(), opponentCard, hand, i, playerClass[i], decisionBox);
        }
        */
        // TODO 랜덤 말고 인공지능으로 고치기
        /*
        string lottery = decisionBox[Random.Range(0, decisionBox.Count)];
        bm.RpcPrintLog("lottery is " + lottery + ".");
        if (opponent == null)
        {
            objectTarget = bm.GetPlayers()[int.Parse(lottery[0].ToString())];
        }
        lottery = lottery.Substring(1);
        if (myHand[0].GetCardName() == lottery) playCardAI = myHand[0];
        else if (myHand[1].GetCardName() == lottery) playCardAI = myHand[1];
        else Debug.LogError("lottery is invalid.");
        */
        playCardAI = myHand[Random.Range(0, 2)];
    }

    /// <summary>
    /// 인공지능이 자신을 목표로 하는 플레이어들을 추정하게 하는 함수입니다. 플레이어들을 분류한 정보를 목록으로 반환합니다.
    /// </summary>
    private List<int> AIObjectRelation()
    {
        List<int> enemyPoint = new List<int>(); // 인덱스는 플레이어 인덱스이고, 그 값이 높을수록 그 플레이어가 자신의 천적일 가능성이 높다.
        for (int i = 0; i < 5; i++)
        {
            enemyPoint.Add(4);
        }
        enemyPoint[GetPlayerIndex()] = -1;  // 자신은 자신의 천적이 아니다.

        foreach(Exchange ex in bm.GetExchanges())
        {
            if (ex.GetIsFreezed()) continue;

            // 자신의 턴에 한 교환들 중에서
            if (ex.GetTurnPlayer().Equals(this))
            {
                if (ex.GetObjectPlayerCard().GetCardName() == "Attack")     // 상대가 공격 카드를 냈다면
                {
                    enemyPoint[ex.GetObjectPlayer().GetPlayerIndex()] += 1;
                }
                else if (ex.GetObjectPlayerCard().GetCardName() == "Bomb")  // 상대가 폭탄 카드를 냈다면
                {
                    enemyPoint[ex.GetObjectPlayer().GetPlayerIndex()] += 1;
                }
                else if (ex.GetObjectPlayerCard().GetCardName() == "Heal" && ex.GetTurnPlayerHealth() != maxHealth) // 상대의 치유 카드로 체력이 회복된 경우
                {
                    enemyPoint[ex.GetObjectPlayer().GetPlayerIndex()] -= 1;
                    if (enemyPoint[ex.GetObjectPlayer().GetPlayerIndex()] < 0) enemyPoint[ex.GetObjectPlayer().GetPlayerIndex()] = 0;
                }
            }
            // 상대가 자신에게 걸어온 교환 중에서
            else if (ex.GetObjectPlayer().Equals(this))
            {
                if (ex.GetTurnPlayerCard().GetCardName() == "Attack")
                {
                    enemyPoint[ex.GetTurnPlayer().GetPlayerIndex()] += 2;
                }
                else if (ex.GetTurnPlayerCard().GetCardName() == "Bomb")
                {
                    enemyPoint[ex.GetTurnPlayer().GetPlayerIndex()] += 1;
                }
                else if (ex.GetTurnPlayerCard().GetCardName() == "Heal" && ex.GetObjectPlayerHealth() != maxHealth)
                {
                    enemyPoint[ex.GetTurnPlayer().GetPlayerIndex()] -= 2;
                    if (enemyPoint[ex.GetTurnPlayer().GetPlayerIndex()] < 0) enemyPoint[ex.GetTurnPlayer().GetPlayerIndex()] = 0;
                }
            }
        }

        int max = -1;
        for (int i = 0; i < 5; i++)
        {
            if (enemyPoint[i] > max) max = enemyPoint[i];
        }

        List<int> isEnemy = new List<int>();    // 천적일 것으로 생각되는 플레이어들의 인덱스 목록입니다. 크기는 1 이상 4 이하입니다.
        for (int i = 0; i < 5; i++)
        {
            if (enemyPoint[i] == max)
            {
                isEnemy.Add(i);     // 가장 높은 enemyPoint를 받은 플레이어들의 인덱스를 기억
                enemyPoint[i] = -1;
            }
        }

        if (isEnemy.Count < 2)
        {
            int maxNum = 0;         // 공동 2등의 enemyPoint를 받은 플레이어 수
            max = -1;
            for (int i = 0; i < 5; i++)
            {
                if (enemyPoint[i] > max)
                {
                    max = enemyPoint[i];
                    maxNum = 1;
                }
                else if (enemyPoint[i] == max)
                {
                    maxNum++;
                }
            }

            if (max != -1 && maxNum <= 2)   // 2등이 있고 공동 2등이 3명이 아닐 때
            {
                for (int i = 0; i < 5; i++)
                {
                    if (enemyPoint[i] == max)
                    {
                        isEnemy.Add(i);     // 두 번째로 높은 enemyPoint를 받은 플레이어들의 인덱스를 기억
                    }
                }
            }
        }
        bm.GetTarget(GetPlayerIndex());
        List<int> playerClass = new List<int>();
        for (int i = 0; i < 5; i++)
        {
            if (i == GetPlayerIndex())
                playerClass.Add(-1);                                            // 자기 자신
            else
            {
                int c = 0;
                if (isEnemy.IndexOf(i) != -1) c += 2;                           // 천적
                if (bm.GetTarget(GetPlayerIndex()).IndexOf(i) != -1) c += 1;    // 자신의 목표
                playerClass.Add(c);
            }
        }

        /* 
         * playerClass[i]의 값에 따라서 i번째 인덱스의 플레이어를 다음과 같이 분류한다.
         * -1: 자기 자신, 
         * 0: 자신과 아무 관계도 아닌 상대, 
         * 1: 자신의 목표이지만 천적이 아닌 상대,
         * 2: 천적이지만 자신의 목표가 아닌 상대,
         * 3: 자신의 목표이면서 천적인 상대
         */

        return playerClass;
    }

    /// <summary>
    /// 인공지능이 각 플레이어가 어떤 카드를 손패에 들고 있는지 추정하게 하는 함수입니다.
    /// </summary>
    private List<string> AIHandEstimation()
    {
        // TODO 각 플레이어가 했던 마지막 교환의 결과를 바탕으로 현재 손패의 카드 분배 상황을 추정해서 목록으로 반환하기
        List<Card> myHand = bm.GetPlayerHand(this);
        List<Exchange> exc = bm.GetExchanges();
        List<string> handName = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            handName.Add("?");
        }

        for (int i = 0; i < 5; i++)
        {
            // 자기 자신이면 무슨 카드를 들고 있는지 이미 알고 있다.
            if (i == GetPlayerIndex())
            {
                handName[2 * i] = myHand[0].GetCardName();
                handName[2 * i + 1] = myHand[1].GetCardName();
                continue;
            }
            // i번째 인덱스를 갖는 플레이어가 참여한 마지막 교환(빙결된 턴 제외)을 찾는다.
            for (int j = exc.Count - 1; j >= 0; j--)
            {
                // 빙결된 턴은 마지막 교환으로 취급하지 않는다. 빙결 효과가 나타났다고 해서 그때까지 빙결 카드를 들고 있다고는 보장할 수 없다.
                if (exc[j].GetIsFreezed())
                {
                    // 빙결된 턴에 피해를 받으면 그 플레이어는 폭탄을 들고 있는 것이 확실하다.
                    if (exc[j].GetTurnPlayer().GetPlayerIndex() == i && exc[j].GetTurnPlayerHealthVariation() == -1)
                    {
                        handName[2 * i] = "Bomb";
                    }
                    continue;
                }
                else if (exc[j].GetTurnPlayer().GetPlayerIndex() == i)
                {
                    // 마지막 교환에서 체력이 회복된 경우 치유 카드를 받은 것이 확실하다.
                    if (exc[j].GetTurnPlayerHealthVariation() == 1)
                    {
                        if (handName[2 * i] == "?")
                        {
                            handName[2 * i] = "Heal";
                            handName[2 * i + 1] = "NoBomb";
                        }
                        else if (handName[2 * i] == "Bomb")
                        {
                            // 폭탄을 들고 있을 수 없다.
                            Log("How do you have Bomb card?");
                            handName[2 * i] = "Heal";
                            handName[2 * i + 1] = "NoBomb";
                        }
                        else
                        {
                            handName[2 * i + 1] = "Heal";
                            break;
                        }
                    }
                    // 마지막 교환에서 체력이 2 깎인 경우 폭탄 카드를 들고 있었고 공격 카드를 새로 받은 것이 확실하다.
                    else if (exc[j].GetTurnPlayerHealthVariation() == -2)
                    {
                        if (handName[2 * i] == "?" || handName[2 * i] == "Bomb" || handName[2 * i] == "Attack")
                        {
                            handName[2 * i] = "Attack";
                            handName[2 * i + 1] = "Bomb";
                            break;
                        }
                        else
                        {
                            Log("How do you have " + handName[2 * i] + " card?");
                            handName[2 * i] = "Attack";
                            handName[2 * i + 1] = "Bomb";
                            break;
                        }
                    }

                    // 마지막 교환이 자신과의 교환이었다면 자신이 준 카드를 들고 있을 것이다.
                    if (exc[j].GetObjectPlayer().Equals(this))
                    {
                        if (handName[2 * i] == "?") handName[2 * i] = exc[j].GetObjectPlayerCard().GetCardName();
                        else if (handName[2 * i] != exc[j].GetObjectPlayerCard().GetCardName())
                            handName[2 * i + 1] = exc[j].GetObjectPlayerCard().GetCardName();
                    }
                    break;
                }
                else if (exc[j].GetObjectPlayer().GetPlayerIndex() == i)
                {
                    // 마지막 교환에서 체력이 회복된 경우
                    if (exc[j].GetObjectPlayerHealthVariation() == 1)
                    {
                        if (handName[2 * i] == "?") handName[2 * i] = "Heal";
                        else
                        {
                            handName[2 * i + 1] = "Heal";
                            break;
                        }
                    }
                    // 마지막 교환에서 체력이 1 깎인 경우
                    else if (exc[j].GetObjectPlayerHealthVariation() == -1)
                    {
                        if (handName[2 * i] == "?") handName[2 * i] = "Attack";
                        else
                        {
                            handName[2 * i + 1] = "Attack";
                            break;
                        }
                    }

                    if (exc[j].GetTurnPlayer().Equals(this))
                    {
                        if (handName[2 * i] == "?") handName[2 * i] = exc[j].GetTurnPlayerCard().GetCardName();
                        else if (handName[2 * i] != exc[j].GetTurnPlayerCard().GetCardName())
                            handName[2 * i + 1] = exc[j].GetTurnPlayerCard().GetCardName();
                    }
                    break;
                }
            }
        }

        /* TODO 임시 코드 */
        string m = GetName() + " thinks:";
        for (int i = 0; i < 10; i++)
        {
            m += " " + ((i/2)+1) + handName[i];
        }
        Log(m);
        if (isServer) bm.RpcPrintLog(m);

        int r;
        if (handName.IndexOf("Bomb") == -1 && handName.IndexOf("?") != -1)
        {
            do
            {
                r = Random.Range(0, 10);
            } while (handName[r] != "?");
            handName[r] = "Bomb";
        }
        if ((r = handName.IndexOf("NoBomb")) != -1) handName[r] = "?";
        if (handName.IndexOf("Deceive") == -1 && handName.IndexOf("?") != -1)
        {
            do
            {
                r = Random.Range(0, 10);
            } while (handName[r] != "?" && handName[r] != "NoBomb");
            handName[r] = "Deceive";
        }
        if (handName.IndexOf("Freeze") == -1 && handName.IndexOf("?") != -1)
        {
            do
            {
                r = Random.Range(0, 10);
            } while (handName[r] != "?" && handName[r] != "NoBomb");
            handName[r] = "Freeze";
        }
        if (handName.IndexOf("Avoid") == -1 && handName.IndexOf("?") != -1)
        {
            do
            {
                r = Random.Range(0, 10);
            } while (handName[r] != "?" && handName[r] != "NoBomb");
            handName[r] = "Avoid";
        }
        if (handName.IndexOf("Heal") == -1 && handName.IndexOf("?") != -1)
        {
            do
            {
                r = Random.Range(0, 10);
            } while (handName[r] != "?" && handName[r] != "NoBomb");
            handName[r] = "Heal";
        }
        if (handName.IndexOf("Heal") == handName.LastIndexOf("Heal") && handName.IndexOf("?") != -1)
        {
            do
            {
                r = Random.Range(0, 10);
            } while (handName[r] != "?" && handName[r] != "NoBomb");
            handName[r] = "Heal";
        }
        for (int i = 0; i < 10; i++)
        {
            if (handName[i] == "?" || handName[i] == "NoBomb") handName[i] = "Attack";
        }

        /* TODO 임시 코드 */
        string m2 = GetName() + " estimates:";
        for (int i = 0; i < 10; i++)
        {
            m2 += " " + ((i / 2) + 1) + handName[i];
        }
        Log(m2);

        return handName;
    }

    /// <summary>
    /// 인공지능이 상대가 자신에게 어떤 카드를 내려고 할지 예측하게 하는 함수입니다.
    /// </summary>
    /// <param name="playerClass">자신이 바라보는, 자신에 대한 상대의 목표 관계</param>
    /// <param name="card1">상대 손패에 있을 것 같은 카드</param>
    /// <param name="card2">상대 손패에 있을 것 같은 카드</param>
    /// <param name="opponentHealth">상대의 교환 전 남은 체력</param>
    /// <returns>상대가 낼 것으로 생각되는 카드 이름</returns>
    private string AIOpponentPlay(int playerClass, string card1, string card2, int opponentHealth)
    {
        CardDatabase cd = GameObject.Find("BattleManager").GetComponent<CardDatabase>();
        if (cd == null)
        {
            Debug.Log("cd is null in AIopponentPlay.");
            return "Error!";
        }
        else if (playerClass < 0 || playerClass > 3)
        {
            Debug.Log("playerClass out of range [0, 3].");
            return "Error!";
        }
        else if (!cd.VerifyCard(card1) || !cd.VerifyCard(card2))
        {
            Debug.Log("card1("+ card1 + ") or card2(" + card2 + ") is invalid.");
            return "Error!";
        }

        // 상대가 들고 있는 두 카드가 같은 경우 그것밖에 낼 수 없다.
        if (card1 == card2 && (card1 == "Attack" || card1 == "Heal"))
        {
            return card1;
        }
        else if (card1 == card2)
        {
            Debug.Log("How do you have two " + card1 + " cards?");
            return "Error!";
        }

        // 공격, 치유, 폭탄, 회피, 속임, 빙결 순으로 정렬 (하드코딩 주의!)
        if (card2 == "Attack")
        {
            string temp = card1;
            card1 = card2;
            card2 = temp;
        }
        if (card1 != "Attack")
        {
            if (card2 == "Heal")
            {
                string temp = card1;
                card1 = card2;
                card2 = temp;
            }
            if (card1 != "Heal")
            {
                if (card2 == "Bomb")
                {
                    string temp = card1;
                    card1 = card2;
                    card2 = temp;
                }
                if (card1 != "Bomb")
                {
                    if (card2 == "Avoid")
                    {
                        string temp = card1;
                        card1 = card2;
                        card2 = temp;
                    }
                    if (card1 != "Avoid")
                    {
                        if (card2 == "Deceive")
                        {
                            string temp = card1;
                            card1 = card2;
                            card2 = temp;
                        }
                    }
                }
            }
        }

        card1 += card2; // 편의상 이름을 합치기로

        switch (playerClass)
        {
            case 0: // 서로 아무 관계도 아니라고 생각
                switch (card1)
                {
                    case "AttackHeal":
                        return "Heal";
                    case "AttackBomb":
                        return "Bomb";
                    case "AttackAvoid":
                        return "Avoid";
                    case "AttackDeceive":
                        return "Deceive";
                    case "AttackFreeze":
                        return "Freeze";
                    case "HealBomb":
                        if (!bm.GetTurnPlayer().Equals(this) && opponentHealth <= 2)
                        {
                            return "Bomb";
                        }
                        else return "Heal";
                    case "HealAvoid":
                    case "HealDeceive":
                    case "HealFreeze":
                        return "Heal";
                    case "BombAvoid":
                        if (bm.GetTurnPlayer().Equals(this) && GetHealth() <= 2)
                        {
                            return "Avoid";
                        }
                        else return "Bomb";
                    case "BombDeceive":
                    case "BombFreeze":
                        return "Bomb";
                    case "AvoidDeceive":
                        if (Random.Range(0, 1) == 0) return "Deceive";
                        else return "Avoid";
                    case "AvoidFreeze":
                        return "Avoid";
                    case "DeceiveFreeze":
                        return "Freeze";
                }
                break;
            case 1: // 상대는 나를 천적으로 생각
                switch (card1)
                {
                    case "AttackHeal":
                        return "Heal";
                    case "AttackBomb":
                        return "Bomb";
                    case "AttackAvoid":
                        return "Avoid";
                    case "AttackDeceive":
                        return "Deceive";
                    case "AttackFreeze":
                        return "Freeze";
                    case "HealBomb":
                        if (!bm.GetTurnPlayer().Equals(this) && opponentHealth <= 2)
                        {
                            return "Bomb";
                        }
                        else return "Heal";
                    case "HealAvoid":
                        return "Avoid";
                    case "HealDeceive":
                        return "Heal";
                    case "HealFreeze":
                        if (GetHealth() <= 2) return "Heal";
                        else return "Freeze";
                    case "BombAvoid":
                        return "Avoid";
                    case "BombDeceive":
                        return "Deceive";
                    case "BombFreeze":
                        return "Freeze";
                    case "AvoidDeceive":
                        return "Avoid";
                    case "AvoidFreeze":
                        if (opponentHealth <= 2) return "Avoid";
                        else return "Freeze";
                    case "DeceiveFreeze":
                        return "Freeze";
                }
                break;
            case 2: // 상대는 나를 목표로 생각
                switch (card1)
                {
                    case "AttackHeal":
                        return "Attack";
                    case "AttackBomb":
                        return "Bomb";
                    case "AttackAvoid":
                    case "AttackDeceive":
                    case "AttackFreeze":
                        return "Attack";
                    case "HealBomb":
                        return "Bomb";
                    case "HealAvoid":
                        return "Avoid";
                    case "HealDeceive":
                        return "Deceive";
                    case "HealFreeze":
                        return "Freeze";
                    case "BombAvoid":
                    case "BombDeceive":
                    case "BombFreeze":
                        return "Bomb";
                    case "AvoidDeceive":
                        if (Random.Range(0, 1) == 0) return "Avoid";
                        else return "Deceive";
                    case "AvoidFreeze":
                        return "Freeze";
                    case "DeceiveFreeze":
                        if (Random.Range(0, 1) == 0) return "Freeze";
                        else return "Deceive";
                }
                break;
            case 3: // 상대는 나를 목표이자 천적으로 생각
                switch (card1)
                {
                    case "AttackHeal":
                        return "Attack";
                    case "AttackBomb":
                        return "Bomb";
                    case "AttackAvoid":
                        if (opponentHealth <= 2) return "Avoid";
                        else return "Attack";
                    case "AttackDeceive":
                    case "AttackFreeze":
                        return "Attack";
                    case "HealBomb":
                        return "Bomb";
                    case "HealAvoid":
                        return "Avoid";
                    case "HealDeceive":
                        return "Deceive";
                    case "HealFreeze":
                        return "Freeze";
                    case "BombAvoid":
                    case "BombDeceive":
                    case "BombFreeze":
                        return "Bomb";
                    case "AvoidDeceive":
                        if (Random.Range(0, 1) == 0) return "Avoid";
                        else return "Deceive";
                    case "AvoidFreeze":
                        if (opponentHealth <= 2) return "Avoid";
                        else return "Freeze";
                    case "DeceiveFreeze":
                        if (Random.Range(0, 1) == 0) return "Freeze";
                        else return "Deceive";
                }
                break;
        }
        // 위의 경우에 해당되지 않는 경우가 존재할지 모르겠지만
        return "Error!";
    }

    /// <summary>
    /// 인공지능이 자신의 myCard를 상대에게 내는 행동에 대한 점수를 매기고, 그 점수만큼 낼 카드를 box에 넣는 함수입니다.
    /// 행동에 대한 유불리 점수는 내장된 점수표를 따릅니다.
    /// </summary>
    /// <param name="myCard">자신이 낼 카드</param>
    /// <param name="opponentCard">교환할 상대가 낼 것으로 예측한 카드</param>
    /// <param name="hand">추정한 손패 목록 전체</param>
    /// <param name="opponentPlayerIndex">교환할 상대의 인덱스</param>
    /// <param name="playerClass">교환할 상대와의 목표 관계</param>
    /// <param name="box">뽑기 상자</param>
    private void AIScoreBehavior(string myCard, string opponentCard, List<string> hand, int opponentPlayerIndex, int playerClass, List<string> box)
    {
        CardDatabase cd = GameObject.Find("BattleManager").GetComponent<CardDatabase>();
        if (cd == null)
        {
            Debug.Log("cd is null in AIScoreBehavior.");
            return;
        }
        else if (!cd.VerifyCard(myCard) || !cd.VerifyCard(opponentCard))
        {
            Debug.Log("myCard or opponentCard is invalid.");
            return;
        }
        else if (opponentPlayerIndex < 0 || opponentPlayerIndex >= 5)
        {
            Debug.Log("opponentPlayerIndex out of range [0, 4].");
            return;
        }
        else if (hand.Count != 10)
        {
            Debug.Log("The number of cards in hand is not equal to 10.");
            return;
        }
        else if (hand[GetPlayerIndex() * 2] != myCard && hand[GetPlayerIndex() * 2 + 1] != myCard)
        {
            Debug.Log("You don't have " + myCard + " card!");
            return;
        }
        else if (hand[opponentPlayerIndex * 2] != opponentCard && hand[opponentPlayerIndex * 2 + 1] != opponentCard)
        {
            Debug.Log("You don't think your opponent has " + opponentCard + " card!");
            return;
        }
        else if (playerClass < 0 || playerClass > 3)
        {
            Debug.Log("playerClass out of range [0, 3].");
            return;
        }

        int score = 0;
        string voidCard = myCard;   // 만약 상대가 속임을 쓴다고 예측했다면, 내가 원래 내려던 카드를 기억한다.
        int opponentHealth = bm.GetPlayers()[opponentPlayerIndex].GetHealth();

        // 내가 속임을 낼 경우의 점수는 상대가 내려고 하지 않았던 카드를 낼 때의 기준으로 계산된다.
        if (myCard == "Deceive")
        {
            if (hand[opponentPlayerIndex * 2] == opponentCard)
                opponentCard = hand[opponentPlayerIndex * 2 + 1];
            else opponentCard = hand[opponentPlayerIndex * 2];
        }
        // 상대가 속임을 낼 경우의 점수는 내가 내려고 하지 않았던 카드를 낼 때의 기준으로 계산된다.
        else if (opponentCard == "Deceive")
        {
            if (hand[GetPlayerIndex() * 2] == myCard)
                myCard = hand[GetPlayerIndex() * 2 + 1];
            else myCard = hand[GetPlayerIndex() * 2];
        }
        switch (playerClass)
        {
            case 0:
                switch (myCard)
                {
                    case "Attack":
                        switch (opponentCard)
                        {
                            case "Attack": score = 1; break;
                            case "Heal": score = 3; break;
                            case "Bomb": score = 1; break;
                            case "Deceive": score = 2; break;
                            case "Avoid": score = 3; break;
                            case "Freeze": score = 2; break;
                        }
                        break;
                    case "Heal":
                        switch (opponentCard)
                        {
                            case "Attack": score = 10; break;
                            case "Heal": score = 18; break;
                            case "Bomb": score = 10; break;
                            case "Deceive": score = 13; break;
                            case "Avoid": score = 9; break;
                            case "Freeze": score = 13; break;
                        }
                        if (opponentHealth <= 2) score += 6;
                        break;
                    case "Bomb":
                        switch (opponentCard)
                        {
                            case "Attack": score = 4; break;
                            case "Heal": score = 8; break;
                            case "Deceive": score = 5; break;
                            case "Avoid": score = 5; break;
                            case "Freeze": score = 5; break;
                        }
                        if (GetHealth() <= 2) score += 12;
                        if (bm.GetTurnPlayer().Equals(this)) score += 2;
                        if (!bm.GetTurnPlayer().Equals(this) && opponentHealth <= 2) score /= 2;    // 상대가 죽으면 안됨
                        break;
                    case "Deceive":
                        switch (opponentCard)
                        {
                            case "Attack": score = 4; break;
                            case "Heal": score = 8; break;
                            case "Bomb": score = 4; break;
                            case "Avoid": score = 5; break;
                            case "Freeze": score = 5; break;
                        }
                        if (GetHealth() <= 2) score /= 2;   // 아군을 속이는 것은 불리한 행동
                        break;
                    case "Avoid":
                        switch (opponentCard)
                        {
                            case "Attack": score = 10; break;
                            case "Heal": score = 18; break;
                            case "Bomb": score = 10; break;
                            case "Deceive": score = 13; break;
                            case "Freeze": score = 13; break;
                        }
                        if (opponentHealth <= 2) score += 4;
                        break;
                    case "Freeze":
                        switch (opponentCard)
                        {
                            case "Attack": score = 4; break;
                            case "Heal": score = 8; break;
                            case "Bomb": score = 4; break;
                            case "Deceive": score = 5; break;
                            case "Avoid": score = 5; break;
                        }
                        break;
                }
                break;
            case 1:
                switch (myCard)
                {
                    case "Attack":
                        switch (opponentCard)
                        {
                            case "Attack": score = 16; break;
                            case "Heal": score = 20; break;
                            case "Bomb": score = 16; break;
                            case "Deceive": score = 17; break;
                            case "Avoid": score = 16; break;
                            case "Freeze": score = 17; break;
                        }
                        if (opponentHealth <= 2) score += 7;
                        break;
                    case "Heal":
                        switch (opponentCard)
                        {
                            case "Attack": score = 1; break;
                            case "Heal": score = 3; break;
                            case "Bomb": score = 1; break;
                            case "Deceive": score = 2; break;
                            case "Avoid": score = 3; break;
                            case "Freeze": score = 2; break;
                        }
                        break;
                    case "Bomb":
                        switch (opponentCard)
                        {
                            case "Attack": score = 25; break;
                            case "Heal": score = 29; break;
                            case "Deceive": score = 26; break;
                            case "Avoid": score = 26; break;
                            case "Freeze": score = 26; break;
                        }
                        if (GetHealth() <= 2) score += 10;
                        if (bm.GetTurnPlayer().Equals(this)) score += 5;
                        if (GetHealth() <= 2 && bm.GetTurnPlayer().Equals(this)) score += 5;    // 유리한 교환
                        if (opponentHealth <= 2 && !bm.GetTurnPlayer().Equals(this)) score += 5;
                        break;
                    case "Deceive":
                        switch (opponentCard)
                        {
                            case "Attack": score = 4; break;
                            case "Heal": score = 8; break;
                            case "Bomb": score = 4; break;
                            case "Avoid": score = 5; break;
                            case "Freeze": score = 5; break;
                        }
                        if (GetHealth() <= 2) score /= 2;   // 아군을 속이는 것은 불리한 행동
                        break;
                    case "Avoid":
                        switch (opponentCard)
                        {
                            case "Attack": score = 8; break;
                            case "Heal": score = 8; break;
                            case "Bomb": score = 7; break;
                            case "Deceive": score = 8; break;
                            case "Freeze": score = 8; break;
                        }
                        break;
                    case "Freeze":
                        switch (opponentCard)
                        {
                            case "Attack": score = 9; break;
                            case "Heal": score = 13; break;
                            case "Bomb": score = 9; break;
                            case "Deceive": score = 10; break;
                            case "Avoid": score = 10; break;
                        }
                        break;
                }
                break;
            case 2:
                switch (myCard)
                {
                    case "Attack":
                        switch (opponentCard)
                        {
                            case "Attack": score = 1; break;
                            case "Heal": score = 3; break;
                            case "Bomb": score = 1; break;
                            case "Deceive": score = 3; break;
                            case "Avoid": score = 3; break;
                            case "Freeze": score = 2; break;
                        }
                        break;
                    case "Heal":
                        switch (opponentCard)
                        {
                            case "Attack": score = 9; break;
                            case "Heal": score = 13; break;
                            case "Bomb": score = 9; break;
                            case "Deceive": score = 13; break;
                            case "Avoid": score = 9; break;
                            case "Freeze": score = 10; break;
                        }
                        if (GetHealth() <= 2) score -= 5;   // 천적 피하기
                        if (opponentHealth <= 2) score += 6;
                        break;
                    case "Bomb":
                        switch (opponentCard)
                        {
                            case "Attack": score = 4; break;
                            case "Heal": score = 8; break;
                            case "Deceive": score = 8; break;
                            case "Avoid": score = 5; break;
                            case "Freeze": score = 5; break;
                        }
                        if (GetHealth() <= 2) score += 8;
                        if (bm.GetTurnPlayer().Equals(this)) score += 2;
                        if (GetHealth() <= 2 && bm.GetTurnPlayer().Equals(this)) score -= 10;    // 천적 피하기
                        if (!bm.GetTurnPlayer().Equals(this) && opponentHealth <= 2) score /= 2;    // 상대가 죽으면 안됨
                        break;
                    case "Deceive":
                        switch (opponentCard)
                        {
                            case "Attack": score = 9; break;
                            case "Heal": score = 13; break;
                            case "Bomb": score = 9; break;
                            case "Avoid": score = 10; break;
                            case "Freeze": score = 10; break;
                        }
                        break;
                    case "Avoid":
                        switch (opponentCard)
                        {
                            case "Attack": score = 12; break;
                            case "Heal": score = 12; break;
                            case "Bomb": score = 11; break;
                            case "Deceive": score = 12; break;
                            case "Freeze": score = 12; break;
                        }
                        if (opponentHealth <= 2) score += 6;
                        if (GetHealth() <= 2) score += 3;
                        break;
                    case "Freeze":
                        switch (opponentCard)
                        {
                            case "Attack": score = 11; break;
                            case "Heal": score = 15; break;
                            case "Bomb": score = 11; break;
                            case "Deceive": score = 15; break;
                            case "Avoid": score = 11; break;
                        }
                        if (GetHealth() <= 2) score -= 5;   // 천적 피하기
                        break;
                }
                break;
            case 3:
                switch (myCard)
                {
                    case "Attack":
                        switch (opponentCard)
                        {
                            case "Attack": score = 14; break;
                            case "Heal": score = 18; break;
                            case "Bomb": score = 14; break;
                            case "Deceive": score = 18; break;
                            case "Avoid": score = 14; break;
                            case "Freeze": score = 15; break;
                        }
                        if (opponentHealth <= 2) score += 7;
                        if (GetHealth() <= 2) score -= 10;  // 천적 피하기
                        break;
                    case "Heal":
                        switch (opponentCard)
                        {
                            case "Attack": score = 1; break;
                            case "Heal": score = 3; break;
                            case "Bomb": score = 1; break;
                            case "Deceive": score = 3; break;
                            case "Avoid": score = 3; break;
                            case "Freeze": score = 2; break;
                        }
                        if (GetHealth() <= 2 && score > 3) score = 3;   // 천적 피하기
                        break;
                    case "Bomb":
                        switch (opponentCard)
                        {
                            case "Attack": score = 25; break;
                            case "Heal": score = 29; break;
                            case "Deceive": score = 29; break;
                            case "Avoid": score = 26; break;
                            case "Freeze": score = 26; break;
                        }
                        if (GetHealth() <= 2) score += 5;
                        if (bm.GetTurnPlayer().Equals(this)) score += 5;
                        if (opponentHealth <= 2 && !bm.GetTurnPlayer().Equals(this)) score += 5;
                        if (GetHealth() <= 2 && bm.GetTurnPlayer().Equals(this)) score -= 20;   // 천적 피하기
                        break;
                    case "Deceive":
                        switch (opponentCard)
                        {
                            case "Attack": score = 9; break;
                            case "Heal": score = 13; break;
                            case "Bomb": score = 9; break;
                            case "Avoid": score = 10; break;
                            case "Freeze": score = 10; break;
                        }
                        break;
                    case "Avoid":
                        switch (opponentCard)
                        {
                            case "Attack": score = 10; break;
                            case "Heal": score = 10; break;
                            case "Bomb": score = 9; break;
                            case "Deceive": score = 10; break;
                            case "Freeze": score = 10; break;
                        }
                        if (GetHealth() <= 2) score += 9;
                        break;
                    case "Freeze":
                        switch (opponentCard)
                        {
                            case "Attack": score = 9; break;
                            case "Heal": score = 13; break;
                            case "Bomb": score = 9; break;
                            case "Deceive": score = 13; break;
                            case "Avoid": score = 9; break;
                        }
                        if (GetHealth() <= 2) score -= 5;   // 천적 피하기
                        break;
                }
                break;
        }
        if (score < 1) score = 1;

        score = score * score;  // 유리한 행동을 할 확률과 불리한 행동을 할 확률의 차이를 크게 벌린다.

        // 상대가 속임을 낼 것이라면, 뽑기에 넣기 전에 내가 원래 내려던 카드로 다시 바꿔준다.
        if (opponentCard == "Deceive")
        {
            myCard = voidCard;
        }
        bm.RpcPrintLog(score + " lotteries say " + opponentPlayerIndex + myCard + ".");
        for (int i = 0; i < score; i++)
        {
            box.Add(opponentPlayerIndex + myCard);
        }
    }
}