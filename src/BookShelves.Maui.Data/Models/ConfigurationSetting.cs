using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShelves.Maui.Data.Models;

[Table(Constants.ConfigurationSettingTable), PrimaryKey(nameof(Id))]
internal class ConfigurationSetting
{
    public ConfigurationSetting() { }

    public int Id { get; set; }

    public string? Key { get; set; }

    public string? Value { get; set; }

    public DateTime LastUpdateTime { get; set; }
}
