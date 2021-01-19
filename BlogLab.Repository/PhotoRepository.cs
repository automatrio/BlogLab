using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BlogLab.Models.Photo;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace BlogLab.Repository
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly IConfiguration _config; // allows us to read "appsettings.json"

        public PhotoRepository(IConfiguration config)
        {
            _config = config;
        }

        public async Task<int> DeleteAsync(int photoId)
        {
            int affectedRows = 0;
            
            using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                affectedRows = await connection.ExecuteAsync(
                    "dbo.Photo_Delete",
                    new {@PhotoId = photoId},
                    commandType: CommandType.StoredProcedure
                );
            }

            return affectedRows;
        }

        public async Task<List<Photo>> GetAllByUserIdAsync(int ApplicationUserId)
        {
            IEnumerable<Photo> photos;

            using(SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                photos = await connection.QueryAsync<Photo>(
                    "dbo.Photo_GetByUserId",
                    new { @ApplicationUserId = ApplicationUserId },
                    commandType: CommandType.StoredProcedure
                );


            }

            return photos.ToList();
        }

        public async Task<Photo> GetAsync(int photoId)
        {
            Photo photo;

            using(SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                photo = await connection.QueryFirstOrDefaultAsync<Photo>(
                    "dbo.Photo_Get",
                    new { @PhotoId = photoId},
                    commandType: CommandType.StoredProcedure
                );
            }
            return photo;
        }

        public async Task<Photo> InsertAsync(PhotoCreate photoCreate, int applicationUserId)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("ImageUrl", typeof(string));
            dataTable.Columns.Add("PublicId", typeof(string));
            dataTable.Columns.Add("Description", typeof(string));

            dataTable.Rows.Add(photoCreate.ImageUrl);
            dataTable.Rows.Add(photoCreate.PublicId);
            dataTable.Rows.Add(photoCreate.Description);

            int newPhotoId;
            
            using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                newPhotoId = await connection.ExecuteScalarAsync<int>(
                    "Photo_Insert",
                    new {
                        @ApplicationUserId = applicationUserId,
                        @Photo = dataTable.AsTableValuedParameter("PhotoType")
                    },
                    commandType: CommandType.StoredProcedure);
            }

            Photo photo = await GetAsync(newPhotoId);

            return photo;
        }
    }
}