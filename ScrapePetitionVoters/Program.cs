using Newtonsoft.Json;

namespace ScrapePetitionVoters
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string basePath = @"E:\MOUNTED\Desktop\";
            var scraper = new Scraper();

            var petitionId = 216794;
            var result = scraper.Scrape(petitionId);

            var date = DateTime.Now.ToString("G");
            date = date.Replace("/", "_");
            date = date.Replace(":", "_");
            date = date.Replace(" ", "-");

            string fileName = $"{petitionId}_{date}.json";
            var json = JsonConvert.SerializeObject(result);

            string fullPath = Path.Join(basePath, fileName);
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            File.WriteAllText(fullPath, json);
            while (true)
            {
                Console.ReadKey();
            }
        }
    }
}
