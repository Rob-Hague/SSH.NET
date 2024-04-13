﻿using System;
using System.Collections.Generic;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents an OpenSSH certificate as described in
    /// https://github.com/openssh/openssh-portable/blob/master/PROTOCOL.certkeys.
    /// </summary>
    // The xmldoc comments in the class are mostly lifted from the linked document.
#pragma warning disable SA1623 // Property summary documentation should match accessors; for the above reason
    public class Certificate
    {
        /// <summary>
        /// The type identifier of the certificate.
        /// </summary>
        /// <remarks>
        /// The value is one of the following:
        /// <list type="bullet">
        ///     <item>ssh-rsa-cert-v01@openssh.com</item>
        ///     <item>ssh-dss-cert-v01@openssh.com</item>
        ///     <item>ecdsa-sha2-nistp256-cert-v01@openssh.com</item>
        ///     <item>ecdsa-sha2-nistp384-cert-v01@openssh.com</item>
        ///     <item>ecdsa-sha2-nistp521-cert-v01@openssh.com</item>
        ///     <item>ssh-ed25519-cert-v01@openssh.com</item>
        /// </list>
        /// </remarks>
        public string Name
        {
            get
            {
                return _data.Name;
            }
        }

        /// <summary>
        /// A CA-provided random bitstring of arbitrary length
        /// (but typically 16 or 32 bytes) included to make attacks that depend on
        /// inducing collisions in the signature hash infeasible.
        /// </summary>
        public byte[] Nonce
        {
            get
            {
                return _data.Nonce;
            }
        }

        /// <summary>
        /// The public key that has been certified by the certificate authority.
        /// </summary>
        public Key Key
        {
            get
            {
                return _data.Key;
            }
        }

        /// <summary>
        /// An optional certificate serial number set by the CA to
        /// provide an abbreviated way to refer to certificates from that CA.
        /// If a CA does not wish to number its certificates, it must set this
        /// field to zero.
        /// </summary>
        public ulong Serial
        {
            get
            {
                return _data.Serial;
            }
        }

        /// <summary>
        /// Specifies whether this certificate is for identification of a user
        /// or a host.
        /// </summary>
        public CertificateType Type
        {
            get
            {
                return (CertificateType)_data.Type;
            }
        }

        /// <summary>
        /// A free-form text field that is filled in by the CA at the time
        /// of signing; the intention is that the contents of this field are used to
        /// identify the identity principal in log messages.
        /// </summary>
        public string KeyId
        {
            get
            {
                return _data.KeyId;
            }
        }

        /// <summary>
        /// The names for which this certificate is valid;
        /// hostnames for SSH_CERT_TYPE_HOST certificates and
        /// usernames for SSH_CERT_TYPE_USER certificates. As a special case, a
        /// zero-length "valid principals" field means the certificate is valid for
        /// any principal of the specified type.
        /// </summary>
        public IList<string> ValidPrincipals
        {
            get
            {
                return _data.ValidPrincipals;
            }
        }

        /// <summary>
        /// The beginning of the validity period of the certificate.
        /// </summary>
        public DateTimeOffset ValidAfter
        {
            get
            {
                return DateTimeOffset.FromUnixTimeSeconds((long)_data.ValidAfter);
            }
        }

        /// <summary>
        /// The end of the validity period of the certificate.
        /// </summary>
        public DateTimeOffset ValidBefore
        {
            get
            {
                return _data.ValidBefore == ulong.MaxValue
                    ? DateTimeOffset.MaxValue
                    : DateTimeOffset.FromUnixTimeSeconds((long)_data.ValidBefore);
            }
        }

        /// <summary>
        /// A set of zero or more options on the certificate's validity.
        /// The key identifies the option and the value encodes
        /// option-specific information.
        /// All such options are "critical" in the sense that an implementation
        /// must refuse to authorise a key that has an unrecognised option.
        /// </summary>
        public IDictionary<string, string> CriticalOptions
        {
            get
            {
                return _data.CriticalOptions;
            }
        }

        /// <summary>
        /// A set of zero or more optional extensions. These extensions
        /// are not critical, and an implementation that encounters one that it does
        /// not recognise may safely ignore it.
        /// </summary>
        public IDictionary<string, string> Extensions
        {
            get
            {
                return _data.Extensions;
            }
        }

        /// <summary>
        /// The signature key field contains the CA key used to sign the
        /// certificate. The valid key types for CA keys are ssh-rsa,
        /// ssh-dss, ssh-ed25519 and the ECDSA types ecdsa-sha2-nistp256,
        /// ecdsa-sha2-nistp384, ecdsa-sha2-nistp521. "Chained" certificates, where
        /// the signature key type is a certificate type itself are NOT supported.
        /// Note that it is possible for a RSA certificate key to be signed by a
        /// Ed25519 or ECDSA CA key and vice-versa.
        /// </summary>
        public byte[] SignatureKey
        {
            get
            {
                return _data.SignatureKey;
            }
        }

        /// <summary>
        /// The signature computed over all preceding fields from the initial string
        /// up to, and including the signature key. Signatures are computed and
        /// encoded according to the rules defined for the CA's public key algorithm
        /// (RFC4253 section 6.6 for ssh-rsa and ssh-dss, RFC5656 for the ECDSA
        /// types, and RFC8032 for Ed25519).
        /// </summary>
        public byte[] Signature
        {
            get
            {
                return _data.Signature;
            }
        }

        /// <summary>
        /// The encoded certificate bytes.
        /// </summary>
        internal byte[] Bytes { get; }

        /// <summary>
        /// The encoded bytes of the certificate which are used
        /// to calculate <see cref="Signature"/>.
        /// This consists of all of the fields before (i.e. except from)
        /// <see cref="Signature"/>.
        /// </summary>
        internal byte[] BytesForSignature
        {
            get
            {
                return Bytes.Take((int)_data.ByteCountBeforeSignature);
            }
        }

        private readonly CertificateData _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="Certificate"/>
        /// class based on the data encoded in <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The encoded public-key certificate data.</param>
        public Certificate(byte[] data)
        {
            Bytes = data;
            _data = new CertificateData();
            _data.Load(Bytes);
        }

        private sealed class CertificateData : SshData
        {
            public string Name { get; set; }

            public byte[] Nonce { get; set; }

            public Key Key { get; set; }

            public ulong Serial { get; set; }

            public uint Type { get; set; }

            public string KeyId { get; set; }

            public List<string> ValidPrincipals { get; set; }

            public ulong ValidAfter { get; set; }

            public ulong ValidBefore { get; set; }

            public Dictionary<string, string> CriticalOptions { get; set; }

            public Dictionary<string, string> Extensions { get; set; }

            public byte[] SignatureKey { get; set; }

            /// <summary>
            /// Returns the number of bytes in the encoded certificate data
            /// up to and including <see cref="SignatureKey"/>.
            /// Used for verifying <see cref="Signature"/> which is calculated
            /// from those bytes.
            /// </summary>
            public long ByteCountBeforeSignature { get; set; }

            public byte[] Signature { get; set; }

            protected override void LoadData()
            {
                Name = ReadString();
                Nonce = ReadBinary();
                Key = ReadPublicKey();
                Serial = ReadUInt64();
                Type = ReadUInt32();
                KeyId = ReadString();
                ValidPrincipals = ReadValidPrincipals(ReadBinary());
                ValidAfter = ReadUInt64();
                ValidBefore = ReadUInt64();
                CriticalOptions = ReadExtensionPair(ReadBinary());
                Extensions = ReadExtensionPair(ReadBinary());
                _ = ReadBinary(); // Unused reserved field
                SignatureKey = ReadBinary();

                ByteCountBeforeSignature = DataStream.Position;

                Signature = ReadBinary();
            }

            private Key ReadPublicKey()
            {
                switch (Name)
                {
                    case "ssh-rsa-cert-v01@openssh.com":
                        return new RsaKey(new SshKeyData("ssh-rsa", LoadPublicKeys(2)));
                    case "ssh-dss-cert-v01@openssh.com":
                        return new DsaKey(new SshKeyData("ssh-dss", LoadPublicKeys(4)));
                    case "ecdsa-sha2-nistp256-cert-v01@openssh.com":
                    case "ecdsa-sha2-nistp384-cert-v01@openssh.com":
                    case "ecdsa-sha2-nistp521-cert-v01@openssh.com":
                        return new EcdsaKey(new SshKeyData(Name.Substring(0, 19), LoadPublicKeys(2)));
                    case "ssh-ed25519-cert-v01@openssh.com":
                        return new ED25519Key(new SshKeyData("ssh-ed25519", LoadPublicKeys(1)));
                    default:
                        throw new NotSupportedException($"Certificate type '{Name}'.");
                }

                BigInteger[] LoadPublicKeys(int numPublicKeyFields)
                {
                    var keys = new BigInteger[numPublicKeyFields];

                    for (var i = 0; i < numPublicKeyFields; i++)
                    {
                        keys[i] = ReadBinary().ToBigInteger();
                    }

                    return keys;
                }
            }

            private static Dictionary<string, string> ReadExtensionPair(byte[] data)
            {
                var result = new Dictionary<string, string>();
                using var reader = new SshDataStream(data);

                while (!reader.IsEndOfData)
                {
                    var extensionName = reader.ReadString();
                    var extensionData = reader.ReadString();
                    result.Add(extensionName, extensionData);
                }

                return result;
            }

            private static List<string> ReadValidPrincipals(byte[] data)
            {
                var result = new List<string>();
                using var reader = new SshDataStream(data);

                while (!reader.IsEndOfData)
                {
                    result.Add(reader.ReadString());
                }

                return result;
            }

            protected override void SaveData()
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Used to specify whether a certificate is for identification of a user
        /// or a host.
        /// </summary>
#pragma warning disable CA1028 // Enum Storage should be Int32; match the type specified in PROTOCOL.certkeys
        public enum CertificateType : uint
#pragma warning restore CA1028 // Enum Storage should be Int32
        {
            /// <summary>
            /// The certificate is for identification of a user (SSH_CERT_TYPE_USER).
            /// </summary>
            User = 1,

            /// <summary>
            /// The certificate is for identification of a host (SSH_CERT_TYPE_HOST).
            /// </summary>
            Host = 2
        }
    }
}
