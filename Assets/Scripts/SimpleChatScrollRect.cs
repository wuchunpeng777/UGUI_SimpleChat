using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SimpleChatScrollRect : ScrollRect
{
    class ChatInfo
    {
        public float y;
        public float height;
        public string msg;
        public Transform target;
    }

    public RectTransform rt_prefab;

    public RectTransform rt_top;
    public RectTransform rt_pool;

    public Action act_topAppend;


    public Action<string, Transform> act_updateItem;


    private TextMeshProUGUI prefab_txt;
    private float contentHeight = 0;

    private bool triggerTop = false;

    Vector2 anchorMin = new Vector2(0, 1);
    Vector2 anchorMax = new Vector2(1, 1);
    Vector2 pivot = new Vector2(0.5f, 1);

    private List<ChatInfo> data = new List<ChatInfo>();
    private Queue<ChatInfo> dataPool = new Queue<ChatInfo>();

    protected override void Awake()
    {
        base.Awake();

        onValueChanged.AddListener(OnValueChange);
        prefab_txt = rt_prefab.Find("msg").GetComponent<TextMeshProUGUI>();
    }

    void AddToPool(ChatInfo info)
    {
        if (info.target)
            info.target.SetParent(rt_pool, false);
        info.target = null;
    }

    Transform GetFromPool()
    {
        if (rt_pool.childCount == 0)
        {
            GameObject go = GameObject.Instantiate(rt_prefab.gameObject);
            return go.transform;
        }
        else
        {
            return rt_pool.GetChild(0);
        }
    }

    void OnValueChange(Vector2 vec)
    {
        for (int i = 0; i < data.Count; i++)
        {
            ChatInfo info = data[i];
            if (info.target != null)
            {
                //检测是否不在区域了
                if (!IsInViewport(info))
                    AddToPool(info);
            }
            else
            {
                //检测是否在区域
                if (IsInViewport(info))
                    AddToViewport(info);
            }
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);

        if (triggerTop)
        {
            if (content.anchoredPosition.y >= -rt_top.sizeDelta.y)
            {
                triggerTop = false;
                rt_top.gameObject.SetActive(false);
            }
        }
        else
        {
            if (content.anchoredPosition.y < -rt_top.sizeDelta.y)
            {
                triggerTop = true;
                rt_top.gameObject.SetActive(true);
            }
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        if (triggerTop)
        {
            if (act_topAppend != null)
                act_topAppend();
            rt_top.gameObject.SetActive(false);
            triggerTop = false;
        }
    }

    bool IsInViewport(ChatInfo info)
    {
        float topY = -content.anchoredPosition.y + viewport.rect.height * 5;
        float bottomY = -content.anchoredPosition.y - viewport.rect.height - viewport.rect.height * 5;

        if (info.y - info.height <= topY && info.y >= bottomY)
            return true;
        return false;
    }

    void AddToViewport(ChatInfo info)
    {
        GameObject go = GetFromPool().gameObject;
        go.transform.SetParent(content, false);
        info.target = go.transform;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        var oMin = rt.offsetMin;
        oMin.x = 0;
        rt.offsetMin = oMin;
        var oMax = rt.offsetMax;
        oMax.x = 0;
        rt.offsetMax = oMax;


        var pos = rt.anchoredPosition;
        pos.y = info.y;
        rt.anchoredPosition = pos;

        act_updateItem(info.msg, go.transform);
    }


    ChatInfo GetDataFromPool()
    {
        if (dataPool.Count == 0)
            return new ChatInfo();
        else
        {
            return dataPool.Dequeue();
        }
    }

    ChatInfo AddData(string msg)
    {
        prefab_txt.text = msg;
        prefab_txt.ForceMeshUpdate();

        LayoutRebuilder.ForceRebuildLayoutImmediate(rt_prefab);
        ChatInfo info = GetDataFromPool();
        info.msg = msg;
        info.y = data.Count > 0 ? data[data.Count - 1].y - data[data.Count - 1].height : 0;
        info.height = rt_prefab.sizeDelta.y;
        data.Add(info);

        contentHeight += info.height;
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);

        return info;
    }

    public void AppendMsgToHead(string msg)
    {
        ChatInfo lastInfo = data.Count == 0 ? null : data[0];

        prefab_txt.text = msg;
        prefab_txt.ForceMeshUpdate();

        LayoutRebuilder.ForceRebuildLayoutImmediate(rt_prefab);
        ChatInfo info = GetDataFromPool();
        info.msg = msg;
        info.height = rt_prefab.sizeDelta.y;
        info.y = 0;
        data.Insert(0, info);

        AddToViewport(info);

        contentHeight += info.height;
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);

        float totalH = -info.height;
        for (int i = 1; i < data.Count; i++)
        {
            ChatInfo ct = data[i];
            ct.y = totalH;
            if (ct.target != null)
            {
                var p = ct.target.GetComponent<RectTransform>().anchoredPosition;
                p.y = ct.y;
                ct.target.GetComponent<RectTransform>().anchoredPosition = p;
            }

            totalH -= ct.height;
        }

        if (lastInfo != null)
        {
            var p = content.anchoredPosition;
            p.y = p.y - lastInfo.y;
            content.anchoredPosition = p;
        }
    }

    public void ResetData(List<string> _data)
    {
        for (int i = 0; i < data.Count; i++)
        {
            ChatInfo info = data[i];
            AddToPool(info);
        }

        foreach (ChatInfo info in data)
        {
            dataPool.Enqueue(info);
        }

        contentHeight = 0;
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
        data.Clear();

        foreach (string s in _data)
        {
            AddData(s);
        }

        verticalNormalizedPosition = 0;

        for (int i = data.Count - 1; i >= 0; i--)
        {
            ChatInfo info = data[i];
            if (IsInViewport(info))
                AddToViewport(info);
            else
            {
                break;
            }
        }
    }

    public void AddMsg(string msg)
    {
        ChatInfo lastInfo = data.Count == 0 ? null : data[data.Count - 1];
        ChatInfo info = AddData(msg);
        if (lastInfo == null)
        {
            AddToViewport(info);
            ProcMovement();
        }
        else
        {
            float bottomY = -content.anchoredPosition.y - viewport.rect.height;

            if (lastInfo.y >= bottomY || lastInfo.y - lastInfo.height >= bottomY)
            {
                AddToViewport(info);
                ProcMovement();
            }
        }
    }

    private Coroutine cor_move;

    void ProcMovement()
    {
        if (cor_move != null)
            StopCoroutine(cor_move);
        this.DOKill();
        if (velocity.y == 0)
            cor_move = StartCoroutine(DelayMove());
    }

    WaitForEndOfFrame wait = new WaitForEndOfFrame();

    IEnumerator DelayMove()
    {
        yield return wait;
        this.DOVerticalNormalizedPos(0, 0.3f).SetEase(Ease.InOutSine);
    }
}