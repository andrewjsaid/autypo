using Autypo;
using Autypo.Configuration;

const int fuzziness = 4; // This is a large number for demo purposes only. No user types this badly!

string[] allowedCommands = ["create", "read", "update", "delete"];

IAutypoComplete autypoComplete = await AutypoFactory.CreateCompleteAsync(config => config
    .WithDataSource(allowedCommands)
    .WithIndex(c => c, index => index.WithFuzziness(fuzziness)));

string? userInput;
do
{
    Console.Write("Which command do you want to execute? Options are create, read, update or delete: ");
    userInput = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(userInput))
    {
        continue;
    }
    if (userInput == "exit")
    {
        break;
    }

    if (allowedCommands.Contains(userInput))
    {
        Console.WriteLine($"You chose: {userInput}");
    }
    else
    {
        IEnumerable<string> results = autypoComplete.Complete(userInput);
        string? suggestion = results.FirstOrDefault();
        if (suggestion is not null)
        {
            Console.WriteLine($"Did you mean: {suggestion}");
        }
        else
        {
            Console.WriteLine("Unrecognized command. Please try again.");
        }
    }

    Console.WriteLine(); // spacing
}
while (userInput is not null);
