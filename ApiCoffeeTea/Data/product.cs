using System;
using System.Collections.Generic;

namespace ApiCoffeeTea.Data;

public partial class product
{
    public int id { get; set; }

    public int? category_id { get; set; }

    public string name { get; set; } = null!;

    public string type { get; set; } = null!;

    public string sku { get; set; } = null!;

    public decimal price { get; set; }

    public int quantity { get; set; }

    public string? description { get; set; }

    public string? image_url { get; set; }

    public string? roast_level { get; set; }

    public string? processing { get; set; }

    public string? origin_country { get; set; }

    public string? origin_region { get; set; }

    public bool deleted { get; set; }

    public virtual ICollection<cart> carts { get; set; } = new List<cart>();

    public virtual category? category { get; set; }

    public virtual ICollection<order_item> order_items { get; set; } = new List<order_item>();

    public virtual product_detail? product_detail { get; set; }

    public virtual ICollection<review> reviews { get; set; } = new List<review>();

    public virtual ICollection<article> articles { get; set; } = new List<article>();
}
 