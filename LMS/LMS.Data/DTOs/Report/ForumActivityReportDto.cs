using System;

namespace LMS.Web.Repositories.DTOs
{
    public class ForumActivityReportDto
    {
        public int ForumId { get; set; }
        public string ForumTitle { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int TotalTopics { get; set; }
        public int TotalPosts { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalViews { get; set; }
        public DateTime LastActivityDate { get; set; }
        public string MostActiveUser { get; set; } = string.Empty;
        public int MostActiveUserPosts { get; set; }
        public string MostPopularTopic { get; set; } = string.Empty;
        public int MostPopularTopicReplies { get; set; }
        public double AveragePostsPerUser { get; set; }
        public double AverageRepliesPerTopic { get; set; }
    }
}