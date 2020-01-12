using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

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
    public abstract class BaseClass_PacketPort
    {
        public List<BaseClass_Packet> PacketstoSend { get; protected set; } = new List<BaseClass_Packet>();
        public List<int> PacketBytesSent { get; protected set; } = new List<int>();
        public List<BaseClass_Packet> PacketsReadfromPort { get; protected set; } = new List<BaseClass_Packet>();

        public abstract void GetPacketsfromPort();
        public virtual async Task GetPacketsfromPortAsync()
        {
            await Task.Run(() =>
            {
                GetPacketsfromPort();
            });
        }
        public abstract void SendPacketstoPort();
        public virtual async Task SendPacketstoPortAsync()
        {
            await Task.Run(() => 
            {
                SendPacketstoPort();
            });
        }
    }
    public class ConsolePort:BaseClass_PacketPort
    {
       
        public override void GetPacketsfromPort()
        {

        }
       
        public override void SendPacketstoPort()
        {

        }
        
    }
    public class FileStreamPort : BaseClass_PacketPort
    {

        public override void GetPacketsfromPort()
        {

        }

        public override void SendPacketstoPort()
        {

        }

    }
    public class NetworkStreamPort : BaseClass_PacketPort
    {

        public override void GetPacketsfromPort()
        {

        }

        public override void SendPacketstoPort()
        {

        }

    }
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
        public static readonly byte jsonOpenDelim = 0x7b;
        public static readonly byte jsonCloseDelim = 0x7d;

        public abstract int PacketKey { get; }
        public int ModuleID { get; set; } = 0;
        protected abstract bool MatchesThisPacket(BaseClass_Packet p2Match);

        public bool MatchesPacket(BaseClass_Packet p2Match)
        {
            if (p2Match.PacketKey != PacketKey)
                return false;

            return MatchesThisPacket(p2Match);
        }

        public byte[] ToByteArray(SerializationType SerializationTypeSelect)
        {
            switch(SerializationTypeSelect)
            {
                case SerializationType.text_JSON: return toJSON_bytes();
                default: throw new NotImplementedException("Serialization Types other than JSON are not yet Supported!");
            }            
        }
        public void ToByteStream(SerializationType SerializationTypeSelect, Stream StreamToWrite)
        {
            StreamToWrite.Write(ToByteArray(SerializationTypeSelect));
        }
        public void ToByteStreamAndLog(SerializationType SerializationTypeSelect, Stream StreamToWrite, List<List<byte>> PacketLogList)
        {
            StreamToWrite.Write(ToByteArray(SerializationTypeSelect));
        }
        
        protected virtual byte[] toJSON_bytes()
        {
            return JsonSerializer.SerializeToUtf8Bytes<BaseClass_Packet>(this);
        }
        
        
        public static async Task<List<byte>> ReadToEndAsync(Stream BaseStream, int msLoopSleepTime)
        {
            int numBytes2Read = 10;
            byte[] BytesArray;
            List<byte> outBytes = new List<byte>(numBytes2Read);
            int numBytesRead = 0;


            if (!BaseStream.CanSeek || !BaseStream.CanRead)
                throw new Exception("ReadToEnd can only be performed on seekable, readable streams.");

            numBytes2Read = (int)(BaseStream.Length - BaseStream.Position);
            if (numBytes2Read > 0)
            {
                do
                {                    
                    numBytes2Read = (int)(BaseStream.Length - BaseStream.Position);                  

                    if (numBytes2Read > 0)
                    {
                        BytesArray = new byte[numBytes2Read];
                        numBytesRead = await BaseStream.ReadAsync(BytesArray, 0, BytesArray.Length);
                        if (numBytesRead > 0)
                        {
                            System.Threading.Thread.Sleep(msLoopSleepTime);
                            outBytes.AddRange(BytesArray);
                        }
                    }
                    else
                        numBytesRead = 0;
                } while (numBytesRead > 0);
                return outBytes;
            }

            return null;
        }
        public static async Task<List<BaseClass_Packet>> ReadPacketsAsync(SerializationType SerializationTypeSelect, Stream BaseStream, int msLoopSleepTime)
        {
            List<byte> bytesRead = await ReadToEndAsync(BaseStream, msLoopSleepTime);
            if ((bytesRead != null))
            {
                if(bytesRead.Count>0)
                {
                    return ParsePackets(SerializationTypeSelect, bytesRead);
                }
                return null;
            }
            else
                return null;
        }
   

        public static List<BaseClass_Packet> ParsePackets(SerializationType DeSerializationTypeSelect, List<byte> bytes2Parse)
        {
            BaseClass_Packet DeserializedPacket = null;
            List<BaseClass_Packet> outPacks = null;
            switch(DeSerializationTypeSelect)
            {
                case SerializationType.text_JSON:
                    {
                        int openIndex = 0;
                        int closeIndex = 0;
                        int openDepth = 0;
                        int byteLength = 0;
                        for(int i = 0; i<bytes2Parse.Count; i++)
                        {
                            if(bytes2Parse[i]== jsonOpenDelim)
                            {
                                openDepth++;
                                if(openDepth==1)
                                {
                                    openIndex = i;
                                }
                            }
                            else if(bytes2Parse[i] == jsonCloseDelim)
                            {
                                openDepth--;

                                if (openDepth == 0)
                                {
                                    closeIndex = i;
                                    byteLength = closeIndex - openIndex + 1;
                                    byte[] packetTokenBytes = bytes2Parse.GetRange(openIndex, byteLength).ToArray();
                                    if (byteLength > 2)
                                    {
                                        if (DeSerializeDefaultPacket(ref packetTokenBytes, DeSerializationTypeSelect, out DeserializedPacket))
                                        {
                                            if (outPacks == null)
                                                outPacks = new List<BaseClass_Packet>();
                                            outPacks.Add(DeserializedPacket);
                                        }
                                        else if (BaseClass_ModuleExecutionSystem.DeSerializeKnownPacket(ref packetTokenBytes, DeSerializationTypeSelect, ref DeserializedPacket))
                                        {
                                            if (outPacks == null)
                                                outPacks = new List<BaseClass_Packet>();
                                            outPacks.Add(DeserializedPacket);
                                        }
                                        else
                                        {
                                            if (outPacks == null)
                                                outPacks = new List<BaseClass_Packet>();
                                            outPacks.Add(new UnknownPacket_2 { UnknownBytes = packetTokenBytes });
                                        }
                                    }                                    
                                }
                            }                            
                        }                                            
                    }
                    break;
                default: throw new NotImplementedException("De-Serialization types other than text JSON are not yet supported.");
            }
            return outPacks;
        }

        public static bool DeSerializeDefaultPacket(ref byte[] bytesIn, SerializationType sT, out BaseClass_Packet outPack)
        {
            switch (sT)
            {
                case SerializationType.text_JSON:
                    {
                        outPack = JsonSerializer.Deserialize<BaseClass_Packet>(new ReadOnlySpan<byte>(bytesIn));
                        if (outPack == null)
                            return false;
                        switch(outPack.PacketKey)
                        {
                            case 0: outPack = JsonSerializer.Deserialize<ExceptionPacket_0>(new ReadOnlySpan<byte>(bytesIn));return true;
                            case 1: outPack = JsonSerializer.Deserialize<StatusPacket_1>(new ReadOnlySpan<byte>(bytesIn)); return true;
                        }
                    }
                    break;
                default: throw new NotImplementedException("De-serialization Types other than JSON are not yet Supported!");
            }
            return false;
        }        
    }



    public enum SerializationType
    {
        text_JSON = 0,
        bin_PACKED = 1,
        bin_DELIMITED
    }
    //public class PacketReader : BinaryReader
    //{
    //    public SerializationType ReaderSerializationType { get; protected set; } = SerializationType.text_JSON;
    //    public PacketReader(Stream baseStreamIn) : base(baseStreamIn) { }
    //    public async Task<List<byte>> ReadToEndAsync()
    //    {
    //        if (BaseStream.Length > BaseStream.Position)
    //        {
    //            byte[] BytesArray;
    //            int numBytesToRead = (int)(BaseStream.Length - BaseStream.Position);
    //            int numBytesRead = 0;
    //            List<byte> outBytes = new List<byte>(numBytesToRead);
    //            await Task.Run(() =>
    //            {
    //                do
    //                {
    //                    if (BaseStream.Length > BaseStream.Position)
    //                    {
    //                        numBytesToRead = (int)(BaseStream.Length - BaseStream.Position);
    //                        BytesArray = new byte[numBytesToRead];
    //                        numBytesRead = BaseStream.Read(BytesArray, 0, BytesArray.Length);
    //                        if (numBytesRead > 0)
    //                        {
    //                            System.Threading.Thread.Sleep(10);
    //                            outBytes.AddRange(BytesArray);
    //                        }
    //                    }
    //                    else
    //                        numBytesRead = 0;
    //                } while (numBytesRead > 0);
    //            });
    //            return outBytes;
    //        }
    //        return null;
    //    }
    //}
    //public class PacketWriter : BinaryWriter
    //{
    //    public SerializationType ReaderSerializationType { get; protected set; } = SerializationType.text_JSON;
    //    public PacketWriter(Stream streamIn) : base(streamIn) { }
    //}



    public class ExceptionPacket_0:BaseClass_Packet
    {
        public override int PacketKey { get; } = 0;
        protected override bool MatchesThisPacket(BaseClass_Packet p2Match)
        {
            return(((ExceptionPacket_0)p2Match).PackedException.Message==PackedException.Message);
        }
        public ExeSysException PackedException { get; set; }
        protected override byte[] toJSON_bytes()
        {
            return JsonSerializer.SerializeToUtf8Bytes<ExceptionPacket_0>(this);
        }
    }

    public class StatusPacket_1:BaseClass_Packet
    {
        public override int PacketKey { get; } = 1;
        protected override bool MatchesThisPacket(BaseClass_Packet p2Match)
        {
            return (((StatusPacket_1)p2Match).CleanBaseLibVersion == CleanBaseLibVersion);
        }
        protected override byte[] toJSON_bytes()
        {
            return JsonSerializer.SerializeToUtf8Bytes<StatusPacket_1>(this);
        }
        public Version CleanBaseLibVersion { get; set; }
    }
    
    public class UnknownPacket_2:BaseClass_Packet
    {
        public override int PacketKey { get; } = 2;
        protected override bool MatchesThisPacket(BaseClass_Packet p2Match)
        {
            return (((UnknownPacket_2)p2Match).UnknownBytes == UnknownBytes);
        }
        protected override byte[] toJSON_bytes()
        {
            return JsonSerializer.SerializeToUtf8Bytes<UnknownPacket_2>(this);
        }
        public byte[] UnknownBytes { get; set; }
    }
    public class ExitPacket_3 : BaseClass_Packet
    {
        public override int PacketKey { get; } = 3;
        protected override bool MatchesThisPacket(BaseClass_Packet p2Match)
        {
            return (((ExitPacket_3)p2Match).ShouldExitWillExit == ShouldExitWillExit);
        }
        protected override byte[] toJSON_bytes()
        {
            return JsonSerializer.SerializeToUtf8Bytes<ExitPacket_3>(this);
        }
        public bool ShouldExitWillExit { get; set; }
    }
}
