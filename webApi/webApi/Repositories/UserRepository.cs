using Microsoft.EntityFrameworkCore;
using webApi.Model;

namespace webApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserInfo> GetUserByIdAsync(string id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<UserInfo> CreateOrUpdateUserAsync(UserInfo user)
        {
            var existingUser = await _context.Users.FindAsync(user.Id);
            
            if (existingUser == null)
            {
                user.CreatedAt = DateTime.UtcNow;
                _context.Users.Add(user);
            }
            else
            {
                existingUser.Email = user.Email;
                existingUser.Username = user.Username;
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.ImageUrl = user.ImageUrl;
                existingUser.ProfileImageUrl = user.ProfileImageUrl;
                existingUser.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 