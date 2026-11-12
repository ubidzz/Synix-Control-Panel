// A test-only stdin echo process. It never evaluates or executes received text.
using System.Text;
Console.InputEncoding = new UTF8Encoding(false);
Console.OutputEncoding = new UTF8Encoding(false);
while (Console.ReadLine() is { } line)
{
    Console.WriteLine("received:" + line);
}
