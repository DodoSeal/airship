﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;

namespace Luau {
    public class LuauSignalWrapper {
        [DisallowMultipleComponent]
        internal class LuauSignalDestroyWatcher : MonoBehaviour {
            internal Action<bool> DestroyCallback;
            internal LuauContext Context;
            private bool _destroyed = false;
            private void OnContextReset(LuauContext ctx) {
                if (ctx != Context || _destroyed) return;
                _destroyed = true;
                LuauCore.onResetInstance -= OnContextReset;
                DestroyCallback.Invoke(true);
            }
            private void Awake() {
                LuauCore.onResetInstance += OnContextReset;
            }
            private void OnDestroy() {
                if (_destroyed) return;
                _destroyed = true;
                LuauCore.onResetInstance -= OnContextReset;
                DestroyCallback.Invoke(false);
            }
        }

        internal event Action RequestDisconnect;
        
        private readonly LuauContext _context;
        private readonly IntPtr _thread;
        private readonly int _instanceId;
        private readonly ulong _propNameHash;
        
#if UNITY_EDITOR
        private static List<LuauSignalWrapper> _staticSignalWrappers = new();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() {
            foreach (var signalWrapper in _staticSignalWrappers) {
                signalWrapper.Destroy();
            }
            _staticSignalWrappers = new List<LuauSignalWrapper>();
        }
#endif
        
        private static void WritePropertyToThread(IntPtr thread, object parameter) {
            if (parameter == null) {
                LuauCore.WritePropertyToThread(thread, null, null);
            } else {
                LuauCore.WritePropertyToThread(thread, parameter, parameter.GetType());
            }
        }

        public LuauSignalWrapper(LuauContext context, IntPtr thread, int instanceId, ulong propNameHash) {
            _context = context;
            _thread = thread;
            _instanceId = instanceId;
            _propNameHash = propNameHash;
        }
        
        public void HandleEvent_0() {
            HandleEvent();
        }
        
        public void HandleEvent_1<T0>(T0 p0) {
            HandleEvent(p0);
        }
        
        public void HandleEvent_2<T0, T1>(T0 p0, T1 p1) {
            HandleEvent(p0, p1);
        }
        
        public void HandleEvent_3<T0, T1, T2>(T0 p0, T1 p1, T2 p2) {
            HandleEvent(p0, p1, p2);
        }
        
        public void HandleEvent_4<T0, T1, T2, T3>(T0 p0, T1 p1, T2 p2, T3 p3) {
            HandleEvent(p0, p1, p2, p3);
        }

        private void HandleEvent(params object[] p) {
            Profiler.BeginSample("HandleCSToLuauSignalEvent");
            
            // var threadData = ThreadDataManager.GetThreadDataByPointer(_thread);
            // if (threadData != null && !threadData.m_error) {
                foreach (var param in p) {
                    WritePropertyToThread(_thread, param);
                }

                var alive = LuauPlugin.LuauEmitSignal(_context, _thread, _instanceId, _propNameHash, p.Length);
                if (!alive) {
                    RequestDisconnect?.Invoke();
                }
            // }
            
            Profiler.EndSample();
        }

        public void Destroy() {
            RequestDisconnect?.Invoke();
        }

        private static void AddSignalDestroyWatcher(GameObject go, LuauContext context, Action<bool> onDestroy) {
            if (go.GetComponent<LuauSignalDestroyWatcher>() != null) return;
        
            var destroyWatcher = go.AddComponent<LuauSignalDestroyWatcher>();
            destroyWatcher.Context = context;
            destroyWatcher.DestroyCallback = onDestroy;
        }

        private static GameObject GetGameObjectFromObject(object obj) {
            if (obj is GameObject go) return go;
            return obj is not MonoBehaviour behaviour ? null : behaviour.gameObject;
        }
        
        public static int HandleCsEvent(LuauContext context, IntPtr thread, object objectReference, int instanceId, ulong propNameHash, EventInfo eventInfo, bool staticClass) {
            var newSignalCreated = LuauPlugin.LuauPushSignal(context, thread, instanceId, propNameHash);
            if (newSignalCreated) {
                GameObject go = null;
                if (!staticClass) {
                    go = GetGameObjectFromObject(objectReference);
                    if (go == null) return 0;
                }
            
                LuauPlugin.LuauPinThread(thread);
                
                var signalWrapper = new LuauSignalWrapper(context, thread, instanceId, propNameHash);

                var eventInfoParams = eventInfo.EventHandlerType.GetMethod("Invoke").GetParameters();

                var handlerMethodName = $"HandleEvent_{eventInfoParams.Length}";
                var method = signalWrapper.GetType().GetMethod(handlerMethodName);
                var d = Delegate.CreateDelegate(eventInfo.EventHandlerType, signalWrapper, method.MakeGenericMethod(eventInfo.EventHandlerType.GetGenericArguments()));
                eventInfo.AddEventHandler(objectReference, d);
                
                signalWrapper.RequestDisconnect += () => {
                    eventInfo.RemoveEventHandler(objectReference, d);
                };

                if (!staticClass) {
                    // Disconnect when the object is destroyed or the context is reset:
                    AddSignalDestroyWatcher(go, context, (contextReset) => {
                        if (!contextReset) {
                            LuauPlugin.LuauDestroySignals(context, thread, instanceId);
                            LuauPlugin.LuauUnpinThread(thread);
                        }
                        eventInfo.RemoveEventHandler(objectReference, d);
                    });
                } else {
                    // Disconnect static C# events when the associated LuauContext is reset:
                    Action<LuauContext> reset = null;
                    reset = (ctx) => {
                        if (ctx != context) return;
                        LuauCore.onResetInstance -= reset;
                        eventInfo.RemoveEventHandler(objectReference, d);
                    };
                    LuauCore.onResetInstance += reset;
#if UNITY_EDITOR
                    // Ensure static connections get cleaned up in-editor between plays without domain reloading:
                    _staticSignalWrappers.Add(signalWrapper);
#endif
                }
            }
            return 1;
        }
        
        public static int HandleUnityEvent0(LuauContext context, IntPtr thread, object objectReference, int instanceId, ulong propNameHash, UnityEvent unityEvent) {
            var newSignalCreated = LuauPlugin.LuauPushSignal(context, thread, instanceId, propNameHash);
            if (newSignalCreated) {
                var go = GetGameObjectFromObject(objectReference);
                if (go == null) return 0;
            
                LuauPlugin.LuauPinThread(thread);
            
                var signalWrapper = new LuauSignalWrapper(context, thread, instanceId, propNameHash);
                unityEvent.AddListener(signalWrapper.HandleEvent_0);
                signalWrapper.RequestDisconnect += () => {
                    unityEvent.RemoveListener(signalWrapper.HandleEvent_0);
                };

                AddSignalDestroyWatcher(go, context, (contextReset) => {
                    if (!contextReset) {
                        LuauPlugin.LuauDestroySignals(context, thread, instanceId);
                        LuauPlugin.LuauUnpinThread(thread);
                    }
                    unityEvent.RemoveListener(signalWrapper.HandleEvent_0);
                });
            }
            return 1;
        }
        
        public static int HandleUnityEvent1<T0>(LuauContext context, IntPtr thread, object objectReference, int instanceId, ulong propNameHash, UnityEvent<T0> unityEvent) {
            var newSignalCreated = LuauPlugin.LuauPushSignal(context, thread, instanceId, propNameHash);
            if (newSignalCreated) {
                var go = GetGameObjectFromObject(objectReference);
                if (go == null) return 0;
            
                LuauPlugin.LuauPinThread(thread);
            
                var signalWrapper = new LuauSignalWrapper(context, thread, instanceId, propNameHash);
                unityEvent.AddListener(signalWrapper.HandleEvent_1);
                signalWrapper.RequestDisconnect += () => {
                    unityEvent.RemoveListener(signalWrapper.HandleEvent_1);
                };

                AddSignalDestroyWatcher(go, context, (contextReset) => {
                    if (!contextReset) {
                        LuauPlugin.LuauDestroySignals(context, thread, instanceId);
                        LuauPlugin.LuauUnpinThread(thread);
                    }
                    unityEvent.RemoveListener(signalWrapper.HandleEvent_1);
                });
            }
            return 1;
        }
        
        public static int HandleUnityEvent2<T0, T1>(LuauContext context, IntPtr thread, object objectReference, int instanceId, ulong propNameHash, UnityEvent<T0, T1> unityEvent) {
            var newSignalCreated = LuauPlugin.LuauPushSignal(context, thread, instanceId, propNameHash);
            if (newSignalCreated) {
                var go = GetGameObjectFromObject(objectReference);
                if (go == null) return 0;
            
                LuauPlugin.LuauPinThread(thread);
            
                var signalWrapper = new LuauSignalWrapper(context, thread, instanceId, propNameHash);
                unityEvent.AddListener(signalWrapper.HandleEvent_2);
                signalWrapper.RequestDisconnect += () => {
                    unityEvent.RemoveListener(signalWrapper.HandleEvent_2);
                };

                AddSignalDestroyWatcher(go, context, (contextReset) => {
                    if (!contextReset) {
                        LuauPlugin.LuauDestroySignals(context, thread, instanceId);
                        LuauPlugin.LuauUnpinThread(thread);
                    }
                    unityEvent.RemoveListener(signalWrapper.HandleEvent_2);
                });
            }
            return 1;
        }
        
        public static int HandleUnityEvent3<T0, T1, T2>(LuauContext context, IntPtr thread, object objectReference, int instanceId, ulong propNameHash, UnityEvent<T0, T1, T2> unityEvent) {
            var newSignalCreated = LuauPlugin.LuauPushSignal(context, thread, instanceId, propNameHash);
            if (newSignalCreated) {
                var go = GetGameObjectFromObject(objectReference);
                if (go == null) return 0;
            
                LuauPlugin.LuauPinThread(thread);
            
                var signalWrapper = new LuauSignalWrapper(context, thread, instanceId, propNameHash);
                unityEvent.AddListener(signalWrapper.HandleEvent_3);
                signalWrapper.RequestDisconnect += () => {
                    unityEvent.RemoveListener(signalWrapper.HandleEvent_3);
                };

                AddSignalDestroyWatcher(go, context, (contextReset) => {
                    if (!contextReset) {
                        LuauPlugin.LuauDestroySignals(context, thread, instanceId);
                        LuauPlugin.LuauUnpinThread(thread);
                    }
                    unityEvent.RemoveListener(signalWrapper.HandleEvent_3);
                });
            }
            return 1;
        }
        
        public static int HandleUnityEvent4<T0, T1, T2, T3>(LuauContext context, IntPtr thread, object objectReference, int instanceId, ulong propNameHash, UnityEvent<T0, T1, T2, T3> unityEvent) {
            var newSignalCreated = LuauPlugin.LuauPushSignal(context, thread, instanceId, propNameHash);
            if (newSignalCreated) {
                var go = GetGameObjectFromObject(objectReference);
                if (go == null) return 0;
            
                LuauPlugin.LuauPinThread(thread);
            
                var signalWrapper = new LuauSignalWrapper(context, thread, instanceId, propNameHash);
                unityEvent.AddListener(signalWrapper.HandleEvent_4);
                signalWrapper.RequestDisconnect += () => {
                    unityEvent.RemoveListener(signalWrapper.HandleEvent_4);
                };

                AddSignalDestroyWatcher(go, context, (contextReset) => {
                    if (!contextReset) {
                        LuauPlugin.LuauDestroySignals(context, thread, instanceId);
                        LuauPlugin.LuauUnpinThread(thread);
                    }
                    unityEvent.RemoveListener(signalWrapper.HandleEvent_4);
                });
            }
            return 1;
        }
    }
}
