using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Octokit;
using Octokit.Internal;
using Octokit.Reactive;

class Program
{
    static string owner = "https://api.github.com/orgs/gmallick9090/repos";
    static string name = "gmallick9090";

    static InMemoryCredentialStore credentials = new InMemoryCredentialStore(new Credentials("your-token-here"));
    static ObservableGitHubClient client = new ObservableGitHubClient(new ProductHeaderValue("ophion"), credentials);

    static void Main(string[] args)
    {
        var request = new PullRequestRequest();
        var results = new Dictionary<string, List<int>>();

        // fetch all open pull requests
        client.PullRequest.GetAllForRepository(owner, name, request)
            .SelectMany(pr =>
            {
                
                return client.PullRequest.Files(owner, name, pr.Number)
                    .Select(file => Tuple.Create(pr.Number, file.FileName));
            })
            .Subscribe(data =>
            {
                if (results.ContainsKey(data.Item2))
                {
                    results[data.Item2].Add(data.Item1);
                }
                else
                {
                    var list = new List<int> { data.Item1 };
                    results[data.Item2] = list;
                }
            },
                ex =>
                {
                    Console.WriteLine("Exception found: " + ex.ToString());
                },
                () =>
                {
                    var sortbyPopularity = results
                        .OrderByDescending(kvp => kvp.Value.Count);

                    foreach (var file in sortbyPopularity)
                    {
                        Console.WriteLine("File: {0}", file.Key);

                        foreach (var id in file.Value)
                        {
                            Console.WriteLine(" - PR: {0}", id);
                        }

                        Console.WriteLine();
                    }
                });

        Console.ReadLine();
    }
}