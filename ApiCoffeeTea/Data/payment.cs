using System;
using System.Collections.Generic;

namespace ApiCoffeeTea.Data;

public partial class payment
{
    public int id { get; set; }

    public int order_id { get; set; }

    public string method { get; set; } = null!;

    public decimal amount { get; set; }

    public string status { get; set; } = null!;

    public DateTime? paid_at { get; set; }

    public string? provider_txn_id { get; set; }

    public virtual order order { get; set; } = null!;
}
