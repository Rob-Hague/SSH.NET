﻿using System;
using System.Collections.Generic;
using System.Linq;

using Renci.SshNet.Common;

namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// Contains SFTP file attributes.
    /// </summary>
    public class SftpFileAttributes
    {
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1310 // Field names should not contain underscore
        private const uint S_IFMT = 0xF000; // bitmask for the file type bitfields
        private const uint S_IFSOCK = 0xC000; // socket
        private const uint S_IFLNK = 0xA000; // symbolic link
        private const uint S_IFREG = 0x8000; // regular file
        private const uint S_IFBLK = 0x6000; // block device
        private const uint S_IFDIR = 0x4000; // directory
        private const uint S_IFCHR = 0x2000; // character device
        private const uint S_IFIFO = 0x1000; // FIFO
        private const uint S_ISUID = 0x0800; // set UID bit
        private const uint S_ISGID = 0x0400; // set-group-ID bit (see below)
        private const uint S_ISVTX = 0x0200; // sticky bit (see below)
        private const uint S_IRUSR = 0x0100; // owner has read permission
        private const uint S_IWUSR = 0x0080; // owner has write permission
        private const uint S_IXUSR = 0x0040; // owner has execute permission
        private const uint S_IRGRP = 0x0020; // group has read permission
        private const uint S_IWGRP = 0x0010; // group has write permission
        private const uint S_IXGRP = 0x0008; // group has execute permission
        private const uint S_IROTH = 0x0004; // others have read permission
        private const uint S_IWOTH = 0x0002; // others have write permission
        private const uint S_IXOTH = 0x0001; // others have execute permission
#pragma warning restore SA1310 // Field names should not contain underscore
#pragma warning restore IDE1006 // Naming Styles

        private readonly DateTime _originalLastAccessTimeUtc;
        private readonly DateTime _originalLastWriteTimeUtc;
        private readonly long _originalSize;
        private readonly int _originalUserId;
        private readonly int _originalGroupId;
        private readonly uint _originalPermissions;
        private readonly IDictionary<string, string> _originalExtensions;

        internal bool IsLastAccessTimeChanged
        {
            get { return _originalLastAccessTimeUtc != LastAccessTimeUtc; }
        }

        internal bool IsLastWriteTimeChanged
        {
            get { return _originalLastWriteTimeUtc != LastWriteTimeUtc; }
        }

        internal bool IsSizeChanged
        {
            get { return _originalSize != Size; }
        }

        internal bool IsUserIdChanged
        {
            get { return _originalUserId != UserId; }
        }

        internal bool IsGroupIdChanged
        {
            get { return _originalGroupId != GroupId; }
        }

        internal bool IsPermissionsChanged
        {
            get { return _originalPermissions != Permissions; }
        }

        internal bool IsExtensionsChanged
        {
            get { return _originalExtensions != null && Extensions != null && !_originalExtensions.SequenceEqual(Extensions); }
        }

        /// <summary>
        /// Gets or sets the local time the current file or directory was last accessed.
        /// </summary>
        /// <value>
        /// The local time that the current file or directory was last accessed.
        /// </value>
        public DateTime LastAccessTime
        {
            get
            {
                return ToLocalTime(LastAccessTimeUtc);
            }

            set
            {
                LastAccessTimeUtc = ToUniversalTime(value);
            }
        }

        /// <summary>
        /// Gets or sets the local time when the current file or directory was last written to.
        /// </summary>
        /// <value>
        /// The local time the current file was last written.
        /// </value>
        public DateTime LastWriteTime
        {
            get
            {
                return ToLocalTime(LastWriteTimeUtc);
            }

            set
            {
                LastWriteTimeUtc = ToUniversalTime(value);
            }
        }

        /// <summary>
        /// Gets or sets the UTC time the current file or directory was last accessed.
        /// </summary>
        /// <value>
        /// The UTC time that the current file or directory was last accessed.
        /// </value>
        public DateTime LastAccessTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the UTC time when the current file or directory was last written to.
        /// </summary>
        /// <value>
        /// The UTC time the current file was last written.
        /// </value>
        public DateTime LastWriteTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the size, in bytes, of the current file.
        /// </summary>
        /// <value>
        /// The size of the current file in bytes.
        /// </value>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets file user id.
        /// </summary>
        /// <value>
        /// File user id.
        /// </value>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets file group id.
        /// </summary>
        /// <value>
        /// File group id.
        /// </value>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets a value indicating whether file represents a socket.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if file represents a socket; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsSocket
        {
            get
            {
                return (Permissions & S_IFMT) == S_IFSOCK;
            }
        }

        /// <summary>
        /// Gets a value indicating whether file represents a symbolic link.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if file represents a symbolic link; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsSymbolicLink
        {
            get
            {
                return (Permissions & S_IFMT) == S_IFLNK;
            }
        }

        /// <summary>
        /// Gets a value indicating whether file represents a regular file.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if file represents a regular file; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsRegularFile
        {
            get
            {
                return (Permissions & S_IFMT) == S_IFREG;
            }
        }

        /// <summary>
        /// Gets a value indicating whether file represents a block device.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if file represents a block device; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsBlockDevice
        {
            get
            {
                return (Permissions & S_IFMT) == S_IFBLK;
            }
        }

        /// <summary>
        /// Gets a value indicating whether file represents a directory.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if file represents a directory; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsDirectory
        {
            get
            {
                return (Permissions & S_IFMT) == S_IFDIR;
            }
        }

        /// <summary>
        /// Gets a value indicating whether file represents a character device.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if file represents a character device; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsCharacterDevice
        {
            get
            {
                return (Permissions & S_IFMT) == S_IFCHR;
            }
        }

        /// <summary>
        /// Gets a value indicating whether file represents a named pipe.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if file represents a named pipe; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsNamedPipe
        {
            get
            {
                return (Permissions & S_IFMT) == S_IFIFO;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the setuid bit is set.
        /// </summary>
        /// <value>
        /// <c>true</c> if the setuid bit is set; otherwise, <c>false</c>.
        /// </value>
        public bool IsUIDBitSet
        {
            get
            {
                return (Permissions & S_ISUID) == S_ISUID;
            }
            set
            {
                if (value)
                {
                    Permissions |= S_ISUID;
                }
                else
                {
                    Permissions &= ~S_ISUID;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the setgid bit is set.
        /// </summary>
        /// <value>
        /// <c>true</c> if the setgid bit is set; otherwise, <c>false</c>.
        /// </value>
        public bool IsGroupIDBitSet
        {
            get
            {
                return (Permissions & S_ISGID) == S_ISGID;
            }
            set
            {
                if (value)
                {
                    Permissions |= S_ISGID;
                }
                else
                {
                    Permissions &= ~S_ISGID;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the sticky bit is set.
        /// </summary>
        /// <value>
        /// <c>true</c> if the sticky bit is set; otherwise, <c>false</c>.
        /// </value>
        public bool IsStickyBitSet
        {
            get
            {
                return (Permissions & S_ISVTX) == S_ISVTX;
            }
            set
            {
                if (value)
                {
                    Permissions |= S_ISVTX;
                }
                else
                {
                    Permissions &= ~S_ISVTX;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the owner can read from this file.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if owner can read from this file; otherwise, <see langword="false"/>.
        /// </value>
        public bool OwnerCanRead
        {
            get
            {
                return (Permissions & S_IRUSR) == S_IRUSR;
            }
            set
            {
                if (value)
                {
                    Permissions |= S_IRUSR;
                }
                else
                {
                    Permissions &= ~S_IRUSR;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the owner can write into this file.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if owner can write into this file; otherwise, <see langword="false"/>.
        /// </value>
        public bool OwnerCanWrite
        {
            get
            {
                return (Permissions & S_IWUSR) == S_IWUSR;
            }
            set
            {
                if (value)
                {
                    Permissions |= S_IWUSR;
                }
                else
                {
                    Permissions &= ~S_IWUSR;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the owner can execute this file.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if owner can execute this file; otherwise, <see langword="false"/>.
        /// </value>
        public bool OwnerCanExecute
        {
            get
            {
                return (Permissions & S_IXUSR) == S_IXUSR;
            }
            set
            {
                if (value)
                {
                    Permissions |= S_IXUSR;
                }
                else
                {
                    Permissions &= ~S_IXUSR;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the group members can read from this file.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if group members can read from this file; otherwise, <see langword="false"/>.
        /// </value>
        public bool GroupCanRead
        {
            get
            {
                return (Permissions & S_IRGRP) == S_IRGRP;
            }
            set
            {
                if (value)
                {
                    Permissions |= S_IRGRP;
                }
                else
                {
                    Permissions &= ~S_IRGRP;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the group members can write into this file.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if group members can write into this file; otherwise, <see langword="false"/>.
        /// </value>
        public bool GroupCanWrite
        {
            get
            {
                return (Permissions & S_IWGRP) == S_IWGRP;
            }
            set
            {
                if (value)
                {
                    Permissions |= S_IWGRP;
                }
                else
                {
                    Permissions &= ~S_IWGRP;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the group members can execute this file.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if group members can execute this file; otherwise, <see langword="false"/>.
        /// </value>
        public bool GroupCanExecute
        {
            get
            {
                return (Permissions & S_IXGRP) == S_IXGRP;
            }
            set
            {
                if (value)
                {
                    Permissions |= S_IXGRP;
                }
                else
                {
                    Permissions &= ~S_IXGRP;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the others can read from this file.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if others can read from this file; otherwise, <see langword="false"/>.
        /// </value>
        public bool OthersCanRead
        {
            get
            {
                return (Permissions & S_IROTH) == S_IROTH;
            }
            set
            {
                if (value)
                {
                    Permissions |= S_IROTH;
                }
                else
                {
                    Permissions &= ~S_IROTH;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the others can write into this file.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if others can write into this file; otherwise, <see langword="false"/>.
        /// </value>
        public bool OthersCanWrite
        {
            get
            {
                return (Permissions & S_IWOTH) == S_IWOTH;
            }
            set
            {
                if (value)
                {
                    Permissions |= S_IWOTH;
                }
                else
                {
                    Permissions &= ~S_IWOTH;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the others can execute this file.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if others can execute this file; otherwise, <see langword="false"/>.
        /// </value>
        public bool OthersCanExecute
        {
            get
            {
                return (Permissions & S_IXOTH) == S_IXOTH;
            }
            set
            {
                if (value)
                {
                    Permissions |= S_IXOTH;
                }
                else
                {
                    Permissions &= ~S_IXOTH;
                }
            }
        }

        /// <summary>
        /// Gets the extensions.
        /// </summary>
        /// <value>
        /// The extensions.
        /// </value>
        public IDictionary<string, string> Extensions { get; private set; }

        internal uint Permissions { get; private set; }

        private SftpFileAttributes()
        {
        }

        internal SftpFileAttributes(DateTime lastAccessTimeUtc, DateTime lastWriteTimeUtc, long size, int userId, int groupId, uint permissions, IDictionary<string, string> extensions)
        {
            LastAccessTimeUtc = _originalLastAccessTimeUtc = lastAccessTimeUtc;
            LastWriteTimeUtc = _originalLastWriteTimeUtc = lastWriteTimeUtc;
            Size = _originalSize = size;
            UserId = _originalUserId = userId;
            GroupId = _originalGroupId = groupId;
            Permissions = _originalPermissions = permissions;
            Extensions = _originalExtensions = extensions;
        }

        internal string GetPermissionsString()
        {
            // https://en.wikipedia.org/wiki/File-system_permissions#Symbolic_notation

            var chars = new char[10];

            // https://en.wikipedia.org/wiki/Unix_file_types
            chars[0] = IsRegularFile ? '-'
                : IsDirectory ? 'd'
                : IsSymbolicLink ? 'l'
                : IsNamedPipe ? 'p'
                : IsSocket ? 's'
                : IsCharacterDevice ? 'c'
                : IsBlockDevice ? 'b'
                : '-';

            chars[1] = OwnerCanRead ? 'r' : '-';
            chars[2] = OwnerCanWrite ? 'w' : '-';

            if (OwnerCanExecute)
            {
                chars[3] = IsUIDBitSet ? 's' : 'x';
            }
            else
            {
                chars[3] = IsUIDBitSet ? 'S' : '-';
            }

            chars[4] = GroupCanRead ? 'r' : '-';
            chars[5] = GroupCanWrite ? 'w' : '-';

            if (GroupCanExecute)
            {
                chars[6] = IsGroupIDBitSet ? 's' : 'x';
            }
            else
            {
                chars[6] = IsGroupIDBitSet ? 'S' : '-';
            }

            chars[7] = OthersCanRead ? 'r' : '-';
            chars[8] = OthersCanWrite ? 'w' : '-';

            if (OthersCanExecute)
            {
                chars[9] = IsStickyBitSet ? 't' : 'x';
            }
            else
            {
                chars[9] = IsStickyBitSet ? 'T' : '-';
            }

            return new string(chars);
        }

        /// <summary>
        /// Sets the permissions.
        /// </summary>
        /// <param name="mode">The mode.</param>
        public void SetPermissions(short mode)
        {
            var special = (uint) Math.DivRem(mode, 1000, out var userGroupOther);

            var user = (uint)Math.DivRem(userGroupOther, 100, out var groupOther);

            var group = (uint)Math.DivRem(groupOther, 10, out var iOther);

            var other = (uint)iOther;

            if ((special & ~7u) != 0 || (user & ~7u) != 0 || (group & ~7u) != 0 || (other & ~7u) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(mode));
            }

            Permissions = (Permissions & ~0x0FFFu) | (special << 9) | (user << 6) | (group << 3) | other;
        }

        /// <summary>
        /// Returns a byte array representing the current <see cref="SftpFileAttributes"/>.
        /// </summary>
        /// <returns>
        /// A byte array representing the current <see cref="SftpFileAttributes"/>.
        /// </returns>
        public byte[] GetBytes()
        {
            using (var stream = new SshDataStream(4))
            {
                uint flag = 0;

                if (IsSizeChanged && IsRegularFile)
                {
                    flag |= 0x00000001;
                }

                if (IsUserIdChanged || IsGroupIdChanged)
                {
                    flag |= 0x00000002;
                }

                if (IsPermissionsChanged)
                {
                    flag |= 0x00000004;
                }

                if (IsLastAccessTimeChanged || IsLastWriteTimeChanged)
                {
                    flag |= 0x00000008;
                }

                if (IsExtensionsChanged)
                {
                    flag |= 0x80000000;
                }

                stream.Write(flag);

                if (IsSizeChanged && IsRegularFile)
                {
                    stream.Write((ulong) Size);
                }

                if (IsUserIdChanged || IsGroupIdChanged)
                {
                    stream.Write((uint) UserId);
                    stream.Write((uint) GroupId);
                }

                if (IsPermissionsChanged)
                {
                    stream.Write(Permissions);
                }

                if (IsLastAccessTimeChanged || IsLastWriteTimeChanged)
                {
                    var time = (uint) ((LastAccessTimeUtc.ToFileTimeUtc() / 10000000) - 11644473600);
                    stream.Write(time);
                    time = (uint) ((LastWriteTimeUtc.ToFileTimeUtc() / 10000000) - 11644473600);
                    stream.Write(time);
                }

                if (IsExtensionsChanged)
                {
                    foreach (var item in Extensions)
                    {
                        /*
                         * TODO: we write as ASCII but read as UTF8 !!!
                         */

                        stream.Write(item.Key, SshData.Ascii);
                        stream.Write(item.Value, SshData.Ascii);
                    }
                }

                return stream.ToArray();
            }
        }

        internal static readonly SftpFileAttributes Empty = new SftpFileAttributes();

        internal static SftpFileAttributes FromBytes(SshDataStream stream)
        {
            const uint SSH_FILEXFER_ATTR_SIZE = 0x00000001;
            const uint SSH_FILEXFER_ATTR_UIDGID = 0x00000002;
            const uint SSH_FILEXFER_ATTR_PERMISSIONS = 0x00000004;
            const uint SSH_FILEXFER_ATTR_ACMODTIME = 0x00000008;
            const uint SSH_FILEXFER_ATTR_EXTENDED = 0x80000000;

            var flag = stream.ReadUInt32();

            long size = -1;
            var userId = -1;
            var groupId = -1;
            uint permissions = 0;
            DateTime accessTime;
            DateTime modifyTime;
            Dictionary<string, string> extensions = null;

            if ((flag & SSH_FILEXFER_ATTR_SIZE) == SSH_FILEXFER_ATTR_SIZE)
            {
                size = (long) stream.ReadUInt64();
            }

            if ((flag & SSH_FILEXFER_ATTR_UIDGID) == SSH_FILEXFER_ATTR_UIDGID)
            {
                userId = (int) stream.ReadUInt32();

                groupId = (int) stream.ReadUInt32();
            }

            if ((flag & SSH_FILEXFER_ATTR_PERMISSIONS) == SSH_FILEXFER_ATTR_PERMISSIONS)
            {
                permissions = stream.ReadUInt32();
            }

            if ((flag & SSH_FILEXFER_ATTR_ACMODTIME) == SSH_FILEXFER_ATTR_ACMODTIME)
            {
                // The incoming times are "Unix times", so they're already in UTC.  We need to preserve that
                // to avoid losing information in a local time conversion during the "fall back" hour in DST.
                var time = stream.ReadUInt32();
                accessTime = DateTime.FromFileTimeUtc((time + 11644473600) * 10000000);
                time = stream.ReadUInt32();
                modifyTime = DateTime.FromFileTimeUtc((time + 11644473600) * 10000000);
            }
            else
            {
                accessTime = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
                modifyTime = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
            }

            if ((flag & SSH_FILEXFER_ATTR_EXTENDED) == SSH_FILEXFER_ATTR_EXTENDED)
            {
                var extendedCount = (int) stream.ReadUInt32();
                extensions = new Dictionary<string, string>(extendedCount);
                for (var i = 0; i < extendedCount; i++)
                {
                    var extensionName = stream.ReadString(SshData.Utf8);
                    var extensionData = stream.ReadString(SshData.Utf8);
                    extensions.Add(extensionName, extensionData);
                }
            }

            return new SftpFileAttributes(accessTime, modifyTime, size, userId, groupId, permissions, extensions);
        }

        internal static SftpFileAttributes FromBytes(byte[] buffer)
        {
            using (var stream = new SshDataStream(buffer))
            {
                return FromBytes(stream);
            }
        }

        private static DateTime ToLocalTime(DateTime value)
        {
            DateTime result;

            if (value == DateTime.MinValue)
            {
                result = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Local);
            }
            else
            {
                result = value.ToLocalTime();
            }

            return result;
        }

        private static DateTime ToUniversalTime(DateTime value)
        {
            DateTime result;

            if (value == DateTime.MinValue)
            {
                result = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
            }
            else
            {
                result = value.ToUniversalTime();
            }

            return result;
        }
    }
}
