using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace WebCrawler
{
    class Program
    {
        static Queue<Uri> frontier = new Queue<Uri>();
        static Dictionary<string, bool> visitedUrls = new Dictionary<string, bool>();

        static void Main(string[] args)
        {
            Console.Write("Enter an URL: ");
            var urlStr = Console.ReadLine();

            Console.Write("Number of links: ");
            int linksCount = Convert.ToInt32(Console.ReadLine());

            Uri initialUrl = new UriBuilder(urlStr).Uri;
            frontier.Enqueue(initialUrl);

            Crawl(linksCount);

            foreach (KeyValuePair<string, bool> kv in visitedUrls)
                Console.WriteLine("{0,-10} {1}", kv.Value, kv.Key);
        }

        private static void Crawl(int maxLinks)
        {
            while (frontier.Count > 0 && visitedUrls.Count < maxLinks)
            {
                Uri url = frontier.Dequeue();
                if (!visitedUrls.ContainsKey(url.ToString()))
                    VisitPage(url);
            }
        }

        private static void VisitPage(Uri url)
        {
            visitedUrls.Add(url.ToString(), true);
            try
            {
                Uri hostUrl = new UriBuilder(url.Host).Uri;

                WebClient wc = new WebClient();
                string webPage = wc.DownloadString(url);

                var urlTagPattern = new Regex(@"<a.*?href=[""'](?<url>.*?)[""'].*?>(?<name>.*?)</a>", RegexOptions.IgnoreCase);

                var hrefPattern = new Regex("href\\s*=\\s*(?:\"(?<1>[^\"]*)\"|(?<1>\\S+))", RegexOptions.IgnoreCase);

                var links = urlTagPattern.Matches(webPage);
                foreach (Match href in links)
                {
                    string newUrl = hrefPattern.Match(href.Value).Groups[1].Value;
                    Uri absoluteUrl = NormalizeUrl(hostUrl, newUrl);
                    if (absoluteUrl != null && absoluteUrl.Scheme == Uri.UriSchemeHttp || absoluteUrl.Scheme == Uri.UriSchemeHttps)
                    {
                        if (! visitedUrls.ContainsKey(absoluteUrl.ToString()))
                            frontier.Enqueue(absoluteUrl);
                    }
                }
            }
            catch
            {
                visitedUrls[url.ToString()] = false;
            }

        }

        static Uri NormalizeUrl(Uri hostUrl, string url)
        {
            return Uri.TryCreate(hostUrl, url, out Uri absoluteUrl) ? absoluteUrl : null;
        }
    }
}
