using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using BlogLab.Models.Blog;
using BlogLab.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogLab.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly IBlogRepository _blogRepository;
        private readonly IPhotoRepository _photoRepository;
        
        public BlogController(IBlogRepository blogRepository, IPhotoRepository photoRepository)
        {
            _blogRepository = blogRepository;
            _photoRepository = photoRepository;
        }


        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Blog>> Create(BlogCreate blogCreate)
        {
            int applicationUserId = int.Parse(User.Claims.First(i => i.Type == JwtRegisteredClaimNames.NameId).Value);

            if (blogCreate.PhotoId.HasValue)
            {
                var photo = await _photoRepository.GetAsync(blogCreate.PhotoId.Value);  // load the photo

                if (photo.ApplicationUserId != applicationUserId) // Preventing malicious users from upload other users' photos
                {
                    return BadRequest("You did not upload the photo.");
                }
            }

            var blog = await _blogRepository.UpsertAsync(blogCreate, applicationUserId);

            return Ok(blog);
        }


        // Think of it like this: http://localhost:5000/api/Blog/?PageSize=10&Param2=12
        [HttpGet("{blogId}")]
        public async Task<ActionResult<PagedResults<Blog>>> GetAll([FromQuery] BlogPaging blogPaging) // See above for [FromQuery]
        {
            PagedResults<Blog> blogs = await _blogRepository.GetAllAsync(blogPaging);

            return Ok(blogs);
        }

        [HttpGet("{blogId}")]
        public async Task<ActionResult<Blog>> Get(int blogId)
        {
            Blog blog = await _blogRepository.GetAsync(blogId);

            return Ok(blog);
        }

        [HttpGet("user/{applicationUserId}")]
        public async Task<ActionResult<List<Blog>>> GetAllFamous()
        {
            List<Blog> famousBlogs = await _blogRepository.GetAllFamousAsync();

            return Ok(famousBlogs);
        }

        [HttpGet("user/{applicationUserId}")]
        public async Task<ActionResult<List<Blog>>> GetByApplicationUserId(int applicationUserId)
        {
            List<Blog> blogs = await _blogRepository.GetAllByUserIdAsync(applicationUserId);

            return Ok(blogs);
        }

        [Authorize]
        [HttpDelete("{blogId}")]
        public async Task<ActionResult<int>> Delete(int blogId)
        {
            int applicationUserId = int.Parse(User.Claims.First(i => i.Type == JwtRegisteredClaimNames.NameId).Value);
            
            var foundblog = await _blogRepository.GetAsync(blogId);

            if (foundblog != null)
            {
                if (foundblog.ApplicationUserId != applicationUserId) return BadRequest("You did not create this blog.");

                int affectedRows = await _blogRepository.DeleteAsync(foundblog.BlogId);

                return Ok(affectedRows);
            }
            else
            {
                return BadRequest("Blog not found.");
            }
        }

    }
}