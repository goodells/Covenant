using System;
using System.Collections.Generic;
using System.Threading;

namespace Covenant {
    class EventEmitter {
        public enum HandlerReaction {
            Remove,
            Persist
        }

        private Dictionary<string, List<Func<object, HandlerReaction>>> handlersActions = new Dictionary<string, List<Func<object, HandlerReaction>>>();

        public virtual EventEmitter Bind(string action, Func<object, HandlerReaction> handler) {
            List<Func<object, HandlerReaction>> handlers;

            if (!handlersActions.TryGetValue(action, out handlers)) {
                handlers = handlersActions[action] = new List<Func<object, HandlerReaction>>();
            }

            handlers.Add(handler);

            return this;
        }

        public EventEmitter Bind(string action, Func<object, object> handler) {
            return Bind(action, delegate(object argument) {
                handler(argument);

                return HandlerReaction.Remove;
            });
        }

        public EventEmitter Bind(string action, Func<object> handler) {
            return Bind(action, delegate(object argument) {
                return handler();
            });
        }

        public EventEmitter Bind(string action, Action<object> handler) {
            return Bind(action, delegate(object argument) {
                handler(argument);

                return null;
            });
        }

        public EventEmitter Bind(string action, Action handler) {
            return Bind(action, delegate(object argument) {
                handler();

                return null;
            });
        }

        public EventEmitter Bind<T>(string action, Func<T, object> handler) {
            return Bind(action, delegate(object argument) {
                return handler((T)argument);
            });
        }

        public EventEmitter Bind<T>(string action, Action<T> handler) {
            return Bind<T>(action, delegate(T argument) {
                handler(argument);

                return null;
            });
        }

        public virtual EventEmitter Trigger(string action, object argument) {
            var thread = new Thread(new ThreadStart(delegate() {
                Thread.Sleep(100);

                List<Func<object, HandlerReaction>> handlers;

                lock (handlersActions) {
                    if (!handlersActions.TryGetValue(action, out handlers) || handlers.Count == 0) {
                        return;
                    }

                    foreach (var handler in new List<Func<object, HandlerReaction>>(handlers)) {
                        var reaction = handler(argument);

                        switch (reaction) {
                            case HandlerReaction.Remove:
                                handlers.Remove(handler);

                                break;
                            case HandlerReaction.Persist:
                                // Do nothing

                                break;
                        }
                    }

                    if (handlers.Count == 0) {
                        handlersActions.Remove(action);
                    }
                }
            }));
            
            thread.Start();
            
            return this;
        }

        public EventEmitter Trigger(string action) {
            return Trigger(action, null);
        }
    }
}
