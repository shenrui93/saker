
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.ConstrainedExecution;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Win32.SafeHandles;

namespace Uyi.Win32API
{

    [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
    internal static unsafe class HttpApi
    {
        const string HTTPAPI = "httpapi.dll";

        [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern uint HttpInitialize(HTTPAPI_VERSION version, uint flags, void* pReserved);

        [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern uint HttpReceiveRequestEntityBody(CriticalHandle requestQueueHandle, ulong requestId, uint flags, void* pEntityBuffer, uint entityBufferLength, out uint bytesReturned, NativeOverlapped* pOverlapped);
        [DllImport(HTTPAPI, EntryPoint = "HttpReceiveRequestEntityBody", ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern uint HttpReceiveRequestEntityBody2(CriticalHandle requestQueueHandle, ulong requestId, uint flags, void* pEntityBuffer, uint entityBufferLength, out uint bytesReturned, [In] SafeHandle pOverlapped);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
        [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern uint HttpReceiveClientCertificate(CriticalHandle requestQueueHandle, ulong connectionId, uint flags, HTTP_SSL_CLIENT_CERT_INFO* pSslClientCertInfo, uint sslClientCertInfoSize, uint* pBytesReceived, NativeOverlapped* pOverlapped);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
        [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern uint HttpReceiveClientCertificate(CriticalHandle requestQueueHandle, ulong connectionId, uint flags, byte* pSslClientCertInfo, uint sslClientCertInfoSize, uint* pBytesReceived, NativeOverlapped* pOverlapped);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
        [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern uint HttpReceiveHttpRequest(CriticalHandle requestQueueHandle, ulong requestId, uint flags, HTTP_REQUEST* pRequestBuffer, uint requestBufferLength, uint* pBytesReturned, NativeOverlapped* pOverlapped);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
        [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern uint HttpSendHttpResponse(CriticalHandle requestQueueHandle, ulong requestId, uint flags, HTTP_RESPONSE* pHttpResponse, void* pCachePolicy, uint* pBytesSent, SafeLocalFree pRequestBuffer, uint requestBufferLength, NativeOverlapped* pOverlapped, void* pLogData);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
        [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern uint HttpSendResponseEntityBody(CriticalHandle requestQueueHandle, ulong requestId, uint flags, ushort entityChunkCount, HTTP_DATA_CHUNK* pEntityChunks, uint* pBytesSent, SafeLocalFree pRequestBuffer, uint requestBufferLength, NativeOverlapped* pOverlapped, void* pLogData);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
        [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern uint HttpCancelHttpRequest(CriticalHandle requestQueueHandle, ulong requestId, IntPtr pOverlapped);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
        [DllImport(HTTPAPI, EntryPoint = "HttpSendResponseEntityBody", ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern uint HttpSendResponseEntityBody2(CriticalHandle requestQueueHandle, ulong requestId, uint flags, ushort entityChunkCount, IntPtr pEntityChunks, out uint pBytesSent, SafeLocalFree pRequestBuffer, uint requestBufferLength, SafeHandle pOverlapped, IntPtr pLogData);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
        [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern uint HttpWaitForDisconnect(CriticalHandle requestQueueHandle, ulong connectionId, NativeOverlapped* pOverlapped);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
        [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern uint HttpCreateServerSession(HTTPAPI_VERSION version, ulong* serverSessionId, uint reserved);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
        [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern uint HttpCreateUrlGroup(ulong serverSessionId, ulong* urlGroupId, uint reserved);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
        [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern uint HttpAddUrlToUrlGroup(ulong urlGroupId, string pFullyQualifiedUrl, ulong context, uint pReserved);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
        [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern uint HttpSetUrlGroupProperty(ulong urlGroupId, HTTP_SERVER_PROPERTY serverProperty, IntPtr pPropertyInfo, uint propertyInfoLength);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
        [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern uint HttpRemoveUrlFromUrlGroup(ulong urlGroupId, string pFullyQualifiedUrl, uint flags);

        [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern uint HttpCloseServerSession(ulong serverSessionId);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Implementation requires unmanaged code usage")]
        [DllImport(HTTPAPI, ExactSpelling = true, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        internal static extern uint HttpCloseUrlGroup(ulong urlGroupId);

        internal enum HTTP_API_VERSION
        {
            Invalid,
            Version10,
            Version20,
        }

        // see http.w for definitions
        internal enum HTTP_SERVER_PROPERTY
        {
            HttpServerAuthenticationProperty,
            HttpServerLoggingProperty,
            HttpServerQosProperty,
            HttpServerTimeoutsProperty,
            HttpServerQueueLengthProperty,
            HttpServerStateProperty,
            HttpServer503VerbosityProperty,
            HttpServerBindingProperty,
            HttpServerExtendedAuthenticationProperty,
            HttpServerListenEndpointProperty,
            HttpServerChannelBindProperty,
            HttpServerProtectionLevelProperty,
        }

        //
        // Currently only one request info type is supported but the enum is for future extensibility.
        //
        internal enum HTTP_REQUEST_INFO_TYPE
        {
            HttpRequestInfoTypeAuth,
        }

        internal enum HTTP_RESPONSE_INFO_TYPE
        {
            HttpResponseInfoTypeMultipleKnownHeaders,
            HttpResponseInfoTypeAuthenticationProperty,
            HttpResponseInfoTypeQosProperty,
        }

        internal enum HTTP_TIMEOUT_TYPE
        {
            EntityBody,
            DrainEntityBody,
            RequestQueue,
            IdleConnection,
            HeaderWait,
            MinSendRate,
        }

        internal const int MaxTimeout = 6;

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_VERSION
        {
            internal ushort MajorVersion;
            internal ushort MinorVersion;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_KNOWN_HEADER
        {
            internal ushort RawValueLength;
            internal sbyte* pRawValue;
        }

        [StructLayout(LayoutKind.Sequential, Size = 32)]
        internal struct HTTP_DATA_CHUNK
        {
            internal HTTP_DATA_CHUNK_TYPE DataChunkType;
            internal uint p0;
            internal byte* pBuffer;
            internal uint BufferLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTPAPI_VERSION
        {
            internal ushort HttpApiMajorVersion;
            internal ushort HttpApiMinorVersion;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_COOKED_URL
        {
            internal ushort FullUrlLength;
            internal ushort HostLength;
            internal ushort AbsPathLength;
            internal ushort QueryStringLength;
            internal ushort* pFullUrl;
            internal ushort* pHost;
            internal ushort* pAbsPath;
            internal ushort* pQueryString;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SOCKADDR
        {
            internal ushort sa_family;
            internal byte sa_data;
            internal byte sa_data_02;
            internal byte sa_data_03;
            internal byte sa_data_04;
            internal byte sa_data_05;
            internal byte sa_data_06;
            internal byte sa_data_07;
            internal byte sa_data_08;
            internal byte sa_data_09;
            internal byte sa_data_10;
            internal byte sa_data_11;
            internal byte sa_data_12;
            internal byte sa_data_13;
            internal byte sa_data_14;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_TRANSPORT_ADDRESS
        {
            internal SOCKADDR* pRemoteAddress;
            internal SOCKADDR* pLocalAddress;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_SSL_CLIENT_CERT_INFO
        {
            internal uint CertFlags;
            internal uint CertEncodedSize;
            internal byte* pCertEncoded;
            internal void* Token;
            internal byte CertDeniedByMapper;
        }

        internal enum HTTP_SERVICE_BINDING_TYPE : uint
        {
            HttpServiceBindingTypeNone = 0,
            HttpServiceBindingTypeW,
            HttpServiceBindingTypeA
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_SERVICE_BINDING_BASE
        {
            internal HTTP_SERVICE_BINDING_TYPE Type;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_REQUEST_CHANNEL_BIND_STATUS
        {
            internal IntPtr ServiceName;
            internal IntPtr ChannelToken;
            internal uint ChannelTokenSize;
            internal uint Flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_UNKNOWN_HEADER
        {
            internal ushort NameLength;
            internal ushort RawValueLength;
            internal sbyte* pName;
            internal sbyte* pRawValue;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_SSL_INFO
        {
            internal ushort ServerCertKeySize;
            internal ushort ConnectionKeySize;
            internal uint ServerCertIssuerSize;
            internal uint ServerCertSubjectSize;
            internal sbyte* pServerCertIssuer;
            internal sbyte* pServerCertSubject;
            internal HTTP_SSL_CLIENT_CERT_INFO* pClientCertInfo;
            internal uint SslClientCertNegotiated;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_RESPONSE_HEADERS
        {
            internal ushort UnknownHeaderCount;
            internal HTTP_UNKNOWN_HEADER* pUnknownHeaders;
            internal ushort TrailerCount;
            internal HTTP_UNKNOWN_HEADER* pTrailers;
            internal HTTP_KNOWN_HEADER KnownHeaders;
            internal HTTP_KNOWN_HEADER KnownHeaders_02;
            internal HTTP_KNOWN_HEADER KnownHeaders_03;
            internal HTTP_KNOWN_HEADER KnownHeaders_04;
            internal HTTP_KNOWN_HEADER KnownHeaders_05;
            internal HTTP_KNOWN_HEADER KnownHeaders_06;
            internal HTTP_KNOWN_HEADER KnownHeaders_07;
            internal HTTP_KNOWN_HEADER KnownHeaders_08;
            internal HTTP_KNOWN_HEADER KnownHeaders_09;
            internal HTTP_KNOWN_HEADER KnownHeaders_10;
            internal HTTP_KNOWN_HEADER KnownHeaders_11;
            internal HTTP_KNOWN_HEADER KnownHeaders_12;
            internal HTTP_KNOWN_HEADER KnownHeaders_13;
            internal HTTP_KNOWN_HEADER KnownHeaders_14;
            internal HTTP_KNOWN_HEADER KnownHeaders_15;
            internal HTTP_KNOWN_HEADER KnownHeaders_16;
            internal HTTP_KNOWN_HEADER KnownHeaders_17;
            internal HTTP_KNOWN_HEADER KnownHeaders_18;
            internal HTTP_KNOWN_HEADER KnownHeaders_19;
            internal HTTP_KNOWN_HEADER KnownHeaders_20;
            internal HTTP_KNOWN_HEADER KnownHeaders_21;
            internal HTTP_KNOWN_HEADER KnownHeaders_22;
            internal HTTP_KNOWN_HEADER KnownHeaders_23;
            internal HTTP_KNOWN_HEADER KnownHeaders_24;
            internal HTTP_KNOWN_HEADER KnownHeaders_25;
            internal HTTP_KNOWN_HEADER KnownHeaders_26;
            internal HTTP_KNOWN_HEADER KnownHeaders_27;
            internal HTTP_KNOWN_HEADER KnownHeaders_28;
            internal HTTP_KNOWN_HEADER KnownHeaders_29;
            internal HTTP_KNOWN_HEADER KnownHeaders_30;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_REQUEST_HEADERS
        {
            internal ushort UnknownHeaderCount;
            internal HTTP_UNKNOWN_HEADER* pUnknownHeaders;
            internal ushort TrailerCount;
            internal HTTP_UNKNOWN_HEADER* pTrailers;
            internal HTTP_KNOWN_HEADER KnownHeaders;
            internal HTTP_KNOWN_HEADER KnownHeaders_02;
            internal HTTP_KNOWN_HEADER KnownHeaders_03;
            internal HTTP_KNOWN_HEADER KnownHeaders_04;
            internal HTTP_KNOWN_HEADER KnownHeaders_05;
            internal HTTP_KNOWN_HEADER KnownHeaders_06;
            internal HTTP_KNOWN_HEADER KnownHeaders_07;
            internal HTTP_KNOWN_HEADER KnownHeaders_08;
            internal HTTP_KNOWN_HEADER KnownHeaders_09;
            internal HTTP_KNOWN_HEADER KnownHeaders_10;
            internal HTTP_KNOWN_HEADER KnownHeaders_11;
            internal HTTP_KNOWN_HEADER KnownHeaders_12;
            internal HTTP_KNOWN_HEADER KnownHeaders_13;
            internal HTTP_KNOWN_HEADER KnownHeaders_14;
            internal HTTP_KNOWN_HEADER KnownHeaders_15;
            internal HTTP_KNOWN_HEADER KnownHeaders_16;
            internal HTTP_KNOWN_HEADER KnownHeaders_17;
            internal HTTP_KNOWN_HEADER KnownHeaders_18;
            internal HTTP_KNOWN_HEADER KnownHeaders_19;
            internal HTTP_KNOWN_HEADER KnownHeaders_20;
            internal HTTP_KNOWN_HEADER KnownHeaders_21;
            internal HTTP_KNOWN_HEADER KnownHeaders_22;
            internal HTTP_KNOWN_HEADER KnownHeaders_23;
            internal HTTP_KNOWN_HEADER KnownHeaders_24;
            internal HTTP_KNOWN_HEADER KnownHeaders_25;
            internal HTTP_KNOWN_HEADER KnownHeaders_26;
            internal HTTP_KNOWN_HEADER KnownHeaders_27;
            internal HTTP_KNOWN_HEADER KnownHeaders_28;
            internal HTTP_KNOWN_HEADER KnownHeaders_29;
            internal HTTP_KNOWN_HEADER KnownHeaders_30;
            internal HTTP_KNOWN_HEADER KnownHeaders_31;
            internal HTTP_KNOWN_HEADER KnownHeaders_32;
            internal HTTP_KNOWN_HEADER KnownHeaders_33;
            internal HTTP_KNOWN_HEADER KnownHeaders_34;
            internal HTTP_KNOWN_HEADER KnownHeaders_35;
            internal HTTP_KNOWN_HEADER KnownHeaders_36;
            internal HTTP_KNOWN_HEADER KnownHeaders_37;
            internal HTTP_KNOWN_HEADER KnownHeaders_38;
            internal HTTP_KNOWN_HEADER KnownHeaders_39;
            internal HTTP_KNOWN_HEADER KnownHeaders_40;
            internal HTTP_KNOWN_HEADER KnownHeaders_41;
        }

        internal enum HTTP_VERB : int
        {
            HttpVerbUnparsed = 0,
            HttpVerbUnknown = 1,
            HttpVerbInvalid = 2,
            HttpVerbOPTIONS = 3,
            HttpVerbGET = 4,
            HttpVerbHEAD = 5,
            HttpVerbPOST = 6,
            HttpVerbPUT = 7,
            HttpVerbDELETE = 8,
            HttpVerbTRACE = 9,
            HttpVerbCONNECT = 10,
            HttpVerbTRACK = 11,
            HttpVerbMOVE = 12,
            HttpVerbCOPY = 13,
            HttpVerbPROPFIND = 14,
            HttpVerbPROPPATCH = 15,
            HttpVerbMKCOL = 16,
            HttpVerbLOCK = 17,
            HttpVerbUNLOCK = 18,
            HttpVerbSEARCH = 19,
            HttpVerbMaximum = 20,
        }

        internal static readonly string[] HttpVerbs = new string[] {
                null,
                "Unknown",
                "Invalid",
                "OPTIONS",
                "GET",
                "HEAD",
                "POST",
                "PUT",
                "DELETE",
                "TRACE",
                "CONNECT",
                "TRACK",
                "MOVE",
                "COPY",
                "PROPFIND",
                "PROPPATCH",
                "MKCOL",
                "LOCK",
                "UNLOCK",
                "SEARCH",
            };

        internal enum HTTP_DATA_CHUNK_TYPE : int
        {
            HttpDataChunkFromMemory = 0,
            HttpDataChunkFromFileHandle = 1,
            HttpDataChunkFromFragmentCache = 2,
            HttpDataChunkMaximum = 3,
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_RESPONSE_INFO
        {
            internal HTTP_RESPONSE_INFO_TYPE Type;
            internal uint Length;
            internal void* pInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_RESPONSE
        {
            internal uint Flags;
            internal HTTP_VERSION Version;
            internal ushort StatusCode;
            internal ushort ReasonLength;
            internal sbyte* pReason;
            internal HTTP_RESPONSE_HEADERS Headers;
            internal ushort EntityChunkCount;
            internal HTTP_DATA_CHUNK* pEntityChunks;
            internal ushort ResponseInfoCount;
            internal HTTP_RESPONSE_INFO* pResponseInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_REQUEST_INFO
        {
            internal HTTP_REQUEST_INFO_TYPE InfoType;
            internal uint InfoLength;
            internal void* pInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_REQUEST
        {
            internal uint Flags;
            internal ulong ConnectionId;
            internal ulong RequestId;
            internal ulong UrlContext;
            internal HTTP_VERSION Version;
            internal HTTP_VERB Verb;
            internal ushort UnknownVerbLength;
            internal ushort RawUrlLength;
            internal sbyte* pUnknownVerb;
            internal sbyte* pRawUrl;
            internal HTTP_COOKED_URL CookedUrl;
            internal HTTP_TRANSPORT_ADDRESS Address;
            internal HTTP_REQUEST_HEADERS Headers;
            internal ulong BytesReceived;
            internal ushort EntityChunkCount;
            internal HTTP_DATA_CHUNK* pEntityChunks;
            internal ulong RawConnectionId;
            internal HTTP_SSL_INFO* pSslInfo;
            internal ushort RequestInfoCount;
            internal HTTP_REQUEST_INFO* pRequestInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_TIMEOUT_LIMIT_INFO
        {
            internal HTTP_FLAGS Flags;
            internal ushort EntityBody;
            internal ushort DrainEntityBody;
            internal ushort RequestQueue;
            internal ushort IdleConnection;
            internal ushort HeaderWait;
            internal uint MinSendRate;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HTTP_BINDING_INFO
        {
            internal HTTP_FLAGS Flags;
            internal IntPtr RequestQueueHandle;
        }

        // see http.w for definitions
        [Flags]
        internal enum HTTP_FLAGS : uint
        {
            NONE = 0x00000000,
            HTTP_RECEIVE_REQUEST_FLAG_COPY_BODY = 0x00000001,
            HTTP_RECEIVE_SECURE_CHANNEL_TOKEN = 0x00000001,
            HTTP_SEND_RESPONSE_FLAG_DISCONNECT = 0x00000001,
            HTTP_SEND_RESPONSE_FLAG_MORE_DATA = 0x00000002,
            HTTP_SEND_RESPONSE_FLAG_BUFFER_DATA = 0x00000004,
            HTTP_SEND_RESPONSE_FLAG_RAW_HEADER = 0x00000004,
            HTTP_SEND_REQUEST_FLAG_MORE_DATA = 0x00000001,
            HTTP_PROPERTY_FLAG_PRESENT = 0x00000001,
            HTTP_INITIALIZE_SERVER = 0x00000001,
            HTTP_INITIALIZE_CBT = 0x00000004,
            HTTP_SEND_RESPONSE_FLAG_OPAQUE = 0x00000040,
        }

        const int HttpHeaderRequestMaximum = (int)HttpRequestHeader.UserAgent + 1;
        const int HttpHeaderResponseMaximum = (int)HttpResponseHeader.WwwAuthenticate + 1;

        internal static class HTTP_REQUEST_HEADER_ID
        {
            internal static string ToString(int position)
            {
                return m_Strings[position];
            }

            private static string[] m_Strings = {
                    "Cache-Control",
                    "Connection",
                    "Date",
                    "Keep-Alive",
                    "Pragma",
                    "Trailer",
                    "Transfer-Encoding",
                    "Upgrade",
                    "Via",
                    "Warning",

                    "Allow",
                    "Content-Length",
                    "Content-Type",
                    "Content-Encoding",
                    "Content-Language",
                    "Content-Location",
                    "Content-MD5",
                    "Content-Range",
                    "Expires",
                    "Last-Modified",

                    "Accept",
                    "Accept-Charset",
                    "Accept-Encoding",
                    "Accept-Language",
                    "Authorization",
                    "Cookie",
                    "Expect",
                    "From",
                    "Host",
                    "If-Match",

                    "If-Modified-Since",
                    "If-None-Match",
                    "If-Range",
                    "If-Unmodified-Since",
                    "Max-Forwards",
                    "Proxy-Authorization",
                    "Referer",
                    "Range",
                    "Te",
                    "Translate",
                    "User-Agent",
                };
        }

        internal static class HTTP_RESPONSE_HEADER_ID
        {
            private static Hashtable m_Hashtable;

            static HTTP_RESPONSE_HEADER_ID()
            {
                m_Hashtable = new Hashtable((int)Enum.HttpHeaderResponseMaximum);
                for (int i = 0; i < (int)Enum.HttpHeaderResponseMaximum; i++)
                {
                    m_Hashtable.Add(m_Strings[i], i);
                }
            }

            internal static int IndexOfKnownHeader(string HeaderName)
            {
                object index = m_Hashtable[HeaderName];
                return index == null ? -1 : (int)index;
            }

            internal static string ToString(int position)
            {
                return m_Strings[position];
            }

            internal enum Enum
            {
                HttpHeaderCacheControl = 0,    // general-header [section 4.5]
                HttpHeaderConnection = 1,    // general-header [section 4.5]
                HttpHeaderDate = 2,    // general-header [section 4.5]
                HttpHeaderKeepAlive = 3,    // general-header [not in rfc]
                HttpHeaderPragma = 4,    // general-header [section 4.5]
                HttpHeaderTrailer = 5,    // general-header [section 4.5]
                HttpHeaderTransferEncoding = 6,    // general-header [section 4.5]
                HttpHeaderUpgrade = 7,    // general-header [section 4.5]
                HttpHeaderVia = 8,    // general-header [section 4.5]
                HttpHeaderWarning = 9,    // general-header [section 4.5]

                HttpHeaderAllow = 10,   // entity-header  [section 7.1]
                HttpHeaderContentLength = 11,   // entity-header  [section 7.1]
                HttpHeaderContentType = 12,   // entity-header  [section 7.1]
                HttpHeaderContentEncoding = 13,   // entity-header  [section 7.1]
                HttpHeaderContentLanguage = 14,   // entity-header  [section 7.1]
                HttpHeaderContentLocation = 15,   // entity-header  [section 7.1]
                HttpHeaderContentMd5 = 16,   // entity-header  [section 7.1]
                HttpHeaderContentRange = 17,   // entity-header  [section 7.1]
                HttpHeaderExpires = 18,   // entity-header  [section 7.1]
                HttpHeaderLastModified = 19,   // entity-header  [section 7.1]


                // Response Headers

                HttpHeaderAcceptRanges = 20,   // response-header [section 6.2]
                HttpHeaderAge = 21,   // response-header [section 6.2]
                HttpHeaderEtag = 22,   // response-header [section 6.2]
                HttpHeaderLocation = 23,   // response-header [section 6.2]
                HttpHeaderProxyAuthenticate = 24,   // response-header [section 6.2]
                HttpHeaderRetryAfter = 25,   // response-header [section 6.2]
                HttpHeaderServer = 26,   // response-header [section 6.2]
                HttpHeaderSetCookie = 27,   // response-header [not in rfc]
                HttpHeaderVary = 28,   // response-header [section 6.2]
                HttpHeaderWwwAuthenticate = 29,   // response-header [section 6.2]

                HttpHeaderResponseMaximum = 30,


                HttpHeaderMaximum = 41
            }

            private static string[] m_Strings = {
                    "Cache-Control",
                    "Connection",
                    "Date",
                    "Keep-Alive",
                    "Pragma",
                    "Trailer",
                    "Transfer-Encoding",
                    "Upgrade",
                    "Via",
                    "Warning",

                    "Allow",
                    "Content-Length",
                    "Content-Type",
                    "Content-Encoding",
                    "Content-Language",
                    "Content-Location",
                    "Content-MD5",
                    "Content-Range",
                    "Expires",
                    "Last-Modified",

                    "Accept-Ranges",
                    "Age",
                    "ETag",
                    "Location",
                    "Proxy-Authenticate",
                    "Retry-After",
                    "Server",
                    "Set-Cookie",
                    "Vary",
                    "WWW-Authenticate",
                };
        }

        private static HTTPAPI_VERSION version;
        private static volatile bool extendedProtectionSupported;

        //
        // This property is used by HttpListener to pass the version structure to the native layer in API
        // calls. 
        //
        internal static HTTPAPI_VERSION Version
        {
            get
            {
                return version;
            }
        }

        //
        // This property is used by HttpListener to get the Api version in use so that it uses appropriate 
        // Http APIs.
        //
        internal static HTTP_API_VERSION ApiVersion
        {
            get
            {
                if (version.HttpApiMajorVersion == 2 && version.HttpApiMinorVersion == 0)
                {
                    return HTTP_API_VERSION.Version20;
                }
                else if (version.HttpApiMajorVersion == 1 && version.HttpApiMinorVersion == 0)
                {
                    return HTTP_API_VERSION.Version10;
                }
                else
                {
                    return HTTP_API_VERSION.Invalid;
                }
            }
        }

        //
        // returns 'true' if http.sys supports CBT: either the system is Win7+, or http.sys was patched.
        //
        internal static bool ExtendedProtectionSupported
        {
            get
            {
                return extendedProtectionSupported;
            }
        }

        static HttpApi()
        {
            InitHttpApi(2, 0);
        }

        private static void InitHttpApi(ushort majorVersion, ushort minorVersion)
        {
            version.HttpApiMajorVersion = majorVersion;
            version.HttpApiMinorVersion = minorVersion;

           // GlobalLog.Print("HttpApi::.ctor() calling HttpApi.HttpInitialize() for Version " + majorVersion + "." + minorVersion);

            // For pre-Win7 OS versions, we need to check whether http.sys contains the CBT patch.
            // We do so by passing HTTP_INITIALIZE_CBT flag to HttpInitialize. If the flag is not 
            // supported, http.sys is not patched. Note that http.sys will return invalid parameter
            // also on Win7, even though it shipped with CBT support. Therefore we must not pass
            // the flag on Win7 and later.
            uint statusCode = ErrorCodes.ERROR_SUCCESS;
            extendedProtectionSupported = true;

            if (ComNetOS.IsWin7orLater)
            {
                // on Win7 and later, we don't pass the CBT flag. CBT is always supported.
                statusCode = HttpApi.HttpInitialize(version, (uint)HTTP_FLAGS.HTTP_INITIALIZE_SERVER, null);
            }
            else
            {
                statusCode = HttpApi.HttpInitialize(version,
                    (uint)(HTTP_FLAGS.HTTP_INITIALIZE_SERVER | HTTP_FLAGS.HTTP_INITIALIZE_CBT), null);

                // if the status code is INVALID_PARAMETER, http.sys does not support CBT.
                if (statusCode == ErrorCodes.ERROR_INVALID_PARAMETER)
                {
                    if (Logging.On) Logging.PrintWarning(Logging.HttpListener, SR.GetString(SR.net_listener_cbt_not_supported));

                    // try again without CBT flag: HttpListener can still be used, but doesn't support EP
                    extendedProtectionSupported = false;
                    statusCode = HttpApi.HttpInitialize(version, (uint)HTTP_FLAGS.HTTP_INITIALIZE_SERVER, null);
                }
            }

            supported = statusCode == ErrorCodes.ERROR_SUCCESS;

            GlobalLog.Print("HttpApi::.ctor() call to HttpApi.HttpInitialize() returned:" + statusCode + " supported:" + supported);
        }

        static volatile bool supported;
        internal static bool Supported
        {
            get
            {
                return supported;
            }
        }

        // Server API

        internal static WebHeaderCollection GetHeaders(byte[] memoryBlob, IntPtr originalAddress)
        {
            GlobalLog.Enter("HttpApi::GetHeaders()");

            // Return value.
            WebHeaderCollection headerCollection = new WebHeaderCollection(WebHeaderCollectionType.HttpListenerRequest);
            fixed (byte* pMemoryBlob = memoryBlob)
            {
                HTTP_REQUEST* request = (HTTP_REQUEST*)pMemoryBlob;
                long fixup = pMemoryBlob - (byte*)originalAddress;
                int index;

                // unknown headers
                if (request->Headers.UnknownHeaderCount != 0)
                {
                    HTTP_UNKNOWN_HEADER* pUnknownHeader = (HTTP_UNKNOWN_HEADER*)(fixup + (byte*)request->Headers.pUnknownHeaders);
                    for (index = 0; index < request->Headers.UnknownHeaderCount; index++)
                    {
                        // For unknown headers, when header value is empty, RawValueLength will be 0 and 
                        // pRawValue will be null.
                        if (pUnknownHeader->pName != null && pUnknownHeader->NameLength > 0)
                        {
                            string headerName = new string(pUnknownHeader->pName + fixup, 0, pUnknownHeader->NameLength);
                            string headerValue;
                            if (pUnknownHeader->pRawValue != null && pUnknownHeader->RawValueLength > 0)
                            {
                                headerValue = new string(pUnknownHeader->pRawValue + fixup, 0, pUnknownHeader->RawValueLength);
                            }
                            else
                            {
                                headerValue = string.Empty;
                            }
                            headerCollection.AddInternal(headerName, headerValue);
                        }
                        pUnknownHeader++;
                    }
                }

                // known headers
                HTTP_KNOWN_HEADER* pKnownHeader = &request->Headers.KnownHeaders;
                for (index = 0; index < HttpHeaderRequestMaximum; index++)
                {
                    // For known headers, when header value is empty, RawValueLength will be 0 and 
                    // pRawValue will point to empty string ("\0")
                    if (pKnownHeader->pRawValue != null)
                    {
                        string headerValue = new string(pKnownHeader->pRawValue + fixup, 0, pKnownHeader->RawValueLength);
                        headerCollection.AddInternal(HTTP_REQUEST_HEADER_ID.ToString(index), headerValue);
                    }
                    pKnownHeader++;
                }
            }

            GlobalLog.Leave("HttpApi::GetHeaders()");
            return headerCollection;
        }

        private static string GetKnownHeader(HTTP_REQUEST* request, long fixup, int headerIndex)
        {
            GlobalLog.Enter("HttpApi::GetKnownHeader()");
            string header = null;

            HTTP_KNOWN_HEADER* pKnownHeader = (&request->Headers.KnownHeaders) + headerIndex;
            GlobalLog.Print("HttpApi::GetKnownHeader() pKnownHeader:0x" + ((IntPtr)pKnownHeader).ToString("x"));
            GlobalLog.Print("HttpApi::GetKnownHeader() pRawValue:0x" + ((IntPtr)pKnownHeader->pRawValue).ToString("x") + " RawValueLength:" + pKnownHeader->RawValueLength.ToString());
            // For known headers, when header value is empty, RawValueLength will be 0 and 
            // pRawValue will point to empty string ("\0")
            if (pKnownHeader->pRawValue != null)
            {
                header = new string(pKnownHeader->pRawValue + fixup, 0, pKnownHeader->RawValueLength);
            }

            GlobalLog.Leave("HttpApi::GetKnownHeader() return:" + ValidationHelper.ToString(header));
            return header;
        }

        internal static string GetKnownHeader(HTTP_REQUEST* request, int headerIndex)
        {
            return GetKnownHeader(request, 0, headerIndex);
        }

        internal static string GetKnownHeader(byte[] memoryBlob, IntPtr originalAddress, int headerIndex)
        {
            fixed (byte* pMemoryBlob = memoryBlob)
            {
                return GetKnownHeader((HTTP_REQUEST*)pMemoryBlob, pMemoryBlob - (byte*)originalAddress, headerIndex);
            }
        }

        private unsafe static string GetVerb(HTTP_REQUEST* request, long fixup)
        {
            GlobalLog.Enter("HttpApi::GetVerb()");
            string verb = null;

            if ((int)request->Verb > (int)HTTP_VERB.HttpVerbUnknown && (int)request->Verb < (int)HTTP_VERB.HttpVerbMaximum)
            {
                verb = HttpVerbs[(int)request->Verb];
            }
            else if (request->Verb == HTTP_VERB.HttpVerbUnknown && request->pUnknownVerb != null)
            {
                verb = new string(request->pUnknownVerb + fixup, 0, request->UnknownVerbLength);
            }

            GlobalLog.Leave("HttpApi::GetVerb() return:" + ValidationHelper.ToString(verb));
            return verb;
        }

        internal unsafe static string GetVerb(HTTP_REQUEST* request)
        {
            return GetVerb(request, 0);
        }

        internal unsafe static string GetVerb(byte[] memoryBlob, IntPtr originalAddress)
        {
            fixed (byte* pMemoryBlob = memoryBlob)
            {
                return GetVerb((HTTP_REQUEST*)pMemoryBlob, pMemoryBlob - (byte*)originalAddress);
            }
        }

        internal static HTTP_VERB GetKnownVerb(byte[] memoryBlob, IntPtr originalAddress)
        {
            GlobalLog.Enter("HttpApi::GetKnownVerb()");

            // Return value.
            HTTP_VERB verb = HTTP_VERB.HttpVerbUnknown;
            fixed (byte* pMemoryBlob = memoryBlob)
            {
                HTTP_REQUEST* request = (HTTP_REQUEST*)pMemoryBlob;
                if ((int)request->Verb > (int)HTTP_VERB.HttpVerbUnparsed && (int)request->Verb < (int)HTTP_VERB.HttpVerbMaximum)
                {
                    verb = request->Verb;
                }
            }

            GlobalLog.Leave("HttpApi::GetKnownVerb()");
            return verb;
        }

        internal static uint GetChunks(byte[] memoryBlob, IntPtr originalAddress, ref int dataChunkIndex, ref uint dataChunkOffset, byte[] buffer, int offset, int size)
        {
            GlobalLog.Enter("HttpApi::GetChunks() memoryBlob:" + ValidationHelper.ToString(memoryBlob));

            // Return value.
            uint dataRead = 0;
            fixed (byte* pMemoryBlob = memoryBlob)
            {
                HTTP_REQUEST* request = (HTTP_REQUEST*)pMemoryBlob;
                long fixup = pMemoryBlob - (byte*)originalAddress;

                if (request->EntityChunkCount > 0 && dataChunkIndex < request->EntityChunkCount && dataChunkIndex != -1)
                {
                    HTTP_DATA_CHUNK* pDataChunk = (HTTP_DATA_CHUNK*)(fixup + (byte*)&request->pEntityChunks[dataChunkIndex]);

                    fixed (byte* pReadBuffer = buffer)
                    {
                        byte* pTo = &pReadBuffer[offset];

                        while (dataChunkIndex < request->EntityChunkCount && dataRead < size)
                        {
                            if (dataChunkOffset >= pDataChunk->BufferLength)
                            {
                                dataChunkOffset = 0;
                                dataChunkIndex++;
                                pDataChunk++;
                            }
                            else
                            {
                                byte* pFrom = pDataChunk->pBuffer + dataChunkOffset + fixup;

                                uint bytesToRead = pDataChunk->BufferLength - (uint)dataChunkOffset;
                                if (bytesToRead > (uint)size)
                                {
                                    bytesToRead = (uint)size;
                                }
                                for (uint i = 0; i < bytesToRead; i++)
                                {
                                    *(pTo++) = *(pFrom++);
                                }
                                dataRead += bytesToRead;
                                dataChunkOffset += bytesToRead;
                            }
                        }
                    }
                }
                //we're finished.
                if (dataChunkIndex == request->EntityChunkCount)
                {
                    dataChunkIndex = -1;
                }
            }

            GlobalLog.Leave("HttpApi::GetChunks()");
            return dataRead;
        }

        internal static IPEndPoint GetRemoteEndPoint(byte[] memoryBlob, IntPtr originalAddress)
        {
            GlobalLog.Enter("HttpApi::GetRemoteEndPoint()");

            SocketAddress v4address = new SocketAddress(AddressFamily.InterNetwork, SocketAddress.IPv4AddressSize);
            SocketAddress v6address = new SocketAddress(AddressFamily.InterNetworkV6, SocketAddress.IPv6AddressSize);

            fixed (byte* pMemoryBlob = memoryBlob)
            {
                HTTP_REQUEST* request = (HTTP_REQUEST*)pMemoryBlob;
                IntPtr address = request->Address.pRemoteAddress != null ? (IntPtr)(pMemoryBlob - (byte*)originalAddress + (byte*)request->Address.pRemoteAddress) : IntPtr.Zero;
                CopyOutAddress(address, ref v4address, ref v6address);
            }

            IPEndPoint endpoint = null;
            if (v4address != null)
            {
                endpoint = IPEndPoint.Any.Create(v4address) as IPEndPoint;
            }
            else if (v6address != null)
            {
                endpoint = IPEndPoint.IPv6Any.Create(v6address) as IPEndPoint;
            }

            GlobalLog.Leave("HttpApi::GetRemoteEndPoint()");
            return endpoint;
        }
 
         
    }


    class SafeLocalFree : SafeHandleZeroOrMinusOneIsInvalid
    { 
        private const int LMEM_FIXED = 0;
        private const int NULL = 0;

        // This returned handle cannot be modified by the application.
        public static SafeLocalFree Zero = new SafeLocalFree(false);

        private SafeLocalFree() : base(true) { }

        private SafeLocalFree(bool ownsHandle) : base(ownsHandle) { }

        public static SafeLocalFree LocalAlloc(int cb)
        {
            SafeLocalFree result = SafeNetHandles.LocalAlloc(LMEM_FIXED, (UIntPtr)cb);
            if (result.IsInvalid)
            {
                result.SetHandleAsInvalid();
                throw new OutOfMemoryException();
            }
            return result;
        }

        override protected bool ReleaseHandle()
        {
            return UnsafeNclNativeMethods.SafeNetHandles.LocalFree(handle) == IntPtr.Zero;
        }
    }


    [System.Security.SuppressUnmanagedCodeSecurityAttribute()]
    internal static class SafeNetHandles
    {
 
        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern SafeLocalFree LocalAlloc(int uFlags, UIntPtr sizetdwBytes);
         
    }


}
