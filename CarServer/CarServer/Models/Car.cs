using System;
using System.Collections.Generic;

namespace CarServer.Models;

public partial class Car
{
    public Guid Id { get; set; }

    public virtual Esp32Control Esp32Control { get; set; } = null!;

    public virtual Esp32Camera Esp32Camera { get; set; } = null!;
}
