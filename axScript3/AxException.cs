using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace axScript3
{
    [Serializable]
    public class AxException : Exception
    {
        public List<String> CallStack;
        public AxInterpreter Caller;

        public AxException(AxInterpreter Caller, String Message) : base(Message)
        {
            this.Caller = Caller;
            CallStack = Caller.CallStack;
            //CallStack.Reverse();
        }

        protected AxException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info != null)
            {
                CallStack = (List<String>) info.GetValue("CallStack", typeof (List<String>));
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            if (info != null)
            {
                info.AddValue("CallStack", CallStack);
            }
        }
    }
}