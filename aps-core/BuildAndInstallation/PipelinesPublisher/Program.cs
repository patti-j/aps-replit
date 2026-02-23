using PipelinesPublisher.Builder;

IBuilder builder;

if (args.Length == 0)
{
    Console.WriteLine("Please enter build type");
    return -1;
}

switch (args[0].ToLower())
{
    case "software":
        builder = new SoftwareBuilder(args);
        break;
    default:
        Console.WriteLine($"Build type '{args[0]}' not found");
        return -1;
}

builder.ValidateArgs(args);
builder.CleanOutputFiles();
builder.Run();

return 0;