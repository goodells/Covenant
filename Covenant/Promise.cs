using System;

namespace Covenant {
    class Promise : EventEmitter {
        // Delegate casting
        protected Func<T, object> CastDelegate<T>(Func<object, object> handler) {
            return delegate(T argument) {
                return handler((object)handler);
            };
        }

        protected Func<object, object> CastDelegate(Func<object, object> handler) {
            return handler;
        }

        protected Func<T, object> CastDelegate<T>(Func<object> handler) {
            return delegate(T argument) {
                return handler();
            };
        }

        protected Func<object, object> CastDelegate(Func<object> handler) {
            return CastDelegate<object>(handler);
        }

        protected Func<T, object> CastDelegate<T>(Action<T> handler) {
            return delegate(T argument) {
                handler(argument);

                return null;
            };
        }

        protected Func<object, object> CastDelegate(Action<object> handler) {
            return CastDelegate<object>(handler);
        }

        protected Func<T, object> CastDelegate<T>(Action handler) {
            return delegate(T argument) {
                handler();

                return null;
            };
        }

        protected Func<object, object> CastDelegate(Action handler) {
            return CastDelegate<object>(handler);
        }

        // State
        public enum State {
            Pending,
            Fulfilled,
            Rejected
        }

        public State state = State.Pending;
        
        // Values
        private object valueFulfillment = null;
        private object valueRejection = null;

        public override EventEmitter Bind(string action, Func<object, HandlerReaction> handler) {
            switch (action) {
                case "fulfillment":
                    if (state == State.Fulfilled) {
                        handler(valueFulfillment);
                    }

                    break;
                case "rejection":
                    if (state == State.Rejected) {
                        handler(valueRejection);
                    }

                    break;
            }

            return base.Bind(action, handler);
        }

        public override EventEmitter Trigger(string action, object argument) {
            if ((action == "fulfillment" || action == "rejection") && state != State.Pending) {
                return this;
            }

            return base.Trigger(action, argument);
        }

        internal Promise Fulfill(object argument) {
            return Trigger("fulfillment", argument) as Promise;
        }

        internal Promise Reject(object argument) {
            return Trigger("rejection", argument) as Promise;
        }

        private void ThenFulfill<T>(Deferred deferred, Func<T, object> handler) {
            Bind<T>("fulfillment", delegate(T argument) {
                var result = handler(argument);

                deferred.Fulfill(result);
            });
        }

        private void ThenReject<T>(Deferred deferred, Func<T, object> handler) {
            Bind<T>("rejection", delegate(T argument) {
                var result = handler(argument);
                
                deferred.Reject(result);
            });
        }

        // Func<T, object> : Func<T, object> -> Action
        public Promise Then<TFulfillment, TRejection>(Func<TFulfillment, object> handlerFulfillment, Func<TRejection, object> handlerRejection) {
            var deferred = new Deferred();

            ThenFulfill(deferred, handlerFulfillment);
            ThenReject(deferred, handlerRejection);

            return deferred.promise;
        }

        public Promise Then<TFulfillment>(Func<TFulfillment, object> handlerFulfillment, Func<object> handlerRejection) {
            return Then(handlerFulfillment, CastDelegate(handlerRejection));
        }

        public Promise Then<TFulfillment, TRejection>(Func<TFulfillment, object> handlerFulfillment, Action<TRejection> handlerRejection) {
            return Then(handlerFulfillment, CastDelegate(handlerRejection));
        }

        public Promise Then<TFulfillment>(Func<TFulfillment, object> handlerFulfillment, Action handlerRejection) {
            return Then(handlerFulfillment, CastDelegate(handlerRejection));
        }

        // Func<object> : Func<T, object> -> Action
        public Promise Then<TRejection>(Func<object> handlerFulfillment, Func<TRejection, object> handlerRejection) {
            return Then(CastDelegate(handlerFulfillment), handlerRejection);
        }

        public Promise Then(Func<object> handlerFulfillment, Func<object> handlerRejection) {
            return Then(CastDelegate(handlerFulfillment), CastDelegate(handlerRejection));
        }

        public Promise Then<TRejection>(Func<object> handlerFulfillment, Action<TRejection> handlerRejection) {
            return Then(CastDelegate(handlerFulfillment), CastDelegate(handlerRejection));
        }

        public Promise Then(Func<object> handlerFulfillment, Action handlerRejection) {
            return Then(CastDelegate(handlerFulfillment), CastDelegate(handlerRejection));
        }

        // Action<T> : Func<T, object> -> Action
        public Promise Then<TFulfillment, TRejection>(Action<TFulfillment> handlerFulfillment, Func<TRejection, object> handlerRejection) {
            return Then(CastDelegate(handlerFulfillment), handlerRejection);
        }

        public Promise Then<TFulfillment>(Action<TFulfillment> handlerFulfillment, Func<object> handlerRejection) {
            return Then(CastDelegate(handlerFulfillment), CastDelegate(handlerRejection));
        }

        public Promise Then<TFulfillment, TRejection>(Action<TFulfillment> handlerFulfillment, Action<TRejection> handlerRejection) {
            return Then(CastDelegate(handlerFulfillment), CastDelegate(handlerRejection));
        }

        public Promise Then<TFulfillment>(Action<TFulfillment> handlerFulfillment, Action handlerRejection) {
            return Then(CastDelegate(handlerFulfillment), CastDelegate(handlerRejection));
        }

        // Action : Func<T, object> -> Action
        public Promise Then<TRejection>(Action handlerFulfillment, Func<TRejection, object> handlerRejection) {
            return Then(CastDelegate(handlerFulfillment), handlerRejection);
        }

        public Promise Then<TRejection>(Action handlerFulfillment, Func<object> handlerRejection) {
            return Then(CastDelegate(handlerFulfillment), CastDelegate(handlerRejection));
        }

        public Promise Then<TRejection>(Action handlerFulfillment, Action<TRejection> handlerRejection) {
            return Then(CastDelegate(handlerFulfillment), CastDelegate(handlerRejection));
        }

        public Promise Then<TRejection>(Action handlerFulfillment, Action handlerRejection) {
            return Then(CastDelegate(handlerFulfillment), CastDelegate(handlerRejection));
        }

        // Func<T, object> -> Action
        public Promise Then<TFulfillment>(Func<TFulfillment, object> handlerFulfillment) {
            var deferred = new Deferred();

            ThenFulfill(deferred, handlerFulfillment);

            return deferred.promise;
        }

        public Promise Then(Func<object> handlerFulfillment) {
            return Then(CastDelegate(handlerFulfillment));
        }

        public Promise Then<TFulfillment>(Action<TFulfillment> handlerFulfillment) {
            return Then(CastDelegate(handlerFulfillment));
        }

        public Promise Then(Action handlerFulfillment) {
            return Then(CastDelegate(handlerFulfillment));
        }
    }
}
