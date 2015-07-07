using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using RestSharp;

namespace stm32lib
{
    static class Program
    {
        static void Main()
        {
            try
            {
                string url = @"http://stm32f4-discovery.com/2014/05/all-stm32f429-libraries-at-one-place/";
                RestClient client = new RestClient(url);
                RestRequest request = new RestRequest(Method.GET);
                Console.WriteLine("\nExecuting page GET request");
                IRestResponse response = client.Execute(request);
                if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    Console.WriteLine(response.ErrorMessage);
                    Console.WriteLine("Exiting");
                    Environment.Exit(-1);
                }
                Console.WriteLine("Obtained page response");
                Regex regex = new Regex(@"https?://stm32f4-discovery.com/\?wpdmdl=\d+");
                MatchCollection matches = regex.Matches(response.Content);
                Console.Write("Matches found : ");
                Console.WriteLine(matches.Count);
                string[] urlStrings = new string[matches.Count];
                for (int i = 0; i < matches.Count; i++)
                {
                    urlStrings[i] = matches[i].Value;
                }
                urlStrings = urlStrings.Distinct().ToArray();
                string path;
                while (true)
                {
                    Console.Write("Existing DIRECTORY to store : ");
                    path = Console.ReadLine();
                    if (path != null && Directory.Exists(path))
                        break;
                    Console.WriteLine("Directory does not exist");
                }

                for (int i = 0; i < urlStrings.Length; i++)
                {
                    RestClient fileClient = new RestClient(urlStrings[i]);
                    RestRequest fileRequest = new RestRequest(Method.HEAD);
                    Console.Write("Entry " + i);
                    Console.Write(" : req HEAD,");
                    IRestResponse fileResponse = fileClient.Execute(fileRequest);
                    Console.Write(" got HEAD,");
                    Parameter contentHeader =
                        fileResponse.Headers.First(
                            item => string.Equals(item.Name, "Content-disposition",
                                StringComparison.InvariantCultureIgnoreCase));
                    Match fileName = Regex.Match((string)contentHeader.Value, @"\w+\.\w+");
                    WebClient webClient = new WebClient();
                    Console.Write(" start GET,");
                    webClient.DownloadFile(urlStrings[i], Path.Combine(path, fileName.Value));
                    Console.WriteLine(" got GET");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception : " + exception.Message);
                Console.ReadKey();
            }
        }
    }
}
