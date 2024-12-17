using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LINQProjects
{
    internal class Program
    {
        static void Main()
        {
            Prison prison = new Prison();
            prison.Work();
        }
    }

    public class Prison
    {
        private readonly Manager _manager;

        public Prison()
        {
            _manager = new Manager();
        }

        public void Work()
        {
            List<Prisoner> prisoners = _manager.Fill();

            ShowStatus(prisoners);

            Console.WriteLine($"\nPrisoners under what articles should be released? " +
                $"(separated by \"{_manager.SeparationSign}\" or interval separated by \"{_manager.IntervalSign}\")");

            bool isValidUserInput;

            do
            {
                string userInput = Console.ReadLine();
                isValidUserInput = _manager.TryConvertArticleNumbers(userInput, out List<string> excludedArticles);

                if (isValidUserInput)
                {
                    Console.WriteLine($"\nPrisoners under the following articles will be amnestied and released:");
                    Console.Write(string.Join(", ", excludedArticles));

                    prisoners = _manager.Release(prisoners, excludedArticles);
                }
                else
                {
                    Console.WriteLine("Article numbers entered incorrectly, please try again...");
                }
            } while (isValidUserInput == false);

            ShowStatus(prisoners);

            Console.WriteLine("Press something..");
            Console.ReadKey();
        }

        private void ShowStatus(List<Prisoner> prisoners)
        {
            const string ShowPrisonersCommand = "1";

            Console.WriteLine($"There are {prisoners.Count} prisoners in the prison.");
            Console.WriteLine($"They have the following articles:{_manager.GetAllPrisonerArticles(prisoners)}");
            Console.WriteLine($"If you want to see all prisoners, press {ShowPrisonersCommand}");
            string userInput = Console.ReadLine();

            if (userInput == ShowPrisonersCommand)
            {
                _manager.ShowPrisoners(prisoners);
            }
        }
    }

    public class Manager
    {
        public Manager()
        {
            SeparationSign = ',';
            IntervalSign = '-';
        }

        public char SeparationSign { get; }
        public char IntervalSign { get; }

        public List<Prisoner> Fill()
        {
            IPrisonersFactory _factory = new PrisonersFactory();
            List<Prisoner> prisoners = _factory.Create();

            return prisoners;
        }

        public void ShowPrisoners(List<Prisoner> prisoners)
        {
            if (prisoners.Count > 0)
            {
                foreach (Prisoner prisoner in prisoners)
                {
                    Console.WriteLine($"Prisoner {prisoner.Name} \tarticle:{prisoner.Article}");
                }
            }
            else
            {
                Console.WriteLine("Prison is empty..");
            }
        }

        public string GetAllPrisonerArticles(List<Prisoner> prisoners)
        {
            return string.Join(", ", prisoners.Select(prisoner => prisoner.Article).Distinct().OrderBy(article => article));
        }

        public List<Prisoner> Release(List<Prisoner> prisoners, List<string> excludedArticles)
        {
            foreach (string article in excludedArticles)
            {
                prisoners = prisoners.Where(prisoner => prisoner.Article != article).ToList();
            }

            return prisoners;
        }

        public bool TryConvertArticleNumbers(string articleNumbers, out List<string> articles)
        {
            bool isValid = true;
            articles = new List<string>();

            List<string> articleList = articleNumbers.Split(SeparationSign).ToList();

            for (int i = 0; i < articleList.Count; i++)
            {
                if (articleList[i].Contains(IntervalSign))
                {
                    List<string> interval = ProcessInterval(articleList[i], out isValid).ToList();

                    if (isValid)
                    {
                        articles.AddRange(interval);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    isValid = int.TryParse(articleList[i], out int value);

                    if (isValid)
                    {
                        articles.Add(value.ToString());
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            articles = articles.Distinct().ToList();
            articles.Sort();

            for (int i = 0; i < articles.Count; i++)
            {
                articles[i] = $"article N{articles[i]}";
            }

            return isValid;
        }

        private List<string> ProcessInterval(string element, out bool isValid)
        {
            isValid = true;
            List<string> articles = new List<string>();

            List<string> articleRange = element.Split(IntervalSign).ToList();

            bool haveInitial = int.TryParse(articleRange.First(), out int initialValue);
            bool haveFinal = int.TryParse(articleRange.Last(), out int finalValue);

            int countValue = 2;

            if (articleRange.Count == countValue && haveInitial && haveFinal && initialValue > 0 && finalValue > 0)
            {
                for (int i = initialValue; i != finalValue;)
                {
                    articles.Add(i.ToString());
                    i = initialValue < finalValue ? i + 1 : i - 1;
                }

                articles.Add(finalValue.ToString());
            }
            else
            {
                isValid = false;
            }

            return articles;
        }
    }

    public class Prisoner
    {
        public Prisoner(string name, string article)
        {
            Name = name;
            Article = article;
        }

        public string Name { get; private set; }
        public string Article { get; private set; }
    }

    public interface IPrisonersFactory
    {
        List<Prisoner> Create();
    }

    public class PrisonersFactory : IPrisonersFactory
    {
        private readonly string _nameFile;
        private readonly string _surnameFile;

        private readonly IArticleFactory _articleFactory;
        private readonly Random _random;

        public PrisonersFactory()
        {
            _nameFile = "Resources/Names.txt";
            _surnameFile = "Resources/Surnames.txt";
            _articleFactory = new ArticleFactory();
            _random = new Random();
        }

        public List<Prisoner> Create()
        {
            List<Prisoner> prisoners = new List<Prisoner>();
            List<string> names = File.ReadAllLines(_nameFile).ToList();
            List<string> surnames = File.ReadAllLines(_surnameFile).ToList();

            int prisonersCount = 10;

            for (int i = 0; i < prisonersCount; i++)
            {
                string name = GetStringValue(names);
                string surname = GetStringValue(surnames);
                string article = _articleFactory.Create();

                prisoners.Add(new Prisoner($"{name} {surname}", article));
            }

            return prisoners.ToList();
        }

        private string GetStringValue(List<string> list)
        {
            string value = list[_random.Next(list.Count)];

            return value;
        }
    }

    public interface IArticleFactory
    {
        string Create();
    }

    public class ArticleFactory : IArticleFactory
    {
        private readonly Random _random = new Random();

        public string Create()
        {
            int articleCount = 15;
            int firstArticleNumber = 1;

            return $"article N{_random.Next(firstArticleNumber, articleCount + 1)}";
        }
    }
}