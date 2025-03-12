using LinguisticsApp.Application.Common.Interfaces.Repository;
using LinguisticsApp.DomainModel.DomainModel;
using LinguisticsApp.DomainModel.RichTypes;
using LinguisticsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguisticsApp.Infrastructure.Repository
{
    public class UserRepository : RepositoryBase<User, UserId>, IUserRepository
    {
        public UserRepository(LinguisticsDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<User?> GetByEmailAsync(Email email)
        {
            var emailValue = email.Value;

            return await _dbContext.Set<User>()
                .Where(u => EF.Property<string>(u, "Username") == emailValue)
                .FirstOrDefaultAsync();
        }
    }
}
