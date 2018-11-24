using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace OOOBotCore
{
    public class User : IEquatable<User>
    {
	    public bool IsOoo => OooPeriods.GetByUserId(UserId).Any(p => p.IsCurrentlyActive);
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

	public static class Users
	{
		private static readonly Dictionary<string, User> _users;

		static Users()
		{
		_users = new Dictionary<string, User>(); 
		}

		public static User FindOrCreateUser(string userId)
		{
			User user;
			if (!_users.ContainsKey(userId))
			{
				user = new User(userId);
				_users.Add(user.UserId, user);
			}
			else
			{
				user = _users[userId];
			}

			return user;
		}
	}
}