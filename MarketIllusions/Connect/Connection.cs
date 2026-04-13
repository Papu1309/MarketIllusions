using System;
using System.Linq;
using System.Data.Entity;

namespace MarketIllusions.Connect
{
    public static class Connection
    {
        public static MarketAirSilenceTimeEntities entities = new MarketAirSilenceTimeEntities();

        public static Users GetUser(string login, string password)
        {
            return entities.Users
                .Include("Roles")
                .FirstOrDefault(u => u.Login == login && u.Password == password);
        }

        public static string GetUserRole(int userId)
        {
            var user = entities.Users
                .Include("Roles")
                .FirstOrDefault(u => u.Id == userId);

            return user?.Roles?.Name;
        }

        public static bool TestConnection()
        {
            try
            {
                entities.Database.Connection.Open();
                entities.Database.Connection.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}