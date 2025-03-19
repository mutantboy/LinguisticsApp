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
            /// This would be a better solution, but mysteriously doesnt work idk why
            /*return await _dbContext.Set<User>()
                .FirstOrDefaultAsync(u => EF.Property<string>(u, "Username") == email.Value);*/


            return await _dbContext.Set<User>()
                .FromSqlRaw("SELECT * FROM Users WHERE Username = {0}", email.Value)
                .FirstOrDefaultAsync();
        }
    }
}
