#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    internal class SftpFileReaderTestSession : ISftpSession
    {
        private byte[]? _handle;
        public byte[] RemoteFile { get; private set; }

        public SftpFileReaderTestSession(byte[] remoteFile)
        {
            RemoteFile = remoteFile;
            IsOpen = true;
        }

        public bool IsOpen { get; private set; }

        public uint CalculateOptimalWriteLength(uint bufferSize, byte[] handle)
        {
            return bufferSize;
        }

        public uint CalculateOptimalReadLength(uint bufferSize)
        {
            return bufferSize;
        }

        public WaitHandle[] CreateWaitHandleArray(WaitHandle waitHandle1, WaitHandle waitHandle2)
        {
            return [waitHandle1, waitHandle2];
        }

        public SftpFileAttributes RequestFStat(byte[] handle, bool nullOnError)
        {
            ValidateHandle(handle);

            return new SftpFileAttributes(default, default, size: RemoteFile.Length, default, default, default, default);
        }

        public byte[] RequestOpen(string path, Flags flags, bool nullOnError = false)
        {
            CheckOpen();
            Assert.IsNull(_handle);

            if ((flags & Flags.CreateNewOrOpen) == Flags.CreateNewOrOpen)
            {
                if ((flags & Flags.Truncate) == Flags.Truncate)
                {
                    RemoteFile = [];
                }
                //else if 
            }

            return _handle = Encoding.UTF8.GetBytes(path);
        }

        public byte[] RequestRead(byte[] handle, ulong offset, uint length)
        {
            ValidateHandle(handle);

            return RemoteFile.Take((int)offset, (int)length);
        }

        public void RequestWrite(byte[] handle, ulong serverOffset, byte[] data, int offset, int length, AutoResetEvent wait, Action<SftpStatusResponse>? writeCompleted = null)
        {
            ValidateHandle(handle);

            byte[] newRemoteFile = RemoteFile
                .Take(0, (int)serverOffset)
                .Concat(data.Take(offset, length));

            if (RemoteFile.Length > newRemoteFile.Length)
            {
                newRemoteFile = newRemoteFile.Concat(RemoteFile.Take(newRemoteFile.Length, RemoteFile.Length - newRemoteFile.Length));
            }

            RemoteFile = newRemoteFile;
        }

        private void ValidateHandle(byte[] handle)
        {
            CheckOpen();

            if (_handle is null)
            {
                throw new InvalidOperationException("File not opened");
            }

            CollectionAssert.AreEqual(_handle, handle);
        }

        private void CheckOpen()
        {
            if (!IsOpen)
            {
                throw new InvalidOperationException("Session not open");
            }
        }

        public int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout)
        {
            return WaitHandle.WaitAny(waitHandles, millisecondsTimeout);
        }

#pragma warning disable IDE0022 // Use block body for method
#pragma warning disable IDE0025 // Use block body for property
#pragma warning disable IDE0027 // Use block body for accessor
        public uint ProtocolVersion => throw new NotImplementedException();

        public string WorkingDirectory => throw new NotImplementedException();

        public int OperationTimeout
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public SftpCloseAsyncResult BeginClose(byte[] handle, AsyncCallback callback, object state) => throw new NotImplementedException();

        public SFtpStatAsyncResult BeginLStat(string path, AsyncCallback callback, object state) => throw new NotImplementedException();

        public SftpOpenAsyncResult BeginOpen(string path, Flags flags, AsyncCallback callback, object state) => throw new NotImplementedException();

        public SftpReadAsyncResult BeginRead(byte[] handle, ulong offset, uint length, AsyncCallback callback, object state) => throw new NotImplementedException();

        public SftpRealPathAsyncResult BeginRealPath(string path, AsyncCallback callback, object state) => throw new NotImplementedException();

        public SFtpStatAsyncResult BeginStat(string path, AsyncCallback callback, object state) => throw new NotImplementedException();

        public void ChangeDirectory(string path) => throw new NotImplementedException();

        public Task ChangeDirectoryAsync(string path, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void Connect() => throw new NotImplementedException();

        public ISftpFileReader CreateFileReader(byte[] handle, ISftpSession sftpSession, uint chunkSize, int maxPendingReads, long? fileSize) => throw new NotImplementedException();

        public void Disconnect() => throw new NotImplementedException();

        public void Dispose() => throw new NotImplementedException();

        public void EndClose(SftpCloseAsyncResult asyncResult) => throw new NotImplementedException();

        public SftpFileAttributes EndLStat(SFtpStatAsyncResult asyncResult) => throw new NotImplementedException();

        public byte[] EndOpen(SftpOpenAsyncResult asyncResult) => throw new NotImplementedException();

        public byte[] EndRead(SftpReadAsyncResult asyncResult) => throw new NotImplementedException();

        public string EndRealPath(SftpRealPathAsyncResult asyncResult) => throw new NotImplementedException();

        public SftpFileAttributes EndStat(SFtpStatAsyncResult asyncResult) => throw new NotImplementedException();

        public string GetCanonicalPath(string path) => throw new NotImplementedException();

        public Task<string> GetCanonicalPathAsync(string path, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void RequestClose(byte[] handle) => throw new NotImplementedException();

        public Task RequestCloseAsync(byte[] handle, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void RequestFSetStat(byte[] handle, SftpFileAttributes attributes) => throw new NotImplementedException();

        public Task<SftpFileAttributes> RequestFStatAsync(byte[] handle, CancellationToken cancellationToken) => throw new NotImplementedException();

        public SftpFileAttributes RequestLStat(string path) => throw new NotImplementedException();

        public Task<SftpFileAttributes> RequestLStatAsync(string path, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void RequestMkDir(string path) => throw new NotImplementedException();

        public Task RequestMkDirAsync(string path, CancellationToken cancellationToken) => throw new NotImplementedException();

        public Task<byte[]> RequestOpenAsync(string path, Flags flags, CancellationToken cancellationToken) => throw new NotImplementedException();

        public byte[] RequestOpenDir(string path, bool nullOnError = false) => throw new NotImplementedException();

        public Task<byte[]> RequestOpenDirAsync(string path, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void RequestPosixRename(string oldPath, string newPath) => throw new NotImplementedException();

        public Task<byte[]> RequestReadAsync(byte[] handle, ulong offset, uint length, CancellationToken cancellationToken) => throw new NotImplementedException();

        public KeyValuePair<string, SftpFileAttributes>[] RequestReadDir(byte[] handle) => throw new NotImplementedException();

        public Task<KeyValuePair<string, SftpFileAttributes>[]> RequestReadDirAsync(byte[] handle, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void RequestRemove(string path) => throw new NotImplementedException();

        public Task RequestRemoveAsync(string path, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void RequestRename(string oldPath, string newPath) => throw new NotImplementedException();

        public Task RequestRenameAsync(string oldPath, string newPath, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void RequestRmDir(string path) => throw new NotImplementedException();

        public Task RequestRmDirAsync(string path, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void RequestSetStat(string path, SftpFileAttributes attributes) => throw new NotImplementedException();

        public SftpFileAttributes RequestStat(string path, bool nullOnError = false) => throw new NotImplementedException();

        public SftpFileSystemInformation RequestStatVfs(string path, bool nullOnError = false) => throw new NotImplementedException();

        public Task<SftpFileSystemInformation> RequestStatVfsAsync(string path, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void RequestSymLink(string linkpath, string targetpath) => throw new NotImplementedException();

        public Task RequestWriteAsync(byte[] handle, ulong serverOffset, byte[] data, int offset, int length, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void WaitOnHandle(WaitHandle waitHandle, int millisecondsTimeout) => throw new NotImplementedException();
    }
}
