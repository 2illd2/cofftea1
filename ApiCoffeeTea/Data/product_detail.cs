using System;
using System.Collections.Generic;

namespace ApiCoffeeTea.Data;

public partial class product_detail
{
    public int id { get; set; }

    public int product_id { get; set; }

    public string? long_description { get; set; }

    public string? brewing_guide { get; set; }

    public string? flavor_notes { get; set; }

    public string? origin_details { get; set; }

    public bool deleted { get; set; }

    public virtual product product { get; set; } = null!;
}
