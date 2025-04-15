using System;
using System.Collections.Generic;

namespace CarServer.Models;

public partial class Accessory
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public virtual ICollection<CarAccessory> CarAccessories { get; set; } = new List<CarAccessory>();
}
