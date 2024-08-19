using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShelves.Maui.Services
{
    internal class TokenService : ITokenService
    {
        public Task<string> GetTokenAsync(string[] scopes, bool silentOnly)
        {
            throw new NotImplementedException();
        }

        public Task<IAccount> GetUserAccountAsync()
        {
            throw new NotImplementedException();
        }

        public void LogOut()
        {
            throw new NotImplementedException();
        }
    }
}
