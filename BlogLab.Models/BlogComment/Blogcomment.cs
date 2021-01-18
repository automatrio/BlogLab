using System;

namespace BlogLab.Models.Blogcomment
{
    public class BlogComment : BlogCommentCreate
    {
        public string Username { get; set; }
        public int ApplicationUser { get; set; }
        public DateTime PublishDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}