﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Card : NetworkBehaviour {

    [SerializeField] private string cardName;
    [SerializeField] private int cardCode; // 0 ~ 9
    // 0: 불, 1: 물, 2: 전기, 3: 바람, 4: 독, 5: 생명, 6: 빛, 7: 어둠, 8: 시간, 9: 타락
    private Image Border; // HighLight
    static public PlayerControl localPlayer = null;

    private static BattleManager bm;

    private bool isMoving = false;                                  // 한 번에 하나의 함수만 실행하기 위해 사용되는 변수
    private Queue<IEnumerator> process = new Queue<IEnumerator>();  // 함수를 순차적으로 실행하기 위한 Queue

    private void Awake()
    {
        Border = GetComponentInChildren<Image>();
        if (Border == null)
        {
            Debug.LogWarning("Border is null");
            return;
        }
        Border.gameObject.SetActive(false);
    }

    private void Start()
    {
        bm = BattleManager.bm;
    }

    private void FixedUpdate()
    {
        if (localPlayer == null)
        {
            return;
        }
        // Queue에서 줄 서있는 함수들을 하나씩 차례로 실행시킵니다.
        if (process.Count != 0 && !isMoving)
        {
            isMoving = true;
            StartCoroutine(process.Dequeue());
        }
    }

    public string GetCardName()
    {
        return cardName;
    }
    

    /// <summary>
    /// 카드 교환 시 카드를 이동시키는 함수입니다. 이제는 뒤집기를 포함합니다.
    /// </summary>
    /// <param name="startDest">10 * (출발 인덱스) + (도착 인덱스)</param>
    [ClientRpc]
    public void RpcMoveCard(int startDest)
    {
        if (startDest > 109 || startDest < 0) return;
        int start = startDest / 10;
        int dest = startDest % 10;

        // 함수를 Queue에 넣어 처리합니다. (Queue는 줄 세우기와 비슷한 개념입니다.)
        process.Enqueue(Move(start, dest));
    }

    /// <summary>
    /// 카드를 뒤집는 함수입니다.
    /// </summary>
    /// <param name="pos">카드 인덱스</param>
    [ClientRpc]
    public void RpcFlipCard(int pos, bool toBack)
    {
        if (pos < 0 || pos > 10) return;

        // 함수를 Queue에 넣어 처리합니다.
        process.Enqueue(Flip(pos, toBack));
    }

    [ClientRpc]
    public void RpcFlipCardImmediate(int pos, bool toBack)
    {
        if (pos < 0 || pos > 10) return;

        // 함수를 Queue에 넣어 처리합니다.
        process.Enqueue(FlipI(pos, toBack));
    }

    IEnumerator Move(int start, int dest)
    {
        Vector3 sp = GetPosition(start);
        Vector3 dp = GetPosition(dest);
        Quaternion sr = GetRotationBack(start);
        Quaternion mr = GetRotationMoving(start, dest);
        Quaternion dr = GetRotationBack(dest);
        Quaternion fr;
        Quaternion br;
        float t = Time.time;

        // 내가 낸 카드를 앞면에서 뒷면으로 뒤집습니다.
        // 이동하는 카드가 공격 카드가 아니어도 어디서든지 앞면에서 뒷면으로 뒤집습니다.
        if (start / 2 == localPlayer.GetPlayerIndex() || (start != 10 && GetCardCode() >= 5))
        {
            t = Time.time;
            fr = GetRotationFront(start);
            br = GetRotationBack(start);
            while (Time.time < t + 1f)
            {
                GetComponent<Transform>().rotation = Quaternion.Slerp(fr, br, (Time.time - t) / 1f);
                yield return null;
            }
            GetComponent<Transform>().rotation = br;
        }

        // 움직일 방향으로 회전입니다.
        t = Time.time;
        while (Time.time < t + (25f / 60f))
        {
            GetComponent<Transform>().rotation = Quaternion.Slerp(sr, mr, (Time.time - t) / (25f / 60f));
            yield return null;
        }
        GetComponent<Transform>().rotation = mr;
        
        // 목적지까지 직진입니다.
        t = Time.time;
        while (Time.time < t + (2f / 3f))
        {
            GetComponent<Transform>().position = Vector3.Lerp(sp, dp, (Time.time - t) / (2f / 3f));
            yield return null;
        }
        GetComponent<Transform>().position = dp;
        
        // 주차할 방향으로 회전입니다.
        t = Time.time;
        while (Time.time < t + (25f / 60f))
        {
            GetComponent<Transform>().rotation = Quaternion.Slerp(mr, dr, (Time.time - t) / (25f / 60f));
            yield return null;
        }
        GetComponent<Transform>().rotation = dr;

        // 내가 받은 카드를 뒷면에서 앞면으로 뒤집습니다.
        // 이동하는 카드가 공격 카드가 아니어도 어디서든지 뒷면에서 앞면으로 뒤집습니다.
        if (dest / 2 == localPlayer.GetPlayerIndex() || GetCardCode() >= 5)
        {
            t = Time.time;
            fr = GetRotationFront(dest);
            br = GetRotationBack(dest);
            while (Time.time < t + 1f)
            {
                GetComponent<Transform>().rotation = Quaternion.Slerp(br, fr, (Time.time - t) / 1f);
                yield return null;
            }
            GetComponent<Transform>().rotation = fr;
        }

        // 교환을 종료합니다. (turnStep이 9일 때만 실행됨)
        localPlayer.CmdAfterExchange();

        yield return null;
        isMoving = false;
    }

    IEnumerator Flip(int pos, bool toBack)
    {
        float t;
        Quaternion fr = GetRotationFront(pos);
        Quaternion br = GetRotationBack(pos);
        
        if (toBack) // 앞면일 때 뒷면으로
        {
            t = Time.time;
            while (Time.time < t + 1f)
            {
                GetComponent<Transform>().rotation = Quaternion.Slerp(fr, br, (Time.time - t) / 1f);
                yield return null;
            }
            GetComponent<Transform>().rotation = br;
        }
        else // 뒷면일 때 앞면으로
        {
            t = Time.time;
            while (Time.time < t + 1f)
            {
                GetComponent<Transform>().rotation = Quaternion.Slerp(br, fr, (Time.time - t) / 1f);
                yield return null;
            }
            GetComponent<Transform>().rotation = fr;
        }
        yield return null;
        isMoving = false;
    }

    IEnumerator FlipI(int pos, bool toBack)
    {
        if (toBack)
        {
            GetComponent<Transform>().rotation = GetRotationBack(pos);
        }
        else
        {
            GetComponent<Transform>().rotation = GetRotationFront(pos);
        }
        isMoving = false;
        yield return null;
    }

    public static Vector3 GetPosition(int pos)
    {
        switch(pos)
        {
            case 0:
                return new Vector3(-0.35f, 0f, 1.2f);
            case 1:
                return new Vector3(0.35f, 0f, 1.2f);
            case 2:
                return new Vector3(1.986576f, 0f, 2.388951f);
            case 3:
                return new Vector3(2.202888f, 0f, 3.05469f);
            case 4:
                return new Vector3(1.577814f, 0f, 4.978455f);
            case 5:
                return new Vector3(1.011502f, 0f, 5.389904f);
            case 6:
                return new Vector3(-1.011502f, 0f, 5.389904f);
            case 7:
                return new Vector3(-1.577814f, 0f, 4.978455f);
            case 8:
                return new Vector3(-2.202888f, 0f, 3.05469f);
            case 9:
                return new Vector3(-1.986576f, 0f, 2.388951f);
            default:
                return new Vector3(0f, 0f, 3.4026f);
        }
    }

    public static Quaternion GetRotationBack(int pos)
    {
        switch(pos)
        {
            case 0:
            case 1:
                return Quaternion.Euler(90f, 90f, 90f);
            case 2:
            case 3:
                return Quaternion.Euler(90f, 18f, 90f);
            case 4:
            case 5:
                return Quaternion.Euler(90f, -54f, 90f);
            case 6:
            case 7:
                return Quaternion.Euler(90f, 234f, 90f);
            case 8:
            case 9:
                return Quaternion.Euler(90f, 162f, 90f);
            default:
                return Quaternion.Euler(90f, -90f, 90f);
        }
    }

    public static Quaternion GetRotationFront(int pos)
    {
        switch (pos)
        {
            case 0:
            case 1:
                return Quaternion.Euler(-90f, 90f, 90f);
            case 2:
            case 3:
                return Quaternion.Euler(-90f, 18f, 90f);
            case 4:
            case 5:
                return Quaternion.Euler(-90f, -54f, 90f);
            case 6:
            case 7:
                return Quaternion.Euler(-90f, 234f, 90f);
            case 8:
            case 9:
                return Quaternion.Euler(-90f, 162f, 90f);
            default:
                return Quaternion.Euler(-90f, -90f, 90f);
        }
    }

    public static Quaternion GetRotationMoving(int start, int dest)
    {
        if (start < 0 || start > 10 || dest < 0 || dest > 10)
        {
            return Quaternion.identity;
        }
        else if (start / 2 == dest / 2)
        {
            return GetRotationFront(start);
        }

        if (start == 0 && dest == 2) return Quaternion.Euler(90f, 153f, 90f);
        else if (start == 0 && dest == 3) return Quaternion.Euler(90f, 144f, 90f);
        else if (start == 0 && dest == 4) return Quaternion.Euler(90f, 117f, 90f);
        else if (start == 0 && dest == 5) return Quaternion.Euler(90f, 108f, 90f);
        else if (start == 0 && dest == 6) return Quaternion.Euler(90f, 81f, 90f);
        else if (start == 0 && dest == 7) return Quaternion.Euler(90f, 72f, 90f);
        else if (start == 0 && dest == 8) return Quaternion.Euler(90f, 45f, 90f);
        else if (start == 0 && dest == 9) return Quaternion.Euler(90f, 36f, 90f);
        else if (start == 1 && dest == 2) return Quaternion.Euler(90f, 144f, 90f);
        else if (start == 1 && dest == 3) return Quaternion.Euler(90f, 135f, 90f);
        else if (start == 1 && dest == 4) return Quaternion.Euler(90f, 108f, 90f);
        else if (start == 1 && dest == 5) return Quaternion.Euler(90f, 99f, 90f);
        else if (start == 1 && dest == 6) return Quaternion.Euler(90f, 72f, 90f);
        else if (start == 1 && dest == 7) return Quaternion.Euler(90f, 63f, 90f);
        else if (start == 1 && dest == 8) return Quaternion.Euler(90f, 36f, 90f);
        else if (start == 1 && dest == 9) return Quaternion.Euler(90f, 27f, 90f);
        else if (start == 2 && dest == 4) return Quaternion.Euler(90f, 81f, 90f);
        else if (start == 2 && dest == 5) return Quaternion.Euler(90f, 72f, 90f);
        else if (start == 2 && dest == 6) return Quaternion.Euler(90f, 45f, 90f);
        else if (start == 2 && dest == 7) return Quaternion.Euler(90f, 36f, 90f);
        else if (start == 2 && dest == 8) return Quaternion.Euler(90f, 9f, 90f);
        else if (start == 2 && dest == 9) return Quaternion.Euler(90f, 0f, 90f);
        else if (start == 3 && dest == 4) return Quaternion.Euler(90f, 72f, 90f);
        else if (start == 3 && dest == 5) return Quaternion.Euler(90f, 63f, 90f);
        else if (start == 3 && dest == 6) return Quaternion.Euler(90f, 36f, 90f);
        else if (start == 3 && dest == 7) return Quaternion.Euler(90f, 27f, 90f);
        else if (start == 3 && dest == 8) return Quaternion.Euler(90f, 0f, 90f);
        else if (start == 3 && dest == 9) return Quaternion.Euler(90f, -9f, 90f);
        else if (start == 4 && dest == 6) return Quaternion.Euler(90f, 9f, 90f);
        else if (start == 4 && dest == 7) return Quaternion.Euler(90f, 0f, 90f);
        else if (start == 4 && dest == 8) return Quaternion.Euler(90f, -27f, 90f);
        else if (start == 4 && dest == 9) return Quaternion.Euler(90f, -36f, 90f);
        else if (start == 5 && dest == 6) return Quaternion.Euler(90f, 0f, 90f);
        else if (start == 5 && dest == 7) return Quaternion.Euler(90f, -9f, 90f);
        else if (start == 5 && dest == 8) return Quaternion.Euler(90f, -36f, 90f);
        else if (start == 5 && dest == 9) return Quaternion.Euler(90f, -45f, 90f);
        else if (start == 6 && dest == 8) return Quaternion.Euler(90f, 297f, 90f);
        else if (start == 6 && dest == 9) return Quaternion.Euler(90f, 288f, 90f);
        else if (start == 7 && dest == 8) return Quaternion.Euler(90f, 288f, 90f);
        else if (start == 7 && dest == 9) return Quaternion.Euler(90f, 279f, 90f);
        else if (start == 2 && dest == 0) return Quaternion.Euler(90f, -27f, 90f);
        else if (start == 3 && dest == 0) return Quaternion.Euler(90f, -36f, 90f);
        else if (start == 4 && dest == 0) return Quaternion.Euler(90f, -63f, 90f);
        else if (start == 5 && dest == 0) return Quaternion.Euler(90f, -72f, 90f);
        else if (start == 6 && dest == 0) return Quaternion.Euler(90f, 261f, 90f);
        else if (start == 7 && dest == 0) return Quaternion.Euler(90f, 252f, 90f);
        else if (start == 8 && dest == 0) return Quaternion.Euler(90f, 225f, 90f);
        else if (start == 9 && dest == 0) return Quaternion.Euler(90f, 216f, 90f);
        else if (start == 2 && dest == 1) return Quaternion.Euler(90f, -36f, 90f);
        else if (start == 3 && dest == 1) return Quaternion.Euler(90f, -45f, 90f);
        else if (start == 4 && dest == 1) return Quaternion.Euler(90f, -72f, 90f);
        else if (start == 5 && dest == 1) return Quaternion.Euler(90f, -81f, 90f);
        else if (start == 6 && dest == 1) return Quaternion.Euler(90f, -108f, 90f);
        else if (start == 7 && dest == 1) return Quaternion.Euler(90f, -117f, 90f);
        else if (start == 8 && dest == 1) return Quaternion.Euler(90f, -144f, 90f);
        else if (start == 9 && dest == 1) return Quaternion.Euler(90f, -153f, 90f);
        else if (start == 4 && dest == 2) return Quaternion.Euler(90f, -99f, 90f);
        else if (start == 5 && dest == 2) return Quaternion.Euler(90f, -108f, 90f);
        else if (start == 6 && dest == 2) return Quaternion.Euler(90f, 225f, 90f);
        else if (start == 7 && dest == 2) return Quaternion.Euler(90f, 216f, 90f);
        else if (start == 8 && dest == 2) return Quaternion.Euler(90f, 189f, 90f);
        else if (start == 9 && dest == 2) return Quaternion.Euler(90f, 180f, 90f);
        else if (start == 4 && dest == 3) return Quaternion.Euler(90f, -108f, 90f);
        else if (start == 5 && dest == 3) return Quaternion.Euler(90f, -117f, 90f);
        else if (start == 6 && dest == 3) return Quaternion.Euler(90f, 216f, 90f);
        else if (start == 7 && dest == 3) return Quaternion.Euler(90f, 207f, 90f);
        else if (start == 8 && dest == 3) return Quaternion.Euler(90f, 180f, 90f);
        else if (start == 9 && dest == 3) return Quaternion.Euler(90f, 171f, 90f);
        else if (start == 6 && dest == 4) return Quaternion.Euler(90f, 189f, 90f);
        else if (start == 7 && dest == 4) return Quaternion.Euler(90f, 180f, 90f);
        else if (start == 8 && dest == 4) return Quaternion.Euler(90f, 153f, 90f);
        else if (start == 9 && dest == 4) return Quaternion.Euler(90f, 144f, 90f);
        else if (start == 6 && dest == 5) return Quaternion.Euler(90f, 180f, 90f);
        else if (start == 7 && dest == 5) return Quaternion.Euler(90f, 171f, 90f);
        else if (start == 8 && dest == 5) return Quaternion.Euler(90f, 144f, 90f);
        else if (start == 9 && dest == 5) return Quaternion.Euler(90f, 135f, 90f);
        else if (start == 8 && dest == 6) return Quaternion.Euler(90f, 117f, 90f);
        else if (start == 9 && dest == 6) return Quaternion.Euler(90f, 108f, 90f);
        else if (start == 8 && dest == 7) return Quaternion.Euler(90f, 108f, 90f);
        else if (start == 9 && dest == 7) return Quaternion.Euler(90f, 99f, 90f);
        else if (start == 10 && dest == 0) return Quaternion.Euler(90f, -81f, 90f);
        else if (start == 10 && dest == 1) return Quaternion.Euler(90f, 261f, 90f);
        else if (start == 10 && dest == 2) return Quaternion.Euler(90f, 207f, 90f);
        else if (start == 10 && dest == 3) return Quaternion.Euler(90f, 189f, 90f);
        else if (start == 10 && dest == 4) return Quaternion.Euler(90f, 135f, 90f);
        else if (start == 10 && dest == 5) return Quaternion.Euler(90f, 117f, 90f);
        else if (start == 10 && dest == 6) return Quaternion.Euler(90f, 63f, 90f);
        else if (start == 10 && dest == 7) return Quaternion.Euler(90f, 45f, 90f);
        else if (start == 10 && dest == 8) return Quaternion.Euler(90f, -9f, 90f);
        else if (start == 10 && dest == 9) return Quaternion.Euler(90f, -27f, 90f);
        else return Quaternion.identity;
    }

    public int GetCardCode()
    {
        return cardCode;
    }

    public void SetHighLight(bool TF)
    {
        Border.gameObject.SetActive(TF);
    }
}
