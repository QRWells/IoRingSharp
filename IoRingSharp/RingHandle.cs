using IoRingSharp.Win32;
using Microsoft.Win32.SafeHandles;

namespace IoRingSharp;

public sealed class RingHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public RingHandle(nint ring) : base(true)
    {
        SetHandle(ring);
    }

    protected override bool ReleaseHandle()
    {
        return KernelBase.CloseIoRing(handle) == 0;
    }
}