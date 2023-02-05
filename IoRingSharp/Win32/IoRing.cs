using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace IoRingSharp.Win32;

public static partial class KernelBase
{
    [Flags]
    public enum IoRingCreateAdvisoryFlags
    {
        IoRingCreateAdvisoryFlagsNone = 0
    }

    // Flags to configure the kernel behavior of an IoRing. The implementation will
    // fail the create call if it does not understand any of the required flags and
    // ignores any advisory flags that it does not understand.
    [Flags]
    public enum IoRingCreateRequiredFlags
    {
        IoRingCreateRequiredFlagsNone = 0
    }

    public enum IoRingVersion
    {
        IoRingVersionInvalid = 0,
        IoRingVersion1,

        /// <summary>Minor update</summary>
        /// <remarks>
        ///     Fixes a bug where user provided completion event may not be signaled
        ///     even if the completion queue transitions from empty to non-empty because
        ///     of a race condition. In earlier version please do a timed wait to work
        ///     around this issue.
        /// </remarks>
        IoRingVersion2
    }

    private const string KernelBaseDll = "KernelBase.dll";

    /// <summary>
    ///     Create an IoRing.
    /// </summary>
    /// <param name="ioRingVersion">The version of the IoRing to create.</param>
    /// <param name="flags">Flags to configure the kernel behavior of an IoRing.</param>
    /// <param name="submissionQueueSize">Size of the submission queue.</param>
    /// <param name="completionQueueSize">Size of the completion queue.</param>
    /// <param name="handle">Handle to the IoRing.</param>
    /// <returns></returns>
    [LibraryImport(KernelBaseDll, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial int CreateIoRing(
        IoRingVersion ioRingVersion,
        IoRingCreateFlags flags,
        uint submissionQueueSize,
        uint completionQueueSize,
        ref nint handle
    );

    [LibraryImport(KernelBaseDll, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial int CloseIoRing(nint h);

    [LibraryImport(KernelBaseDll, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial int GetIoRingInfo(nint h, ref IoRingInfo info);

    [LibraryImport(KernelBaseDll, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial int SubmitIoRing(
        nint h,
        uint waitOperations,
        uint milliseconds,
        uint[] submittedEntries);

    [LibraryImport(KernelBaseDll, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial int PopIoRingCompletion(nint h, ref IoRingCqe completion);

    [LibraryImport(KernelBaseDll, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial int SetIoRingCompletionEvent(nint h, nint hEvent);

    [LibraryImport(KernelBaseDll, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static unsafe partial int BuildIoRingCancelRequest(
        nint ioRing,
        IoRingHandleRef file,
        uint* opToCancel,
        uint* userData);

    /// <summary>
    ///     Builds a submission queue entry for IORING_OP_READ
    /// </summary>
    /// <param name="ioRing"></param>
    /// <param name="fileRef"></param>
    /// <param name="dataRef"></param>
    /// <param name="numberOfBytesToRead"></param>
    /// <param name="fileOffset"></param>
    /// <param name="userData"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    [LibraryImport(KernelBaseDll, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static unsafe partial int BuildIoRingReadFile(
        nint ioRing,
        IoRingHandleRef fileRef,
        IoRingBufferRef dataRef,
        uint numberOfBytesToRead,
        ulong fileOffset,
        nuint userData,
        IoRingSqeFlags flags);

    /// <summary>
    ///     Builds a submission queue entry for IORING_OP_REGISTER_FILES
    /// </summary>
    /// <param name="ioRing"></param>
    /// <param name="count"></param>
    /// <param name="handles"></param>
    /// <param name="userData"></param>
    /// <returns></returns>
    [LibraryImport(KernelBaseDll, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static unsafe partial int BuildIoRingRegisterFileHandles(
        nint ioRing,
        uint count,
        nint[] handles,
        nuint userData
    );

    /// <summary>
    ///     Builds a submission queue entry for IORING_OP_REGISTER_BUFFERS
    /// </summary>
    /// <param name="ioRing"></param>
    /// <param name="count"></param>
    /// <param name="buffers"></param>
    /// <param name="userData"></param>
    /// <returns></returns>
    [LibraryImport(KernelBaseDll, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static unsafe partial int BuildIoRingRegisterBuffers(
        nint ioRing,
        uint count,
        IoRingBufferInfo[] buffers,
        nuint userData
    );

    [LibraryImport(KernelBaseDll, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial int QueryIoRingCapabilities(ref IoRingCapabilities capabilities);

    [LibraryImport(KernelBaseDll, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial int IsIoRingOpSupported(nint ioRing, IoRingOpCode op);

    [StructLayout(LayoutKind.Explicit)]
    internal struct IoRingHandleRef
    {
        public IoRingHandleRef(nint h)
        {
            Kind = IoRingRefKind.IoRingRefRaw;
            Handle = h;
        }

        public IoRingHandleRef(uint index)
        {
            Kind = IoRingRefKind.IoRingRefRegistered;
            Index = index;
        }

        [FieldOffset(0)] private readonly IoRingRefKind Kind;
        [FieldOffset(8)] private readonly nint Handle;
        [FieldOffset(8)] private readonly uint Index;
    }

    internal struct IoRingRegisteredBuffer
    {
        public IoRingRegisteredBuffer(uint index, uint offset)
        {
            _bufferIndex = index;
            _offset = offset;
        }

        // Index of pre-registered buffer
        private uint _bufferIndex;

        // Offset into the pre-registered buffer
        private uint _offset;
    }

    internal unsafe struct IoRingBufferInfo
    {
        private void* _address;
        private uint _length;

        public IoRingBufferInfo(Memory<byte> buffer)
        {
            _address = buffer.Pin().Pointer;
            _length = (uint)buffer.Length;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct IoRingBufferRef
    {
        public IoRingBufferRef(nint address)
        {
            Kind = IoRingRefKind.IoRingRefRaw;
            Address = address;
        }

        public IoRingBufferRef(IoRingRegisteredBuffer registeredBuffer)
        {
            Kind = IoRingRefKind.IoRingRefRegistered;
            IndexAndOffset = registeredBuffer;
        }

        public IoRingBufferRef(uint index, uint offset)
            : this(new IoRingRegisteredBuffer(index, offset))
        {
        }

        [FieldOffset(0)] private readonly IoRingRefKind Kind;
        [FieldOffset(8)] private readonly nint Address;
        [FieldOffset(8)] private readonly IoRingRegisteredBuffer IndexAndOffset;
    }

    [Flags]
    internal enum IoRingSqeFlags
    {
        IoSqeFlagsNone = 0
    }

    public unsafe struct IoRingCqe
    {
        public uint* _userData;
        public int _resultCode;
        public ulong* _information;
    }

    public struct IoRingCreateFlags
    {
        public IoRingCreateRequiredFlags Required = IoRingCreateRequiredFlags.IoRingCreateRequiredFlagsNone;
        public IoRingCreateAdvisoryFlags Advisory = IoRingCreateAdvisoryFlags.IoRingCreateAdvisoryFlagsNone;

        public IoRingCreateFlags()
        {
        }
    }

    public struct IoRingInfo
    {
        public IoRingVersion IoRingVersion;
        public IoRingCreateFlags Flags;
        public uint SubmissionQueueSize;
        public uint CompletionQueueSize;
    }

    /// <summary>
    ///     Flags indicating functionality supported by a given implementation
    /// </summary>
    [Flags]
    public enum IoRingFeatureFlags
    {
        /// <summary>No specific functionality for the implementation</summary>
        IoRingFeatureFlagsNone = 0,

        /// <summary>
        ///     IoRing support is emulated in User Mode (not directly supported by KM)
        /// </summary>
        /// <remarks>
        ///     When this flag is set there is no underlying kernel support for IoRing.
        ///     However, a user mode emulation layer is available to provide application
        ///     compatibility, without the benefit of kernel support.  This provides
        ///     application compatibility at the expense of performance. Thus, it allows
        ///     apps to make a choice at run-time.
        /// </remarks>
        IoRingFeatureUmEmulation = 0x00000001,

        /// <summary>
        ///     If this flag is present the SetIoRingCompletionEvent API is available
        ///     and supported
        /// </summary>
        IoRingFeatureSetCompletionEvent = 0x00000002
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IoRingCapabilities
    {
        public IoRingVersion _maxVersion;
        public uint _maxSubmissionQueueSize;
        public uint _maxCompletionQueueSize;
        public IoRingFeatureFlags _featureFlags;
    }

    /// <summary>
    ///     enum used as discriminator for references to resources that
    ///     support preregistration in an IORING
    /// </summary>
    [Flags]
    internal enum IoRingRefKind
    {
        IoRingRefRaw,
        IoRingRefRegistered
    }

    /// <summary>Values for a submission queue entry opcode</summary>
    /// <remarks>
    ///     To maintain the versioning strategy and compatibility, opcodes are never
    ///     re-used.  New values may be added, thus they always increment as new opcodes
    ///     are added.  Some codes may be deprecated and replaced with a new code but
    ///     the actual op code value is never re-used.
    /// </remarks>
    public enum IoRingOpCode
    {
        /// <summary>Do not perform any I/O</summary>
        /// <remarks>
        ///     Useful for testing overhead performance and draining the queue
        /// </remarks>
        IoRingOpNop,

        /// <summary>Read from a file to a buffer</summary>
        IoRingOpRead,

        /// <summary>Registers an array of file HANDLEs with the IoRing</summary>
        /// <remarks>
        ///     If any existing registration exists, it is completely replaced by the
        ///     registration for this opcode. Any entries in the array with
        ///     INVALID_HANDLE_VALUE are sparse entries, and not used. This allows
        ///     effectively releasing one or more of the previously registered files.
        ///     Unregistration of all current files is accomplished by providing zero
        ///     length array.
        ///     The input array must remain valid until the operation completes. The
        ///     change impacts all entries in the queue after this completes. (E.g.,
        ///     this implicitly has "link" semantics in that any subsequent entry will
        ///     not start until after this is completed)
        /// </remarks>
        IoRingOpRegisterFiles,

        /// <summary>
        ///     Registers an array of IORING_BUFFER_INFO with the IoRing
        /// </summary>
        /// <remarks>
        ///     If any existing registration exists, it is completely replaced by the
        ///     registration for this opcode. Any entries in the array with Address=NULL
        ///     and Length=0 are sparse entries, and not used. This allows effectively
        ///     releasing one or more of the previously registered buffers.
        ///     Unregistration of all current buffers is accomplished by providing zero
        ///     length array.
        ///     The input array must remain valid until the operation completes. The
        ///     change impacts all entries in the queue after this completes. E.g. this
        ///     implicitly has "link" semantics in that any subsequent entry will not
        ///     start until after this completes.
        /// </remarks>
        IoRingOpRegisterBuffers,

        /// <summary>Requests cancellation of a previously submitted operation</summary>
        /// <remarks>
        ///     This attempts to cancel a previously submitted operation. The UserData for the
        ///     operation to cancel is used to identify the operation.
        /// </remarks>
        IoRingOpCancel
    }
}