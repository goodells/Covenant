using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Covenant {
    class Deferred {
        public Promise promise = new Promise();

        private bool AbsorbPromise(Promise promise) {
            promise.Then(delegate(object argument) {
                Fulfill(argument);

                return null;
            }, delegate(object argument) {
                Reject(argument);

                return null;
            });

            return true;
        }

        private bool AbsorbPromise(object argument) {
            try {
                if (argument.GetType() == typeof(Promise)) {
                    return AbsorbPromise(argument as Promise);
                }
            } catch (NullReferenceException exception) {

            }

            return false;
        }

        public void Fulfill(object argument) {
            if (AbsorbPromise(argument)) {
                return;
            }

            promise.Fulfill(argument);
        }

        public void Reject(object argument) {
            if (AbsorbPromise(argument)) {
                return;
            }
        }
    }
}
