using System;
using System.Collections.Generic;

namespace ApiCoffeeTea.Data;

public partial class order_item
{
    public int id { get; set; }

    public int order_id { get; set; }

    public int product_id { get; set; }

    public int qty { get; set; }

    public decimal unit_price { get; set; }

    public virtual order order { get; set; } = null!;

    public virtual product product { get; set; } = null!;
}
