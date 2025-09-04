using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShelves.Shared.DataInterfaces
{
    public interface IBooksSyncService
    {
        void BeginSync();
    }
}
