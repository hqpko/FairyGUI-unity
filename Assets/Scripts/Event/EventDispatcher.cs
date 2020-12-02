using System;
using System.Collections.Generic;

namespace FairyGUI
{
    public delegate void EventCallback0();

    public delegate void EventCallback1(EventContext context);

    public class EventDispatcher : IEventDispatcher
    {
        private Dictionary<string, EventBridge> _dic;

        /// <param name="strType"></param>
        /// <param name="callback"></param>
        public void AddEventListener(string strType, EventCallback1 callback)
        {
            GetBridge(strType).Add(callback);
        }

        /// <param name="strType"></param>
        /// <param name="callback"></param>
        public void AddEventListener(string strType, EventCallback0 callback)
        {
            GetBridge(strType).Add(callback);
        }

        /// <param name="strType"></param>
        /// <param name="callback"></param>
        public void RemoveEventListener(string strType, EventCallback1 callback)
        {
            if (_dic != null && _dic.TryGetValue(strType, out var bridge))
                bridge.Remove(callback);
        }

        /// <param name="strType"></param>
        /// <param name="callback"></param>
        public void RemoveEventListener(string strType, EventCallback0 callback)
        {
            if (_dic != null && _dic.TryGetValue(strType, out var bridge))
                bridge.Remove(callback);
        }

        /// <param name="strType"></param>
        /// <param name="callback"></param>
        public void AddCapture(string strType, EventCallback1 callback)
        {
            GetBridge(strType).AddCapture(callback);
        }

        /// <param name="strType"></param>
        /// <param name="callback"></param>
        public void RemoveCapture(string strType, EventCallback1 callback)
        {
            if (_dic != null && _dic.TryGetValue(strType, out var bridge))
                bridge.RemoveCapture(callback);
        }

        /// <param name="strType"></param>
        public void RemoveEventListeners(string strType = null)
        {
            if (_dic != null && strType != null && _dic.TryGetValue(strType, out var bridge))
                bridge.Clear();
            else if (_dic != null && strType == null)
                foreach (var kv in _dic)
                    kv.Value.Clear();
        }

        /// <param name="strType"></param>
        /// <returns></returns>
        public bool HasEventListeners(string strType)
        {
            var bridge = TryGetEventBridge(strType);
            if (bridge == null)
                return false;

            return !bridge.isEmpty;
        }

        /// <param name="strType"></param>
        /// <returns></returns>
        public bool IsDispatching(string strType)
        {
            var isDispatching = TryGetEventBridge(strType)?._dispatching;
            return isDispatching.GetValueOrDefault();
        }

        private EventBridge TryGetEventBridge(string strType)
        {
            if (_dic != null && _dic.TryGetValue(strType, out var bridge)) return bridge;

            return null;
        }

        internal EventBridge GetEventBridge(string strType)
        {
            if (_dic == null)
                _dic = new Dictionary<string, EventBridge>();

            if (!_dic.TryGetValue(strType, out var bridge))
            {
                bridge = new EventBridge(this);
                _dic[strType] = bridge;
            }

            return bridge;
        }

        /// <param name="strType"></param>
        /// <returns></returns>
        public bool DispatchEvent(string strType)
        {
            return DispatchEvent(strType, null);
        }

        /// <param name="strType"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool DispatchEvent(string strType, object data)
        {
            return InternalDispatchEvent(strType, null, data, null);
        }

        public bool DispatchEvent(string strType, object data, object initiator)
        {
            return InternalDispatchEvent(strType, null, data, initiator);
        }

        private static InputEvent _sCurrentInputEvent = new InputEvent();

        internal bool InternalDispatchEvent(string strType, EventBridge bridge, object data, object initiator)
        {
            if (bridge == null)
                bridge = TryGetEventBridge(strType);

            EventBridge gBridge = null;
            if (this is DisplayObject && ((DisplayObject) this).gOwner != null)
                gBridge = ((DisplayObject) this).gOwner.TryGetEventBridge(strType);

            var b1 = bridge != null && !bridge.isEmpty;
            var b2 = gBridge != null && !gBridge.isEmpty;
            if (b1 || b2)
            {
                var context = EventContext.Get();
                context.initiator = initiator ?? this;
                context.type = strType;
                context.data = data;
                if (data is InputEvent inputEvent)
                    _sCurrentInputEvent = inputEvent;
                context.inputEvent = _sCurrentInputEvent;

                if (b1)
                {
                    bridge.CallCaptureInternal(context);
                    bridge.CallInternal(context);
                }

                if (b2)
                {
                    gBridge.CallCaptureInternal(context);
                    gBridge.CallInternal(context);
                }

                EventContext.Return(context);
                context.initiator = null;
                context.sender = null;
                context.data = null;

                return context._defaultPrevented;
            }
            else
            {
                return false;
            }
        }

        /// <param name="context"></param>
        /// <returns></returns>
        public bool DispatchEvent(EventContext context)
        {
            var bridge = TryGetEventBridge(context.type);
            EventBridge gBridge = null;
            if (this is DisplayObject && ((DisplayObject) this).gOwner != null)
                gBridge = ((DisplayObject) this).gOwner.TryGetEventBridge(context.type);

            var savedSender = context.sender;

            if (bridge != null && !bridge.isEmpty)
            {
                bridge.CallCaptureInternal(context);
                bridge.CallInternal(context);
            }

            if (gBridge != null && !gBridge.isEmpty)
            {
                gBridge.CallCaptureInternal(context);
                gBridge.CallInternal(context);
            }

            context.sender = savedSender;
            return context._defaultPrevented;
        }

        /// <param name="strType"></param>
        /// <param name="data"></param>
        /// <param name="addChain"></param>
        /// <returns></returns>
        internal bool BubbleEvent(string strType, object data, List<EventBridge> addChain)
        {
            var context = EventContext.Get();
            context.initiator = this;

            context.type = strType;
            context.data = data;
            if (data is InputEvent inputEvent)
                _sCurrentInputEvent = inputEvent;
            context.inputEvent = _sCurrentInputEvent;
            var bubbleChain = context.callChain;
            bubbleChain.Clear();

            GetChainBridges(strType, bubbleChain, true);

            var length = bubbleChain.Count;
            for (var i = length - 1; i >= 0; i--)
            {
                bubbleChain[i].CallCaptureInternal(context);
                if (context._touchCapture)
                {
                    context._touchCapture = false;
                    if (strType == "onTouchBegin")
                        Stage.inst.AddTouchMonitor(context.inputEvent.touchId, bubbleChain[i].owner);
                }
            }

            if (!context._stopsPropagation)
            {
                for (var i = 0; i < length; ++i)
                {
                    bubbleChain[i].CallInternal(context);

                    if (context._touchCapture)
                    {
                        context._touchCapture = false;
                        if (strType == "onTouchBegin")
                            Stage.inst.AddTouchMonitor(context.inputEvent.touchId, bubbleChain[i].owner);
                    }

                    if (context._stopsPropagation)
                        break;
                }

                if (addChain != null)
                {
                    length = addChain.Count;
                    for (var i = 0; i < length; ++i)
                    {
                        var bridge = addChain[i];
                        if (bubbleChain.IndexOf(bridge) == -1)
                        {
                            bridge.CallCaptureInternal(context);
                            bridge.CallInternal(context);
                        }
                    }
                }
            }

            EventContext.Return(context);
            context.initiator = null;
            context.sender = null;
            context.data = null;
            return context._defaultPrevented;
        }

        /// <param name="strType"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool BubbleEvent(string strType, object data)
        {
            return BubbleEvent(strType, data, null);
        }

        /// <param name="strType"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool BroadcastEvent(string strType, object data)
        {
            var context = EventContext.Get();
            context.initiator = this;
            context.type = strType;
            context.data = data;
            if (data is InputEvent inputEvent)
                _sCurrentInputEvent = inputEvent;
            context.inputEvent = _sCurrentInputEvent;
            var bubbleChain = context.callChain;
            bubbleChain.Clear();

            if (this is Container)
                GetChildEventBridges(strType, (Container) this, bubbleChain);
            else if (this is GComponent)
                GetChildEventBridges(strType, (GComponent) this, bubbleChain);

            var length = bubbleChain.Count;
            for (var i = 0; i < length; ++i)
                bubbleChain[i].CallInternal(context);

            EventContext.Return(context);
            context.initiator = null;
            context.sender = null;
            context.data = null;
            return context._defaultPrevented;
        }

        private EventBridge GetBridge(string strType)
        {
            if (strType == null)
                throw new Exception("event type cant be null");

            if (_dic == null)
                _dic = new Dictionary<string, EventBridge>();

            if (!_dic.TryGetValue(strType, out var bridge))
            {
                bridge = new EventBridge(this);
                _dic[strType] = bridge;
            }

            return bridge;
        }

        private static void GetChildEventBridges(string strType, Container container, List<EventBridge> bridges)
        {
            var bridge = container.TryGetEventBridge(strType);
            if (bridge != null)
                bridges.Add(bridge);
            if (container.gOwner != null)
            {
                bridge = container.gOwner.TryGetEventBridge(strType);
                if (bridge != null && !bridge.isEmpty)
                    bridges.Add(bridge);
            }

            var count = container.numChildren;
            for (var i = 0; i < count; ++i)
            {
                var obj = container.GetChildAt(i);
                if (obj is Container con)
                {
                    GetChildEventBridges(strType, con, bridges);
                }
                else
                {
                    bridge = obj.TryGetEventBridge(strType);
                    if (bridge != null && !bridge.isEmpty)
                        bridges.Add(bridge);

                    if (obj.gOwner != null)
                    {
                        bridge = obj.gOwner.TryGetEventBridge(strType);
                        if (bridge != null && !bridge.isEmpty)
                            bridges.Add(bridge);
                    }
                }
            }
        }

        private static void GetChildEventBridges(string strType, GComponent container, List<EventBridge> bridges)
        {
            var bridge = container.TryGetEventBridge(strType);
            if (bridge != null)
                bridges.Add(bridge);

            var count = container.numChildren;
            for (var i = 0; i < count; ++i)
            {
                var obj = container.GetChildAt(i);
                if (obj is GComponent com)
                {
                    GetChildEventBridges(strType, com, bridges);
                }
                else
                {
                    bridge = obj.TryGetEventBridge(strType);
                    if (bridge != null)
                        bridges.Add(bridge);
                }
            }
        }

        internal void GetChainBridges(string strType, List<EventBridge> chain, bool bubble)
        {
            var bridge = TryGetEventBridge(strType);
            if (bridge != null && !bridge.isEmpty)
                chain.Add(bridge);

            if (this is DisplayObject && ((DisplayObject) this).gOwner != null)
            {
                bridge = ((DisplayObject) this).gOwner.TryGetEventBridge(strType);
                if (bridge != null && !bridge.isEmpty)
                    chain.Add(bridge);
            }

            if (!bubble)
                return;

            if (this is DisplayObject)
            {
                var element = (DisplayObject) this;
                while ((element = element.parent) != null)
                {
                    bridge = element.TryGetEventBridge(strType);
                    if (bridge != null && !bridge.isEmpty)
                        chain.Add(bridge);

                    if (element.gOwner != null)
                    {
                        bridge = element.gOwner.TryGetEventBridge(strType);
                        if (bridge != null && !bridge.isEmpty)
                            chain.Add(bridge);
                    }
                }
            }
            else if (this is GObject)
            {
                var element = (GObject) this;
                while ((element = element.parent) != null)
                {
                    bridge = element.TryGetEventBridge(strType);
                    if (bridge != null && !bridge.isEmpty)
                        chain.Add(bridge);
                }
            }
        }
    }
}