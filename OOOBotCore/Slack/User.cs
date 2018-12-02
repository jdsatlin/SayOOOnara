using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SayOOOnara
{
    public class User : IEquatable<User>
    {
		[JsonIgnore]
	    public bool IsOoo => OooPeriods.GetByUserId(UserId).Any(p => p.IsCurrentlyActive);

	    [JsonIgnore]
	    public bool HasUpcomingOooPeriods => OooPeriods.GetByUserId(UserId).Any(p => p.StartTime > DateTime.UtcNow);
        public string UserId { get; }
        public string UserName { get; set; }

		/// <summary>
		/// Do not access directly, access via Users.FindOrCreateUser
		/// </summary>
		public User(string userId)
        {
            UserId = userId;
        }


        public bool Equals(User other)
        {
            if (UserId == other.UserId)
            {
                return true;
            }

            return false;
        }
    }

	public class Users
	{
		private static readonly Dictionary<string, User> _users = new Dictionary<string, User>();
		private static IStorage<User> _storageProvider;

		public Users(IStorage<User> storageProvider)
		{
		_storageProvider = storageProvider;
		}

		public static User FindOrCreateUser(string userId)
		{
			User user;
			if (!_users.ContainsKey(userId))
			{
				user = new User(userId);
				_users.Add(user.UserId, user);
				_storageProvider.SaveAll(_users.Select(u => u.Value).ToList());
			}
			else
			{
				user = _users[userId];
			}

			return user;
		}

		public async Task LoadUsers()
		{
			var savedUsers = await _storageProvider.GetAll();
			savedUsers.ForEach(u => _users.Add(u.UserId, u));
		}
	}
}