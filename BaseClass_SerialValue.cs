using System;
using System.Runtime.Serialization;

namespace Clean_BaseLib
{
    /// <summary>
    /// BaseClass_SerialValue is the abstract class for values moved in packets
    /// </summary>
    /// <remarks>
    /// This class can contain data of arbitrary type but it must be serializable
    /// </remarks>
    [Serializable]
    public class BaseClass_SerialValue : ISerializable
    {
        #region Serialization of the Value Class
        /// <summary>
        /// for serialization...
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
