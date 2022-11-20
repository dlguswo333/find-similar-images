class MainProgram
{
    static void Main(string[] args)
    {
        // Display the number of command line arguments.
        Console.WriteLine(args.Length > 0 ? string.Join(" ", args): "Hello world!");
    }
}
