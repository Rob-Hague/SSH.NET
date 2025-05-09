using System;
using System.Buffers;

namespace Renci.SshNet.Sftp.Responses
{
    internal sealed class SftpDataResponse : SftpResponse
    {
        public override SftpMessageTypes SftpMessageType
        {
            get { return SftpMessageTypes.Data; }
        }

        public ReadOnlySequence<byte> Data { get; set; }

        public SftpDataResponse(uint protocolVersion)
            : base(protocolVersion)
        {
        }

        protected override void LoadData()
        {
            throw new NotImplementedException();
        }

        protected override void SaveData()
        {
            throw new NotImplementedException();
        }
    }
}
