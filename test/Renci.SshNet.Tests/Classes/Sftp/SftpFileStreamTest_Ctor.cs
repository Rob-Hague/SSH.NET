using System;
using System.IO;
using System.Linq;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest_Ctor
    {
        [TestMethod]
        public void BadFileMode_ThrowsArgumentOutOfRangeException()
        {
            var ex = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
                new SftpFileStream(new Mock<ISftpSession>().Object, "file.txt", mode: 0, FileAccess.Read, bufferSize: 1024));

            Assert.AreEqual("mode", ex.ParamName);
        }

        [TestMethod]
        public void BadFileAccess_ThrowsArgumentOutOfRangeException()
        {
            var ex = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
                new SftpFileStream(new Mock<ISftpSession>().Object, "file.txt", FileMode.Open, access: 0, bufferSize: 1024));

            Assert.AreEqual("access", ex.ParamName);
        }

        [TestMethod]
        [DataRow(FileMode.Append, FileAccess.Read)]
        [DataRow(FileMode.Append, FileAccess.ReadWrite)]
        [DataRow(FileMode.Create, FileAccess.Read)]
        [DataRow(FileMode.CreateNew, FileAccess.Read)]
        [DataRow(FileMode.Truncate, FileAccess.Read)]
        public void InvalidModeAccessCombination_ThrowsArgumentException(FileMode mode, FileAccess access)
        {
            var ex = Assert.ThrowsExactly<ArgumentException>(() =>
                new SftpFileStream(new Mock<ISftpSession>().Object, "file.txt", mode, access, bufferSize: 1024));

            Assert.AreEqual("mode", ex.ParamName);
        }

        [TestMethod]
        public void ReadWithWriteAccess_ThrowsNotSupportedException()
        {
            var sessionMock = new Mock<ISftpSession>();

            sessionMock.Setup(s => s.IsOpen).Returns(true);

            var s = new SftpFileStream(sessionMock.Object, "file.txt", FileMode.Create, FileAccess.Write, bufferSize: 1024);

            Assert.IsFalse(s.CanRead);

            Assert.Throws<NotSupportedException>(() => _ = s.Read(new byte[4], 0, 4));
            Assert.Throws<NotSupportedException>(() => _ = s.ReadByte());
        }

        [TestMethod]
        public void WriteWithReadAccess_ThrowsNotSupportedException()
        {
            var sessionMock = new Mock<ISftpSession>();

            sessionMock.Setup(s => s.IsOpen).Returns(true);

            var s = new SftpFileStream(sessionMock.Object, "file.txt", FileMode.Open, FileAccess.Read, bufferSize: 1024);

            Assert.IsFalse(s.CanWrite);

            Assert.Throws<NotSupportedException>(() => s.Write(new byte[4], 0, 4));
            Assert.Throws<NotSupportedException>(() => s.WriteByte(0xf));
        }

        [Ignore("Currently throws EndOfStreamException in all cases.")]
        [TestMethod]
        [DataRow(-1, SeekOrigin.Begin)]
        [DataRow(-1, SeekOrigin.Current)]
        [DataRow(-1000, SeekOrigin.End)]
        public void SeekBeforeBeginning_ThrowsIOException(long offset, SeekOrigin origin)
        {
            var sessionMock = new Mock<ISftpSession>();

            sessionMock.Setup(s => s.IsOpen).Returns(true);

            SetupRemoteSize(sessionMock, 128);

            var s = new SftpFileStream(sessionMock.Object, "file.txt", FileMode.Open, FileAccess.Read, bufferSize: 1024);

            Assert.Throws<IOException>(() => s.Seek(offset, origin));
        }

        [TestMethod]
        [DataRow(FileMode.CreateNew, (int)(Flags.Write | Flags.CreateNew))]
        [DataRow(FileMode.Create, (int)(Flags.Write /* TODO | Flags.CreateNewOrOpen */ | Flags.Truncate))]
        [DataRow(FileMode.Open, (int)(Flags.Write))]
        [DataRow(FileMode.OpenOrCreate, (int)(Flags.Write | Flags.CreateNewOrOpen))]
        //[DataRow(FileMode.Truncate, (int)(Flags.Write))] // TODO needs to set size in attrs?
        [DataRow(FileMode.Append, (int)(Flags.Write | Flags.Append | Flags.CreateNewOrOpen))]
        public void RequestOpen_CorrectFlags(FileMode mode, int expectedFlags)
        {
            var sessionMock = new Mock<ISftpSession>();

            sessionMock.Setup(s => s.IsOpen).Returns(true);

            SetupRemoteSize(sessionMock, 128);

            var s = new SftpFileStream(sessionMock.Object, "file.txt", mode, FileAccess.Write, bufferSize: 1024);

            sessionMock.Verify(s => s.RequestOpen("file.txt", (Flags)expectedFlags, It.IsAny<bool>()));

            Assert.AreEqual(128, s.Length);

            if (mode == FileMode.Append)
            {
                Assert.AreEqual(128, s.Position);
            }
            else
            {
                Assert.AreEqual(0, s.Position);
            }
        }

        private static void SetupRemoteSize(Mock<ISftpSession> sessionMock, long size)
        {
            sessionMock.Setup(s => s.RequestFStat(It.IsAny<byte[]>(), It.IsAny<bool>())).Returns(new SftpFileAttributes(
                default, default, size: size, default, default, default, default
                ));
        }

        [TestMethod]
        public void SeekAndWrite()
        {
            var sessionMock = new Mock<ISftpSession>();

            sessionMock.Setup(s => s.CalculateOptimalReadLength(It.IsAny<uint>())).Returns<uint>(x => x);
            sessionMock.Setup(s => s.CalculateOptimalWriteLength(It.IsAny<uint>(), It.IsAny<byte[]>())).Returns<uint, byte[]>((x, _) => x);
            sessionMock.Setup(s => s.IsOpen).Returns(true);

            const int InitialSize = 128;
            SetupRemoteSize(sessionMock, InitialSize);

            var s = new SftpFileStream(sessionMock.Object, "file.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, bufferSize: 1024);

            Assert.IsTrue(s.CanRead);
            Assert.IsTrue(s.CanSeek);
            Assert.IsTrue(s.CanWrite);
            Assert.IsTrue(s.CanTimeout);
            Assert.AreEqual(InitialSize, s.Length);
            Assert.AreEqual(0, s.Position);

            // Seek past the end of the file
            Assert.AreEqual(InitialSize + 20, s.Seek(20, SeekOrigin.End));
            Assert.AreEqual(InitialSize + 20, s.Position);

            byte[] newData = "Some new bytes"u8.ToArray();
            s.Write(newData, 0, newData.Length);
            s.Flush();

            VerifyRequestWrite(sessionMock, newData, serverOffset: InitialSize + 20);

            int newSize = InitialSize + 20 + newData.Length;

            SetupRemoteSize(sessionMock, newSize);

            Assert.AreEqual(newSize, s.Position);
            Assert.AreEqual(newSize, s.Length);

            // Seek backwards to the middle of the file
            Assert.AreEqual(newSize - 60, s.Seek(-60, SeekOrigin.Current));
            Assert.AreEqual(newSize - 60, s.Position);

            newData = "Some more new bytes"u8.ToArray();
            s.Write(newData, 0, newData.Length);
            s.Flush();

            VerifyRequestWrite(sessionMock, newData, serverOffset: newSize - 60);
        }

        [TestMethod]
        public void SeekAndRead()
        {
            var sessionMock = new Mock<ISftpSession>();

            sessionMock.Setup(s => s.CalculateOptimalReadLength(It.IsAny<uint>())).Returns<uint>(x => x);
            sessionMock.Setup(s => s.CalculateOptimalWriteLength(It.IsAny<uint>(), It.IsAny<byte[]>())).Returns<uint, byte[]>((x, _) => x);
            sessionMock.Setup(s => s.IsOpen).Returns(true);

            const int InitialSize = 128;
            byte[] remoteData = Enumerable.Range(0, InitialSize).Select(x => (byte)x).ToArray();
            SetupRemoteSize(sessionMock, InitialSize);

            sessionMock
                .Setup(s => s.RequestRead(It.IsAny<byte[]>(), It.IsAny<ulong>(), It.IsAny<uint>()))
                .Returns<byte[], ulong, uint>((_, offset, length)
                    => remoteData.Take((int)offset, (int)Math.Min((ulong)remoteData.Length - offset, length)));

            var s = new SftpFileStream(sessionMock.Object, "file.txt", FileMode.Open, FileAccess.Read, bufferSize: 1024);

            Assert.IsTrue(s.CanRead);
            Assert.IsTrue(s.CanSeek);
            Assert.IsFalse(s.CanWrite);
            Assert.IsTrue(s.CanTimeout);
            Assert.AreEqual(InitialSize, s.Length);
            Assert.AreEqual(0, s.Position);

            // Seek near the beginning of the file
            Assert.AreEqual(32, s.Seek(32, SeekOrigin.Current));
            Assert.AreEqual(32, s.Position);

            var buffer = new byte[16];
            Assert.AreEqual(16, s.Read(buffer, 0, buffer.Length));

            CollectionAssert.AreEqual(remoteData.Take(32, 16), buffer);

            Array.Clear(buffer, 0, buffer.Length);

            // Seek near the end of the file
            Assert.AreEqual(InitialSize - 3, s.Seek(-3, SeekOrigin.End));
            Assert.AreEqual(InitialSize - 3, s.Position);

            Assert.AreEqual(3, s.Read(buffer, 8, 8));

            CollectionAssert.AreEqual(
                new byte[8].Concat(remoteData.Take(InitialSize - 3, 3).Concat(new byte[5])),
                buffer);

            Assert.AreEqual(InitialSize, s.Position);
            Assert.AreEqual(0, s.Read(buffer, 0, buffer.Length));
        }

        [TestMethod]
        public void Dispose()
        {
            var sessionMock = new Mock<ISftpSession>();

            sessionMock.Setup(s => s.IsOpen).Returns(true);

            var s = new SftpFileStream(sessionMock.Object, "file.txt", FileMode.Create, FileAccess.ReadWrite, bufferSize: 1024);

            Assert.IsTrue(s.CanRead);
            Assert.IsTrue(s.CanSeek);
            Assert.IsTrue(s.CanWrite);

            s.Dispose();
            sessionMock.Verify(p => p.RequestClose(It.IsAny<byte[]>()), Times.Once);

            Assert.IsFalse(s.CanRead);
            Assert.IsFalse(s.CanSeek);
            Assert.IsFalse(s.CanWrite);

            // Test no-op second dispose
            s.Dispose();
            sessionMock.Verify(p => p.RequestClose(It.IsAny<byte[]>()), Times.Once);
        }

        private static void VerifyRequestWrite(Mock<ISftpSession> sessionMock, ReadOnlyMemory<byte> newData, int serverOffset)
        {
            sessionMock.Verify(s => s.RequestWrite(
                /* handle: */         It.IsAny<byte[]>(),
                /* serverOffset: */   (ulong)serverOffset,
                /* data: */           It.Is<byte[]>(x => IndexOf(x, newData) >= 0),
                /* offset: */         It.IsAny<int>(),
                /* length: */         newData.Length,
                /* wait: */           It.IsAny<AutoResetEvent>(),
                /* writeCompleted: */ It.IsAny<Action<SftpStatusResponse>>()),
                Times.Once);
        }

        private static int IndexOf(byte[] searchSpace, ReadOnlyMemory<byte> searchValue)
        {
            // Needed in a (non-local) function because expression lambdas can't contain spans
            return searchSpace.AsSpan().IndexOf(searchValue.Span);
        }

    }
}
