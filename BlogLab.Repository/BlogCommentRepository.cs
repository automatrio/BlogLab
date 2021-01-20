using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BlogLab.Models.Blogcomment;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace BlogLab.Repository
{
    public class BlogCommentRepository : IBlogCommentRepository
    {
        private readonly IConfiguration _config;

        public BlogCommentRepository(IConfiguration config)
        {
            _config = config;
        }

        public async Task<int> DeleteAsync(int blogCommentId)
        {
            int affectedRows = 0;

            using(SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                affectedRows = await connection.ExecuteAsync(
                    "dbo.BlogComment_Delete",
                    new { @BlogCommentId = blogCommentId },
                    commandType: CommandType.StoredProcedure
                );
            }

            return affectedRows;
        }

        public async Task<List<BlogComment>> GetAllAsync(int blogId)
        {
            IEnumerable<BlogComment> blogComments;

            using(SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                blogComments = await connection.QueryAsync<BlogComment>(
                    "dbo.BlogComment_GetAll",
                    new { @BlogId = blogId },
                    commandType: CommandType.StoredProcedure
                );
            }

            return blogComments.ToList();
        }

        public async Task<BlogComment> GetAsync(int blogCommentId)
        {
            BlogComment blogComment;

            using(SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                blogComment = await connection.QueryFirstOrDefaultAsync(
                    "dbo.BlogComment_Get",
                    new { @BlogCommentId = blogCommentId },
                    commandType: CommandType.StoredProcedure
                );
            }

            return blogComment;
        }

        public async Task<BlogComment> UpsertAsync(BlogCommentCreate blogComment, int applicationUserId)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("BlogCommentId");
            dataTable.Columns.Add("ParentBlogCommentId");
            dataTable.Columns.Add("BlogId");
            dataTable.Columns.Add("Content");

            dataTable.Rows.Add(blogComment.BlogCommentId, blogComment.ParentBlogCommentId, blogComment.BlogId, blogComment.Content);

            int? newBlogCommentId;

            using(SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                newBlogCommentId = await connection.ExecuteScalarAsync<int?>(
                    "dbo.BlogComment_Upsert",
                    new {
                        @BlogComment = dataTable.AsTableValuedParameter("BlogCommentType"),
                        @ApplicationUserId = applicationUserId
                    },
                    commandType: CommandType.StoredProcedure
                );
            }

            newBlogCommentId = newBlogCommentId ?? blogComment.BlogCommentId;

            BlogComment newBlogComment = await GetAsync(newBlogCommentId.Value);

            return newBlogComment;
        }
    }
}