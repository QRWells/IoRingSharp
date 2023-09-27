using System.ComponentModel;
using System.Runtime.InteropServices;
using IoRingSharp.Win32;
using static IoRingSharp.Win32.KernelBase;

namespace IoRingSharp;

public sealed class Ring : IDisposable
{
    private readonly RingHandle _handle;

    public Ring(uint sqEntries, uint cqEntries)
    {
        var handle = nint.Zero;
        var capabilities = GetIoRingCapabilities();
        var version = capabilities.MaxVersion switch
        {
            IoRingVersion.IoRingVersionInvalid => throw new NotSupportedException("IoRing is not supported"),
            _ => capabilities.MaxVersion
        };

        if (CreateIoRing(
                version,
                new IoRingCreateFlags(),
                sqEntries,
                cqEntries, ref handle) != 0)
            throw new Win32Exception(Marshal.GetLastWin32Error());

        _handle = new RingHandle(handle);
    }

    public void Dispose()
    {
        _handle.Dispose();
    }

    public IoRingInfo GetIoRingInfo()
    {
        var info = new IoRingInfo();
        if (KernelBase.GetIoRingInfo(_handle.DangerousGetHandle(), ref info) != 0)
            throw new Win32Exception(Marshal.GetLastWin32Error());

        return info;
    }

    public List<IoRingOpCode> GetSupportedOpCodes()
    {
        var opCodes = new List<IoRingOpCode>();
        for (var i = 0; i < 32; i++)
        {
            if (IsIoRingOpSupported(_handle.DangerousGetHandle(), (IoRingOpCode)i) == 0)
                continue;
            opCodes.Add((IoRingOpCode)i);
        }

        return opCodes;
    }

    public static IoRingCapabilities GetIoRingCapabilities()
    {
        var capabilities = new IoRingCapabilities();
        if (QueryIoRingCapabilities(ref capabilities) != 0)
            throw new Win32Exception(Marshal.GetLastWin32Error());

        return capabilities;
    }

    public void RegisterFiles(params FileStream[] files)
    {
        if (files.Length == 0)
            return;
        var fileHandles = new nint[files.Length];
        for (var i = 0; i < files.Length; i++)
            fileHandles[i] = files[i].SafeFileHandle.DangerousGetHandle();
        if (BuildIoRingRegisterFileHandles(
                _handle.DangerousGetHandle(),
                (uint)files.Length,
                fileHandles, 0) != 0)
            throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    public void RegisterBuffers(List<Memory<byte>> buffers)
    {
        if (buffers.Count == 0)
            return;
        var bufferRefs = new IoRingBufferInfo[buffers.Count];
        for (var i = 0; i < buffers.Count; i++)
        {
            bufferRefs[i] = new IoRingBufferInfo(buffers[i]);
        }

        if (BuildIoRingRegisterBuffers(
                _handle.DangerousGetHandle(),
                (uint)bufferRefs.Length,
                bufferRefs, 0) != 0)
            throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    /// <summary>
    /// Builds a read operation.
    /// </summary>
    /// <param name="file">File to read from.</param>
    /// <param name="buffer">Buffer to read into.</param>
    /// <exception cref="Win32Exception">Thrown if the operation fails.</exception>
    public void ReadFile(FileStream file, Span<byte> buffer)
    {
        var handle = file.SafeFileHandle.DangerousGetHandle();
        unsafe
        {
            fixed (void* ptr = buffer)
            {
                var bufferRef = new IoRingBufferRef((nint)ptr);
                if (BuildIoRingReadFile(
                        _handle.DangerousGetHandle(),
                        new IoRingHandleRef(handle),
                        bufferRef, (uint)buffer.Length, 0, 0, 0) != 0)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }

    public void SetCompletionEvent(SafeHandle eventHandle)
    {
        if (SetIoRingCompletionEvent(_handle.DangerousGetHandle(), eventHandle.DangerousGetHandle()) != 0)
            throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    public bool TryGetCompletion(out IoRingCqe cqe)
    {
        cqe = default;
        return PopIoRingCompletion(_handle.DangerousGetHandle(), ref cqe) != 0;
    }

    /// <summary>
    /// Submits all operations without blocking.
    /// </summary>
    public void Submit()
    {
        SubmitIoRing(_handle.DangerousGetHandle(), 0, 0, null);
    }

    /// <summary>
    /// Waits for the specified number of operations.
    /// </summary>
    /// <param name="waitOps">Number of operations to wait for.</param>
    public void SubmitAndWait(uint waitOps)
    {
        SubmitIoRing(_handle.DangerousGetHandle(), waitOps, 0, null);
    }

    /// <summary>
    ///     Waits for the specified number of operations and times out after the specified number of milliseconds.
    /// </summary>
    /// <param name="waitOps">Number of operations to wait for.</param>
    /// <param name="timeout">Timeout in milliseconds.</param>
    public void SubmitAndWait(uint waitOps, uint timeout)
    {
        SubmitIoRing(_handle.DangerousGetHandle(), waitOps, timeout, null);
    }
}