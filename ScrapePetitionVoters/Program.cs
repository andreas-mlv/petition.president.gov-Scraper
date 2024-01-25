using Newtonsoft.Json;

namespace ScrapePetitionVoters
{
    internal class Program
    {
        private static void SaveFile(string petitionId, object result)
        {
            string basePath = @"E:\MOUNTED\Desktop\";
            var date = DateTime.Now.ToString("G");
            date = date.Replace("/", "_");
            date = date.Replace(":", "_");
            date = date.Replace(" ", "-");

            string fileName = $"Petition_{petitionId}_{date}.json";
            var json = JsonConvert.SerializeObject(result);

            string fullPath = Path.Join(basePath, fileName);
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            File.WriteAllText(fullPath, json);
        }

        static void Main(string[] args)
        {
            var scraper = new Scraper();

            var petitionId = 216794;
            var result = scraper.Scrape(petitionId);

            SaveFile(petitionId.ToString(), result);

            while (true)
            {
                Console.ReadKey();
            }
        }
    }
}
