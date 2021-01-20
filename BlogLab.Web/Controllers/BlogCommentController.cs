using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using BlogLab.Models.Blog;
using BlogLab.Models.Blogcomment;
using BlogLab.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogLab.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogCommentController : ControllerBase
    {
        private readonly IBlogRepository _blogRepository;
        private readonly IBlogCommentRepository _blogCommentRepository;
        private readonly IAccountRepository _accountRepository;

        public BlogCommentController(
            IBlogRepository blogRepository,
            IBlogCommentRepository blogCommentRepository,
            IAccountRepository accountRepository)
        {
            _blogRepository = blogRepository;
            _blogCommentRepository = blogCommentRepository;
            _accountRepository = accountRepository;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<BlogComment>> Create(BlogCommentCreate blogCommentCreate)
        {
            int applicationUserId = int.Parse(User.Claims.First(i => i.Type == JwtRegisteredClaimNames.NameId).Value);

            BlogComment newBlogComment = await _blogCommentRepository.UpsertAsync(blogCommentCreate, applicationUserId);

            return Ok(newBlogComment);
        }

        [HttpGet("{blogId}")]
        public async Task<ActionResult<List<BlogComment>>> GetAll(int blogId)
        {
            List<BlogComment> blogComments = await _blogCommentRepository.GetAllAsync(blogId);

            return Ok(blogComments);
        }

        [Authorize]
        [HttpDelete("{blogCommentId}")]
        public async Task<ActionResult<int>> Delete(int blogCommentId)
        {
            int applicationUserId = int.Parse(User.Claims.First(i => i.Type == JwtRegisteredClaimNames.NameId).Value);

            BlogComment blogCommentToDelete = await _blogCommentRepository.GetAsync(blogCommentId);
            
            if(blogCommentToDelete == null) return BadRequest("Non-existing blog comment.");

            int affectedRows = 0;

            if(blogCommentToDelete.ApplicationUserId == applicationUserId)
            {
                affectedRows = await _blogCommentRepository.DeleteAsync(blogCommentId);
            }
            else
            {
                return BadRequest("You're trying to delete a comment that isn't yours.");
            }

            return Ok(affectedRows);
        }
    }
}
