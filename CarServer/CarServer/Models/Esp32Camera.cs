using System;
using System.Collections.Generic;

namespace CarServer.Models;

public partial class Esp32Camera
{
    public Guid Id { get; set; }

    public DateTime LastSeen { get; set; }

    public bool IsOnline { get; set; }

    public virtual Car? Car { get; set; }
}
