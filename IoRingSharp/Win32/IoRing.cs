using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS0169

// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
// ReSharper disable InconsistentNaming

namespace IoRingSharp.Win32;

public static partial class KernelBase
{
    [Flags]
    public enum IORING_CREATE_ADVISORY_FLAGS
    {
        IORING_CREATE_ADVISORY_FLAGS_NONE = 0
    }

    // Flags to configure the kernel behavior of an IoRing. The implementation will
    // fail the create call if it does not understand any of the required flags and
    // ignores any advisory flags that it does not understand.
    [Flags]
    public enum IORING_CREATE_REQUIRED_FLAGS
    {
        IORING_CREATE_REQUIRED_FLAGS_NONE = 0
    }

    public enum IORING_VERSION
    {
        IORING_VERSION_INVALID = 0,
        IORING_VERSION_1,

        /// <summary>Minor update</summary>
        /// <remarks>
        ///     Fixes a bug where user provided completion event may not be signaled
        ///     even if the completion queue transitions from empty to non-empty because
        ///     of a race condition. In earlier version please do a timed wait to work
        ///     around this issue.
        /// </remarks>
        IORING_VERSION_2
    }

    private const string KernelBaseDll = "KernelBase.dll";

    [LibraryImport(KernelBaseDll, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial int CreateIoRing(
        IORING_VERSION ioRingVersion,
        IORING_CREATE_FLAGS flags,
        uint submissionQueueSize,
        uint completionQueueSize,
        ref nint h
    );

    [LibraryImport(KernelBaseDll, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial int CloseIoRing(nint h);

    [LibraryImport(KernelBaseDll, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial int GetIoRingInfo(nint h, ref IORING_INFO info);

    [LibraryImport(KernelBaseDll, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial int SubmitIoRing(
        nint h,
        uint waitOperations,
        uint milliseconds,
        uint[] submittedEntries);

    [LibraryImport(KernelBaseDll, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial int PopIoRingCompletion(nint h, ref IORING_CQE completion);

    [LibraryImport(KernelBaseDll, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial int SetIoRingCompletionEvent(nint h, nint hEvent);

    [LibraryImport(KernelBaseDll, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static unsafe partial int BuildIoRingCancelRequest(
        nint ioRing,
        IORING_HANDLE_REF file,
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
        IORING_HANDLE_REF fileRef,
        IORING_BUFFER_REF dataRef,
        uint numberOfBytesToRead,
        ulong fileOffset,
        nuint userData,
        IORING_SQE_FLAGS flags);

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
        IORING_BUFFER_INFO[] buffers,
        nuint userData
    );

    [LibraryImport(KernelBaseDll, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial int QueryIoRingCapabilities(ref IORING_CAPABILITIES capabilities);

    [LibraryImport(KernelBaseDll, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static partial int IsIoRingOpSupported(nint ioRing, IORING_OP_CODE op);

    [StructLayout(LayoutKind.Explicit)]
    internal struct IORING_HANDLE_REF
    {
        public IORING_HANDLE_REF(nint h)
        {
            Kind = IORING_REF_KIND.IORING_REF_RAW;
            Handle = h;
        }

        public IORING_HANDLE_REF(uint index)
        {
            Kind = IORING_REF_KIND.IORING_REF_REGISTERED;
            Index = index;
        }

        [FieldOffset(0)] private readonly IORING_REF_KIND Kind;
        [FieldOffset(8)] private readonly nint Handle;
        [FieldOffset(8)] private readonly uint Index;
    }

    internal struct IORING_REGISTERED_BUFFER
    {
        public IORING_REGISTERED_BUFFER(uint index, uint offset)
        {
            BufferIndex = index;
            Offset = offset;
        }

        // Index of pre-registered buffer
        public uint BufferIndex;

        // Offset into the pre-registered buffer
        public uint Offset;
    }

    internal struct IORING_BUFFER_INFO
    {
        private nint Address;
        private uint Length;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct IORING_BUFFER_REF
    {
        public IORING_BUFFER_REF(nint address)
        {
            Kind = IORING_REF_KIND.IORING_REF_RAW;
            Address = address;
        }

        public IORING_BUFFER_REF(IORING_REGISTERED_BUFFER registeredBuffer)
        {
            Kind = IORING_REF_KIND.IORING_REF_REGISTERED;
            IndexAndOffset = registeredBuffer;
        }

        public IORING_BUFFER_REF(uint index, uint offset)
            : this(new IORING_REGISTERED_BUFFER(index, offset))
        {
        }

        [FieldOffset(0)] public IORING_REF_KIND Kind;
        [FieldOffset(8)] public nint Address;
        [FieldOffset(8)] public IORING_REGISTERED_BUFFER IndexAndOffset;
    }

    [Flags]
    internal enum IORING_SQE_FLAGS
    {
        IOSQE_FLAGS_NONE = 0
    }

    internal unsafe struct IORING_CQE
    {
        private uint* UserData;
        private int ResultCode;
        private ulong* Information;
    }

    public struct IORING_CREATE_FLAGS
    {
        public IORING_CREATE_REQUIRED_FLAGS Required = IORING_CREATE_REQUIRED_FLAGS.IORING_CREATE_REQUIRED_FLAGS_NONE;
        public IORING_CREATE_ADVISORY_FLAGS Advisory = IORING_CREATE_ADVISORY_FLAGS.IORING_CREATE_ADVISORY_FLAGS_NONE;

        public IORING_CREATE_FLAGS()
        {
        }
    }

    public struct IORING_INFO
    {
        public IORING_VERSION IoRingVersion;
        public IORING_CREATE_FLAGS Flags;
        public uint SubmissionQueueSize;
        public uint CompletionQueueSize;
    }

    /// <summary>
    ///     Flags indicating functionality supported by a given implementation
    /// </summary>
    [Flags]
    internal enum IORING_FEATURE_FLAGS
    {
        /// <summary>No specific functionality for the implementation</summary>
        IORING_FEATURE_FLAGS_NONE = 0,

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
        IORING_FEATURE_UM_EMULATION = 0x00000001,

        /// <summary>
        ///     If this flag is present the SetIoRingCompletionEvent API is available
        ///     and supported
        /// </summary>
        IORING_FEATURE_SET_COMPLETION_EVENT = 0x00000002
    }

    internal struct IORING_CAPABILITIES
    {
        private IORING_VERSION MaxVersion;
        private uint MaxSubmissionQueueSize;
        private uint MaxCompletionQueueSize;
        private IORING_FEATURE_FLAGS FeatureFlags;
    }

    /// <summary>
    ///     enum used as discriminator for references to resources that
    ///     support preregistration in an IORING
    /// </summary>
    [Flags]
    internal enum IORING_REF_KIND
    {
        IORING_REF_RAW,
        IORING_REF_REGISTERED
    }

    /// <summary>Values for a submission queue entry opcode</summary>
    /// <remarks>
    ///     To maintain the versioning strategy and compatibility, opcodes are never
    ///     re-used.  New values may be added, thus they always increment as new opcodes
    ///     are added.  Some codes may be deprecated and replaced with a new code but
    ///     the actual op code value is never re-used.
    /// </remarks>
    internal enum IORING_OP_CODE
    {
        /// <summary>Do not perform any I/O</summary>
        /// <remarks>
        ///     Useful for testing overhead performance and draining the queue
        /// </remarks>
        IORING_OP_NOP,

        /// <summary>Read from a file to a buffer</summary>
        IORING_OP_READ,

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
        IORING_OP_REGISTER_FILES,

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
        IORING_OP_REGISTER_BUFFERS,

        /// <summary>Requests cancellation of a previously submitted operation</summary>
        /// <remarks>
        ///     This attempts to cancel a previously submitted operation. The UserData for the
        ///     operation to cancel is used to identify the operation.
        /// </remarks>
        IORING_OP_CANCEL
    }
}