// See https://aka.ms/new-console-template for more information
using SharedLibrary.Models;

Console.WriteLine("Hello, World!");

while(true)
{
    await Task.Delay(1000);
    Console.WriteLine($"Total: {TweetResponse.TweentTotalCount}");
}
