using System;
using System.Collections.Generic;

namespace CarServer.Models;

public partial class CarAccessory
{
    public Guid IdCar { get; set; }

    public int IdAccessory { get; set; }

    public int Quantity { get; set; }

    public virtual Accessory Accessory { get; set; } = null!;

    public virtual Car Car { get; set; } = null!;
}
