using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Collection of abstract classes forming the basis of clean eco-system on .NET Core 3.0 
/// </summary>
/// <remarks>
/// Enterprise Members of products' computing ecosystem are developed from this clean, reusable, cross-platform core.
/// Enterprise Members may include:
/// - An Engineering GUI
/// -- for developer's use
/// - A Technicians' Assistant
/// -- for use by service staff
/// - Production Line GUI(s)
/// -- for various use by production crew
/// - Quality/Regulatory Database tools
/// - Applications for customers and end-users
/// </remarks>
namespace Clean_BaseLib
{
    /// <summary>
    /// BaseClass_CommandResponse abstract class is a common shared element used by Modules and the Execution System to Transact Packets.
    /// </summary>
    /// <remarks>
    /// Within the clean architecture, this element crosses boundaries and is know by all layers.
    /// </remarks>
    public abstract class BaseClass_CommandResponse
    {
        #region Constructors of the Command Response Class
        /// <summary>
        /// Constructor from Packets
        /// </summary>
        /// <param name="cmdPack">Command Packet</param>
        /// <param name="rspPack">Response Packet</param>
        public BaseClass_CommandResponse(BaseClass_Packet cmdPack, BaseClass_Packet rspPack)
        {
            cmdPacket = cmdPack;
            rspPacket = rspPack;
        }
        #endregion

        #region Properties of the Command Response Class
        /// <summary>
        /// Property exposing the command packet of this command response object
        /// </summary>
        public virtual BaseClass_Packet CommandPacket { set; get; }
        /// <summary>
        /// Property exposing the response packet of this command response object
        /// </summary>
        public virtual BaseClass_Packet ResponsePacket { set; get; }
        /// <summary>
        /// Property indicating if the command packet has been packet, if its ready to be sent
        /// </summary>
        public virtual bool CMDisPacked { set; get; } = false;
        /// <summary>
        /// Property indicating if the command packet has been sent
        /// </summary>
        public virtual bool CMDisSent { set; get; } = false;
        /// <summary>
        /// Property indicating if the response packet has been recieved, if its ready to parse
        /// </summary>
        public virtual bool RSPisReceived { set; get; } = false;
        /// <summary>
        /// Property indicating if the response packet has been parsed into serial values, if it has been deserialized
        /// </summary>
        public virtual bool RSPisParsed { set; get; } = false;
        #endregion

        #region Data Members of the Command Response Class
        /// <summary>
        /// Internal protected packet objects, command and response, inheriting classes can access these. 
        /// Inheriting classes can override the public properties exposing these objects.
        /// </summary>
        protected BaseClass_Packet cmdPacket, rspPacket;
        #endregion
    }
    /// <summary>
    /// BaseClass_Packet is the abstract packet class implementing the ISerialValueCollection interface.
    /// </summary>
    /// <remarks>
    /// Packets are known throughout the computing eco system and pass through the clean boundaries of dependency via serialization.
    /// </remarks>
    public abstract class BaseClass_Packet
    {
        public Int32 ModuleID { get; protected set; } = 0;
        public BaseClass_Packet()
        {
        }
        public BaseClass_Packet(Int32 modID)
        {
            ModuleID = modID;
        }
        public BaseClass_Packet(byte[] packBytes)
        {
            JsonSerializer.Deserialize<BaseClass_Packet>(packBytes);
        }

        public byte[] toJSON_array()
        {
            return toJSON_bytes();
        }
        protected virtual byte[] toJSON_bytes()
        {
            return JsonSerializer.SerializeToUtf8Bytes<BaseClass_Packet>(this);
        }
    }
   
    public class ExceptionPacket:BaseClass_Packet
    {
        public ExeSysException PackedException { get; protected set; }
        protected override byte[] toJSON_bytes()
        {
            return JsonSerializer.SerializeToUtf8Bytes<ExceptionPacket>(this);
        }
        protected ExceptionPacket(byte[] packBytes)
        {
            JsonSerializer.Deserialize<ExceptionPacket>(packBytes);
        }
        public ExceptionPacket(ExeSysException pckdExcpt):base(0)
        {
            PackedException = pckdExcpt;
        }
    }
}
