using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BlogLab.Models.Blog;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace BlogLab.Repository
{
    public class BlogRepository : IBlogRepository
    {
        private readonly IConfiguration _config;

        public BlogRepository(IConfiguration config)
        {
            _config = config;
        }

        public async Task<int> DeleteAsync(int blogId)
        {
            int affectedRows = 0;

            using(SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                affectedRows = await connection.ExecuteAsync(
                    "dbo.Blog_Delete",
                    new { @BlogId = blogId },
                    commandType: CommandType.StoredProcedure
                );
            }

            return affectedRows;
        }

        public async Task<PagedResults<Blog>> GetAllAsync(BlogPaging blogPaging)
        {
            var results = new PagedResults<Blog>();

            using(SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                using(var multi = await connection.QueryMultipleAsync(
                    "dbo.Blog_GetAll",
                    new {
                        @Offset = (blogPaging.Page - 1) * blogPaging.PageSize, // Start from
                        @PageSize = blogPaging.PageSize // Up to
                    },
                    commandType: CommandType.StoredProcedure))
                {
                    results.Items = multi.Read<Blog>();

                    results.TotalCount = multi.ReadFirst<int>();
                }
            }

            return results;
        }

        public async Task<List<Blog>> GetAllByUserIdAsync(int applicationUserId)
        {
            IEnumerable<Blog> blogs;

            using(SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                blogs = await connection.QueryAsync<Blog>(
                    "dbo.Blog_GetByUserId",
                    new { @ApplicationUserId = applicationUserId },
                    commandType: CommandType.StoredProcedure
                );
            }

            return blogs.ToList();
        }

        public async Task<List<Blog>> GetAllFamousAsync()
        {
            IEnumerable<Blog> famousBlogs;

            using(SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                famousBlogs = await connection.QueryAsync<Blog>(
                    "dbo.Blog_GetAllFamous",
                    commandType: CommandType.StoredProcedure
                );
            }

            return famousBlogs.ToList();
        }

        public async Task<Blog> GetAsync(int blogId)
        {
            Blog blog;

            using(SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                blog = await connection.QueryFirstOrDefaultAsync<Blog>(
                    "dbo.Blog_Get",
                    new { @BlogId = blogId},
                    commandType: CommandType.StoredProcedure
                );
            }

            return blog;
        }

        public async Task<Blog> UpsertAsync(BlogCreate blogCreate, int applicationUserId)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("BlogId", typeof(int));
            dataTable.Columns.Add("Title", typeof(string));
            dataTable.Columns.Add("Content", typeof(string));
            dataTable.Columns.Add("PhotoId", typeof(int));

            dataTable.Rows.Add(blogCreate.BlogId, blogCreate.Title, blogCreate.Content);

            int? newBlogId; // only if exists

            using(SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                newBlogId = await connection.ExecuteScalarAsync<int?>(
                    "dbo.Blog_Upsert",
                    new {
                        @Blog = dataTable.AsTableValuedParameter("dbo.BlogType"),
                        @ApplicationUserId = applicationUserId
                    },
                    commandType: CommandType.StoredProcedure
                );
            }

            newBlogId = newBlogId ?? blogCreate.BlogId; // If it's an insert, we'll get the newBlogId; otherwise, the updated blog BlogId.

            Blog blog = await GetAsync(newBlogId.Value);

            return blog;
        }
    }
}