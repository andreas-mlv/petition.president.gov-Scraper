namespace ScrapePetitionVoters
{
    public class Vote
    {
        public int VoteId { get; set; }
        public string VoterName { get; set; }
        public string VoteDate { get; set; }
    }

    public class ScrapeResult
    {
        public string Id { get; set; }
        public string Link
        {
            get
            {
                return $"https://petition.president.gov.ua/petition/{Id}";
            }
        }
        public int TotalPages { get; set; }
        public int TotalVotes { get; set; }
        public List<Vote> Votes { get; set; }

        public string Scraper = "https://github.com/andreas-mlv/petition.president.gov-Scraper";
        public string Author = "Andrii Malyava";
    }

    public class JsonPageResult
    {
        public string Table_html { get; set; }
        public string Pag_html { get; set; }
    }
}
