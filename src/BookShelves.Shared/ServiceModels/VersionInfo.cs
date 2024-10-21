using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShelves.Shared.ServiceModels;

public class VersionInfo
{
    public string CurrentVersion { get; set; } = string.Empty;
    public string CurrentBuild { get; set; } = string.Empty;
}
