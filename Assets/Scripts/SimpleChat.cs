using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimpleChat : MonoBehaviour
{
    public SimpleChatScrollRect scrollRect;
    public TMP_InputField ipt_msg;
    public Button btn_add;
    public Button btn_append;

    private int count;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;

        btn_add.onClick.AddListener(DoSubmit);

        scrollRect.act_topAppend = OnTopAppend;
        scrollRect.act_updateItem = UpdateChatItem;

        btn_append.onClick.AddListener(delegate
        {
            scrollRect.AppendMsgToHead("hiduwadgwuyagdyigwuyagduywgauyduwaguydgauywg---" + count++);
        });
    }

    void DoSubmit()
    {
        if (!string.IsNullOrEmpty(ipt_msg.text))
            scrollRect.AddMsg(ipt_msg.text);
    }

    void UpdateChatItem(string msg, Transform cell)
    {
        TextMeshProUGUI txt_msg = cell.Find("msg").GetComponent<TextMeshProUGUI>();
        txt_msg.text = msg;
    }


    void OnTopAppend()
    {
        for (int i = 0; i < 20; i++)
        {
            scrollRect.AppendMsgToHead("hiduwadgwuyagdyigwuyagduywgauyduwaguydgauywg---" + count++);
        }
    }
}