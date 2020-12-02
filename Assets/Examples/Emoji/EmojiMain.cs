using System;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

public class EmojiMain : MonoBehaviour
{
    private GComponent _mainView;
    private GList _list;
    private GTextInput _input1;
    private GTextInput _input2;
    private GComponent _emojiSelectUI1;
    private GComponent _emojiSelectUI2;

    private class Message
    {
        public string sender;
        public string senderIcon;
        public string msg;
        public bool fromMe;
    }

    private List<Message> _messages;

    private Dictionary<uint, Emoji> _emojies;

    private void Awake()
    {
        UIPackage.AddPackage("UI/Emoji");

        UIConfig.verticalScrollBar = "ui://Emoji/ScrollBar_VT";
        UIConfig.defaultScrollBarDisplay = ScrollBarDisplayType.Auto;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        _messages = new List<Message>();

        _mainView = GetComponent<UIPanel>().ui;

        _list = _mainView.GetChild("list").asList;
        _list.SetVirtual();
        _list.itemProvider = GetListItemResource;
        _list.itemRenderer = RenderListItem;

        _input1 = _mainView.GetChild("input1").asTextInput;
        _input1.onKeyDown.Add(__inputKeyDown1);

        _input2 = _mainView.GetChild("input2").asTextInput;
        _input2.onKeyDown.Add(__inputKeyDown2);

        //作为demo，这里只添加了部分表情素材
        _emojies = new Dictionary<uint, Emoji>();
        for (uint i = 0x1f600; i < 0x1f637; i++)
        {
            var url = UIPackage.GetItemURL("Emoji", Convert.ToString(i, 16));
            if (url != null)
                _emojies.Add(i, new Emoji(url));
        }

        _input2.emojies = _emojies;

        _mainView.GetChild("btnSend1").onClick.Add(__clickSendBtn1);
        _mainView.GetChild("btnSend2").onClick.Add(__clickSendBtn2);

        _mainView.GetChild("btnEmoji1").onClick.Add(__clickEmojiBtn1);
        _mainView.GetChild("btnEmoji2").onClick.Add(__clickEmojiBtn2);

        _emojiSelectUI1 = UIPackage.CreateObject("Emoji", "EmojiSelectUI").asCom;
        _emojiSelectUI1.fairyBatching = true;
        _emojiSelectUI1.GetChild("list").asList.onClickItem.Add(__clickEmoji1);

        _emojiSelectUI2 = UIPackage.CreateObject("Emoji", "EmojiSelectUI_ios").asCom;
        _emojiSelectUI2.fairyBatching = true;
        _emojiSelectUI2.GetChild("list").asList.onClickItem.Add(__clickEmoji2);
    }

    private void AddMsg(string sender, string senderIcon, string msg, bool fromMe)
    {
        var isScrollBottom = _list.scrollPane.isBottomMost;

        var newMessage = new Message();
        newMessage.sender = sender;
        newMessage.senderIcon = senderIcon;
        newMessage.msg = msg;
        newMessage.fromMe = fromMe;
        _messages.Add(newMessage);

        if (newMessage.fromMe)
            if (_messages.Count == 1 || UnityEngine.Random.Range(0f, 1f) < 0.5f)
            {
                var replyMessage = new Message();
                replyMessage.sender = "FairyGUI";
                replyMessage.senderIcon = "r1";
                replyMessage.msg = "Today is a good day. \U0001f600";
                replyMessage.fromMe = false;
                _messages.Add(replyMessage);
            }

        if (_messages.Count > 100)
            _messages.RemoveRange(0, _messages.Count - 100);

        _list.numItems = _messages.Count;

        if (isScrollBottom)
            _list.scrollPane.ScrollBottom();
    }

    private string GetListItemResource(int index)
    {
        var msg = _messages[index];
        if (msg.fromMe)
            return "ui://Emoji/chatRight";
        else
            return "ui://Emoji/chatLeft";
    }

    private void RenderListItem(int index, GObject obj)
    {
        var item = (GButton) obj;
        var msg = _messages[index];
        if (!msg.fromMe)
            item.GetChild("name").text = msg.sender;
        item.icon = UIPackage.GetItemURL("Emoji", msg.senderIcon);

        //Recaculate the text width
        var tf = item.GetChild("msg").asRichTextField;
        tf.emojies = _emojies;
        tf.text = EmojiParser.inst.Parse(msg.msg);
    }

    private void __clickSendBtn1(EventContext context)
    {
        var msg = _input1.text;
        if (msg.Length == 0)
            return;

        AddMsg("Unity", "r0", msg, true);
        _input1.text = "";
    }

    private void __clickSendBtn2(EventContext context)
    {
        var msg = _input2.text;
        if (msg.Length == 0)
            return;

        AddMsg("Unity", "r0", msg, true);
        _input2.text = "";
    }

    private void __clickEmojiBtn1(EventContext context)
    {
        GRoot.inst.ShowPopup(_emojiSelectUI1, (GObject) context.sender, PopupDirection.Up);
    }

    private void __clickEmojiBtn2(EventContext context)
    {
        GRoot.inst.ShowPopup(_emojiSelectUI2, (GObject) context.sender, PopupDirection.Up);
    }

    private void __clickEmoji1(EventContext context)
    {
        var item = (GButton) context.data;
        _input1.ReplaceSelection("[:" + item.text + "]");
    }

    private void __clickEmoji2(EventContext context)
    {
        var item = (GButton) context.data;
        _input2.ReplaceSelection(char.ConvertFromUtf32(Convert.ToInt32(UIPackage.GetItemByURL(item.icon).name, 16)));
    }

    private void __inputKeyDown1(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Return)
            _mainView.GetChild("btnSend1").onClick.Call();
    }

    private void __inputKeyDown2(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Return)
            _mainView.GetChild("btnSend2").onClick.Call();
    }

    private void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape) Application.Quit();
    }
}