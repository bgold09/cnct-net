using System;

namespace Cnct.Core.Configuration
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CnctActionTypeAttribute : Attribute
    {
        public string ActionType { get; set; }

        public CnctActionTypeAttribute(string actionType)
        {
            this.ActionType = actionType;
        }
    }
}
