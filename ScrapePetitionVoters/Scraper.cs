using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace ScrapePetitionVoters
{
    public class Scraper
    {
        private string PetitionApi = "https://petition.president.gov.ua/petition";
        private HttpClient httpClient;
        private int RequestTimeOut = 100;

        public Scraper()
        {
            SetUpClient();
        }

        private void SetUpClient()
        {
            httpClient = new HttpClient();
            // Set user agent to mozilla
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:121.0) Gecko/20100101 Firefox/121.0");
        }

        private bool CheckIfPageExists(int petitionId, int page)
        {
            var fullURL = $"{PetitionApi}/{petitionId}/votes/{page}/json";
            // if page not exists it will redirect to https://petition.president.gov.ua/404
            var response = httpClient.GetAsync(fullURL).Result;
            // check dest url
            var destUrl = response.RequestMessage.RequestUri.ToString();
            return destUrl != "https://petition.president.gov.ua/404";
        }

        private int GetVotePages(int petitionId)
        {
            var regex = new Regex(@"get_voters_page\('(\d*)'\)");

            var fullURL = $"{PetitionApi}/{petitionId}/votes/1/json";
            var response = httpClient.GetAsync(fullURL).Result;
            var json = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<JsonPageResult>(json);

            // find all matches
            var matches = regex.Matches(result.Pag_html);
            // get all group from matches
            var groups = matches.Select(m => m.Groups[1].Value);

            int maxPage = 0;
            foreach (var group in groups)
            {
                var page = int.Parse(group);
                if (page > maxPage)
                {
                    maxPage = page;
                }
            }

            while (!CheckIfPageExists(petitionId, maxPage))
            {
                maxPage--;
            }

            return maxPage;

        }

        private List<Vote> ParseVotes(string html)
        {
            // in each class table_row you get table_cell number, table_cell name and table_cell date.
            // You need to parse it
            var result = new List<Vote>();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var rows = doc.DocumentNode.SelectNodes("//div[@class='table_row']");
            foreach (var row in rows)
            {
                var numberCell = row.SelectNodes(".//div[@class='table_cell number']");
                var nameCell = row.SelectNodes(".//div[@class='table_cell name']");
                var dateCell = row.SelectNodes(".//div[@class='table_cell date']");

                int number = 0;
                string name = "";
                string date = "";

                if (numberCell != null)
                {
                    string sNumber = numberCell[0].InnerText.Replace(".", "");
                    number = int.Parse(sNumber);
                }
                if (nameCell != null)
                {
                    name = nameCell[0].InnerText;
                }
                if (dateCell != null)
                {
                    date = dateCell[0].InnerText;
                }
                var vote = new Vote
                {
                    VoteId = number,
                    VoterName = name,
                    VoteDate = date
                };
                result.Add(vote);
            }

            return result;
        }

        private List<Vote> GetVotesFromPage(int petitionId, int page)
        {
            var result = new List<Vote>();

            var fullURL = $"{PetitionApi}/{petitionId}/votes/{page}";
            var response = httpClient.GetAsync(fullURL).Result;
            var html = response.Content.ReadAsStringAsync().Result;

            result = ParseVotes(html);

            return result;
        }

        private int CalculateTimeLeft(int pages)
        {
            return RequestTimeOut * pages / 1000;
        }

        public ScrapeResult Scrape(int petitionId)
        {
            var result = new ScrapeResult();
            result.Id = petitionId.ToString();
            result.Votes = new List<Vote>();
            result.TotalPages = GetVotePages(petitionId);

            for (int page = 1; page <= result.TotalPages; page++)
            {
                int secLeft = CalculateTimeLeft(result.TotalPages - page);
                int progress = page * 100 / result.TotalPages;
                string info = $"[{progress}%] Parsing {page}/{result.TotalPages} pages. {secLeft} sec left";

                Console.Clear();
                Console.WriteLine($"{info}");

                var votes = GetVotesFromPage(petitionId, page);
                result.Votes.AddRange(votes);
                Thread.Sleep(RequestTimeOut);
            }
            result.TotalVotes = result.Votes.Count;

            return result;
        }

    }
}
