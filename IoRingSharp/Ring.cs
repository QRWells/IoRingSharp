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
        if (CreateIoRing(
                IORING_VERSION.IORING_VERSION_2,
                new IORING_CREATE_FLAGS(),
                sqEntries,
                cqEntries, ref handle) != 0)
            throw new Win32Exception(Marshal.GetLastWin32Error());

        _handle = new RingHandle(handle);
    }

    public void Dispose()
    {
        _handle.Dispose();

        GC.SuppressFinalize(this);
    }

    public IORING_INFO GetIoRingInfo()
    {
        var info = new IORING_INFO();
        if (KernelBase.GetIoRingInfo(_handle.DangerousGetHandle(), ref info) != 0)
            throw new Win32Exception(Marshal.GetLastWin32Error());

        return info;
    }

    public void ReadFile(FileStream file, Span<byte> buffer)
    {
        var handle = file.SafeFileHandle.DangerousGetHandle();
        unsafe
        {
            fixed (void* ptr = buffer)
            {
                var bufferRef = new IORING_BUFFER_REF((nint)ptr);
                if (BuildIoRingReadFile(
                        _handle.DangerousGetHandle(),
                        new IORING_HANDLE_REF(handle),
                        bufferRef, (uint)buffer.Length, 0, 0, 0) != 0)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }

    public void Submit()
    {
        SubmitIoRing(_handle.DangerousGetHandle(), 0, 0, null);
    }

    public void SubmitAndWait(uint waitOps)
    {
        SubmitIoRing(_handle.DangerousGetHandle(), waitOps, 0, null);
    }
}