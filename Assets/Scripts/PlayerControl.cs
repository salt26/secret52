using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Prototype.NetworkLobby;

public class PlayerControl : NetworkBehaviour
{

    [SyncVar] private int currentHealth;    // 현재 남은 체력(실시간으로 변화, 외부 열람 불가)
    [SyncVar] private int currentAttack;    // 현재 공격력(실시간으로 변화, 능력치 패널을 제외한 곳에서 열람 불가)
    [SyncVar] private int currentAuthority;    // 현재 권력(실시간으로 변화, 능력치 패널을 제외한 곳에서 열람 불가)
    [SyncVar] private int currentMentality;    // 현재 권력(실시간으로 변화, 능력치 패널을 제외한 곳에서 열람 불가)
    [SyncVar] private int currentExperience;   // 현재 남은 경험치(실시간으로 변화, 능력치 패널을 제외한 곳에서 열람 불가)
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
    [SerializeField] [SyncVar] private int experience;      // 현재 남은 경험치
    [SyncVar] private bool isFreezed = false;                   // 빙결 여부(true이면 다음 한 번의 내 턴에 교환 불가)

    private List<bool> unveiled = new List<bool>();
    // unveiled의 인덱스는 (플레이어 번호 - 1)이고, 그 값은 해당 플레이어의 속성이 이 플레이어에게 공개되었는지 여부이다.
    // 자기 자신의 속성은 항상 공개되어 있는 것으로 취급한다.

    private bool isAI = false;                      // 인공지능 플레이어 여부(true이면 인공지능, false이면 사람)
    private bool hasDecidedObjectPlayer = false;    // 내 턴에 교환 상대를 선택했는지 여부
    private bool hasDecidedPlayCard = false;        // 교환 시 교환할 카드를 선택했는지 여부
    private PlayerControl objectTarget;             // 내가 선택한 교환 대상
    private Card playCardAI;                        // 인공지능이 낼 카드
    private int statMntlMaxAI = 0;                  // 인공지능이 처음에 한 번 달성하려고 하는 정신력의 최대치
    private int statMntlMinAI = 0;                  // 인공지능이 유지하려고 하는 정신력의 최소치
    private int statTactic = -1;                    // 인공지능이 권력과 공격력 중 주력으로 올릴 능력치를 결정하는 변수

    private RectTransform HealthBar;                // HP UI
    [SerializeField] private GameObject playerCamera;

    private static BattleManager bm;
    private static CardDatabase cd;
    private static StatusUI statusUI;
    private static StatPanelUI spUI;
    private static LogPanelUI lpUI;
    //private static Alert alert;

    private GameObject Border;
    private SpriteRenderer Face;
    private GameObject targetElementImage1;
    private GameObject targetElementImage2;
    private GameObject targetElementText;
    private GameObject targetElementBackground;
    private GameObject myElementImage;
    private GameObject myElementText1;
    private GameObject myElementText2;
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
    public GameObject corruptedImage;
    public GameObject lightedImage;
    private bool isMarked; //마크가 되었는지 여부
    private bool isPlayingCannotRequest;

    private bool isAlerted0;
    private bool isAlerted1;
    private bool isAlerted2;
    private bool isAlerted3;
    private bool isAlerted5;

    private bool isStart;
    private bool isThinking;    // 인공지능의 생각 전 딜레이 동안 true가 됨
    private bool isCardDragging;  // 큰 카드를 드래그하는 동안 true가 됨
    private bool isShowingChange;   // 체력 변화량을 표시하는 애니메이션이 실행될 동안 true가 됨

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
        myElementImage = GameObject.Find("MyElementImage");
        myElementText1 = GameObject.Find("MyElementText1");
        myElementText2 = GameObject.Find("MyElementText2");
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
        isAlerted5 = false;
        isStart = false;
        isThinking = false;
        isCardDragging = false;
        isShowingChange = false;
        statAttack = 3;     // 초기값 3으로 설정
        statAuthority = 1;  // 초기값 1로 설정
        statMentality = 3;  // 초기값 3으로 설정
        experience = 2;     // 초기값 2로 설정
        currentAttack = statAttack;
        currentAuthority = statAuthority;
        currentMentality = statMentality;
        currentExperience = experience;
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
        if (isLocalPlayer && statusUI == null && StatusUI.statusUI != null)
        {
            statusUI = StatusUI.statusUI;
        }
        if (isLocalPlayer && statusUI != null)
        {
            StatusUpdate();
        }
        if (isLocalPlayer && spUI == null && StatPanelUI.statPanelUI != null)
        {
            //Debug.Log("StatPanelUI is not null.");
            spUI = StatPanelUI.statPanelUI;
            spUI.SetLocalPlayer(this);
        }
        if (isLocalPlayer && lpUI == null && LogPanelUI.logPanelUI != null)
        {
            lpUI = LogPanelUI.logPanelUI;
            lpUI.SetLocalPlayer(this);
        }
        if (isLocalPlayer && spUI != null)
        {
            spUI.currentAttack = currentAttack;
            spUI.currentAuthority = currentAuthority;
            spUI.currentExperience = currentExperience;
            spUI.currentMentality = currentMentality;
        }
        if (isLocalPlayer && Input.GetMouseButton(0) && Input.touchCount <= 1 && !isCardDragging
                && (spUI == null || !spUI.GetIsOpen())
                && (lpUI == null || !lpUI.GetIsOpen()))
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
        if (!isShowingChange) healthText.GetComponent<Text>().text = displayedHealth.ToString();
        attackUIText.GetComponent<Text>().text = statAttack.ToString();
        authorityUIText.GetComponent<Text>().text = statAuthority.ToString();

        /* 툴팁을 표시하기 위한 코드입니다. */
        if (isLocalPlayer && Input.GetMouseButton(0) && Input.touchCount <= 1 && !isCardDragging
            && !StatPanelUI.statPanelUI.GetIsOpen() && !LogPanelUI.logPanelUI.GetIsOpen())
        {
            List<Card> hand = bm.GetPlayerHand(this);
            Ray ray = GetComponentInChildren<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 작은 카드의 툴팁을 보여주기 위한 코드입니다.
            // 앞면인 작은 카드를 클릭하고 있는 동안에 카드 이름과 설명을 포함한 툴팁이 나타납니다.
            // 뒷면인 작은 카드를 클릭하고 있는 동안에 비공개 공격 카드 설명을 포함한 툴팁이 나타납니다.
            if (hand != null && Physics.Raycast(ray, out hit, Mathf.Infinity, (1 << 9)))
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
                    tooltip = null;
                }
            }
            // 플레이어 툴팁을 보여주기 위한 코드입니다.
            // 플레이어 이름, 체력, 공격력, 권력, 공개된 속성을 보여줍니다.
            else if (Physics.Raycast(ray, out hit, Mathf.Infinity, (1 << 8)))
            {
                //Log("Click " + hit.collider.name + " / " + hit.point + ".");
                Debug.DrawLine(ray.origin, hit.point, Color.red, 3f);
                if (hit.collider.gameObject.GetComponent<PlayerControl>() != null
                    && Alert.alert != null && tooltip == null)
                {
                    PlayerControl p = hit.collider.gameObject.GetComponent<PlayerControl>();
                    GameObject t = Instantiate(tooltipBox, Alert.alert.gameObject.transform);   // Alert.alert.gameObject는 메인 Canvas
                    tooltip = t.GetComponent<TooltipUI>();
                    if (unveiled[p.GetPlayerIndex()])
                    {
                        string elem;
                        switch (p.GetPlayerElement())
                        {
                            case 0:
                                elem = "불";
                                break;
                            case 1:
                                elem = "물";
                                break;
                            case 2:
                                elem = "전기";
                                break;
                            case 3:
                                elem = "바람";
                                break;
                            case 4:
                                elem = "독";
                                break;
                            default:
                                elem = "알 수 없음";
                                break;
                        }
                        tooltip.SetText(p.GetName(), p.color, "체력: " + p.GetHealth() + "/" + p.maxHealth + "\n공격력: " + p.GetStatAttack() +
                            "\n권력: " + p.GetStatAuthority() + "\n속성: " + elem);
                    }
                    else
                    {
                        tooltip.SetText(p.GetName(), p.color, "체력: " + p.GetHealth() + "/" + p.maxHealth + "\n공격력: " + p.GetStatAttack() +
                            "\n권력: " + p.GetStatAuthority() + "\n속성: 알 수 없음");
                    }
                    tooltip.SetPosition(0.01f, 0.321f, 0.31f, 0.47f);
                    tooltip.Appear();
                }
                else if (hit.collider.gameObject.GetComponent<PlayerControl>() == null && tooltip != null)
                {
                    //Debug.LogWarning("This isn't PlayerControl.");
                    tooltip.Disappear();
                    //tooltip = null;
                }
            }
            // 체력 툴팁을 보여주기 위한 코드입니다.
            else if (Physics.Raycast(ray, out hit, Mathf.Infinity, (1 << 10)))
            {
                //Log("Click " + hit.collider.name + " / " + hit.point + ".");
                if (Alert.alert != null && tooltip == null)
                {
                    GameObject t = Instantiate(tooltipBox, Alert.alert.gameObject.transform);   // Alert.alert.gameObject는 메인 Canvas
                    tooltip = t.GetComponent<TooltipUI>();
                    tooltip.SetText("체력", new Color(0.282f, 1f, 0f), "피해를 받을 때마다 감소하여 0 이하가 되면 쓰러집니다. 최대 체력은 52입니다.");
                    tooltip.SetPosition(0.01f, 0.321f, 0.99f, 0.47f);
                    tooltip.Appear();
                }
            }
            // 공격력 툴팁을 보여주기 위한 코드입니다.
            else if (Physics.Raycast(ray, out hit, Mathf.Infinity, (1 << 11)))
            {
                //Log("Click " + hit.collider.name + " / " + hit.point + ".");
                if (Alert.alert != null && tooltip == null)
                {
                    GameObject t = Instantiate(tooltipBox, Alert.alert.gameObject.transform);   // Alert.alert.gameObject는 메인 Canvas
                    tooltip = t.GetComponent<TooltipUI>();
                    tooltip.SetText("공격력", new Color(0.647f, 0.647f, 0.647f), "공격 카드(불, 물, 전기, 바람, 독)의 효과에 관여하는 능력치입니다.\n공격력이 높으면 상대에게 더 큰 피해를 주고 게임을 빠르게 끝낼 수 있습니다.");
                    tooltip.SetPosition(0.01f, 0.321f, 0.99f, 0.47f);
                    tooltip.Appear();
                }
            }
            // 권력 툴팁을 보여주기 위한 코드입니다.
            else if (Physics.Raycast(ray, out hit, Mathf.Infinity, (1 << 12)))
            {
                //Log("Click " + hit.collider.name + " / " + hit.point + ".");
                if (Alert.alert != null && tooltip == null)
                {
                    GameObject t = Instantiate(tooltipBox, Alert.alert.gameObject.transform);   // Alert.alert.gameObject는 메인 Canvas
                    tooltip = t.GetComponent<TooltipUI>();
                    tooltip.SetText("권력", new Color(0.8f, 0.365f, 0.078f), "교환할 수 있는 대상을 제한하는 능력치입니다.\n자신의 턴에는 자신보다 권력이 낮거나 같은 플레이어에게만 교환을 요청할 수 있습니다. 예외적으로 권력이 가장 낮은 플레이어들은 권력이 가장 높은 플레이어들에게 교환을 요청할 수 있습니다.");
                    tooltip.SetPosition(0.01f, 0.321f, 0.99f, 0.47f);
                    tooltip.Appear();
                }
            }
            // 정신력 툴팁을 보여주기 위한 코드입니다.
            else if (Physics.Raycast(ray, out hit, Mathf.Infinity, (1 << 13)))
            {
                if (Alert.alert != null && tooltip == null)
                {
                    GameObject t = Instantiate(tooltipBox, Alert.alert.gameObject.transform);   // Alert.alert.gameObject는 메인 Canvas
                    tooltip = t.GetComponent<TooltipUI>();
                    tooltip.SetText("정신력", new Color(0.305f, 0.125f, 0.8f), "경험치 획득량에 관여하는 능력치입니다.\n능력치 분배 시간이 될 때마다 자신의 정신력만큼 경험치를 획득합니다. 정신력이 높으면 게임 후반에 일어나는 상황에 유연하게 대처할 수 있습니다.");
                    tooltip.SetPosition(0.01f, 0.321f, 0.99f, 0.47f);
                    tooltip.Appear();
                }
            }
            else if (tooltip != null)   // 마우스를 클릭했지만 클릭한 지점이 아무 것과도 닿지 않은 경우
            {
                //Debug.LogWarning("There is no object.");
                tooltip.Disappear();
                //tooltip = null;
            }
        }
        else if (isLocalPlayer && tooltip != null)  // 클릭하지 않고 있는 경우, 큰 카드를 드래그중인 경우, 두 곳 이상을 동시 터치한 경우
        {
            //Debug.LogWarning("You didn't click anything.");
            tooltip.Disappear();
            //tooltip = null;
        }

        /* 인공지능이 행동하기 위한 코드입니다. */
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
        if (isServer && isAI && bm.GetTurnStep() == 14 && !isThinking)
        {
            isThinking = true;
            StartCoroutine("AIStatDistribute");
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
        if (currentHealth > 0)
        {
            currentHealth += 5;
            // 최대 체력이 52인 것은 UpdateHealthAndStat()에서 처리한다.
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
        if (currentMentality > 2) currentMentality -= 2;
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
        else if (currentHealth > 52)
        {
            currentHealth = 52;
        }
        int HealthChange = displayedHealth - currentHealth;

        if (HealthChange < 0)
        {
            //bm.RpcPrintLog(playerName + " is Healed.");
            RpcHealed(); // 힐을 받음
        }
        else if (HealthChange > 0 && isDead == false)
        {
            //bm.RpcPrintLog(playerName + " is Damaged.");
            RpcDamaged(); // 데미지를 받음
        }
        else if (isDead == true)
        {
            //bm.RpcPrintLog(playerName + " is Dead.");
            RpcDead(); // 뒤짐
        }
        displayedHealth = currentHealth;
        // TODO
        if (HealthChange != 0)
        {
            RpcChanged(HealthChange);
        }

        if (statAttack < currentAttack && statMentality > currentMentality)
        {
            RpcCorrupted(); // 타락함
        }

        if (statAuthority < currentAuthority)
        {
            RpcLighted(); // 빛 효과를 받음
        }
        statAttack = currentAttack;
        statAuthority = currentAuthority;
        statMentality = currentMentality;
        experience = currentExperience;
        //bm.RpcPrintLog(GetName() + "'s statAttack: " + statAttack);
    }

    /// <summary>
    /// 능력치 분배 시간이 끝났을 때 변경사항을 적용하는 함수입니다.
    /// </summary>
    public void UpdateStat()
    {
        if (!isServer) return;
        statAttack = currentAttack;
        statAuthority = currentAuthority;
        statMentality = currentMentality;
        experience = currentExperience;
        if (isAI && isThinking) isThinking = false;
    }

    /// <summary>
    /// 교환 로그들을 살펴보고 자동으로 속성을 밝혀내는 시스템입니다.
    /// </summary>
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
            // (속성 카드로 공격했어도 상대가 생명 카드를 내면 남은 체력에 따라 속성이 밝혀지지 않을 수도 있다. 최소 공격력 4, 최대 체력 52, 회복량 5 기준)
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
                && !(exc.GetObjectPlayerCard().GetCardName().Equals("Dark")
                || (exc.GetObjectPlayerCard().GetCardName().Equals("Life") && exc.GetObjectPlayerHealth() - exc.GetTurnPlayerAttack() >= 47)))
            {
                Unveil(exc.GetObjectPlayer().GetPlayerIndex());
            }
            else if (exc.GetObjectPlayer() == this && exc.GetObjectPlayerCard().GetCardCode() == exc.GetTurnPlayer().GetPlayerElement()
                && !(exc.GetTurnPlayerCard().GetCardName().Equals("Dark")
                || (exc.GetTurnPlayerCard().GetCardName().Equals("Life") && exc.GetTurnPlayerHealth() - exc.GetObjectPlayerAttack() >= 47)))
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
    /// 경험치를 현재 정신력만큼 상승시키는 함수입니다.
    /// </summary>
    public void ExperienceUp()
    {
        if (!isServer) return;
        currentExperience += statMentality;
        if (currentExperience > 9999) currentExperience = 9999;
        experience = currentExperience;
    }

    /// <summary>
    /// 능력치 분배 시에 경험치를 2 소모하여 권력을 1 올리는 함수입니다.
    /// 클라이언트에서만 호출 가능합니다.
    /// TODO 네트워크 상에서 잘 작동하는지 확인하기
    /// </summary>
    public void StatAuthorityUp()
    {
        if (!isLocalPlayer || bm == null || bm.GetPlayerConfirmStat(GetPlayerIndex())) return;
        if (currentAuthority < 99 && currentExperience >= 2)
        {
            currentAuthority++;
            currentExperience -= 2;
            CmdSetAuthority(currentAuthority);
            CmdSetExperience(currentExperience);
        }
    }

    /// <summary>
    /// 능력치 분배 시에 경험치를 4 소모하여 공격력을 1 올리는 함수입니다.
    /// 클라이언트에서만 호출 가능합니다.
    /// TODO 네트워크 상에서 잘 작동하는지 확인하기
    /// </summary>
    public void StatAttackUp()
    {
        if (!isLocalPlayer || bm == null || bm.GetPlayerConfirmStat(GetPlayerIndex())) return;
        if (currentAttack < 99 && currentExperience >= 4)
        {
            currentAttack++;
            currentExperience -= 4;
            CmdSetAttack(currentAttack);
            CmdSetExperience(currentExperience);
        }
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
            CmdSetMentality(currentMentality);
            CmdSetExperience(currentExperience);
        }
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
        CmdSetExperience(currentExperience);
        CmdSetAttack(currentAttack);
        CmdSetAuthority(currentAuthority);
        CmdSetMentality(currentMentality);
    }

    /// <summary>
    /// 능력치 분배 시에 분배한 능력치를 확정짓는 함수입니다.
    /// 클라이언트에서만 호출 가능합니다.
    /// TODO 네트워크 상에서 잘 작동하는지 확인하기
    /// </summary>
    public void StatConfirm()
    {
        if (!isLocalPlayer || bm == null || bm.GetPlayerConfirmStat(GetPlayerIndex())) return;
        /* TODO 바로 대입하면 안 되고 모든 플레이어가 능력치를 확정지어 turnStep이 바뀔 때 대입하여야 한다.
        experience = currentExperience;
        statAttack = currentAttack;
        statAuthority = currentAuthority;
        statMentality = currentMentality;
        */
        // BattleManager에게 능력치를 확정했다는 신호 보내기
        CmdConfirmStat();
    }

    /// <summary>
    /// 능력치 패널이 자동으로 열리도록 하는 함수입니다.
    /// </summary>
    [ClientRpc]
    public void RpcOpenStatPanel()
    {
        if (!isLocalPlayer) return;
        if (spUI != null)
        {
            spUI.OpenPanel();
        }
    }

    /// <summary>
    /// 능력치 분배 시간이 끝났을 때 능력치 패널을 자동으로 닫아주는 함수입니다.
    /// </summary>
    [ClientRpc]
    public void RpcEndStatDistribTime()
    {
        if (!isLocalPlayer) return;
        if (spUI != null)
        {
            spUI.ClosePanel();
        }
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
            if (hit.collider.gameObject.GetComponent<PlayerControl>() != null
                && !hit.collider.gameObject.GetComponent<PlayerControl>().Equals(this))
            {
                if (objectTarget == null) 
                {
                    objectTarget = hit.collider.gameObject.GetComponent<PlayerControl>();
                    if (CanRequestExchange(objectTarget.GetPlayerIndex()))
                    {
                        Instantiate(targetMark, hit.collider.gameObject.GetComponent<PlayerControl>().transform);
                        isMarked = true;
                        //Log("Set " + hit.collider.gameObject.GetComponent<PlayerControl>().GetName() + " to a target.");
                    }
                    else
                    {
                        // 교환 요청이 불가능하면 "권력 때문에 교환을 요청할 수 없습니다." 메시지 띄우기
                        StartCoroutine("CannotRequestExchange");
                        objectTarget = null;
                    }
                }
                else if (!objectTarget.Equals(hit.collider.gameObject.GetComponent<PlayerControl>()))
                {
                    Destroy(GameObject.Find("TargetMark(Clone)"));
                    objectTarget = hit.collider.gameObject.GetComponent<PlayerControl>();
                    if (CanRequestExchange(objectTarget.GetPlayerIndex()))
                    {
                        Instantiate(targetMark, hit.collider.gameObject.GetComponent<PlayerControl>().transform);
                        isMarked = true;
                        //Log("Set " + hit.collider.gameObject.GetComponent<PlayerControl>().GetName() + " to a target.");
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

    /// <summary>
    /// 서버에서 이 플레이어의 current 공격력을 attack으로 동기화합니다.
    /// </summary>
    /// <param name="attack"></param>
    [Command]
    private void CmdSetAttack(int attack)
    {
        currentAttack = attack;
    }

    /// <summary>
    /// 서버에서 이 플레이어의 current 권력을 authority로 동기화합니다.
    /// </summary>
    /// <param name="authority"></param>
    [Command]
    private void CmdSetAuthority(int authority)
    {
        currentAuthority = authority;
    }

    /// <summary>
    /// 서버에서 이 플레이어의 current 정신력을 mentality로 동기화합니다.
    /// </summary>
    /// <param name="mentality"></param>
    [Command]
    private void CmdSetMentality(int mentality)
    {
        currentMentality = mentality;
    }

    /// <summary>
    /// 서버에서 이 플레이어의 current 경험치를 exp로 동기화합니다.
    /// </summary>
    /// <param name="exp"></param>
    [Command]
    private void CmdSetExperience(int exp)
    {
        currentExperience = exp;
    }

    /// <summary>
    /// 서버에 이 플레이어가 능력치 변경을 확정했다는 신호를 보냅니다.
    /// </summary>
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
    /// 속성의 영어 이름(카드 이름과 일치)을 반환합니다.
    /// 잘못된 입력이 들어오면 "?"을 반환합니다.
    /// </summary>
    /// <param name="element">속성 번호</param>
    /// <returns></returns>
    private string GetElementName(int element)
    {
        switch (element)
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
            default:
                return "?";
        }
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

    /// <summary>
    /// AI가 true이면 이 플레이어를 인공지능 플레이어로 설정하는 함수입니다.
    /// 인공지능으로 설정되면 능력치 분배 전략이 함께 결정됩니다.
    /// </summary>
    /// <param name="AI"></param>
    public void SetAI(bool AI)
    {
        isAI = AI;
        if (!AI)
        {
            statMntlMaxAI = 0;
            statMntlMinAI = 0;
            statTactic = -1;
            return;
        }

        List<int> randomBox = new List<int>();
        randomBox.Add(6);
        for (int i = 0; i < 3; i++)
        {
            randomBox.Add(6);
            randomBox.Add(7);
        }
        for (int i = 0; i < 2; i++)
        {
            randomBox.Add(8);
            randomBox.Add(9);
        }
        randomBox.Add(10);
        randomBox.Add(11);
        statMntlMaxAI = randomBox[Random.Range(0, randomBox.Count)];

        randomBox.Clear();
        randomBox.Add(1);
        randomBox.Add(2);
        randomBox.Add(3);
        randomBox.Add(4);
        for (int i = 0; i < 3; i++)
        {
            randomBox.Add(5);
            randomBox.Add(6);
        }
        if (statMntlMaxAI >= 8) randomBox.Add(6);
        for (int i = 0; i < 2; i++)
        {
            if (statMntlMaxAI >= 7)
            {
                randomBox.Add(7);
            }
            if (statMntlMaxAI >= 8)
            {
                randomBox.Add(8);
            }
        }
        for (int i = 9; i < statMntlMaxAI; i++)
        {
            randomBox.Add(i);
        }
        statMntlMinAI = randomBox[Random.Range(0, randomBox.Count)];

        randomBox.Clear();
        for (int i = 0; i < 5; i++)
        {
            randomBox.Add(0);
        }
        for (int i = 0; i < 8 - statMntlMinAI; i += 2)
        {
            // 정신력 최소 유지치가 낮으면 공격력 테크를 탈 가능성이 높아진다.
            randomBox.Add(1);
        }
        if (statMntlMaxAI < 7) randomBox.Add(1);
        for (int i = 0; i < 2; i++)
        {
            randomBox.Add(1);
        }
        for (int i = 0; i < statMntlMinAI - 5; i += 2)
        {
            // 정신력 최소 유지치가 높으면 권력 테크를 탈 가능성이 높아진다.
            randomBox.Add(2);
        }
        for (int i = 0; i < 3; i++)
        {
            randomBox.Add(2);
        }
        statTactic = randomBox[Random.Range(0, randomBox.Count)];
        // statTactic이 0이면 권력과 공격력을 랜덤으로 반반씩 올린다.
        // 1이면 공격력 위주로 올린다.
        // 2이면 권력 위주로 올린다.
    }

    /*
    public void SetThisPlayerToAI(NetworkConnection conn)
    {
        if (!isServer) return;
        SetAI(true);
        RpcSetAI(true);
        //GetComponent<NetworkIdentity>().localPlayerAuthority = false;
        GetComponent<NetworkIdentity>().RemoveClientAuthority(conn);
        SetName(GetName() + "(AI)");
    }
    */

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

        switch (bm.GetPlayerElement(GetPlayerIndex()))
        {
            case 0:
                myElementImage.GetComponent<RawImage>().texture = Resources.Load("Elements/Fire element", typeof(Texture)) as Texture;
                break;
            case 1:
                myElementImage.GetComponent<RawImage>().texture = Resources.Load("Elements/Water element2", typeof(Texture)) as Texture;
                break;
            case 2:
                myElementImage.GetComponent<RawImage>().texture = Resources.Load("Elements/Electricity element", typeof(Texture)) as Texture;
                break;
            case 3:
                myElementImage.GetComponent<RawImage>().texture = Resources.Load("Elements/Wind element", typeof(Texture)) as Texture;
                break;
            case 4:
                myElementImage.GetComponent<RawImage>().texture = Resources.Load("Elements/Poison element2", typeof(Texture)) as Texture;
                break;
        }

        yield return new WaitForSeconds(2f);
        int frame2 = 32;
        Vector3 elemPos = GetComponentInChildren<Camera>().WorldToViewportPoint(
            elementSprite.gameObject.transform.position);
        for (int i = 0; i < frame2; i++)
        {
            myElementImage.GetComponent<RectTransform>().anchorMin = Vector2.Lerp(new Vector2(0.42f, 0.57f), new Vector2(elemPos.x - 0.08f, elemPos.y - 0.045f), i / (float)frame2);
            myElementImage.GetComponent<RectTransform>().anchorMax = Vector2.Lerp(new Vector2(0.58f, 0.66f), new Vector2(elemPos.x + 0.08f, elemPos.y + 0.045f), i / (float)frame2);
            myElementImage.GetComponent<RectTransform>().rotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(0f, 0f, 180f), i / (float)frame2);
            
            myElementText1.GetComponent<Text>().color = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), i / (float)frame2);
            myElementText2.GetComponent<Text>().color = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), i / (float)frame2);
            yield return new WaitForFixedUpdate();
        }
        myElementText1.SetActive(false);
        myElementText2.SetActive(false);

        switch (bm.GetPlayerElement(t[0]))
        {
            case 0:
                targetElementImage1.GetComponent<RawImage>().texture = Resources.Load("Elements/Fire element", typeof(Texture)) as Texture;
                break;
            case 1:
                targetElementImage1.GetComponent<RawImage>().texture = Resources.Load("Elements/Water element2", typeof(Texture)) as Texture;
                break;
            case 2:
                targetElementImage1.GetComponent<RawImage>().texture = Resources.Load("Elements/Electricity element", typeof(Texture)) as Texture;
                break;
            case 3:
                targetElementImage1.GetComponent<RawImage>().texture = Resources.Load("Elements/Wind element", typeof(Texture)) as Texture;
                break;
            case 4:
                targetElementImage1.GetComponent<RawImage>().texture = Resources.Load("Elements/Poison element2", typeof(Texture)) as Texture;
                break;
        }

        switch (bm.GetPlayerElement(t[1]))
        {
            case 0:
                targetElementImage2.GetComponent<RawImage>().texture = Resources.Load("Elements/Fire element", typeof(Texture)) as Texture;
                break;
            case 1:
                targetElementImage2.GetComponent<RawImage>().texture = Resources.Load("Elements/Water element2", typeof(Texture)) as Texture;
                break;
            case 2:
                targetElementImage2.GetComponent<RawImage>().texture = Resources.Load("Elements/Electricity element", typeof(Texture)) as Texture;
                break;
            case 3:
                targetElementImage2.GetComponent<RawImage>().texture = Resources.Load("Elements/Wind element", typeof(Texture)) as Texture;
                break;
            case 4:
                targetElementImage2.GetComponent<RawImage>().texture = Resources.Load("Elements/Poison element2", typeof(Texture)) as Texture;
                break;
        }

        frame2 = 16;
        for (int i = 0; i < frame2; i++)
        {
            targetElementImage1.GetComponent<RawImage>().color = Color.Lerp(new Color(1f, 1f, 1f, 0f), Color.white, i / (float)frame2);
            targetElementImage2.GetComponent<RawImage>().color = Color.Lerp(new Color(1f, 1f, 1f, 0f), Color.white, i / (float)frame2);
            targetElementText.GetComponent<Text>().color = Color.Lerp(new Color(1f, 1f, 1f, 0f), Color.white, i / (float)frame2);

            myElementImage.GetComponent<RectTransform>().anchorMin = Vector2.Lerp(new Vector2(elemPos.x - 0.08f, elemPos.y - 0.045f), new Vector2(elemPos.x, elemPos.y), i / (float)frame2);
            myElementImage.GetComponent<RectTransform>().anchorMax = Vector2.Lerp(new Vector2(elemPos.x + 0.08f, elemPos.y + 0.045f), new Vector2(elemPos.x, elemPos.y), i / (float)frame2);
            myElementImage.GetComponent<RawImage>().color = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), i / (float)frame2);
            yield return new WaitForFixedUpdate();
        }
        myElementImage.SetActive(false);

        targetElementImage1.GetComponent<RawImage>().color = Color.white;
        targetElementImage2.GetComponent<RawImage>().color = Color.white;
        targetElementText.GetComponent<Text>().color = Color.white;

        yield return new WaitForSeconds(2f);
        int frame = 64;
        for (int i = 0; i < frame; i++)
        {
            targetElementImage1.GetComponent<RectTransform>().anchorMin = Vector2.Lerp(new Vector2(0.25f, 0.58f), new Vector2(0.04f, 0.93f), i / (float)frame);
            targetElementImage1.GetComponent<RectTransform>().anchorMax = Vector2.Lerp(new Vector2(0.46f, 0.7f), new Vector2(0.13f, 0.98f), i / (float)frame);
            
            targetElementImage2.GetComponent<RectTransform>().anchorMin = Vector2.Lerp(new Vector2(0.54f, 0.58f), new Vector2(0.14f, 0.93f), i / (float)frame);
            targetElementImage2.GetComponent<RectTransform>().anchorMax = Vector2.Lerp(new Vector2(0.75f, 0.7f), new Vector2(0.23f, 0.98f), i / (float)frame);

            targetElementText.GetComponent<RectTransform>().anchorMin = Vector2.Lerp(new Vector2(0.26f, 0.52f), new Vector2(0.26f, 0.93f), i / (float)frame);
            targetElementText.GetComponent<RectTransform>().anchorMax = Vector2.Lerp(new Vector2(0.74f, 0.57f), new Vector2(0.74f, 0.98f), i / (float)frame);

            targetElementBackground.GetComponent<RectTransform>().anchorMin = Vector2.Lerp(new Vector2(0.23f, 0.52f), new Vector2(0.02f, 0.92f), i / (float)frame);
            targetElementBackground.GetComponent<RectTransform>().anchorMax = Vector2.Lerp(new Vector2(0.77f, 0.71f), new Vector2(0.77f, 0.99f), i / (float)frame);
            yield return new WaitForFixedUpdate();
        }

        targetElementImage1.GetComponent<RectTransform>().anchorMin = new Vector2(0.04f, 0.93f);
        targetElementImage1.GetComponent<RectTransform>().anchorMax = new Vector2(0.13f, 0.98f);

        targetElementImage2.GetComponent<RectTransform>().anchorMin = new Vector2(0.15f, 0.93f);
        targetElementImage2.GetComponent<RectTransform>().anchorMax = new Vector2(0.24f, 0.98f);

        targetElementText.GetComponent<RectTransform>().anchorMin = new Vector2(0.26f, 0.93f);
        targetElementText.GetComponent<RectTransform>().anchorMax = new Vector2(0.74f, 0.98f);
        
        targetElementBackground.GetComponent<RectTransform>().anchorMin = new Vector2(0.02f, 0.92f);
        targetElementBackground.GetComponent<RectTransform>().anchorMax = new Vector2(0.77f, 0.99f);

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
        //Debug.Log(msg);
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
        if (statusUI == null) return;
        int ts = bm.GetTurnStep();
        bool isTP = (Equals(bm.GetTurnPlayer()));
        bool isOP = (Equals(bm.GetObjectPlayer()));
        string s = "";

        if (ts == 0)
        {
            statusUI.SetText("대전 시작!");
            statusUI.PlainText();
        }
        else if (ts == 2 && isTP && ((spUI != null && spUI.GetIsOpen()) || (lpUI != null && lpUI.GetIsOpen())))
        {
            if (!isAlerted0)
            {
                Alert.alert.CreateAlert(0);
                isAlerted0 = true;
            }
            statusUI.SetText("열린 창을 닫고 교환을 진행하세요.");
            statusUI.HighlightText();
        }
        else if (ts == 2 && isTP && objectTarget == null && (spUI == null || !spUI.GetIsOpen()) && (lpUI == null || !lpUI.GetIsOpen()))
        {
            if (!isAlerted0)
            {
                Alert.alert.CreateAlert(0);
                isAlerted0 = true;
            }
            statusUI.SetText("교환하고 싶은 상대의 캐릭터를 누르세요.");
            statusUI.PlainText();
        }
        else if (ts == 2 && isTP && objectTarget != null && (spUI == null || !spUI.GetIsOpen()) && (lpUI == null || !lpUI.GetIsOpen()))
        {
            statusUI.SetText("교환하고 싶은, 하단의 카드 하나를 위로 드래그해서 내세요.");
            statusUI.PlainText();
        }
        else if (ts == 2)
        {
            statusUI.SetText(bm.GetTurnPlayer().GetName() + "의 턴");
            statusUI.PlainText();
            isAlerted1 = false;
            isAlerted5 = false;
        }
        else if (ts == 3 && isTP)
        {
            statusUI.SetText("상대에게 교환 요청을 보냈습니다. 기다리세요.");
            statusUI.PlainText();
        }
        else if (ts == 3 && isOP && ((spUI != null && spUI.GetIsOpen()) || (lpUI != null && lpUI.GetIsOpen())))
        {
            if (!isAlerted1)
            {
                Alert.alert.CreateAlert(1);
                isAlerted1 = true;
            }
            statusUI.SetText("교환 요청을 받았습니다. 열린 창을 닫고 교환을 진행하세요.");
            statusUI.HighlightText();
        }
        else if (ts == 3 && isOP && (spUI == null || !spUI.GetIsOpen()) && (lpUI == null || !lpUI.GetIsOpen()))
        {
            if (!isAlerted1)
            {
                Alert.alert.CreateAlert(1);
                isAlerted1 = true;
            }
            statusUI.SetText("교환 요청을 받았습니다. 교환하고 싶은, 하단의 카드 하나를 위로 드래그해서 내세요.");
            statusUI.PlainText();
        }
        else if (ts == 3)
        {
            statusUI.SetText(bm.GetTurnPlayer().GetName() + "이(가) " + bm.GetObjectPlayer().GetName() + "에게 교환을 요청했습니다.");
            statusUI.PlainText();
        }
        else if (ts == 4 || ts == 9)
        {
            statusUI.SetText("교환중...");
            statusUI.PlainText();
            isAlerted0 = false;
        }
        else if ((ts == 5 || ts == 11))
        {
            statusUI.SetText(bm.GetTurnPlayer().GetName() + "이(가) 빙결되어 이번 턴에 교환할 수 없습니다.");
            statusUI.PlainText();
        }
        else if (ts == 7 || ts == 16)
        {
            statusUI.SetText("마법 시전!");
            statusUI.PlainText();
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
            statusUI.SetText("대전 종료!\n" + s + "승리!");
            statusUI.HighlightText();
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
            statusUI.SetText("누군가가 게임을 나갔습니다. 대전을 진행할 수 없으므로 종료합니다.");
            statusUI.HighlightText();
        }
        else if (ts == 13 || ts == 15)
        {
            if (!isAlerted5)
            {
                Alert.alert.CreateAlert(5);
                isAlerted5 = true;
            }
            statusUI.SetText("능력치 분배 시간!");
            statusUI.PlainText();
        }
        else if (ts == 14 && spUI != null && spUI.GetIsOpen() && !bm.GetPlayerConfirmStat(GetPlayerIndex()))
        {
            statusUI.SetText("능력치를 원하는만큼 올리고 확정 버튼을 누르세요.");
            statusUI.PlainText();
        }
        else if (ts == 14 && spUI != null && !spUI.GetIsOpen() && !bm.GetPlayerConfirmStat(GetPlayerIndex()))
        {
            statusUI.SetText("능력치 분배를 완료해야 합니다! '나의 능력치' 창을 여세요.");
            statusUI.HighlightText();
        }
        else if (ts == 14 && spUI != null && bm.GetPlayerConfirmStat(GetPlayerIndex()))
        {
            statusUI.SetText("다른 플레이어들이 능력치를 확정하기를 기다리는 중...");
            statusUI.PlainText();
        }
        else
        {
            statusUI.ClearText();
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

    [ClientRpc]
    public void RpcCorrupted()
    {
        StartCoroutine("CorruptedAnimation");
    }

    [ClientRpc]
    public void RpcLighted()
    {
        StartCoroutine("LightedAnimation");
    }

    [ClientRpc]
    public void RpcChanged(int change)
    {
        StartCoroutine(ChangedAnimation(change));
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

    IEnumerator CorruptedAnimation()
    {
        int frame = 40;
        attackUIText.GetComponent<Text>().color = new Color(0.475f, 0.208f, 0.871f);
        attackUIText.GetComponent<Text>().fontSize = 19;
        for (int i = 0; i < frame; i++)
        {
            corruptedImage.GetComponent<Image>().color = Color.Lerp(new Color(1f, 1f, 1f, 0f), new Color(1f, 1f, 1f, 0.9f), i / (float)frame);
            yield return new WaitForFixedUpdate();
        }
        for (int i = 0; i < frame; i++)
        {
            corruptedImage.GetComponent<Image>().color = Color.Lerp(new Color(1f, 1f, 1f, 0.9f), new Color(1f, 1f, 1f, 0f), i / (float)frame);
            yield return new WaitForFixedUpdate();
        }
        corruptedImage.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
        attackUIText.GetComponent<Text>().color = Color.black;
        attackUIText.GetComponent<Text>().fontSize = 15;
    }

    IEnumerator LightedAnimation()
    {
        int frame = 30, frame2 = 30;
        authorityUIText.GetComponent<Text>().color = new Color(1f, 1f, 0.184f);
        authorityUIText.GetComponent<Text>().fontSize = 19;
        for (int i = 0; i < frame; i++)
        {
            lightedImage.GetComponent<Image>().color = Color.Lerp(new Color(1f, 1f, 1f, 0f), new Color(1f, 1f, 1f, 0.9f), i / (float)frame);
            lightedImage.GetComponent<RectTransform>().localRotation = Quaternion.Lerp(Quaternion.Euler(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 150f), i / (float)frame);
            yield return new WaitForFixedUpdate();
        }
        authorityUIText.GetComponent<Text>().color = Color.black;
        for (int i = 0; i < frame2; i++)
        {
            lightedImage.GetComponent<RectTransform>().localRotation = Quaternion.Lerp(Quaternion.Euler(0f, 0f, 150f), Quaternion.Euler(0f, 0f, 240f), i / (float)frame2);
            yield return new WaitForFixedUpdate();
        }
        authorityUIText.GetComponent<Text>().color = new Color(1f, 1f, 0.184f);
        for (int i = 0; i < frame; i++)
        {
            lightedImage.GetComponent<Image>().color = Color.Lerp(new Color(1f, 1f, 1f, 0.9f), new Color(1f, 1f, 1f, 0f), i / (float)frame);
            lightedImage.GetComponent<RectTransform>().localRotation = Quaternion.Lerp(Quaternion.Euler(0f, 0f, 240f), Quaternion.Euler(0f, 0f, 360f), i / (float)frame);
            yield return new WaitForFixedUpdate();
        }
        lightedImage.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
        lightedImage.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, 0f);
        authorityUIText.GetComponent<Text>().color = Color.black;
        authorityUIText.GetComponent<Text>().fontSize = 15;
    }

    IEnumerator ChangedAnimation(int change)
    {
        isShowingChange = true;
        string t = "";
        if (change > 0) // 피해를 받았을 때
        {
            t = "-";
            t += change.ToString();
        }
        else if (change < 0)    // 회복되었을 때
        {
            t = "+";
            t += (-change).ToString();
        }
        healthText.GetComponent<Text>().text = t;
        healthText.GetComponent<Text>().fontSize = 19;
        yield return new WaitForSeconds(1.5f);
        healthText.GetComponent<Text>().fontSize = 15;
        isShowingChange = false;
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

    IEnumerator AIStatDistribute()
    {
        // 아래 주석의 코드는 아몰랑 란듐(Random) 인공지능
        /*
        while (currentExperience >= Mathf.Min(5, currentMentality + 1))
        {
            int r = Random.Range(0, 3);
            switch (r)
            {
                case 0:
                    AIAttackUp();
                    break;
                case 1:
                    AIAuthorityUp();
                    break;
                case 2:
                    AIMentalityUp();
                    break;
            }
            yield return null;
        }
        */

        // 정신력 최대 목표치를 달성할 때까지 정신력을 최우선으로 올린다.
        while (currentMentality < statMntlMaxAI && currentExperience >= currentMentality + 1)
        {
            AIMentalityUp();
            yield return null;
        }

        // 한 번 정신력 최대 목표치에 도달하면 이것을 유지할 필요는 없다.
        if (statMntlMaxAI > 0 && currentMentality >= statMntlMaxAI) statMntlMaxAI = 0;

        // 정신력 최소 유지치에 미달되면 정신력을 최우선으로 올린다.
        while (currentMentality < statMntlMinAI && currentExperience >= currentMentality + 1)
        {
            AIMentalityUp();
            yield return null;
        }

        if (bm != null) { 
            bool upDown3 = true;
            for (int i = 0; i < 5; i++)
            {
                if (i == GetPlayerIndex()) continue;
                if (bm.GetPlayers()[i].GetStatAuthority() > GetStatAuthority() - 3
                    && bm.GetPlayers()[i].GetStatAuthority() < GetStatAuthority() + 3)
                {
                    upDown3 = false;
                    break;
                }
            }

            // 자신과 권력이 2 이하로 차이나는 플레이어가 존재하지 않으면 권력을 올릴 필요가 없다.
            // 다만 위 조건이 만족되어도 25% 확률로 공격력만을 올리지 않을 수 있다.
            if (upDown3 && Random.Range(0, 4) > 0)
            {
                while (currentExperience >= 5)
                {
                    AIAttackUp();
                    yield return null;
                }
            }
        }

        // 목표 천적 관계와 교환 요청 가능 관계 확인
        List<int> playerClass = AIObjectRelation();
        List<bool> canIRequest;
        List<bool> canRequestToMe;
        List<bool> canRequestOnlyToMe;
        AIRequestRelation(out canIRequest, out canRequestToMe, out canRequestOnlyToMe);

        bool onlyAthr = true;
        for (int i = 0; i < 5; i++)
        {
            if (i == GetPlayerIndex()) continue;
            // 목표이거나 속성을 모르는 상대 중에 나와 교환할 수 있는 상대가 있다면
            if (playerClass[i] % 2 == 0 && (canIRequest[i] || canRequestOnlyToMe[i]))
            {
                // 오직 권력만 올릴 필요는 없다.
                onlyAthr = false;
                break;
            }
        }

        // 권력만 올려서 목표와 교환할 수 있게 해야 한다.
        if (onlyAthr)
        {
            while (currentExperience >= 5)
            {
                AIAuthorityUp();
                yield return null;
            }
        }

        if (GetStatAuthority() == bm.GetMostAuthority())
        {
            for (int i = 0; i < 5; i++)
            {
                if (i == GetPlayerIndex()) continue;
                if (bm.GetPlayers()[i].GetStatAuthority() == bm.GetLeastAuthority()
                    && (playerClass[i] % 100) / 10 == 1
                    && bm.GetLeastAuthority() != bm.GetMostAuthority()
                    && bm.GetSecondLeastAuthority() != bm.GetMostAuthority())
                {
                    // 내가 권력 1등인데 권력 꼴지가 나를 노리는 천적이고 권력 계층이 3단계 이상으로 분화되어 있으면
                    // 권력을 올리지 말아야 한다.
                    while (currentExperience >= 5)
                    {
                        AIAttackUp();
                        yield return null;
                    }
                    break;
                }
            }
        }

        if (GetStatAuthority() == bm.GetLeastAuthority())
        {
            for (int i = 0; i < 5; i++)
            {
                if (i == GetPlayerIndex()) continue;
                if (bm.GetPlayers()[i].GetStatAuthority() == bm.GetMostAuthority()
                    && playerClass[i] % 10 == 0 
                    && bm.GetLeastAuthority() != bm.GetMostAuthority()
                    && bm.GetSecondLeastAuthority() != bm.GetMostAuthority())
                {
                    // 내가 권력 꼴지인데 권력 1등이 나의 목표인 것이 확실하고 권력 계층이 3단계 이상으로 분화되어 있으면
                    // 권력을 올리지 말아야 한다. (리스크가 큰 전략이기는 하다.)
                    while (currentExperience >= 5)
                    {
                        AIAttackUp();
                        yield return null;
                    }
                    break;
                }
            }
        }

        // 위의 경우가 아니면 공격력과 권력 중 자신의 전략에 맞게 올린다.
        // 권력 계층이 2단계 이하로만 분화되어 있어 당장 권력이 의미가 없는 경우 공격력을 올릴 확률이 높아진다.
        while (currentExperience >= 5)
        {
            List<int> randomBox = new List<int>();
            
            randomBox.Add(1); // 공격력
            randomBox.Add(1);
            randomBox.Add(1);

            if (statTactic == 1)
            {
                // 공격력 테크
                randomBox.Add(1);
                randomBox.Add(1);
                randomBox.Add(1);
                randomBox.Add(1);
            }
            else if (statTactic == 2)
            {
                // 권력 테크
                randomBox.Add(0); // 권력
                randomBox.Add(0);
                randomBox.Add(0);
                randomBox.Add(0);
            }

            randomBox.Add(0);
            if (bm.GetLeastAuthority() != bm.GetMostAuthority()
                && bm.GetSecondLeastAuthority() != bm.GetMostAuthority())
            {
                randomBox.Add(0);
                randomBox.Add(0);
            }

            randomBox.Add(2);   // 경험치 이월

            int r = randomBox[Random.Range(0, randomBox.Count)];
            switch (r)
            {
                case 0:
                    AIAuthorityUp();
                    break;
                case 1:
                    AIAttackUp();
                    break;
                default:
                    break;
            }
            yield return null;
            if (r == 2) break;
        }

        bm.SetPlayerConfirmStat(GetPlayerIndex(), true);
        yield return null;
        // isThinking은 모든 플레이어가 능력치를 확정하고 나서 false가 됩니다.
    }

    /// <summary>
    /// 상대와의 교환 요청 가능 관계를 계산하는 함수입니다.
    /// </summary>
    /// <param name="canIRequest">내가 요청할 수 있는 플레이어 인덱스의 값이 true가 됨</param>
    /// <param name="canRequestToMe">나에게 요청할 수 있는 플레이어 인덱스의 값이 true가 됨</param>
    /// <param name="canRequestOnlyToMe">나에게만 요청할 수 있는 플레이어 인덱스의 값이 true가 됨</param>
    private void AIRequestRelation(out List<bool> canIRequest, out List<bool> canRequestToMe, out List<bool> canRequestOnlyToMe)
    {
        canIRequest = new List<bool>();
        canRequestToMe = new List<bool>();
        canRequestOnlyToMe = new List<bool>();
        for (int i = 0; i < 5; i++)
        {
            if (i == GetPlayerIndex())
            {
                canIRequest.Add(false);
                canRequestToMe.Add(false);
            }
            else if (bm.GetPlayers()[i].GetStatAuthority() == GetStatAuthority()
                || (bm.GetPlayers()[i].GetStatAuthority() == bm.GetLeastAuthority() && GetStatAuthority() == bm.GetMostAuthority())
                || (bm.GetPlayers()[i].GetStatAuthority() == bm.GetMostAuthority() && GetStatAuthority() == bm.GetLeastAuthority()))
            {
                canIRequest.Add(true);
                canRequestToMe.Add(true);
            }
            else if (bm.GetPlayers()[i].GetStatAuthority() > GetStatAuthority())
            {
                // 상대 권력이 나보다 높으면
                canIRequest.Add(false);
                canRequestToMe.Add(true);
            }
            else
            {
                // 상대 권력이 나보다 낮으면
                canIRequest.Add(true);
                canRequestToMe.Add(false);
            }

            if (i != GetPlayerIndex() && ((bm.GetPlayers()[i].GetStatAuthority() == bm.GetLeastAuthority() && GetStatAuthority() == bm.GetMostAuthority())
                || (bm.GetPlayers()[i].GetStatAuthority() == bm.GetSecondLeastAuthority() && GetStatAuthority() == bm.GetLeastAuthority())))
            {
                bool b = true;
                // 내가 1등이고 상대가 꼴지이거나, 내가 꼴지이고 상대가 꼴지에서 2등인 경우
                // 나와 권력이 같은 플레이어가 아무도 없고 상대와 권력이 같은 플레이어가 아무도 없어야
                // 그 상대가 나에게만 교환을 요청하게 된다.
                for (int j = 0; j < 5; j++)
                {
                    if (j == i || j == GetPlayerIndex()) continue;
                    if (bm.GetPlayers()[j].GetStatAuthority() == bm.GetPlayers()[i].GetStatAuthority()
                        || bm.GetPlayers()[j].GetStatAuthority() == GetStatAuthority())
                    {
                        b = false;
                        break;
                    }
                }
                canRequestOnlyToMe.Add(b);
            }
            else
            {
                canRequestOnlyToMe.Add(false);
            }
        }
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
            
            for (int i = 0; i < 5; i++)
            {
                if (i == GetPlayerIndex()) continue;
                if (!CanRequestExchange(i)) continue;
                string opponentCard = AIOpponentPlay(playerClass[i], hand[2 * i], hand[2 * i + 1], bm.GetPlayers()[i].GetHealth());
                bm.RpcPrintLog("opponentCard is " + opponentCard + ".");
                AIScoreBehavior(myHand[0].GetCardName(), opponentCard, hand, i, playerClass[i], decisionBox);
                AIScoreBehavior(myHand[1].GetCardName(), opponentCard, hand, i, playerClass[i], decisionBox);
            }
            /*
            // TODO 랜덤 말고 인공지능으로 고치기
            int r;
            do
            {
                r = Random.Range(0, 5);
                objectTarget = bm.GetPlayers()[r];
            } while (objectTarget == null || objectTarget.Equals(this) || !CanRequestExchange(r));
            */
        }
        else
        {
            int i = opponent.GetPlayerIndex();
            string opponentCard = AIOpponentPlay(playerClass[i], hand[2 * i], hand[2 * i + 1], bm.GetPlayers()[i].GetHealth());
            bm.RpcPrintLog("opponentCard is " + opponentCard + ".");
            AIScoreBehavior(myHand[0].GetCardName(), opponentCard, hand, i, playerClass[i], decisionBox);
            AIScoreBehavior(myHand[1].GetCardName(), opponentCard, hand, i, playerClass[i], decisionBox);
        }
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
        /*
        // TODO 랜덤 말고 인공지능으로 고치기
        playCardAI = myHand[Random.Range(0, 2)];
        */
    }

    /* 
     * [AIObjectRelation()]
     * playerClass[i]의 값에 따라서 i번째 인덱스의 플레이어를 다음과 같이 분류한다.
     * -1: 자기 자신, 
     * 0 : 자신의 목표이지만 천적이 아닌 상대,
     * 1 : 자신과 상호 협력적인 관계의 상대, 
     * 2 : 속성을 모르지만 천적이 아닌 상대,
     * 10: 자신의 목표이면서 천적인 상대,
     * 11: 자신의 목표는 아니지만 천적인 상대,
     * 12: 속성을 모르지만 천적인 상대 (초기값)
     * 100, 101, 102, 110, 111, 112도 있음
     * 
     * 짝수: 목표이거나 속성을 모르는 상대
     * 홀수: 목표가 아닌 상대
     * 십의 자리 숫자가 1: 천적인 상대
     * 100 이상: 내 속성을 알고 있는 상대
     */
    /// <summary>
    /// 인공지능이 자신을 목표로 하는 플레이어들을 추정하게 하는 함수입니다. 플레이어들을 분류한 정보를 목록으로 반환합니다.
    /// </summary>
    private List<int> AIObjectRelation()
    {
        List<int> enemyPoint = new List<int>();         // 인덱스는 플레이어 인덱스이고, 그 값이 높을수록 그 플레이어가 자신의 천적일 가능성이 높다.
        List<bool> hasEnemyKnowMe = new List<bool>();   // 인덱스는 플레이어 인덱스이고, 그 값이 true이면 상대가 나의 속성을 직접적으로 밝힌 적이 있는 것이다.
        for (int i = 0; i < 5; i++)
        {
            enemyPoint.Add(10);
            hasEnemyKnowMe.Add(false);
        }
        enemyPoint[GetPlayerIndex()] = -1;  // 자신은 자신의 천적이 아니다.

        foreach(Exchange ex in bm.GetExchanges())
        {
            if (ex.GetIsFreezed()) continue;

            // 자신의 턴에 한 교환들 중에서
            if (ex.GetTurnPlayer().Equals(this))
            {
                if (ex.GetObjectPlayerCard().GetCardCode() == GetPlayerElement())   // 상대가 내 속성과 같은 공격 카드를 낸 경우
                {
                    enemyPoint[ex.GetObjectPlayer().GetPlayerIndex()] += 1;
                    if (ex.GetTurnPlayerCard().GetCardName() != "Dark")             // 내가 피해를 받았다면 상대에게 속성이 알려진다.
                    {
                        hasEnemyKnowMe[ex.GetObjectPlayer().GetPlayerIndex()] = true;
                    }
                }
                else if (ex.GetObjectPlayerCard().GetCardName() == "Light")         // 상대가 빛 카드를 냈다면 상대에게 속성이 알려진다.
                {
                    hasEnemyKnowMe[ex.GetObjectPlayer().GetPlayerIndex()] = true;
                }
                else if (ex.GetObjectPlayerCard().GetCardCode() < 5)     // 상대가 공격 카드를 냈다면
                {
                    enemyPoint[ex.GetObjectPlayer().GetPlayerIndex()] += 1;
                    if (hasEnemyKnowMe[ex.GetObjectPlayer().GetPlayerIndex()])  // 상대가 내 속성을 알고 내 속성이 아닌 카드로 공격했다면
                    {
                        enemyPoint[ex.GetObjectPlayer().GetPlayerIndex()] += 2;
                    }
                }
                else if (ex.GetObjectPlayerCard().GetCardName() == "Dark")  // 상대가 어둠 카드를 냈다면
                {
                    enemyPoint[ex.GetObjectPlayer().GetPlayerIndex()] -= 1;
                    if (enemyPoint[ex.GetObjectPlayer().GetPlayerIndex()] < 0) enemyPoint[ex.GetObjectPlayer().GetPlayerIndex()] = 0;
                }
                else if (ex.GetObjectPlayerCard().GetCardName() == "Life") // 상대가 생명 카드를 냈다면
                {
                    enemyPoint[ex.GetObjectPlayer().GetPlayerIndex()] -= 2;
                    if (hasEnemyKnowMe[ex.GetObjectPlayer().GetPlayerIndex()])  // 상대가 내 속성을 알고 생명 카드를 냈다면
                    {
                        enemyPoint[ex.GetObjectPlayer().GetPlayerIndex()] -= 2;
                    }
                    if (enemyPoint[ex.GetObjectPlayer().GetPlayerIndex()] < 0) enemyPoint[ex.GetObjectPlayer().GetPlayerIndex()] = 0;
                }
            }
            // 상대가 자신에게 걸어온 교환 중에서
            else if (ex.GetObjectPlayer().Equals(this))
            {
                if (ex.GetTurnPlayerCard().GetCardCode() == GetPlayerElement())     // 상대가 내 속성과 같은 공격 카드를 낸 경우
                {
                    enemyPoint[ex.GetTurnPlayer().GetPlayerIndex()] += 1;
                    if (ex.GetObjectPlayerCard().GetCardName() != "Dark")           // 내가 피해를 받았다면 상대에게 속성이 알려진다.
                    {
                        hasEnemyKnowMe[ex.GetTurnPlayer().GetPlayerIndex()] = true;
                    }
                }
                else if (ex.GetTurnPlayerCard().GetCardName() == "Light")         // 상대가 빛 카드를 냈다면 상대에게 속성이 알려진다.
                {
                    hasEnemyKnowMe[ex.GetTurnPlayer().GetPlayerIndex()] = true;
                }
                else if (ex.GetTurnPlayerCard().GetCardCode() < 5)
                {
                    enemyPoint[ex.GetTurnPlayer().GetPlayerIndex()] += 1;
                    if (hasEnemyKnowMe[ex.GetTurnPlayer().GetPlayerIndex()])  // 상대가 내 속성을 알고 내 속성이 아닌 카드로 공격했다면
                    {
                        enemyPoint[ex.GetTurnPlayer().GetPlayerIndex()] += 3;
                    }
                }
                else if (ex.GetTurnPlayerCard().GetCardName() == "Dark")
                {
                    enemyPoint[ex.GetTurnPlayer().GetPlayerIndex()] -= 1;
                    if (enemyPoint[ex.GetTurnPlayer().GetPlayerIndex()] < 0) enemyPoint[ex.GetTurnPlayer().GetPlayerIndex()] = 0;
                }
                else if (ex.GetTurnPlayerCard().GetCardName() == "Life")
                {
                    enemyPoint[ex.GetTurnPlayer().GetPlayerIndex()] -= 2;
                    if (hasEnemyKnowMe[ex.GetTurnPlayer().GetPlayerIndex()])
                    {
                        enemyPoint[ex.GetTurnPlayer().GetPlayerIndex()] -= 3;
                    }
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
        List<int> playerClass = new List<int>();
        for (int i = 0; i < 5; i++)
        {
            if (i == GetPlayerIndex())
                playerClass.Add(-1);                                                // 자기 자신
            else
            {
                int c = 0;

                if (hasEnemyKnowMe[i]) c += 100;                                    // 상대가 내 정체를 알고 있음
                if (isEnemy.IndexOf(i) != -1) c += 10;                              // 천적

                if (!GetUnveiled(i)) c += 2;                                        // 자신의 목표인지 모름
                else if (bm.GetTarget(GetPlayerIndex()).IndexOf(i) == -1) c += 1;   // 자신의 목표가 아님
                playerClass.Add(c);
            }
        }

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
            if (bm.GetCardInPosition(i).GetCardCode() >= 5)
            {
                // 공개된 카드
                handName.Add(bm.GetCardInPosition(i).GetCardName());
            }
            else handName.Add("?"); // 비공개된 공격 카드
        }

        /* TODO 임시 코드 */
        string m4 = GetName() + " knows:";
        for (int i = 0; i < 10; i++)
        {
            m4 += " " + ((i / 2) + 1) + handName[i];
        }
        Log(m4);
        if (isServer) bm.RpcPrintLog(m4);

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
                if (exc[j].GetTurnPlayer().GetPlayerIndex() == i)
                {
                    // 상대가 낸 카드가 항상 공개되는 카드이면 확인할 필요가 없다.
                    if (exc[j].GetTurnPlayerCard().GetCardCode() >= 5) break;

                    // 상대의 속성을 알고 있고 그 상대가 자신 속성의 공격을 받은 것이 확실한 경우
                    // (상대의 교환 전 체력이 51 이상이고 생명 카드를 낸 경우에는 따져봐야 한다. 최소 공격력 4, 최대 체력 52, 회복량 5 기준이다.)
                    if (GetUnveiled(i) && ((exc[j].GetTurnPlayerHealthVariation() == -2 && !exc[j].GetTurnPlayerCard().Equals("Life"))
                        /*
                        || (exc[j].GetTurnPlayerHealth() < 50 && exc[j].GetTurnPlayerHealthVariation() == 3 && exc[j].GetTurnPlayerCard().Equals("Life"))
                        || (exc[j].GetTurnPlayerHealth() == 50 && exc[j].GetTurnPlayerHealthVariation() == 2 && exc[j].GetTurnPlayerCard().Equals("Life"))))
                        */
                        || (exc[j].GetTurnPlayerHealth() - exc[j].GetObjectPlayerAttack() < 47 && exc[j].GetTurnPlayerCard().Equals("Life"))))
                    {

                        if (handName[2 * i] == "?")
                        {
                            handName[2 * i] = GetElementName(bm.GetPlayerElement(i));
                        }
                        else
                        {
                            handName[2 * i + 1] = GetElementName(bm.GetPlayerElement(i));
                        }
                    }

                    /*
                    // 마지막 교환이 자신과의 교환이었다면 자신이 준 카드를 들고 있을 것이다.
                    if (exc[j].GetObjectPlayer().Equals(this) && exc[j].GetObjectPlayerCard().GetCardCode() < 5)
                    {
                        if (handName[2 * i] == "?") handName[2 * i] = exc[j].GetObjectPlayerCard().GetCardName();
                        else if (handName[2 * i] != exc[j].GetObjectPlayerCard().GetCardName()
                            && handName[2 * i + 1] == "?")
                            handName[2 * i + 1] = exc[j].GetObjectPlayerCard().GetCardName();
                    }
                    */
                    break;
                }
                else if (exc[j].GetObjectPlayer().GetPlayerIndex() == i)
                {
                    // 상대가 낸 카드가 항상 공개되는 카드이면 확인할 필요가 없다.
                    if (exc[j].GetObjectPlayerCard().GetCardCode() >= 5) break;

                    // 상대의 속성을 알고 있고 그 상대가 자신 속성의 공격을 받은 것이 확실한 경우
                    // (상대의 교환 전 체력이 51 이상이고 생명 카드를 낸 경우에는 따져봐야 한다. 최소 공격력 4, 최대 체력 52, 회복량 5 기준이다.)
                    if (GetUnveiled(i) && ((exc[j].GetObjectPlayerHealthVariation() == -2 && !exc[j].GetObjectPlayerCard().Equals("Life"))
                        /*
                        || (exc[j].GetObjectPlayerHealth() < 50 && exc[j].GetObjectPlayerHealthVariation() == 3 && exc[j].GetObjectPlayerCard().Equals("Life"))
                        || (exc[j].GetObjectPlayerHealth() == 50 && exc[j].GetObjectPlayerHealthVariation() == 2 && exc[j].GetObjectPlayerCard().Equals("Life"))))
                        */
                        || (exc[j].GetObjectPlayerHealth() - exc[j].GetTurnPlayerAttack() < 47 && exc[j].GetObjectPlayerCard().Equals("Life"))))
                    {
                        if (handName[2 * i] == "?")
                        {
                            handName[2 * i] = GetElementName(bm.GetPlayerElement(i));
                        }
                        else
                        {
                            handName[2 * i + 1] = GetElementName(bm.GetPlayerElement(i));
                        }
                    }

                    /*
                    // 마지막 교환이 자신과의 교환이었다면 자신이 준 카드를 들고 있을 것이다.
                    if (exc[j].GetTurnPlayer().Equals(this) && exc[j].GetObjectPlayerCard().GetCardCode() < 5)
                    {
                        if (handName[2 * i] == "?") handName[2 * i] = exc[j].GetTurnPlayerCard().GetCardName();
                        else if (handName[2 * i] != exc[j].GetTurnPlayerCard().GetCardName()
                            && handName[2 * i + 1] == "?")
                            handName[2 * i + 1] = exc[j].GetTurnPlayerCard().GetCardName();
                    }
                    */
                    break;
                }
            }
        }

        /* TODO 임시 코드 */
        string m = GetName() + " thinks:";
        for (int i = 0; i < 10; i++)
        {
            m += " " + ((i/2)+1) + handName[i];
            /*
            for (int j = 0; j < i; j++)
            {
                if (!handName[i].Equals("?") && handName[j].Equals(handName[i])) Debug.LogError("Duplicated in AIHandEstimation!");
            }
            */
        }
        Log(m);
        if (isServer) bm.RpcPrintLog(m);

        int r;
        if (handName.IndexOf("Fire") == -1 && handName.IndexOf("?") != -1)
        {
            do
            {
                r = Random.Range(0, 10);
            } while (handName[r] != "?");
            handName[r] = "Fire";
        }
        if (handName.IndexOf("Water") == -1 && handName.IndexOf("?") != -1)
        {
            do
            {
                r = Random.Range(0, 10);
            } while (handName[r] != "?");
            handName[r] = "Water";
        }
        if (handName.IndexOf("Electricity") == -1 && handName.IndexOf("?") != -1)
        {
            do
            {
                r = Random.Range(0, 10);
            } while (handName[r] != "?");
            handName[r] = "Electricity";
        }
        if (handName.IndexOf("Wind") == -1 && handName.IndexOf("?") != -1)
        {
            do
            {
                r = Random.Range(0, 10);
            } while (handName[r] != "?");
            handName[r] = "Wind";
        }
        if (handName.IndexOf("Poison") == -1 && handName.IndexOf("?") != -1)
        {
            do
            {
                r = Random.Range(0, 10);
            } while (handName[r] != "?");
            handName[r] = "Poison";
        }
        for (int i = 0; i < 10; i++)
        {
            if (handName[i] == "?")
            {
                string m3 = "Error in AIHandEstimation!";
                Log(m3);
                if (isServer) bm.RpcPrintLog(m3);
                handName[i] = "Fire";
            }
        }

        /* TODO 임시 코드 */
        string m2 = GetName() + " estimates:";
        for (int i = 0; i < 10; i++)
        {
            m2 += " " + ((i / 2) + 1) + handName[i];
        }
        Log(m2);
        if (isServer) bm.RpcPrintLog(m2);

        return handName;
    }

    /// <summary>
    /// 인공지능이 상대가 자신에게 어떤 카드를 내려고 할지 예측하게 하는 함수입니다.
    /// </summary>
    /// <param name="playerClass">자신이 바라보는, 자신에 대한 상대의 목표 관계</param>
    /// <param name="card1">상대 손패에 있을 것 같은 카드</param>
    /// <param name="card2">상대 손패에 있을 것 같은 카드</param>
    /// <param name="opponentHealth">상대의 교환 전 남은 체력</param>
    /// <returns>상대가 낼 것으로 생각되는 카드 이름 (공격 카드는 "Element" 또는 "Attack"으로 변환됨)</returns>
    private string AIOpponentPlay(int playerClass, string card1, string card2, int opponentHealth)
    {
        CardDatabase cd = GameObject.Find("BattleManager").GetComponent<CardDatabase>();
        if (cd == null)
        {
            Debug.Log("cd is null in AIopponentPlay.");
            return "Error!";
        }
        else if (playerClass < 0 || (playerClass >= 3 && playerClass < 10) || (playerClass >= 13 && playerClass < 100)
            || (playerClass >= 103 && playerClass < 110) || playerClass >= 113)
        {
            Debug.Log("playerClass is invalid.");
            return "Error!";
        }
        else if (!cd.VerifyCard(card1) || !cd.VerifyCard(card2))
        {
            Debug.Log("card1("+ card1 + ") or card2(" + card2 + ") is invalid.");
            return "Error!";
        }

        /*
        string rCard1 = card1;
        string rCard2 = card2;
        */

        // card1이 내 속성과 같은 공격 카드이고, 상대가 내 속성을 알고 있는 경우
        if (cd.GetCardCode(card1) == GetPlayerElement() && playerClass >= 100)
        {
            card1 = "Element";
            //Debug.LogWarning("We found Element in AIOpponentPlay!");
        }
        else if (cd.GetCardCode(card1) < 5)
        {
            card1 = "Attack";
        }

        // card2이 내 속성과 같은 공격 카드이고, 상대가 내 속성을 알고 있는 경우
        if (cd.GetCardCode(card2) == GetPlayerElement() && playerClass >= 100)
        {
            card2 = "Element";
            //Debug.LogWarning("We found Element in AIOpponentPlay!");
        }
        else if (cd.GetCardCode(card2) < 5)
        {
            card2 = "Attack";
        }

        // "Element", "Attack" 순으로 정렬하고, 나머지는 카드 번호가 빠른 것이 앞에 오도록(Life, Light, Dark, Time, Corruption) 정렬
        if (card2.Equals("Element"))
        {
            string temp = card1;
            card1 = card2;
            card2 = temp;
        }
        if (!card1.Equals("Element") && card2.Equals("Attack"))
        {
            string temp = card1;
            card1 = card2;
            card2 = temp;
        }
        if (!card1.Equals("Element") && !card2.Equals("Element")
            && !card1.Equals("Attack") && !card2.Equals("Attack") 
            && cd.GetCardCode(card1) > cd.GetCardCode(card2))
        {
            string temp = card1;
            card1 = card2;
            card2 = temp;
        }
        card1 += card2; // 편의상 이름을 합치기로

        switch (playerClass)
        {
            case 0: // 상대가 내 속성을 모르고 나를 천적으로 생각
            case 2:
            case 10:
            case 12:
                switch (card1)
                {
                    case "AttackAttack":
                        return "Attack";
                    case "AttackLife":
                        if (opponentHealth <= GetStatAttack() * 2 + 2)
                            return "Life";
                        else return "Attack";
                    case "AttackLight":
                        return "Light";
                    case "AttackDark":
                        if (opponentHealth <= GetStatAttack() * 2 + 2)
                            return "Dark";
                        else return "Attack";
                    case "AttackTime":
                        return "Attack";
                    case "AttackCorruption":
                        return "Corruption";
                    case "LifeLight":
                        if (opponentHealth <= GetStatAttack() * 2 + 2)
                            return "Life";
                        else return "Light";
                    case "LifeDark":
                        return "Dark";
                    case "LifeTime":
                        return "Time";
                    case "LifeCorruption":
                        if (opponentHealth <= GetStatAttack() * 2 + 2)
                            return "Life";
                        else return "Corruption";
                    case "LightDark":
                        return "Light";
                    case "LightTime":
                        return "Light";
                    case "LightCorruption":
                        return "Corruption";
                    case "DarkTime":
                        return "Time";
                    case "DarkCorruption":
                        if (opponentHealth <= GetStatAttack() * 2 + 2)
                            return "Dark";
                        else return "Corruption";
                    case "TimeCorruption":
                        return "Corruption";
                }
                break;
            case 1: // 상대가 내 속성을 모르고 내가 천적이 아니라고 생각
            case 11:
                switch (card1)
                {
                    case "AttackAttack":
                        return "Attack";
                    case "AttackLife":
                        if (opponentHealth <= GetStatAttack() * 2 + 2)
                            return "Life";
                        else return "Attack";
                    case "AttackLight":
                        return "Light";
                    case "AttackDark":
                        return "Attack";
                    case "AttackTime":
                        return "Attack";
                    case "AttackCorruption":
                        return "Corruption";
                    case "LifeLight":
                        return "Light";
                    case "LifeDark":
                        return "Life";
                    case "LifeTime":
                        return "Life";
                    case "LifeCorruption":
                        return "Corruption";
                    case "LightDark":
                        return "Light";
                    case "LightTime":
                        return "Light";
                    case "LightCorruption":
                        return "Corruption";
                    case "DarkTime":
                        return "Time";
                    case "DarkCorruption":
                        return "Corruption";
                    case "TimeCorruption":
                        return "Corruption";
                }
                break;
            case 100:
            case 102: // 상대는 내가 목표가 아니라는 것을 알고 있고 천적이라고 생각함
                switch (card1)
                {
                    case "ElementAttack":
                        return "Element";
                    case "ElementLife":
                        return "Life";
                    case "ElementLight":
                        return "Light";
                    case "ElementDark":
                        return "Dark";
                    case "ElementTime":
                        return "Time";
                    case "ElementCorruption":
                        return "Corruption";
                    case "AttackAttack":
                        return "Attack";
                    case "AttackLife":
                        return "Life";
                    case "AttackLight":
                        return "Light";
                    case "AttackDark":
                        return "Dark";
                    case "AttackTime":
                        return "Time";
                    case "AttackCorruption":
                        return "Corruption";
                    case "LifeLight":
                        return "Life";
                    case "LifeDark":
                        if (Random.Range(0, 2) == 0) return "Life";
                        else return "Dark";
                    case "LifeTime":
                        return "Time";
                    case "LifeCorruption":
                        if (opponentHealth <= GetStatAttack() * 2 + 2)
                            return "Life";
                        else return "Corruption";
                    case "LightDark":
                        return "Dark";
                    case "LightTime":
                        return "Time";
                    case "LightCorruption":
                        return "Corruption";
                    case "DarkTime":
                        return "Time";
                    case "DarkCorruption":
                        if (opponentHealth <= GetStatAttack() * 2 + 2)
                            return "Dark";
                        else return "Corruption";
                    case "TimeCorruption":
                        if (opponentHealth <= GetStatAttack() * 2 + 2)
                            return "Time";
                        else return "Corruption";
                }
                break;
            case 101: // 상대는 내가 목표가 아니라는 것을 알고 있고 천적이 아니라고 생각함
                switch (card1)
                {
                    case "ElementAttack":
                        return "Element";
                    case "ElementLife":
                        return "Life";
                    case "ElementLight":
                        return "Light";
                    case "ElementDark":
                        return "Dark";
                    case "ElementTime":
                        return "Time";
                    case "ElementCorruption":
                        return "Corruption";
                    case "AttackAttack":
                        return "Attack";
                    case "AttackLife":
                        return "Life";
                    case "AttackLight":
                        return "Light";
                    case "AttackDark":
                        return "Dark";
                    case "AttackTime":
                        return "Time";
                    case "AttackCorruption":
                        return "Corruption";
                    case "LifeLight":
                        return "Life";
                    case "LifeDark":
                        return "Life";
                    case "LifeTime":
                        return "Life";
                    case "LifeCorruption":
                        if (opponentHealth <= GetStatAttack() * 2 + 2)
                            return "Life";
                        else return "Corruption";
                    case "LightDark":
                        return "Light";
                    case "LightTime":
                        return "Light";
                    case "LightCorruption":
                        return "Corruption";
                    case "DarkTime":
                        return "Time";
                    case "DarkCorruption":
                        return "Corruption";
                    case "TimeCorruption":
                        return "Corruption";
                }
                break;
            case 110:
            case 112: // 상대는 내가 목표라는 것을 알고 있고 천적이라고 생각함
                switch (card1)
                {
                    case "ElementAttack":
                        return "Attack";
                    case "ElementLife":
                        return "Life";
                    case "ElementLight":
                        return "Light";
                    case "ElementDark":
                        return "Dark";
                    case "ElementTime":
                        return "Time";
                    case "ElementCorruption":
                        return "Corruption";
                    case "AttackAttack":
                        return "Attack";
                    case "AttackLife":
                        return "Attack";
                    case "AttackLight":
                        return "Attack";
                    case "AttackDark":
                        if (opponentHealth <= GetStatAttack() * 2 + 2)
                            return "Dark";
                        else return "Attack";
                    case "AttackTime":
                        return "Time";
                    case "AttackCorruption":
                        return "Attack";
                    case "LifeLight":
                        return "Light";
                    case "LifeDark":
                        return "Dark";
                    case "LifeTime":
                        return "Time";
                    case "LifeCorruption":
                        if (opponentHealth <= GetStatAttack() * 2 + 2)
                            return "Life";
                        else return "Corruption";
                    case "LightDark":
                        return "Dark";
                    case "LightTime":
                        return "Time";
                    case "LightCorruption":
                        return "Corruption";
                    case "DarkTime":
                        return "Time";
                    case "DarkCorruption":
                        return "Corruption";
                    case "TimeCorruption":
                        return "Time";
                }
                break;
            case 111: // 상대는 내가 목표라는 것을 알고 있고 천적이 아니라고 생각함
                switch (card1)
                {
                    case "ElementAttack":
                        return "Attack";
                    case "ElementLife":
                        return "Element";
                    case "ElementLight":
                        return "Element";
                    case "ElementDark":
                        return "Element";
                    case "ElementTime":
                        return "Element";
                    case "ElementCorruption":
                        return "Corruption";
                    case "AttackAttack":
                        return "Attack";
                    case "AttackLife":
                        return "Attack";
                    case "AttackLight":
                        return "Attack";
                    case "AttackDark":
                        return "Attack";
                    case "AttackTime":
                        return "Attack";
                    case "AttackCorruption":
                        return "Attack";
                    case "LifeLight":
                        return "Light";
                    case "LifeDark":
                        return "Dark";
                    case "LifeTime":
                        return "Life";
                    case "LifeCorruption":
                        return "Corruption";
                    case "LightDark":
                        return "Light";
                    case "LightTime":
                        return "Light";
                    case "LightCorruption":
                        return "Corruption";
                    case "DarkTime":
                        return "Time";
                    case "DarkCorruption":
                        return "Corruption";
                    case "TimeCorruption":
                        return "Corruption";
                }
                break;
        }
        // 위의 경우에 해당되지 않는 경우가 존재할지 모르겠지만
        Debug.LogError(card1 + " is error in AIOpponentPlay.");
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
        else if (!cd.VerifyCard(myCard)
            || (!cd.VerifyCard(opponentCard) && !opponentCard.Equals("Attack") && !opponentCard.Equals("Element")))
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
        else if (!opponentCard.Equals("Attack") && !opponentCard.Equals("Element")
            && hand[opponentPlayerIndex * 2] != opponentCard && hand[opponentPlayerIndex * 2 + 1] != opponentCard)
        {
            Debug.Log("You don't think your opponent has " + opponentCard + " card!");
            return;
        }
        else if (playerClass < 0 || (playerClass >= 3 && playerClass < 10) || (playerClass >= 13 && playerClass < 100)
            || (playerClass >= 103 && playerClass < 110) || playerClass >= 113)
        {
            Debug.Log("playerClass is invalid.");
            return;
        }

        int score = 0;
        string voidCard = myCard;   // 만약 상대가 시간을 쓴다고 예측했다면, 내가 원래 내려던 카드를 기억한다.
        int opponentHealth = bm.GetPlayers()[opponentPlayerIndex].GetHealth();
        string opponentElement = "?";
        if (GetUnveiled(opponentPlayerIndex)) opponentElement = GetElementName(bm.GetPlayerElement(opponentPlayerIndex));

        // 내가 시간을 낼 경우의 점수는 상대가 내려고 하지 않았던 카드를 낼 때의 기준으로 계산된다.
        if (myCard == "Time")
        {
            if (hand[opponentPlayerIndex * 2].Equals(opponentCard)
                || (cd.GetCardCode(hand[opponentPlayerIndex * 2]) == GetPlayerElement() && opponentCard.Equals("Element"))
                || (cd.GetCardCode(hand[opponentPlayerIndex * 2]) < 5 && opponentCard.Equals("Attack")))
                opponentCard = hand[opponentPlayerIndex * 2 + 1];
            else opponentCard = hand[opponentPlayerIndex * 2];
        }
        // 상대가 시간을 낼 경우의 점수는 내가 내려고 하지 않았던 카드를 낼 때의 기준으로 계산된다.
        else if (opponentCard == "Time")
        {
            if (hand[GetPlayerIndex() * 2] == myCard)
                myCard = hand[GetPlayerIndex() * 2 + 1];
            else myCard = hand[GetPlayerIndex() * 2];
        }
        switch (playerClass)
        {
            case 0:     // 목표인 상대
            case 100:
            case 10:
            case 110:
                switch (myCard)
                {
                    case "Fire":
                    case "Water":
                    case "Electricity":
                    case "Wind":
                    case "Poison":
                        if (opponentElement.Equals(myCard))
                        {
                            // 내가 상대 속성의 카드를 내는 경우
                            switch (opponentCard)
                            {
                                case "Element": score = 20; break;
                                case "Attack": score = 10 - (bm.GetPlayers()[opponentPlayerIndex].GetStatAttack() / 2); break;
                                case "Life": score = 32; break;
                                case "Light": score = 32; break;
                                case "Dark": score = 25; break;
                                case "Time": score = 25; break;
                                case "Corruption": score = 16; break;
                            }
                        }
                        else
                        {
                            switch (opponentCard)
                            {
                                case "Element": score = 40; break;
                                case "Attack": score = 20 - (bm.GetPlayers()[opponentPlayerIndex].GetStatAttack() / 2); break;
                                case "Life": score = 52; break;
                                case "Light": score = 52; break;
                                case "Dark": score = 36; break;
                                case "Time": score = 52; break;
                                case "Corruption": score = 36; break;
                            }
                        }
                        if ((playerClass % 100) / 10 == 1)
                        {
                            score -= 10; // 천적에게 교환할 확률 감소
                            if (GetHealth() <= bm.GetPlayers()[opponentPlayerIndex].GetStatAttack() * 2 + 2)
                            {
                                score -= 10;
                            }
                        }
                        if (opponentHealth <= GetStatAttack() * 2 + 2) score += 7;
                        if ((opponentCard.Equals("Attack") || opponentCard.Equals("Element"))
                            && GetHealth() <= bm.GetPlayers()[opponentPlayerIndex].GetStatAttack())
                            score /= 2; // 자신이 킬각이고 상대가 공격 카드를 낼 것으로 예측한 경우 교환할 확률 대폭 감소
                        break;
                    case "Life":
                        switch (opponentCard)
                        {
                            case "Element": score = 2; break;
                            case "Attack": score = 1; break;
                            case "Light": score = 5; break;
                            case "Dark": score = 10; break;
                            case "Time": score = 5; break;
                            case "Corruption": score = 1; break;
                        }
                        if ((playerClass % 100) / 10 == 1) score -= 10; // 천적에게 교환할 확률 감소
                        if (opponentHealth <= GetStatAttack() * 2 + 2) score -= 3;
                        break;
                    case "Light":
                        switch (opponentCard)
                        {
                            case "Element": score = 13; break;
                            case "Attack": score = 7 - (bm.GetPlayers()[opponentPlayerIndex].GetStatAttack() / 2); break;
                            case "Life": score = 25; break;
                            case "Dark": score = 34; break;
                            case "Time": score = 18; break;
                            case "Corruption": score = 9; break;
                        }
                        if ((playerClass % 100) / 10 == 1) score -= 10; // 천적에게 교환할 확률 감소
                        if ((opponentCard.Equals("Attack") || opponentCard.Equals("Element"))
                            && GetHealth() <= bm.GetPlayers()[opponentPlayerIndex].GetStatAttack())
                            score /= 2; // 자신이 킬각이고 상대가 공격 카드를 낼 것으로 예측한 경우 교환할 확률 대폭 감소
                        break;
                    case "Dark":
                        switch (opponentCard)
                        {
                            case "Element": score = 20; break;
                            case "Attack": score = 18; break;
                            case "Life": score = 4; break;
                            case "Light": score = 4; break;
                            case "Time": score = 4; break;
                            case "Corruption": score = 4; break;
                        }
                        if ((playerClass % 100) / 10 == 1)
                        {
                            score += 7; // 천적에게 교환할 확률 증가
                            if (GetHealth() <= bm.GetPlayers()[opponentPlayerIndex].GetStatAttack() * 2 + 2)
                            {
                                score += 9;
                            }
                        }
                        if (opponentHealth <= GetStatAttack() * 2 + 2) score -= 5;
                        break;
                    case "Time":
                        switch (opponentCard)
                        {
                            case "Element": score = 4; break;
                            case "Attack": score = 2; break;
                            case "Life": score = 13; break;
                            case "Light": score = 20; break;
                            case "Dark": score = 29; break;
                            case "Corruption": score = 4; break;
                        }
                        if (GetHealth() <= bm.GetPlayers()[opponentPlayerIndex].GetStatAttack() * 2 + 2) score /= 2;
                        if ((opponentCard.Equals("Attack") || opponentCard.Equals("Element"))
                            && GetHealth() <= bm.GetPlayers()[opponentPlayerIndex].GetStatAttack())
                            score /= 2; // 자신이 킬각이고 상대가 공격 카드를 낼 것으로 예측한 경우 교환할 확률 대폭 감소
                        break;
                    case "Corruption":
                        if (statTactic == 1)
                        {
                            switch (opponentCard)
                            {
                                case "Element": score = 9; break;
                                case "Attack": score = 2; break;
                                case "Life": score = 9; break;
                                case "Light": score = 9; break;
                                case "Dark": score = 13; break;
                                case "Time": score = 8; break;
                            }
                            break;
                        }
                        else
                        {
                            switch (opponentCard)
                            {
                                case "Element": score = 29; break;
                                case "Attack": score = 14 - (bm.GetPlayers()[opponentPlayerIndex].GetStatAttack() / 2); break;
                                case "Life": score = 41; break;
                                case "Light": score = 41; break;
                                case "Dark": score = 50; break;
                                case "Time": score = 29; break;
                            }
                        }
                        if ((playerClass % 100) / 10 == 1) score -= 10; // 천적에게 교환할 확률 감소
                        if (GetStatMentality() < statMntlMinAI) score += 10;    // 정신력에 투자하는 중에는 낼 확률 증가
                        if ((opponentCard.Equals("Attack") || opponentCard.Equals("Element"))
                            && GetHealth() <= bm.GetPlayers()[opponentPlayerIndex].GetStatAttack())
                            score /= 2; // 자신이 킬각이고 상대가 공격 카드를 낼 것으로 예측한 경우 교환할 확률 대폭 감소
                        break;
                }
                break;
            case 1:     // 목표가 아닌 상대
            case 101:
            case 11:
            case 111:
                switch (myCard)
                {
                    case "Fire":
                    case "Water":
                    case "Electricity":
                    case "Wind":
                    case "Poison":
                        if (opponentElement.Equals(myCard))
                        {
                            // 내가 상대 속성의 카드를 내는 경우
                            switch (opponentCard)
                            {
                                case "Element": score = 4; break;
                                case "Attack": score = 2; break;
                                case "Life": score = 29; break;
                                case "Light": score = 29; break;
                                case "Dark": score = 29; break;
                                case "Time": score = 4; break;
                                case "Corruption": score = 4; break;
                            }
                        }
                        else
                        {
                            switch (opponentCard)
                            {
                                case "Element": score = 2; break;
                                case "Attack": score = 1; break;
                                case "Life": score = 3; break;
                                case "Light": score = 3; break;
                                case "Dark": score = 5; break;
                                case "Time": score = 1; break;
                                case "Corruption": score = 1; break;
                            }
                        }
                        if ((playerClass % 100) / 10 == 1) score -= 10;             // 천적에게 교환할 확률 감소
                        if (opponentHealth <= GetStatAttack() * 2 + 2) score /= 2;  // 상대 체력이 낮으면 교환 확률 감소
                        if ((opponentCard.Equals("Attack") || opponentCard.Equals("Element"))
                            && GetHealth() <= bm.GetPlayers()[opponentPlayerIndex].GetStatAttack())
                            score /= 2; // 자신이 킬각이고 상대가 공격 카드를 낼 것으로 예측한 경우 교환할 확률 대폭 감소
                        break;
                    case "Life":
                        switch (opponentCard)
                        {
                            case "Element": score = 36; break;
                            case "Attack": score = 20 - (bm.GetPlayers()[opponentPlayerIndex].GetStatAttack() / 2); break;
                            case "Light": score = 61; break;
                            case "Dark": score = 45; break;
                            case "Time": score = 52; break;
                            case "Corruption": score = 36; break;
                        }
                        if ((playerClass % 100) / 10 == 0 && opponentHealth <= 20)
                            score += 6; // 천적이 아닌 상대가 체력이 낮으면 확률 증가
                        if ((playerClass % 100) / 10 == 1)
                        {
                            score -= 10; // 천적에게 교환할 확률 감소
                            if (opponentHealth <= 20) score += 6;
                            if (GetHealth() <= bm.GetPlayers()[opponentPlayerIndex].GetStatAttack() * 2 + 2) score -= 5;
                        }
                        if ((opponentCard.Equals("Attack") || opponentCard.Equals("Element"))
                            && GetHealth() <= bm.GetPlayers()[opponentPlayerIndex].GetStatAttack())
                            score /= 2; // 자신이 킬각이고 상대가 공격 카드를 낼 것으로 예측한 경우 교환할 확률 대폭 감소
                        break;
                    case "Light":
                        switch (opponentCard)
                        {
                            case "Element": score = 16; break;
                            case "Attack": score = 10 - (bm.GetPlayers()[opponentPlayerIndex].GetStatAttack() / 2); break;
                            case "Life": score = 41; break;
                            case "Dark": score = 32; break;
                            case "Time": score = 32; break;
                            case "Corruption": score = 16; break;
                        }
                        if ((playerClass % 100) / 10 == 1) score -= 10; // 천적에게 교환할 확률 감소
                        if ((opponentCard.Equals("Attack") || opponentCard.Equals("Element"))
                            && GetHealth() <= bm.GetPlayers()[opponentPlayerIndex].GetStatAttack())
                            score /= 2; // 자신이 킬각이고 상대가 공격 카드를 낼 것으로 예측한 경우 교환할 확률 대폭 감소
                        break;
                    case "Dark":
                        switch (opponentCard)
                        {
                            case "Element": score = 34; break;
                            case "Attack": score = 34; break;
                            case "Life": score = 9; break;
                            case "Light": score = 9; break;
                            case "Time": score = 13; break;
                            case "Corruption": score = 5; break;
                        }
                        if ((playerClass % 100) / 10 == 0 && opponentHealth <= 20)
                            score += 4; // 천적이 아닌 상대가 체력이 낮으면 확률 증가
                        if ((playerClass % 100) / 10 == 1)
                        {
                            score += 7; // 천적에게 교환할 확률 증가
                            if (opponentHealth <= 20) score += 6;
                            if (GetHealth() <= bm.GetPlayers()[opponentPlayerIndex].GetStatAttack() * 2 + 2) score += 3;
                        }
                        break;
                    case "Time":
                        switch (opponentCard)
                        {
                            case "Element": score = 2; break;
                            case "Attack": score = 1; break;
                            case "Life": score = 26; break;
                            case "Light": score = 26; break;
                            case "Dark": score = 10; break;
                            case "Corruption": score = 1; break;
                        }
                        break;
                    case "Corruption":
                        if (statTactic == 1)
                        {
                            switch (opponentCard)
                            {
                                case "Element": score = 6; break;
                                case "Attack": score = 2; break;
                                case "Life": score = 8; break;
                                case "Light": score = 8; break;
                                case "Dark": score = 13; break;
                                case "Time": score = 5; break;
                            }
                            break;
                        }
                        else
                        {
                            switch (opponentCard)
                            {
                                case "Element": score = 24; break;
                                case "Attack": score = 20 - (bm.GetPlayers()[opponentPlayerIndex].GetStatAttack() / 2); break;
                                case "Life": score = 52; break;
                                case "Light": score = 52; break;
                                case "Dark": score = 45; break;
                                case "Time": score = 37; break;
                            }
                        }
                        if ((playerClass % 100) / 10 == 1) score -= 10; // 천적에게 교환할 확률 감소
                        if (GetStatMentality() < statMntlMinAI) score += 10;    // 정신력에 투자하는 중에는 낼 확률 증가
                        if ((opponentCard.Equals("Attack") || opponentCard.Equals("Element"))
                            && GetHealth() <= bm.GetPlayers()[opponentPlayerIndex].GetStatAttack())
                            score /= 2; // 자신이 킬각이고 상대가 공격 카드를 낼 것으로 예측한 경우 교환할 확률 대폭 감소
                        break;
                }
                break;
            case 2:     // 속성을 모르는 상대
            case 102:
            case 12:
            case 112:
                switch (myCard)
                {
                    case "Fire":
                    case "Water":
                    case "Electricity":
                    case "Wind":
                    case "Poison":
                        switch (opponentCard)
                        {
                            case "Element": score = 25; break;
                            case "Attack": score = 14 - (bm.GetPlayers()[opponentPlayerIndex].GetStatAttack() / 2); break;
                            case "Life": score = 41; break;
                            case "Light": score = 50; break;
                            case "Dark": score = 29; break;
                            case "Time": score = 34; break;
                            case "Corruption": score = 25; break;
                        }
                        if ((playerClass % 100) / 10 == 1) score -= 5; // 천적에게 교환할 확률 감소
                        if ((opponentCard.Equals("Attack") || opponentCard.Equals("Element"))
                            && GetHealth() <= bm.GetPlayers()[opponentPlayerIndex].GetStatAttack())
                            score /= 2; // 자신이 킬각이고 상대가 공격 카드를 낼 것으로 예측한 경우 교환할 확률 대폭 감소
                        break;
                    case "Life":
                        switch (opponentCard)
                        {
                            case "Element": score = 9; break;
                            case "Attack": score = 7 - (bm.GetPlayers()[opponentPlayerIndex].GetStatAttack() / 2); break;
                            case "Light": score = 18; break;
                            case "Dark": score = 25; break;
                            case "Time": score = 9; break;
                            case "Corruption": score = 9; break;
                        }
                        if ((playerClass % 100) / 10 == 1) score -= 5; // 천적에게 교환할 확률 감소
                        if ((opponentCard.Equals("Attack") || opponentCard.Equals("Element"))
                            && GetHealth() <= bm.GetPlayers()[opponentPlayerIndex].GetStatAttack())
                            score /= 2; // 자신이 킬각이고 상대가 공격 카드를 낼 것으로 예측한 경우 교환할 확률 대폭 감소
                        break;
                    case "Light":
                        switch (opponentCard)
                        {
                            case "Element": score = 49; break;
                            case "Attack": score = 26 - (bm.GetPlayers()[opponentPlayerIndex].GetStatAttack() / 2); break;
                            case "Life": score = 65; break;
                            case "Dark": score = 58; break;
                            case "Time": score = 61; break;
                            case "Corruption": score = 49; break;
                        }
                        if ((playerClass % 100) / 10 == 1) score -= 5; // 천적에게 교환할 확률 감소
                        if ((opponentCard.Equals("Attack") || opponentCard.Equals("Element"))
                            && GetHealth() <= bm.GetPlayers()[opponentPlayerIndex].GetStatAttack())
                            score /= 2; // 자신이 킬각이고 상대가 공격 카드를 낼 것으로 예측한 경우 교환할 확률 대폭 감소
                        break;
                    case "Dark":
                        switch (opponentCard)
                        {
                            case "Element": score = 29; break;
                            case "Attack": score = 36; break;
                            case "Life": score = 4; break;
                            case "Light": score = 5; break;
                            case "Time": score = 4; break;
                            case "Corruption": score = 2; break;
                        }
                        break;
                    case "Time":
                        switch (opponentCard)
                        {
                            case "Element": score = 4; break;
                            case "Attack": score = 2; break;
                            case "Life": score = 13; break;
                            case "Light": score = 20; break;
                            case "Dark": score = 20; break;
                            case "Corruption": score = 4; break;
                        }
                        break;
                    case "Corruption":
                        if (statTactic == 1)
                        {
                            switch (opponentCard)
                            {
                                case "Element": score = 6; break;
                                case "Attack": score = 2; break;
                                case "Life": score = 8; break;
                                case "Light": score = 8; break;
                                case "Dark": score = 13; break;
                                case "Time": score = 5; break;
                            }
                            break;
                        }
                        else
                        {
                            switch (opponentCard)
                            {
                                case "Element": score = 35; break;
                                case "Attack": score = 26 - (bm.GetPlayers()[opponentPlayerIndex].GetStatAttack() / 2); break;
                                case "Life": score = 61; break;
                                case "Light": score = 61; break;
                                case "Dark": score = 54; break;
                                case "Time": score = 50; break;
                            }
                        }
                        if ((playerClass % 100) / 10 == 1) score -= 5; // 천적에게 교환할 확률 감소
                        if (GetStatMentality() < statMntlMinAI) score += 10;    // 정신력에 투자하는 중에는 낼 확률 증가
                        if ((opponentCard.Equals("Attack") || opponentCard.Equals("Element"))
                            && GetHealth() <= bm.GetPlayers()[opponentPlayerIndex].GetStatAttack())
                            score /= 2; // 자신이 킬각이고 상대가 공격 카드를 낼 것으로 예측한 경우 교환할 확률 대폭 감소
                        break;
                }
                break;
        }
        if (score < 1) score = 1;

        score = score * score;  // 유리한 행동을 할 확률과 불리한 행동을 할 확률의 차이를 크게 벌린다.

        // 상대가 시간을 낼 것이라면, 뽑기에 넣기 전에 내가 원래 내려던 카드로 다시 바꿔준다.
        if (opponentCard == "Time")
        {
            myCard = voidCard;
        }
        bm.RpcPrintLog(score + " lotteries say " + opponentPlayerIndex + myCard + ".");
        for (int i = 0; i < score; i++)
        {
            box.Add(opponentPlayerIndex + myCard);
        }
    }

    public void AIAttackUp()
    {
        if (currentAttack < 99 && currentExperience >= 5)
        {
            currentAttack++;
            currentExperience -= 5;
        }
    }
    
    public void AIAuthorityUp()
    {
        if (currentAuthority < 99 && currentExperience >= 5)
        {
            currentAuthority++;
            currentExperience -= 5;
        }
    }

    public void AIMentalityUp()
    {
        if (currentMentality < 99 && currentExperience >= currentMentality + 1)
        {
            currentMentality++;
            currentExperience -= currentMentality;
        }
    }
    
    /*
    public void CopyPlayerControl(PlayerControl old)
    {
        if (!isServer || old == null) return;
        currentHealth = old.currentHealth;    // 현재 남은 체력(실시간으로 변화, 외부 열람 불가)
        currentAttack = old.currentAttack;    // 현재 공격력(실시간으로 변화, 능력치 패널을 제외한 곳에서 열람 불가)
        currentAuthority = old.currentAuthority;    // 현재 권력(실시간으로 변화, 능력치 패널을 제외한 곳에서 열람 불가)
        currentMentality = old.currentMentality;    // 현재 권력(실시간으로 변화, 능력치 패널을 제외한 곳에서 열람 불가)
        currentExperience = old.currentExperience;   // 현재 남은 경험치(실시간으로 변화, 능력치 패널을 제외한 곳에서 열람 불가)
        maxHealth = old.maxHealth;      // 최대 체력(초기 체력)
        isDead = old.isDead;  // 사망 여부(true이면 사망)
        displayedHealth = old.displayedHealth;                      // 현재 남은 체력(턴이 끝날 때만 변화, 외부 열람 가능)
        statAttack = old.statAttack;      // 현재 공격력(외부 열람 가능)
        statAuthority = old.statAuthority;   // 현재 권력(외부 열람 가능)
        statMentality = old.statMentality;   // 현재 정신력(외부 열람 가능)
        experience = old.experience;      // 현재 남은 경험치
        isFreezed = old.isFreezed;                   // 빙결 여부(true이면 다음 한 번의 내 턴에 교환 불가)

        unveiled = old.unveiled;
        // unveiled의 인덱스는 (플레이어 번호 - 1)이고, 그 값은 해당 플레이어의 속성이 이 플레이어에게 공개되었는지 여부이다.
        // 자기 자신의 속성은 항상 공개되어 있는 것으로 취급한다.
        objectTarget = old.objectTarget;
    }
    */
}