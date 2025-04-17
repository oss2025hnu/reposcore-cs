using Cocona;

public class Program
{
    public static void Main(string[] args)
    {
        CoconaApp.Run(Commands>(args);
    }
}

public class Commands
{
    public void Run(
        [Argument] string[] repository,
        [Option("vervose",'v', Description = "자세한 로그 출력을 활성화합니다.")]
    {
        Console.WriteLine($"Repository: {string.Join("\n",repository)}");

        if (verbose)
        {
            Console.WriteLine("Verbose mode is enabled.");
        }
    }
}
