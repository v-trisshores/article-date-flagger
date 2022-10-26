using System.Diagnostics.Metrics;
using System.Text.RegularExpressions;

namespace ConsoleApp1
{
    internal class Program
    {
        static readonly string s_repoRootFolderPath = @"D:\Repos\OfficeDocs-Support-pr";
        static readonly string s_productRootFolderPath = @"Exchange";
        static readonly int s_articleYearsOlderThan = 2;

        static void Main()
        {
            List<ArticleInfo> flaggedArticles = new();

            string[] articlePaths = Directory.GetFiles(Path.Combine(s_repoRootFolderPath, s_productRootFolderPath), "*.md", SearchOption.AllDirectories);
            foreach (var articlePath in articlePaths)
            {
                // Get ms.date
                string content = File.ReadAllText(articlePath);
                Match m = Regex.Match(content, @"ms\.date:.*?(?<flagDate>\d\d?/\d\d?/\d\d\d\d)", RegexOptions.Singleline);
                if (!m.Success) 
                    continue;

                string[] flagDateSegments = m.Groups["flagDate"].Value.Split("/");
                DateOnly flagDate = new(int.Parse(flagDateSegments[2]), int.Parse(flagDateSegments[0]), int.Parse(flagDateSegments[1]));
                
                // Check whether ms.date is older than specified years.
                if (flagDate > DateOnly.FromDateTime(DateTime.Now.AddYears(-s_articleYearsOlderThan)))
                    continue;

                // Get title.
                m = Regex.Match(content, @"title:(?<title>.*?)\n", RegexOptions.Multiline);
                if (!m.Success)
                    continue;
                string title = m.Groups["title"].Value.Trim(new[] { ' ', '\r', '\n' });

                ArticleInfo ai = new(flagDate, title, articlePath);// Path.GetRelativePath(s_repoRootFolderPath, articlePath));
                flaggedArticles.Add(ai);
            }

            int counter = 1;
            flaggedArticles.ForEach(x => x.PrintDetails(counter++));

            Console.Write("\r\nPress any key to exit");
            Console.ReadKey();
        }
    }

    internal class ArticleInfo
    {
        internal string Title;
        internal DateOnly FlagDate;
        internal string FilePath;

        internal ArticleInfo(DateOnly flagDate, string title, string filePath)
        {
            FlagDate = flagDate;
            Title = title;
            FilePath = filePath;
        }

        internal void PrintDetails(int index)
        {
            Console.WriteLine($"{index}. {FlagDate:MM/dd/yyyy}, {Title}");
        }
    }
}