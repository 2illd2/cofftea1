using System;
using System.Collections.Generic;

namespace ApiCoffeeTea.Data;

public partial class shipment
{
    public int id { get; set; }

    public int order_id { get; set; }

    public string? carrier { get; set; }

    public string? tracking_number { get; set; }

    public string? status { get; set; }

    public DateTime? shipped_at { get; set; }

    public DateTime? delivered_at { get; set; }

    public virtual order order { get; set; } = null!;
}
