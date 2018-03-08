using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIPointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] private int kind = -1;  // 0: 공격력, 1: 권력, 2: 정신력, 3: 경험치
    private int active;

    public GameObject tooltipBox;
    public TutorialUI tutorialPanel;
    private static TooltipUI tooltip;

    void Awake()
    {
        tooltip = null;
        active = -1;
    }

    void FixedUpdate()
    {
        if (tooltip != null) return;
        if (active == 0)
        {
            GameObject t = Instantiate(tooltipBox, Alert.alert.gameObject.transform);   // Alert.alert.gameObject는 메인 Canvas
            tooltip = t.GetComponent<TooltipUI>();
            tooltip.SetText("공격력", new Color(0.647f, 0.647f, 0.647f), "공격 카드(불, 물, 전기, 바람, 독)의 효과에 관여하는 능력치입니다.\n공격력이 높으면 상대에게 더 큰 피해를 주고 게임을 빠르게 끝낼 수 있습니다.");
            tooltip.SetPosition(0.01f, 0.321f, 0.99f, 0.47f);
            tooltip.Appear();
            active = -1;
        }
        else if (active == 1)
        {
            GameObject t = Instantiate(tooltipBox, Alert.alert.gameObject.transform);   // Alert.alert.gameObject는 메인 Canvas
            tooltip = t.GetComponent<TooltipUI>();
            tooltip.SetText("권력", new Color(0.8f, 0.365f, 0.078f), "교환할 수 있는 대상을 제한하는 능력치입니다.\n자신의 턴에는 자신보다 권력이 낮거나 같은 플레이어에게만 교환을 요청할 수 있습니다. 예외적으로 권력이 가장 낮은 플레이어들은 권력이 가장 높은 플레이어들에게 교환을 요청할 수 있습니다.");
            tooltip.SetPosition(0.01f, 0.321f, 0.99f, 0.47f);
            tooltip.Appear();
            active = -1;
        }
        else if (active == 2)
        {
            GameObject t = Instantiate(tooltipBox, Alert.alert.gameObject.transform);   // Alert.alert.gameObject는 메인 Canvas
            tooltip = t.GetComponent<TooltipUI>();
            tooltip.SetText("정신력", new Color(0.305f, 0.125f, 0.8f), "경험치 획득량에 관여하는 능력치입니다.\n능력치 분배 시간이 될 때마다 자신의 정신력만큼 경험치를 획득합니다. 정신력이 높으면 게임 후반에 일어나는 상황에 유연하게 대처할 수 있습니다.");
            tooltip.SetPosition(0.01f, 0.321f, 0.99f, 0.47f);
            tooltip.Appear();
            active = -1;
        }
        else if (active == 3)
        {
            GameObject t = Instantiate(tooltipBox, Alert.alert.gameObject.transform);   // Alert.alert.gameObject는 메인 Canvas
            tooltip = t.GetComponent<TooltipUI>();
            tooltip.SetText("경험치", new Color(0.137f, 0.729f, 0.118f), "공격력, 권력 또는 정신력을 올릴 때 필요한 자원입니다. 능력치 분배 시간이 될 때마다 자신의 정신력만큼 경험치를 획득합니다.");
            tooltip.SetPosition(0.01f, 0.321f, 0.99f, 0.47f);
            tooltip.Appear();
            active = -1;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Alert.alert == null) return;
        tutorialPanel.Check();
        active = kind;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip == null) return;
        tooltip.Disappear();
        active = -1;
    }
}
