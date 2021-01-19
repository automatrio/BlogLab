using System.Collections.Generic;
using System.Threading.Tasks;
using BlogLab.Models.Blogcomment;

namespace BlogLab.Repository
{
    public interface IBlogCommentRepository
    {
        public Task<int> DeleteAsync(int blogCommentId);
        public Task<BlogComment> GetAsync(int blogCommentId);
        public Task<List<BlogComment>> GetAllAsync(int blogId);
        public Task<BlogComment> UpsertAsync(BlogCommentCreate blogComment, int applicationUserId);
    }
}