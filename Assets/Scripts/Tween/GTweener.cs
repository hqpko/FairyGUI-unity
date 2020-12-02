using System;
using UnityEngine;

#if FAIRYGUI_TOLUA
using LuaInterface;
#endif

namespace FairyGUI
{
    public delegate void GTweenCallback();

    /// <param name="tweener"></param>
    public delegate void GTweenCallback1(GTweener tweener);

    public interface ITweenListener
    {
        /// <param name="tweener"></param>
        void OnTweenStart(GTweener tweener);

        /// <param name="tweener"></param>
        void OnTweenUpdate(GTweener tweener);

        /// <param name="tweener"></param>
        void OnTweenComplete(GTweener tweener);
    }

    public class GTweener
    {
        internal object _target;
        internal TweenPropType _propType;
        internal bool _killed;
        internal bool _paused;

        private float _delay;
        private float _duration;
        private float _breakpoint;
        private EaseType _easeType;
        private float _easeOvershootOrAmplitude;
        private float _easePeriod;
        private int _repeat;
        private bool _yoyo;
        private float _timeScale;
        private bool _ignoreEngineTimeScale;
        private bool _snapping;
        private object _userData;
        private GPath _path;
        private CustomEase _customEase;

        private GTweenCallback _onUpdate;
        private GTweenCallback _onStart;
        private GTweenCallback _onComplete;
        private GTweenCallback1 _onUpdate1;
        private GTweenCallback1 _onStart1;
        private GTweenCallback1 _onComplete1;
        private ITweenListener _listener;

        private TweenValue _startValue;
        private TweenValue _endValue;
        private TweenValue _value;
        private TweenValue _deltaValue;
        private int _valueSize;

        private bool _started;
        private int _ended;
        private float _elapsedTime;
        private float _normalizedTime;
        private int _smoothStart;

        public GTweener()
        {
            _startValue = new TweenValue();
            _endValue = new TweenValue();
            _value = new TweenValue();
            _deltaValue = new TweenValue();
        }

        /// <param name="value"></param>
        /// <returns></returns>
        public GTweener SetDelay(float value)
        {
            _delay = value;
            return this;
        }

        public float delay => _delay;

        /// <param name="value"></param>
        /// <returns></returns>
        public GTweener SetDuration(float value)
        {
            _duration = value;
            return this;
        }

        public float duration => _duration;

        /// <param name="value"></param>
        /// <returns></returns>
        public GTweener SetBreakpoint(float value)
        {
            _breakpoint = value;
            return this;
        }

        /// <param name="value"></param>
        /// <returns></returns>
        public GTweener SetEase(EaseType value)
        {
            _easeType = value;
            return this;
        }

        /// <param name="value"></param>
        /// <param name="customEase"></param>
        /// <returns></returns>
        public GTweener SetEase(EaseType value, CustomEase customEase)
        {
            _easeType = value;
            _customEase = customEase;
            return this;
        }

        /// <param name="value"></param>
        /// <returns></returns>
        public GTweener SetEasePeriod(float value)
        {
            _easePeriod = value;
            return this;
        }

        /// <param name="value"></param>
        /// <returns></returns>
        public GTweener SetEaseOvershootOrAmplitude(float value)
        {
            _easeOvershootOrAmplitude = value;
            return this;
        }

        /// <param name="times"></param>
        /// <param name="yoyo"></param>
        /// <returns></returns>
        public GTweener SetRepeat(int times, bool yoyo = false)
        {
            _repeat = times;
            _yoyo = yoyo;
            return this;
        }

        public int repeat => _repeat;

        /// <param name="value"></param>
        /// <returns></returns>
        public GTweener SetTimeScale(float value)
        {
            _timeScale = value;
            return this;
        }

        /// <param name="value"></param>
        /// <returns></returns>
        public GTweener SetIgnoreEngineTimeScale(bool value)
        {
            _ignoreEngineTimeScale = value;
            return this;
        }

        /// <param name="value"></param>
        /// <returns></returns>
        public GTweener SetSnapping(bool value)
        {
            _snapping = value;
            return this;
        }

        /// <param name="value"></param>
        /// <returns></returns>
        public GTweener SetPath(GPath value)
        {
            _path = value;
            return this;
        }

        /// <param name="value"></param>
        /// <returns></returns>
        public GTweener SetTarget(object value)
        {
            _target = value;
            _propType = TweenPropType.None;
            return this;
        }

        /// <param name="value"></param>
        /// <param name="propType"></param>
        /// <returns></returns>
        public GTweener SetTarget(object value, TweenPropType propType)
        {
            _target = value;
            _propType = propType;
            return this;
        }

        public object target => _target;

        /// <param name="value"></param>
        /// <returns></returns>
        public GTweener SetUserData(object value)
        {
            _userData = value;
            return this;
        }

        public object userData => _userData;

        /// <param name="callback"></param>
        /// <returns></returns>
#if FAIRYGUI_TOLUA
        [NoToLua]
#endif
        public GTweener OnUpdate(GTweenCallback callback)
        {
            _onUpdate = callback;
            return this;
        }

        /// <param name="callback"></param>
        /// <returns></returns>
#if FAIRYGUI_TOLUA
        [NoToLua]
#endif
        public GTweener OnStart(GTweenCallback callback)
        {
            _onStart = callback;
            return this;
        }

        /// <param name="callback"></param>
        /// <returns></returns>
#if FAIRYGUI_TOLUA
        [NoToLua]
#endif
        public GTweener OnComplete(GTweenCallback callback)
        {
            _onComplete = callback;
            return this;
        }

        /// <param name="callback"></param>
        /// <returns></returns>
        public GTweener OnUpdate(GTweenCallback1 callback)
        {
            _onUpdate1 = callback;
            return this;
        }

        /// <param name="callback"></param>
        /// <returns></returns>
        public GTweener OnStart(GTweenCallback1 callback)
        {
            _onStart1 = callback;
            return this;
        }

        /// <param name="callback"></param>
        /// <returns></returns>
        public GTweener OnComplete(GTweenCallback1 callback)
        {
            _onComplete1 = callback;
            return this;
        }

        /// <param name="value"></param>
        /// <returns></returns>
        public GTweener SetListener(ITweenListener value)
        {
            _listener = value;
            return this;
        }

        public TweenValue startValue => _startValue;

        public TweenValue endValue => _endValue;

        public TweenValue value => _value;

        public TweenValue deltaValue => _deltaValue;

        public float normalizedTime => _normalizedTime;

        public bool completed => _ended != 0;

        public bool allCompleted => _ended == 1;

        /// <param name="paused"></param>
        /// <returns></returns>
        public GTweener SetPaused(bool paused)
        {
            _paused = paused;
            if (_paused)
                _smoothStart = 0;
            return this;
        }

        /// <param name="time"></param>
        public void Seek(float time)
        {
            if (_killed)
                return;

            _elapsedTime = time;
            if (_elapsedTime < _delay)
            {
                if (_started)
                    _elapsedTime = _delay;
                else
                    return;
            }

            Update();
        }

        /// <param name="complete"></param>
        public void Kill(bool complete = false)
        {
            if (_killed)
                return;

            if (complete)
            {
                if (_ended == 0)
                {
                    if (_breakpoint >= 0)
                        _elapsedTime = _delay + _breakpoint;
                    else if (_repeat >= 0)
                        _elapsedTime = _delay + _duration * (_repeat + 1);
                    else
                        _elapsedTime = _delay + _duration * 2;
                    Update();
                }

                CallCompleteCallback();
            }

            _killed = true;
        }

        internal GTweener _To(float start, float end, float duration)
        {
            _valueSize = 1;
            _startValue.x = start;
            _endValue.x = end;
            _value.x = start;
            _duration = duration;
            return this;
        }

        internal GTweener _To(Vector2 start, Vector2 end, float duration)
        {
            _valueSize = 2;
            _startValue.vec2 = start;
            _endValue.vec2 = end;
            _value.vec2 = start;
            _duration = duration;
            return this;
        }

        internal GTweener _To(Vector3 start, Vector3 end, float duration)
        {
            _valueSize = 3;
            _startValue.vec3 = start;
            _endValue.vec3 = end;
            _value.vec3 = start;
            _duration = duration;
            return this;
        }

        internal GTweener _To(Vector4 start, Vector4 end, float duration)
        {
            _valueSize = 4;
            _startValue.vec4 = start;
            _endValue.vec4 = end;
            _value.vec4 = start;
            _duration = duration;
            return this;
        }

        internal GTweener _To(Color start, Color end, float duration)
        {
            _valueSize = 4;
            _startValue.color = start;
            _endValue.color = end;
            _value.color = start;
            _duration = duration;
            return this;
        }

        internal GTweener _To(double start, double end, float duration)
        {
            _valueSize = 5;
            _startValue.d = start;
            _endValue.d = end;
            _value.d = start;
            _duration = duration;
            return this;
        }

        internal GTweener _Shake(Vector3 start, float amplitude, float duration)
        {
            _valueSize = 6;
            _startValue.vec3 = start;
            _startValue.w = amplitude;
            _duration = duration;
            _easeType = EaseType.Linear;
            return this;
        }

        internal void _Init()
        {
            _delay = 0;
            _duration = 0;
            _breakpoint = -1;
            _easeType = EaseType.QuadOut;
            _timeScale = 1;
            _ignoreEngineTimeScale = false;
            _easePeriod = 0;
            _easeOvershootOrAmplitude = 1.70158f;
            _snapping = false;
            _repeat = 0;
            _yoyo = false;
            _valueSize = 0;
            _started = false;
            _paused = false;
            _killed = false;
            _elapsedTime = 0;
            _normalizedTime = 0;
            _ended = 0;
            _path = null;
            _customEase = null;
            _smoothStart = Time.frameCount == 1 ? 3 : 1; //刚启动时会有多帧的超时
        }

        internal void _Reset()
        {
            _target = null;
            _listener = null;
            _userData = null;
            _onStart = _onUpdate = _onComplete = null;
            _onStart1 = _onUpdate1 = _onComplete1 = null;
        }

        internal void _Update()
        {
            if (_ended != 0) //Maybe completed by seek
            {
                CallCompleteCallback();
                _killed = true;
                return;
            }

            float dt;
            if (_smoothStart > 0)
            {
                _smoothStart--;
                dt = Mathf.Clamp(Time.unscaledDeltaTime, 0,
                    Application.targetFrameRate > 0 ? 1.0f / Application.targetFrameRate : 0.016f);
                if (!_ignoreEngineTimeScale)
                    dt *= Time.timeScale;
            }
            else if (_ignoreEngineTimeScale)
            {
                dt = Time.unscaledDeltaTime;
            }
            else
            {
                dt = Time.deltaTime;
            }

            if (_timeScale != 1)
                dt *= _timeScale;
            if (dt == 0)
                return;

            _elapsedTime += dt;
            Update();

            if (_ended != 0)
                if (!_killed)
                {
                    CallCompleteCallback();
                    _killed = true;
                }
        }

        private void Update()
        {
            _ended = 0;

            if (_valueSize == 0) //DelayedCall
            {
                if (_elapsedTime >= _delay + _duration)
                    _ended = 1;

                return;
            }

            if (!_started)
            {
                if (_elapsedTime < _delay)
                    return;

                _started = true;
                CallStartCallback();
                if (_killed)
                    return;
            }

            var reversed = false;
            var tt = _elapsedTime - _delay;
            if (_breakpoint >= 0 && tt >= _breakpoint)
            {
                tt = _breakpoint;
                _ended = 2;
            }

            if (_repeat != 0)
            {
                var round = Mathf.FloorToInt(tt / _duration);
                tt -= _duration * round;
                if (_yoyo)
                    reversed = round % 2 == 1;

                if (_repeat > 0 && _repeat - round < 0)
                {
                    if (_yoyo)
                        reversed = _repeat % 2 == 1;
                    tt = _duration;
                    _ended = 1;
                }
            }
            else if (tt >= _duration)
            {
                tt = _duration;
                _ended = 1;
            }

            _normalizedTime = EaseManager.Evaluate(_easeType, reversed ? _duration - tt : tt, _duration,
                _easeOvershootOrAmplitude, _easePeriod, _customEase);

            _value.SetZero();
            _deltaValue.SetZero();

            if (_valueSize == 5)
            {
                var d = _startValue.d + (_endValue.d - _startValue.d) * _normalizedTime;
                if (_snapping)
                    d = Math.Round(d);
                _deltaValue.d = d - _value.d;
                _value.d = d;
                _value.x = (float) d;
            }
            else if (_valueSize == 6)
            {
                if (_ended == 0)
                {
                    var r = UnityEngine.Random.insideUnitSphere;
                    r.x = r.x > 0 ? 1 : -1;
                    r.y = r.y > 0 ? 1 : -1;
                    r.z = r.z > 0 ? 1 : -1;
                    r *= _startValue.w * (1 - _normalizedTime);

                    _deltaValue.vec3 = r;
                    _value.vec3 = _startValue.vec3 + r;
                }
                else
                {
                    _value.vec3 = _startValue.vec3;
                }
            }
            else if (_path != null)
            {
                var vec3 = _path.GetPointAt(_normalizedTime);
                if (_snapping)
                {
                    vec3.x = Mathf.Round(vec3.x);
                    vec3.y = Mathf.Round(vec3.y);
                    vec3.z = Mathf.Round(vec3.z);
                }

                _deltaValue.vec3 = vec3 - _value.vec3;
                _value.vec3 = vec3;
            }
            else
            {
                for (var i = 0; i < _valueSize; i++)
                {
                    var n1 = _startValue[i];
                    var n2 = _endValue[i];
                    var f = n1 + (n2 - n1) * _normalizedTime;
                    if (_snapping)
                        f = Mathf.Round(f);
                    _deltaValue[i] = f - _value[i];
                    _value[i] = f;
                }

                _value.d = _value.x;
            }

            if (_target != null && _propType != TweenPropType.None)
                TweenPropTypeUtils.SetProps(_target, _propType, _value);

            CallUpdateCallback();
        }

        private void CallStartCallback()
        {
            if (GTween.catchCallbackExceptions)
            {
                try
                {
                    if (_onStart1 != null)
                        _onStart1(this);
                    if (_onStart != null)
                        _onStart();
                    if (_listener != null)
                        _listener.OnTweenStart(this);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("FairyGUI: error in start callback > " + e.Message);
                }
            }
            else
            {
                if (_onStart1 != null)
                    _onStart1(this);
                if (_onStart != null)
                    _onStart();
                if (_listener != null)
                    _listener.OnTweenStart(this);
            }
        }

        private void CallUpdateCallback()
        {
            if (GTween.catchCallbackExceptions)
            {
                try
                {
                    if (_onUpdate1 != null)
                        _onUpdate1(this);
                    if (_onUpdate != null)
                        _onUpdate();
                    if (_listener != null)
                        _listener.OnTweenUpdate(this);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("FairyGUI: error in update callback > " + e.Message);
                }
            }
            else
            {
                if (_onUpdate1 != null)
                    _onUpdate1(this);
                if (_onUpdate != null)
                    _onUpdate();
                if (_listener != null)
                    _listener.OnTweenUpdate(this);
            }
        }

        private void CallCompleteCallback()
        {
            if (GTween.catchCallbackExceptions)
            {
                try
                {
                    if (_onComplete1 != null)
                        _onComplete1(this);
                    if (_onComplete != null)
                        _onComplete();
                    if (_listener != null)
                        _listener.OnTweenComplete(this);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("FairyGUI: error in complete callback > " + e.Message);
                }
            }
            else
            {
                if (_onComplete1 != null)
                    _onComplete1(this);
                if (_onComplete != null)
                    _onComplete();
                if (_listener != null)
                    _listener.OnTweenComplete(this);
            }
        }
    }
}