using System;
using System.Threading;
using System.Threading.Tasks;
using BlogLab.Models.Account;
using Microsoft.AspNetCore.Identity;

namespace BlogLab.Repository
{
    public interface IAccountRepository
    {
        public Task<IdentityResult> CreateAsync(ApplicationUserIdentity user,
            CancellationToken cancellationToken); // an object passed by Identity to cancel the method

        public Task<ApplicationUserIdentity> GetByUsernameAsync(string normalizedUsername,
            CancellationToken cancellationToken);

    }
}