using dotenv.net;

public static class EnvConfig
{
    public static void LoadEnv()
    {
        var projectDir = Directory.GetCurrentDirectory();
        var rootDir = Path.Combine(projectDir, "..");
        var envPath = Path.Combine(rootDir, ".env");

        DotEnv.Fluent()
            .WithEnvFiles(envPath)
            .WithOverwriteExistingVars()
            .Load();
    }
}
