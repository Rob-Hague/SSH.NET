using System;
using System.IO;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

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
        public void Append_WritesToEndOfFile()
        {
            var sessionMock = new Mock<ISftpSession>();

            sessionMock.Setup(s => s.CalculateOptimalReadLength(It.IsAny<uint>())).Returns<uint>(x => x);
            sessionMock.Setup(s => s.CalculateOptimalWriteLength(It.IsAny<uint>(), It.IsAny<byte[]>())).Returns<uint, byte[]>((x, _) => x);
            sessionMock.Setup(s => s.IsOpen).Returns(true);

            const long InitialSize = 128;
            SetupRemoteSize(InitialSize);

            void SetupRemoteSize(long size)
            {
                sessionMock.Setup(s => s.RequestFStat(It.IsAny<byte[]>(), It.IsAny<bool>())).Returns(new SftpFileAttributes(
                    default, default, size: size, default, default, default, default
                    ));
            }

            var s = new SftpFileStream(sessionMock.Object, "file.txt", FileMode.Append, FileAccess.Write, bufferSize: 1024);

            Assert.IsFalse(s.CanRead);
            Assert.IsTrue(s.CanSeek);
            Assert.IsTrue(s.CanWrite);
            Assert.IsTrue(s.CanTimeout);
            Assert.AreEqual(InitialSize, s.Length);
            Assert.AreEqual(InitialSize, s.Position);

            byte[] newData = "Some new bytes"u8.ToArray();
            s.Write(newData, 0, newData.Length);
            s.Flush();

            sessionMock.Verify(s => s.RequestWrite(
                /* handle: */         It.IsAny<byte[]>(),
                /* serverOffset: */   InitialSize,
                /* data: */           It.IsAny<byte[]>(),
                /* offset: */         It.IsAny<int>(),
                /* length: */         newData.Length,
                /* wait: */           It.IsAny<AutoResetEvent>(),
                /* writeCompleted: */ It.IsAny<Action<SftpStatusResponse>>()),
                Times.Once);

            long newSize = InitialSize + newData.Length;

            SetupRemoteSize(newSize);

            Assert.AreEqual(newSize, s.Position);
            Assert.AreEqual(newSize, s.Length);
        }

        [TestMethod]
        public void AppendWrite()
        {
            var session = new SftpFileReaderTestSession("These are the bytes of the remote file"u8.ToArray());

            var s = new SftpFileStream(session, "file.txt", FileMode.Append, FileAccess.Write, bufferSize: 1024);

            Assert.IsFalse(s.CanRead);
            Assert.IsTrue(s.CanSeek);
            Assert.IsTrue(s.CanWrite);
            Assert.IsTrue(s.CanTimeout);
            Assert.AreEqual(session.RemoteFile.Length, s.Position);

            Assert.Throws<NotSupportedException>(() => _ = s.Read(new byte[4], 0, 4));
            Assert.Throws<NotSupportedException>(() => _ = s.ReadByte());

            byte[] newData = "Some extra bytes"u8.ToArray();
            s.Write(newData, 0, newData.Length);
            s.Flush();

            CollectionAssert.AreEqual(
                "These are the bytes of the remote fileSome extra bytes"u8.ToArray(),
                session.RemoteFile);

            Assert.AreEqual(session.RemoteFile.Length, s.Position);
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

            var s = new SftpFileStream(sessionMock.Object, "file.txt", mode, FileAccess.Write, bufferSize: 1024);

            sessionMock.Verify(s => s.RequestOpen("file.txt", (Flags)expectedFlags, It.IsAny<bool>()));
        }

        [TestMethod]
        public void Create_Truncates()
        {
            var sessionMock = new Mock<ISftpSession>();

            sessionMock.Setup(s => s.CalculateOptimalReadLength(It.IsAny<uint>())).Returns<uint>(x => x);
            sessionMock.Setup(s => s.CalculateOptimalWriteLength(It.IsAny<uint>(), It.IsAny<byte[]>())).Returns<uint, byte[]>((x, _) => x);
            sessionMock.Setup(s => s.IsOpen).Returns(true);

            var s = new SftpFileStream(sessionMock.Object, "file.txt", FileMode.Create, FileAccess.ReadWrite, bufferSize: 1024);

            //sessionMock.Setup(s => s.RequestFStat(It.IsAny<byte[]>(), It.IsAny<bool>())).Returns(new SftpFileAttributes())

            Assert.IsTrue(s.CanRead);
            Assert.IsTrue(s.CanSeek);
            Assert.IsTrue(s.CanWrite);
            Assert.IsTrue(s.CanTimeout);
            //Assert.AreEqual(0, s.Length);

            Assert.AreEqual(0, s.Read(new byte[4], 0, 4));

            sessionMock.Verify(s => s.RequestRead(It.IsAny<byte[]>(), 0, It.Is<uint>(x => x >= 4)));

            byte[] newData = "Some new bytes"u8.ToArray();
            s.Write(newData, 0, newData.Length);
            s.Flush();

            sessionMock.Verify(s => s.RequestWrite(
                /* handle: */         It.IsAny<byte[]>(),
                /* serverOffset: */   0,
                /* data: */           It.IsAny<byte[]>(),
                /* offset: */         It.IsAny<int>(),
                /* length: */         newData.Length,
                /* wait: */           It.IsAny<AutoResetEvent>(),
                /* writeCompleted: */ It.IsAny<Action<SftpStatusResponse>>()));

            //Assert.AreEqual(newData.Length, s.Position);
            //Assert.AreEqual(newData.Length, s.Length);

            s.Write(newData, 5, 3);
            s.Flush();

            sessionMock.Verify(s => s.RequestWrite(
                /* handle: */         It.IsAny<byte[]>(),
                /* serverOffset: */   (ulong)newData.Length,
                /* data: */           It.IsAny<byte[]>(),
                /* offset: */         It.IsAny<int>(),
                /* length: */         3,
                /* wait: */           It.IsAny<AutoResetEvent>(),
                /* writeCompleted: */ It.IsAny<Action<SftpStatusResponse>>()));
        }

    }
}
