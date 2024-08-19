using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShelves.Maui.Services
{
    internal interface ITokenService
    {
        Task<string> GetTokenAsync(string[] scopes, bool silentOnly);
        Task<IAccount> GetUserAccountAsync();
        void LogOut();
    }
}
