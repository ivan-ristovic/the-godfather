namespace TheGodfather.Services.Common;

public static class ContentPathResolver
{
    public static string Resolve(string path)
    {
        if (Path.IsPathRooted(path) || File.Exists(path) || Directory.Exists(path))
            return path;

        string outputPath = Path.Combine(AppContext.BaseDirectory, path);
        return File.Exists(outputPath) || Directory.Exists(outputPath) ? outputPath : path;
    }
}
