using System;
using System.Collections.Generic;
using System.Text;

namespace Clean_BaseLib
{
    public abstract class BaseClass_AppProgInterface
    {
        public abstract BaseClass_PacketPort InterfacePort { get; protected set; }
    }
}
