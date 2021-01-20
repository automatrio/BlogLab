using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using BlogLab.Models.Account;
using BlogLab.Models.Blog;
using BlogLab.Models.Photo;
using BlogLab.Repository;
using BlogLab.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogLab.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotoController : ControllerBase
    {
        private readonly IPhotoRepository _photoRepository;
        private readonly IBlogRepository _blogRepository;
        private readonly IPhotoService _photoService;
        
        public PhotoController(
            IPhotoRepository photoRepository, 
            IBlogRepository blogRepository,
            IPhotoService photoService
            )
            {
                _photoRepository = photoRepository;
                _blogRepository = blogRepository;
                _photoService = photoService;
            }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Photo>> UploadPhoto(IFormFile file)
        {
            // User* is a property of the abstract class ControllerBase from which this one inherits,
            // it stores a ClaimsPrincipal object.
            //
            // ClaimsPrincipal(claimsIdentity) -> ClaimsIdentity(claims) -> IList<Claim> claims -> Claim
            //
            // The token we created earlier,
            /*
                public string CreateToken(ApplicationUserIdentity user)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(JwtRegisteredClaimNames.NameId, user.ApplicationUserId.ToString()), // key-value pair
                        new Claim(JwtRegisteredClaimNames.UniqueName, user.Username)
                    };
            */
            // contains a key-value pair, consisting of a standardized key (provided by JwtRegisteredClaimNames);
            // the line below intends to retrieve the UserId stored in the token by reading the first value in
            // the list of Claims it contains. the First() LINQ method checks if the Claim type (key) is indeed
            // a UserId (Jwt...NameId), then returns its value in the form of a Claim object. The value is taken
            // by means of the .Value property of the selected Claim, and then converted to integer.

            int applicationUserId = int.Parse(User.Claims.First(i => i.Type == JwtRegisteredClaimNames.NameId).Value);

            var uploadResult = await _photoService.AddPhotoAsync(file);

            if (uploadResult.Error != null) return BadRequest(uploadResult.Error.Message);

            var photoCreate = new PhotoCreate()
            {
                PublicId = uploadResult.PublicId,
                // URL is the server address, URN is the resource's name, and URI is the whole thing + protocol.
                ImageUrl = uploadResult.SecureUrl.AbsoluteUri,
                Description = file.FileName
            };

            Photo newPhoto = await _photoRepository.InsertAsync(photoCreate, applicationUserId);

            return Ok(newPhoto);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<Photo>>> GetByApplicationUserId()
        {
            int applicationUserId =  int.Parse(  User.Claims.First(i => i.Type == JwtRegisteredClaimNames.NameId).Value  );

            List<Photo> photos = await _photoRepository.GetAllByUserIdAsync(applicationUserId);

            return Ok(photos);
        }

        [HttpGet("{photoId}")]
        public async Task<ActionResult<Photo>> Get(int photoId)
        {
            Photo photo = await _photoRepository.GetAsync(photoId);

            return Ok(photo);
        }

        [Authorize]
        [HttpDelete("{photoId}")]
        public async Task<ActionResult<int>> Delete(int photoId)
        {
            int applicationUserId = int.Parse(User.Claims.First(i => i.Type == JwtRegisteredClaimNames.NameId).Value);

            Photo foundPhoto = await _photoRepository.GetAsync(photoId);

            if (foundPhoto != null)
            {
                if (foundPhoto.ApplicationUserId == applicationUserId) // "Are you the user who created this photo?"
                {
                    List<Blog> blogs = await _blogRepository.GetAllByUserIdAsync(applicationUserId);

                    var photoUsedInBlog = blogs.Any(b => b.PhotoId == photoId); // checking if photo is currently in use

                    if (photoUsedInBlog) return BadRequest("Cannont remove a photo being used in published blog(s).");

                    var deleteResult = await _photoService.DeletePhotoAsync(foundPhoto.PublicId); // deleting from Cloudinary

                    if (deleteResult.Error != null) return BadRequest(deleteResult.Error.Message);

                    var affectRows = await _photoRepository.DeleteAsync(foundPhoto.PhotoId); // deleting from database

                    return Ok(affectRows); // returns the amount of photos deleted (shouldn't it be just one?)
                }
                else
                {
                    return BadRequest("Photo wasn't uploaded by current user.");
                }
            }

            return BadRequest("Photo doesn't exist.");
        }
    }
}