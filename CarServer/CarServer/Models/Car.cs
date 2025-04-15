using System;
using System.Collections.Generic;

namespace CarServer.Models;

public partial class Car
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ICollection<CarAccessory> CarAccessories { get; set; } = new List<CarAccessory>();

    public virtual Esp32Control Esp32Control { get; set; } = null!;

    public virtual Esp32Camera Esp32Camera { get; set; } = null!;
}
