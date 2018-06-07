﻿// 
// Keychain.cs
//
// Authors: Michael Hutchinson <mhutchinson@novell.com>
//          Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
// Copyright (c) 2013 Xamarin Inc. (http://www.xamarin.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Virgil.SDK
{
    internal class Keychain
    {
        const string CoreFoundationLib = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
        const string SecurityLib = "/System/Library/Frameworks/Security.framework/Security";

        public static Keychain Default = new Keychain(IntPtr.Zero);

        readonly IntPtr keychain;

        Keychain(IntPtr keychain)
        {
            this.keychain = keychain;
        }

        [DllImport(CoreFoundationLib, EntryPoint = "CFRelease")]
        static extern void CFReleaseInternal(IntPtr cfRef);

        static void CFRelease(IntPtr cfRef)
        {
            if (cfRef != IntPtr.Zero)
                CFReleaseInternal(cfRef);
        }

        [DllImport(SecurityLib)]
        static extern OSStatus SecKeychainCopyDefault(ref IntPtr keychain);

        [DllImport(SecurityLib)]
        static extern OSStatus SecKeychainGetPath(IntPtr keychain, out uint ioPathLength, IntPtr pathName);

        #region Managing Certificates

        [DllImport(SecurityLib)]
        static extern OSStatus SecCertificateAddToKeychain(IntPtr certificate, IntPtr keychain);

        [DllImport(SecurityLib)]
        static extern IntPtr SecCertificateCreateWithData(IntPtr allocator, IntPtr data);

        [DllImport(SecurityLib)]
        static extern IntPtr SecCertificateCopyData(IntPtr certificate);

        [DllImport(SecurityLib)]
        static extern OSStatus SecCertificateCopyCommonName(IntPtr certificate, out IntPtr commonName);

        #endregion

        #region Managing Identities

        [DllImport(SecurityLib)]
        static extern OSStatus SecIdentityCopyCertificate(IntPtr identityRef, out IntPtr certificateRef);

        [DllImport(SecurityLib)]
        static extern OSStatus SecIdentityCopyPrivateKey(IntPtr identityRef, out IntPtr privateKeyRef);

        // WARNING: deprecated in Mac OS X 10.7
        [DllImport(SecurityLib)]
        static extern OSStatus SecIdentitySearchCreate(IntPtr keychainOrArray, CssmKeyUse keyUsage, out IntPtr searchRef);

        // WARNING: deprecated in Mac OS X 10.7
        [DllImport(SecurityLib)]
        static extern OSStatus SecIdentitySearchCopyNext(IntPtr searchRef, out IntPtr identity);

        // Note: SecIdentitySearch* has been replaced with SecItemCopyMatching

        //[DllImport (SecurityLib)]
        //OSStatus SecItemCopyMatching (CFDictionaryRef query, CFTypeRef *result);

        #endregion

        #region Getting Information About Security Result Codes

        [DllImport(SecurityLib)]
        static extern IntPtr SecCopyErrorMessageString(OSStatus status, IntPtr reserved);

        #endregion

        #region Managing Keychains

        [DllImport(SecurityLib)]
        static extern OSStatus SecKeychainCreate(string pathName, uint passwordLength, byte[] password,
                                                  bool promptUser, IntPtr initialAccess, ref IntPtr keychain);

        [DllImport(SecurityLib)]
        static extern OSStatus SecKeychainOpen(string pathName, ref IntPtr keychain);

        [DllImport(SecurityLib)]
        static extern OSStatus SecKeychainDelete(IntPtr keychain);

        internal static Keychain Create(string path, string password)
        {
            var passwd = Encoding.UTF8.GetBytes(password);
            var handle = IntPtr.Zero;

            var status = SecKeychainCreate(path, (uint)passwd.Length, passwd, false, IntPtr.Zero, ref handle);
            if (status != OSStatus.Ok)
                throw new Exception(GetError(status));

            return new Keychain(handle);
        }

        public static Keychain Open(string path)
        {
            var handle = IntPtr.Zero;

            var status = SecKeychainOpen(path, ref handle);
            if (status != OSStatus.Ok)
                throw new Exception(GetError(status));

            return new Keychain(handle);
        }

        internal void Delete()
        {
            var status = SecKeychainDelete(keychain);
            if (status != OSStatus.Ok)
                throw new Exception(GetError(status));
        }

        #endregion

        #region Storing and Retrieving Passwords

        [DllImport(SecurityLib)]
        static extern OSStatus SecKeychainAddInternetPassword(IntPtr keychain, uint serverNameLength, byte[] serverName, uint securityDomainLength,
                                                               byte[] securityDomain, uint accountNameLength, byte[] accountName, uint pathLength,
                                                               byte[] path, ushort port, SecProtocolType protocol, SecAuthenticationType authType,
                                                               uint passwordLength, byte[] passwordData, ref IntPtr itemRef);
        [DllImport(SecurityLib)]
        static extern OSStatus SecKeychainFindInternetPassword(IntPtr keychain, uint serverNameLength, byte[] serverName, uint securityDomainLength,
                                                                byte[] securityDomain, uint accountNameLength, byte[] accountName, uint pathLength,
                                                                byte[] path, ushort port, SecProtocolType protocol, SecAuthenticationType authType,
                                                                out uint passwordLength, out IntPtr passwordData, ref IntPtr itemRef);

        [DllImport(SecurityLib)]
        internal static extern OSStatus SecKeychainAddGenericPassword(IntPtr keychain, uint serviceNameLength, byte[] serviceName,
                                                              uint accountNameLength, byte[] accountName, uint passwordLength,
                                                              byte[] passwordData, ref IntPtr itemRef);
        [DllImport(SecurityLib)]
        internal static extern OSStatus SecKeychainFindGenericPassword(IntPtr keychain, uint serviceNameLength, byte[] serviceName,
                                                               uint accountNameLength, byte[] accountName, out uint passwordLength,
                                                               out IntPtr passwordData, ref IntPtr itemRef);

        #endregion

        #region Searching for Keychain Items

        [DllImport(SecurityLib)]
        static extern unsafe OSStatus SecKeychainSearchCreateFromAttributes(IntPtr keychainOrArray, SecItemClass itemClass, SecKeychainAttributeList* attrList, out IntPtr searchRef);

        [DllImport(SecurityLib)]
        static extern OSStatus SecKeychainSearchCopyNext(IntPtr searchRef, out IntPtr itemRef);

        #endregion

        #region Creating and Deleting Keychain Items

        [StructLayout(LayoutKind.Sequential)]
        struct SecKeychainAttributeList
        {
            public int Count;
            public IntPtr Attrs;

            public SecKeychainAttributeList(int count, IntPtr attrs)
            {
                Count = count;
                Attrs = attrs;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SecKeychainAttribute
        {
            public SecItemAttr Tag;
            public uint Length;
            public IntPtr Data;

            public SecKeychainAttribute(SecItemAttr tag, uint length, IntPtr data)
            {
                Tag = tag;
                Length = length;
                Data = data;
            }
        }

        [DllImport(SecurityLib)]
        static extern unsafe OSStatus SecKeychainItemCreateFromContent(SecItemClass itemClass, SecKeychainAttributeList* attrList,
                                                                        uint passwordLength, byte[] password, IntPtr keychain,
                                                                        IntPtr initialAccess, ref IntPtr itemRef);

        [DllImport(SecurityLib)]
        internal static extern OSStatus SecKeychainItemDelete(IntPtr itemRef);

        #endregion

        #region Managing Keychain Items

        [StructLayout(LayoutKind.Sequential)]
        unsafe struct SecKeychainAttributeInfo
        {
            public uint Count;
            public int* Tag;
            public int* Format;
        }

        [DllImport(SecurityLib)]
        static extern unsafe OSStatus SecKeychainItemCopyAttributesAndData(IntPtr itemRef, SecKeychainAttributeInfo* info, ref SecItemClass itemClass,
                                                                            SecKeychainAttributeList** attrList, ref uint length, ref IntPtr outData);

        [DllImport(SecurityLib)]
        static extern unsafe OSStatus SecKeychainItemModifyAttributesAndData(IntPtr itemRef, SecKeychainAttributeList* attrList, uint length, byte[] data);

        [DllImport(SecurityLib)]
        static extern OSStatus SecKeychainItemCopyContent(IntPtr itemRef, ref SecItemClass itemClass, IntPtr attrList, ref uint length, ref IntPtr data);

        [DllImport(SecurityLib)]
        static extern OSStatus SecKeychainItemFreeContent(IntPtr attrList, IntPtr data);

        #endregion

        #region CFRange

        struct CFRange
        {
            public IntPtr Location, Length;
            public CFRange(IntPtr l, IntPtr len)
            {
                Location = l;
                Length = len;
            }
        }

        #endregion

        #region CFData

        [DllImport(CoreFoundationLib)]
        extern static IntPtr CFDataGetLength(IntPtr data);

        [DllImport(CoreFoundationLib)]
        extern static void CFDataGetBytes(IntPtr data, CFRange range, IntPtr buffer);

        [DllImport(CoreFoundationLib)]
        extern static IntPtr CFDataCreate(IntPtr allocator, byte[] buffer, IntPtr length);

        static byte[] CFDataGetBytes(IntPtr data)
        {
            if (data == IntPtr.Zero)
                return null;

            long len = (long)CFDataGetLength(data);
            if (len < 1 || len > int.MaxValue)
                return null;

            byte[] buffer = new byte[(int)len];
            unsafe
            {
                fixed (byte* bufptr = buffer)
                {
                    CFDataGetBytes(data, new CFRange(IntPtr.Zero, (IntPtr)len), (IntPtr)bufptr);
                }
            }

            return buffer;
        }

        #endregion

        #region CFString

        [DllImport(CoreFoundationLib, CharSet = CharSet.Unicode)]
        extern static IntPtr CFStringGetLength(IntPtr handle);

        [DllImport(CoreFoundationLib, CharSet = CharSet.Unicode)]
        extern static IntPtr CFStringGetCharactersPtr(IntPtr handle);

        [DllImport(CoreFoundationLib, CharSet = CharSet.Unicode)]
        extern static IntPtr CFStringGetCharacters(IntPtr handle, CFRange range, IntPtr buffer);

        static string CFStringGetString(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                return null;

            int length = (int)CFStringGetLength(handle);
            var unicode = CFStringGetCharactersPtr(handle);
            IntPtr buffer = IntPtr.Zero;
            string str;

            if (unicode == IntPtr.Zero)
            {
                var range = new CFRange(IntPtr.Zero, (IntPtr)length);
                buffer = Marshal.AllocCoTaskMem(length * 2);
                CFStringGetCharacters(handle, range, buffer);
                unicode = buffer;
            }

            unsafe
            {
                str = new string((char*)unicode, 0, length);
            }

            if (buffer != IntPtr.Zero)
                Marshal.FreeCoTaskMem(buffer);

            return str;
        }

        #endregion


        static string GetError(OSStatus status)
        {
            IntPtr str = IntPtr.Zero;
            try
            {
                str = SecCopyErrorMessageString(status, IntPtr.Zero);
                return CFStringGetString(str);
            }
            catch
            {
                return status.ToString();
            }
            finally
            {
                if (str != IntPtr.Zero)
                    CFRelease(str);
            }
        }


        static SecAuthenticationType GetSecAuthenticationType(string query)
        {
            if (string.IsNullOrEmpty(query))
                return SecAuthenticationType.Any;

            string auth = "default";
            foreach (var pair in query.Substring(1).Split(new char[] { '&' }))
            {
                var kvp = pair.ToLowerInvariant().Split(new char[] { '=' });
                if (kvp[0] == "auth" && kvp.Length == 2)
                {
                    auth = kvp[1];
                    break;
                }
            }

            switch (auth.ToLowerInvariant())
            {
                case "ntlm": return SecAuthenticationType.NTLM;
                case "msn": return SecAuthenticationType.MSN;
                case "dpa": return SecAuthenticationType.DPA;
                case "rpa": return SecAuthenticationType.RPA;
                case "httpbasic": case "basic": return SecAuthenticationType.HTTPBasic;
                case "httpdigest": case "digest": return SecAuthenticationType.HTTPDigest;
                case "htmlform": case "form": return SecAuthenticationType.HTMLForm;
                case "default": return SecAuthenticationType.Default;
                default: return SecAuthenticationType.Any;
            }
        }

        static SecProtocolType GetSecProtocolType(string protocol)
        {
            switch (protocol.ToLowerInvariant())
            {
                case "ftp": return SecProtocolType.FTP;
                case "ftpaccount": return SecProtocolType.FTPAccount;
                case "http": return SecProtocolType.HTTP;
                case "irc": return SecProtocolType.IRC;
                case "nntp": return SecProtocolType.NNTP;
                case "pop3": return SecProtocolType.POP3;
                case "smtp": return SecProtocolType.SMTP;
                case "socks": return SecProtocolType.SOCKS;
                case "imap": return SecProtocolType.IMAP;
                case "ldap": return SecProtocolType.LDAP;
                case "appletalk": return SecProtocolType.AppleTalk;
                case "afp": return SecProtocolType.AFP;
                case "telnet": return SecProtocolType.Telnet;
                case "ssh": return SecProtocolType.SSH;
                case "ftps": return SecProtocolType.FTPS;
                case "httpproxy": return SecProtocolType.HTTPProxy;
                case "httpsproxy": return SecProtocolType.HTTPSProxy;
                case "ftpproxy": return SecProtocolType.FTPProxy;
                case "cifs": return SecProtocolType.CIFS;
                case "smb": return SecProtocolType.SMB;
                case "rtsp": return SecProtocolType.RTSP;
                case "rtspproxy": return SecProtocolType.RTSPProxy;
                case "daap": return SecProtocolType.DAAP;
                case "eppc": return SecProtocolType.EPPC;
                case "ipp": return SecProtocolType.IPP;
                case "nntps": return SecProtocolType.NNTPS;
                case "ldaps": return SecProtocolType.LDAPS;
                case "telnets": return SecProtocolType.TelnetS;
                case "imaps": return SecProtocolType.IMAPS;
                case "ircs": return SecProtocolType.IRCS;
                case "pop3s": return SecProtocolType.POP3S;
                case "cvspserver": return SecProtocolType.CVSpserver;
                case "svn": return SecProtocolType.SVN;
                default: return SecProtocolType.Any;
            }
        }

        static unsafe OSStatus ReplaceInternetPassword(IntPtr item, byte[] desc, byte[] passwd)
        {
            fixed (byte* descPtr = desc)
            {
                SecKeychainAttribute* attrs = stackalloc SecKeychainAttribute[1];
                int n = 0;

                if (desc != null)
                    attrs[n++] = new SecKeychainAttribute(SecItemAttr.Description, (uint)desc.Length, (IntPtr)descPtr);

                SecKeychainAttributeList attrList = new SecKeychainAttributeList(n, (IntPtr)attrs);

                return SecKeychainItemModifyAttributesAndData(item, &attrList, (uint)passwd.Length, passwd);
            }
        }

        unsafe OSStatus AddInternetPassword(byte[] label, byte[] desc, SecAuthenticationType auth, byte[] user, byte[] passwd, SecProtocolType protocol, byte[] host, int port, byte[] path)
        {
            // Note: the following code does more-or-less the same as:
            //SecKeychainAddInternetPassword (CurrentKeychain, (uint) host.Length, host, 0, null,
            //                                (uint) user.Length, user, (uint) path.Length, path, (ushort) port,
            //                                protocol, auth, (uint) passwd.Length, passwd, ref item);

            fixed (byte* labelPtr = label, descPtr = desc, userPtr = user, hostPtr = host, pathPtr = path)
            {
                SecKeychainAttribute* attrs = stackalloc SecKeychainAttribute[8];
                int* protoPtr = (int*)&protocol;
                int* authPtr = (int*)&auth;
                int* portPtr = &port;
                int n = 0;

                attrs[n++] = new SecKeychainAttribute(SecItemAttr.Label, (uint)label.Length, (IntPtr)labelPtr);
                if (desc != null)
                    attrs[n++] = new SecKeychainAttribute(SecItemAttr.Description, (uint)desc.Length, (IntPtr)descPtr);
                attrs[n++] = new SecKeychainAttribute(SecItemAttr.Account, (uint)user.Length, (IntPtr)userPtr);
                attrs[n++] = new SecKeychainAttribute(SecItemAttr.Protocol, (uint)4, (IntPtr)protoPtr);
                attrs[n++] = new SecKeychainAttribute(SecItemAttr.AuthType, (uint)4, (IntPtr)authPtr);
                attrs[n++] = new SecKeychainAttribute(SecItemAttr.Server, (uint)host.Length, (IntPtr)hostPtr);
                attrs[n++] = new SecKeychainAttribute(SecItemAttr.Port, (uint)4, (IntPtr)portPtr);
                attrs[n++] = new SecKeychainAttribute(SecItemAttr.Path, (uint)path.Length, (IntPtr)pathPtr);

                SecKeychainAttributeList attrList = new SecKeychainAttributeList(n, (IntPtr)attrs);

                var item = IntPtr.Zero;
                var result = SecKeychainItemCreateFromContent(SecItemClass.InternetPassword, &attrList, (uint)passwd.Length, passwd, keychain, IntPtr.Zero, ref item);
                CFRelease(item);

                return result;
            }
        }

        public unsafe void AddInternetPassword(Uri uri, string username, string password)
        {
            byte[] path = Encoding.UTF8.GetBytes(string.Join(string.Empty, uri.Segments).Substring(1)); // don't include the leading '/'
            byte[] passwd = Encoding.UTF8.GetBytes(password);
            byte[] host = Encoding.UTF8.GetBytes(uri.Host);
            byte[] user = Encoding.UTF8.GetBytes(username);
            var auth = GetSecAuthenticationType(uri.Query);
            var protocol = GetSecProtocolType(uri.Scheme);
            IntPtr passwordData = IntPtr.Zero;
            IntPtr item = IntPtr.Zero;
            uint passwordLength = 0;
            int port = uri.Port;
            byte[] desc = null;

            if (auth == SecAuthenticationType.HTMLForm)
                desc = WebFormPassword;

            // See if there is already a password there for this uri
            var result = SecKeychainFindInternetPassword(keychain, (uint)host.Length, host, 0, null,
                                                          (uint)user.Length, user, (uint)path.Length, path, (ushort)port,
                                                          protocol, auth, out passwordLength, out passwordData, ref item);

            if (result == OSStatus.Ok)
            {
                // If there is, replace it with the new one
                result = ReplaceInternetPassword(item, desc, passwd);
                CFRelease(item);
            }
            else
            {
                var label = Encoding.UTF8.GetBytes(string.Format("{0} ({1})", uri.Host, Uri.UnescapeDataString(uri.UserInfo)));

                result = AddInternetPassword(label, desc, auth, user, passwd, protocol, host, port, path);
            }

            if (result != OSStatus.Ok && result != OSStatus.DuplicateItem)
                throw new Exception("Could not add internet password to keychain: " + GetError(result));
        }

        static readonly byte[] WebFormPassword = Encoding.UTF8.GetBytes("Web form password");

        public unsafe void AddInternetPassword(Uri uri, string password)
        {
            byte[] path = Encoding.UTF8.GetBytes(string.Join(string.Empty, uri.Segments).Substring(1)); // don't include the leading '/'
            byte[] user = Encoding.UTF8.GetBytes(Uri.UnescapeDataString(uri.UserInfo));
            byte[] passwd = Encoding.UTF8.GetBytes(password);
            byte[] host = Encoding.UTF8.GetBytes(uri.Host);
            var auth = GetSecAuthenticationType(uri.Query);
            var protocol = GetSecProtocolType(uri.Scheme);
            IntPtr passwordData = IntPtr.Zero;
            IntPtr item = IntPtr.Zero;
            uint passwordLength = 0;
            int port = uri.Port;
            byte[] desc = null;

            if (auth == SecAuthenticationType.HTMLForm)
                desc = WebFormPassword;

            // See if there is already a password there for this uri
            var result = SecKeychainFindInternetPassword(keychain, (uint)host.Length, host, 0, null,
                                                          (uint)user.Length, user, (uint)path.Length, path, (ushort)port,
                                                          protocol, auth, out passwordLength, out passwordData, ref item);

            if (result == OSStatus.Ok)
            {
                // If there is, replace it with the new one
                result = ReplaceInternetPassword(item, desc, passwd);
                CFRelease(item);
            }
            else
            {
                // Otherwise add a new entry with the password
                var label = Encoding.UTF8.GetBytes(string.Format("{0} ({1})", uri.Host, Uri.UnescapeDataString(uri.UserInfo)));

                result = AddInternetPassword(label, desc, auth, user, passwd, protocol, host, port, path);
            }

            if (result != OSStatus.Ok && result != OSStatus.DuplicateItem)
                throw new Exception("Could not add internet password to keychain: " + GetError(result));
        }

   


    }

    enum SecItemClass : uint
    {
        InternetPassword = 1768842612, // 'inet'
        GenericPassword = 1734700656, // 'genp'
        AppleSharePassword = 1634953328, // 'ashp'
        Certificate = 0x80000000 + 0x1000,
        PublicKey = 0x0000000A + 5,
        PrivateKey = 0x0000000A + 6,
        SymmetricKey = 0x0000000A + 7
    }

    enum SecItemAttr : int
    {
        CreationDate = 1667522932,
        ModDate = 1835295092,
        Description = 1684370275,
        Comment = 1768123764,
        Creator = 1668445298,
        Type = 1954115685,
        ScriptCode = 1935897200,
        Label = 1818321516,
        Invisible = 1768846953,
        Negative = 1852139361,
        CustomIcon = 1668641641,
        Account = 1633903476,
        Service = 1937138533,
        Generic = 1734700641,
        SecurityDomain = 1935961454,
        Server = 1936881266,
        AuthType = 1635023216,
        Port = 1886351988,
        Path = 1885434984,
        Volume = 1986817381,
        Address = 1633969266,
        Signature = 1936943463,
        Protocol = 1886675820,
        CertificateType = 1668577648,
        CertificateEncoding = 1667591779,
        CrlType = 1668445296,
        CrlEncoding = 1668443747,
        Alias = 1634494835,
    }

    internal enum OSStatus
    {
        Ok = 0,
        AuthFailed = -25293,
        NoSuchKeychain = -25294,
        DuplicateKeychain = -25296,
        DuplicateItem = -25299,
        ItemNotFound = -25300,
        NoDefaultKeychain = -25307,
        DecodeError = -26275,
    }

    enum SecKeyAttribute
    {
        KeyClass = 0,
        PrintName = 1,
        Alias = 2,
        Permanent = 3,
        Private = 4,
        Modifiable = 5,
        Label = 6,
        ApplicationTag = 7,
        KeyCreator = 8,
        KeyType = 9,
        KeySizeInBits = 10,
        EffectiveKeySize = 11,
        StartDate = 12,
        EndDate = 13,
        Sensitive = 14,
        AlwaysSensitive = 15,
        Extractable = 16,
        NeverExtractable = 17,
        Encrypt = 18,
        Decrypt = 19,
        Derive = 20,
        Sign = 21,
        Verify = 22,
        SignRecover = 23,
        VerifyRecover = 24,
        Wrap = 25,
        Unwrap = 26,
    }

    enum SecAuthenticationType : int
    {
        NTLM = 1835824238,
        MSN = 1634628461,
        DPA = 1633775716,
        RPA = 1633775730,
        HTTPBasic = 1886680168,
        HTTPDigest = 1685353576,
        HTMLForm = 1836216166,
        Default = 1953261156,
        Any = 0
    }

    enum SecProtocolType : int
    {
        FTP = 1718906912,
        FTPAccount = 1718906977,
        HTTP = 1752462448,
        IRC = 1769104160,
        NNTP = 1852732528,
        POP3 = 1886351411,
        SMTP = 1936553072,
        SOCKS = 1936685088,
        IMAP = 1768776048,
        LDAP = 1818517872,
        AppleTalk = 1635019883,
        AFP = 1634103328,
        Telnet = 1952803950,
        SSH = 1936943136,
        FTPS = 1718906995,
        HTTPProxy = 1752461432,
        HTTPSProxy = 1752462200,
        FTPProxy = 1718907000,
        CIFS = 1667851891,
        SMB = 1936548384,
        RTSP = 1920234352,
        RTSPProxy = 1920234360,
        DAAP = 1684103536,
        EPPC = 1701867619,
        IPP = 1768976416,
        NNTPS = 1853124723,
        LDAPS = 1818521715,
        TelnetS = 1952803955,
        IMAPS = 1768779891,
        IRCS = 1769104243,
        POP3S = 1886351475,
        CVSpserver = 1668707184,
        SVN = 1937141280,
        Any = 0
    }

    [Flags]
    enum CssmKeyUse : uint
    {
        Encrypt = 0x00000001,
        Decrypt = 0x00000002,
        Sign = 0x00000004,
        Verify = 0x00000008,
        SignRecover = 0x00000010,
        VerifyRecover = 0x00000020,
        Wrap = 0x00000040,
        Unwrap = 0x00000080,
        Derive = 0x00000100,
        Any = 0x80000000,
    }

    [Flags]
    enum CssmTPAppleCertStatus : uint
    {
        Expired = 0x00000001,
        NotValidYet = 0x00000002,
        IsInInputCerts = 0x00000004,
        IsInAnchors = 0x00000008,
        IsRoot = 0x00000010,
        IsFromNet = 0x00000020
    }

    enum CssmDbAttributeFormat : int
    {
        String = 0,
        Int32 = 1,
        UInt32 = 2,
        BigNum = 3,
        Real = 4,
        DateTime = 5,
        Blob = 6,
        MultiUInt32 = 7,
        Complex = 8
    }

    enum CertificateStatus
    {
        Valid,
        Expired,
        RootExpired,
        Unknown
    }
}