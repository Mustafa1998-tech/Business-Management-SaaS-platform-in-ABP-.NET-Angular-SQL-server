using System.IO;

namespace SaasSystem.Configuration;

public static class DotEnvLoader
{
    public static string? LoadFromCurrentDirectory(bool overwriteExisting = false)
    {
        string? envFilePath = FindNearestEnvFile(Directory.GetCurrentDirectory());
        if (envFilePath is null)
        {
            return null;
        }

        LoadFile(envFilePath, overwriteExisting);
        return envFilePath;
    }

    private static string? FindNearestEnvFile(string startDirectory)
    {
        DirectoryInfo? directory = new(startDirectory);
        while (directory is not null)
        {
            string candidatePath = Path.Combine(directory.FullName, ".env");
            if (File.Exists(candidatePath))
            {
                return candidatePath;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static void LoadFile(string filePath, bool overwriteExisting)
    {
        foreach (string rawLine in File.ReadLines(filePath))
        {
            string line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith("export ", StringComparison.OrdinalIgnoreCase))
            {
                line = line[7..].Trim();
            }

            int separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            string key = line[..separatorIndex].Trim();
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            string value = line[(separatorIndex + 1)..].Trim();
            if (value.Length >= 2 &&
                ((value.StartsWith('"') && value.EndsWith('"')) ||
                 (value.StartsWith('\'') && value.EndsWith('\''))))
            {
                value = value[1..^1];
            }

            if (!overwriteExisting && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key)))
            {
                continue;
            }

            Environment.SetEnvironmentVariable(key, value);
        }
    }
}

