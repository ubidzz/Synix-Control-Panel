using System.Text;

namespace Synix_Control_Panel.SynixApp.ServerHandler;

/// <summary>Windows argv parsing/serialization for native processes; never cmd.exe syntax.</summary>
internal static class WindowsLaunchArguments
{
    internal static IReadOnlyList<string> Parse(string commandLine)
    {
        if (commandLine.IndexOfAny(['\0', '\r', '\n']) >= 0)
            throw new ArgumentException(LocalizationManager.Get("Validation.ExtraArguments.LineBreaks"));
        List<string> arguments = [];
        int index = 0;
        while (index < commandLine.Length)
        {
            while (index < commandLine.Length && commandLine[index] is ' ' or '\t') index++;
            if (index == commandLine.Length) break;
            StringBuilder value = new();
            bool quoted = false;
            while (index < commandLine.Length && (quoted || commandLine[index] is not (' ' or '\t')))
            {
                int slashes = 0;
                while (index < commandLine.Length && commandLine[index] == '\\') { slashes++; index++; }
                if (index < commandLine.Length && commandLine[index] == '"')
                {
                    value.Append('\\', slashes / 2);
                    if (slashes % 2 != 0) value.Append('"');
                    else if (quoted && index + 1 < commandLine.Length && commandLine[index + 1] == '"')
                    { value.Append('"'); index++; }
                    else quoted = !quoted;
                    index++;
                }
                else
                {
                    value.Append('\\', slashes);
                    if (index < commandLine.Length && (quoted || commandLine[index] is not (' ' or '\t')))
                        value.Append(commandLine[index++]);
                }
            }
            if (quoted) throw new ArgumentException(LocalizationManager.Get("Validation.ExtraArguments.UnclosedQuote"));
            arguments.Add(value.ToString());
        }
        return arguments;
    }

    internal static string Quote(string argument)
    {
        if (argument.IndexOfAny(['\0', '\r', '\n']) >= 0)
            throw new ArgumentException(LocalizationManager.Get("Validation.ExtraArguments.LineBreaks"));
        if (argument.Length > 0 && argument.IndexOfAny([' ', '\t', '"']) < 0) return argument;
        StringBuilder result = new("\"");
        int slashes = 0;
        foreach (char character in argument)
        {
            if (character == '\\') { slashes++; continue; }
            result.Append('\\', character == '"' ? slashes * 2 + 1 : slashes);
            result.Append(character);
            slashes = 0;
        }
        result.Append('\\', slashes * 2);
        return result.Append('"').ToString();
    }
}
