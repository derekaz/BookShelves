using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShelves.Shared.Data.Interfaces;

public interface IBooksSyncService
{
    Task BeginSyncAsync();
}
