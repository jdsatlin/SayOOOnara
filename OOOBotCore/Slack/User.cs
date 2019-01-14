using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SayOOOnara.Persistance;

namespace SayOOOnara
{
    public class User : IEquatable<User>
    {
	    public bool IsOoo => OooPeriods.GetByUserId(Id).Any(p => p.IsCurrentlyActive);

	    public bool HasUpcomingOooPeriods => OooPeriods.GetByUserId(Id).Any(p => p.StartTime > DateTime.UtcNow);
        public string Id { get; set; }
        public string UserName { get; set; }

	    private User()
	    {

	    }

		/// <summary>
		/// Do not access directly, access via Users.FindOrCreateUser
		/// </summary>
		public User(string id)
        {
            Id = id;
        }


        public bool Equals(User other)
        {
            if (Id == other.Id)
            {
                return true;
            }

            return false;
        }
    }

	public class Users
	{
		private static readonly Dictionary<string, User> _users = new Dictionary<string, User>();
		private OooContext Context { get; set; }

		public Users()
		{
			Context = new OooContext();
			
		}

		public static User Find(string userId)
		{
			_users.TryGetValue(userId, out var user);
			return user;
		}

		public static async Task<User> FindOrCreate(string userId, string userName)
		{
			User user;
			if (!_users.ContainsKey(userId))
			{
				user = new User(userId);
				user.UserName = userName;
				var context = new OooContext();
				context.Users.Add(user);
				await context.SaveChangesAsync();
				_users.Add(user.Id, user);
			}
			else
			{
				user = _users[userId];
			}

			return user;
		}

		public async Task LoadUsers()
		{
			var savedUsers = await Context.Users.ToListAsync();
			savedUsers.ForEach(u => _users.Add(u.Id, u));
		}
	}
}