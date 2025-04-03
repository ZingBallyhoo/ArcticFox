namespace ArcticFox.PolyType.Amf.Packet
{
    public class AmfPacketConverter(AmfOptions options, AmfConverter<AmfHeader> headerConverter, AmfConverter<AmfMessage> messageConverter) : AmfConverter<AmfPacket>
    {
        public override void Write(ref AmfEncoder encoder, AmfPacket? value)
        {
            encoder.PutUInt16((ushort)value!.m_version);
            
            encoder.PutUInt16(checked((ushort)value.m_headers.Count));
            foreach (var header in value.m_headers)
            {
                headerConverter.Write(ref encoder, header);
            }
            
            encoder.PutUInt16(checked((ushort)value.m_messages.Count));
            foreach (var header in value.m_messages)
            {
                messageConverter.Write(ref encoder, header);
            }
        }

        public override AmfPacket? Read(ref AmfDecoder decoder)
        {
            var packet = new AmfPacket
            {
                m_version = (AmfVersion)decoder.ReadUInt16()
            };
            
            ReadHeaders(ref decoder, packet);
            ReadMessages(ref decoder, packet);
            return packet;
        }
        
        private void ReadHeaders(ref AmfDecoder decoder, AmfPacket packet)
        {
            var headerCount = decoder.ReadUInt16();
            if (headerCount > options.m_maxHeaders)
            {
                throw new InvalidDataException($"number of packet headers over configured limit. {headerCount} > {options.m_maxHeaders}");
            }
            
            packet.m_headers.EnsureCapacity(headerCount);
            for (var i = 0; i < headerCount; i++)
            {
                packet.m_headers.Add(headerConverter.Read(ref decoder)!);
            }
        }
        
        private void ReadMessages(ref AmfDecoder decoder, AmfPacket packet)
        {
            var messageCount = decoder.ReadUInt16();
            if (messageCount > options.m_maxMessages)
            {
                throw new InvalidDataException($"number of packet messages over configured limit. {messageCount} > {options.m_maxMessages}");
            }
            
            packet.m_messages.EnsureCapacity(messageCount);
            for (var i = 0; i < messageCount; i++)
            {
                packet.m_messages.Add(messageConverter.Read(ref decoder)!);
            }
        }
    }
}