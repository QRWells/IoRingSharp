using System.Text;
using IoRingSharp.Win32;

namespace IoRingSharp.Test;

public class Tests
{
    [Test]
    public void Test1()
    {
        var capabilities = Ring.GetIoRingCapabilities();

        if (capabilities.MaxVersion is KernelBase.IoRingVersion.IoRingVersionInvalid)
        {
            Assert.Pass("IoRing is not supported");
        }
        
        using var ring = new Ring(4, 4);

        Assert.That(ring, Is.Not.Null);

        var opcodes = ring.GetSupportedOpCodes();
        
        if (!opcodes.Contains(KernelBase.IoRingOpCode.IoRingOpRead))
        {
            Assert.Pass("ReadFixed is not supported");
        }

        var file = File.Create("test.txt");
        const string str = "Hello World!";
        file.Write(Encoding.UTF8.GetBytes(str));
        file.Close();

        var buffer = new byte[16];
        file = File.Open("test.txt", FileMode.Open, FileAccess.Read, FileShare.Read);
        ring.ReadFile(file, buffer);
        ring.Submit();

        var len = Array.IndexOf(buffer, (byte)0);
        var result = Encoding.UTF8.GetString(buffer, 0, len);
        Assert.That(result, Is.EqualTo(str));
        file.Close();

        File.Delete("test.txt");
    }
}