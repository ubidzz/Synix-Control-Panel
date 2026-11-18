// A test-only stdin echo process. It never evaluates or executes received text.
using System.Text;
using System.Text.Json;
Console.InputEncoding = new UTF8Encoding(false);
Console.OutputEncoding = new UTF8Encoding(false);
if (args.FirstOrDefault() == "--synix-argv")
{
    Console.WriteLine(JsonSerializer.Serialize(args.Skip(1)));
    return;
}
if (args.FirstOrDefault() == "--synix-tcp")
{
    await LocalTcpFixture.RunAsync(args[1], args[2], args.ElementAtOrDefault(3));
    return;
}
while (Console.ReadLine() is { } line)
{
    Console.WriteLine("received:" + line);
}
