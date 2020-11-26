#if FAIRYGUI_TOLUA
using LuaInterface;
#endif

namespace FairyGUI
{

    public class EventListener
    {
        EventBridge _bridge;
        string _type;

        public EventListener(EventDispatcher owner, string type)
        {
            _bridge = owner.GetEventBridge(type);
            _type = type;
        }


        public string type
        {
            get { return _type; }
        }


        /// <param name="callback"></param>
        public void AddCapture(EventCallback1 callback)
        {
            _bridge.AddCapture(callback);
        }


        /// <param name="callback"></param>
        public void RemoveCapture(EventCallback1 callback)
        {
            _bridge.RemoveCapture(callback);
        }


        /// <param name="callback"></param>
        public void Add(EventCallback1 callback)
        {
            _bridge.Add(callback);
        }


        /// <param name="callback"></param>
        public void Remove(EventCallback1 callback)
        {
            _bridge.Remove(callback);
        }


        /// <param name="callback"></param>
#if FAIRYGUI_TOLUA
        [NoToLua]
#endif
        public void Add(EventCallback0 callback)
        {
            _bridge.Add(callback);
        }


        /// <param name="callback"></param>
#if FAIRYGUI_TOLUA
        [NoToLua]
#endif
        public void Remove(EventCallback0 callback)
        {
            _bridge.Remove(callback);
        }


        /// <param name="callback"></param>
        public void Set(EventCallback1 callback)
        {
            _bridge.Clear();
            if (callback != null)
                _bridge.Add(callback);
        }


        /// <param name="callback"></param>
#if FAIRYGUI_TOLUA
        [NoToLua]
#endif
        public void Set(EventCallback0 callback)
        {
            _bridge.Clear();
            if (callback != null)
                _bridge.Add(callback);
        }

#if FAIRYGUI_TOLUA

        /// <param name="func"></param>
        /// <param name="self"></param>
        public void Add(LuaFunction func, LuaTable self)
        {
            _bridge.Add(func, self);
        }


        /// <param name="func"></param>
        /// <param name="self"></param>
        public void Add(LuaFunction func, GComponent self)
        {
            _bridge.Add(func, self);
        }


        /// <param name="func"></param>
        /// <param name="self"></param>
        public void Remove(LuaFunction func, LuaTable self)
        {
            _bridge.Remove(func, self);
        }


        /// <param name="func"></param>
        /// <param name="self"></param>
        public void Remove(LuaFunction func, GComponent self)
        {
            _bridge.Remove(func, self);
        }


        /// <param name="func"></param>
        /// <param name="self"></param>
        public void Set(LuaFunction func, LuaTable self)
        {
            _bridge.Clear();
            if (func != null)
                Add(func, self);
        }


        /// <param name="func"></param>
        /// <param name="self"></param>
        public void Set(LuaFunction func, GComponent self)
        {
            _bridge.Clear();
            if (func != null)
                Add(func, self);
        }
#endif


        public bool isEmpty
        {
            get
            {
                return !_bridge.owner.hasEventListeners(_type);
            }
        }


        public bool isDispatching
        {
            get
            {
                return _bridge.owner.isDispatching(_type);
            }
        }


        public void Clear()
        {
            _bridge.Clear();
        }


        /// <returns></returns>
        public bool Call()
        {
            return _bridge.owner.InternalDispatchEvent(_type, _bridge, null, null);
        }


        /// <param name="data"></param>
        /// <returns></returns>
        public bool Call(object data)
        {
            return _bridge.owner.InternalDispatchEvent(_type, _bridge, data, null);
        }


        /// <param name="data"></param>
        /// <returns></returns>
        public bool BubbleCall(object data)
        {
            return _bridge.owner.BubbleEvent(_type, data);
        }


        /// <returns></returns>
        public bool BubbleCall()
        {
            return _bridge.owner.BubbleEvent(_type, null);
        }


        /// <param name="data"></param>
        /// <returns></returns>
        public bool BroadcastCall(object data)
        {
            return _bridge.owner.BroadcastEvent(_type, data);
        }


        /// <returns></returns>
        public bool BroadcastCall()
        {
            return _bridge.owner.BroadcastEvent(_type, null);
        }
    }
}
