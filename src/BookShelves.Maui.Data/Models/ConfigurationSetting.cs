using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookShelves.Maui.Data.Models;

[Table(Constants.ConfigurationSettingTable), PrimaryKey(nameof(Id))]
public class ConfigurationSetting
{
    public ConfigurationSetting() { }

    public int Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string? Value { get; set; } = null;

    public DateTime LastUpdateTime { get; set; }
}
