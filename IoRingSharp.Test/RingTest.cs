using System.Text;

namespace IoRingSharp.Test;

public class Tests
{
    [Test]
    public void Test1()
    {
        using var ring = new Ring(4, 4);

        Assert.That(ring, Is.Not.Null);

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