using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShelves.Shared.DataInterfaces;

public interface IMacAuthenticationService
{
    //Task<string> Logout(string userName);
    //Task<IEnumerable<IAccount>> GetExistingAccountsAsync();
    Task<string> GetTokenAsync(string[] scopes, bool silentOnly);
    void LogOut();
}
