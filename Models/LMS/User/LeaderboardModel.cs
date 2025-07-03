namespace LMS.Models.User
{
    public class LeaderboardModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Period { get; set; } = string.Empty;

        public string? CourseName { get; set; }

        public List<LeaderboardEntryModel> Entries { get; set; } = new();
    }
}
