using System;
using System.Collections.Generic;

namespace ApiCoffeeTea.Data;

public partial class cart
{
    public int id { get; set; }

    public int user_id { get; set; }

    public int product_id { get; set; }

    public int quantity { get; set; }

    public decimal price { get; set; }

    public DateTime added_at { get; set; }

    public bool deleted { get; set; }

    public virtual product product { get; set; } = null!;

    public virtual user user { get; set; } = null!;
}
