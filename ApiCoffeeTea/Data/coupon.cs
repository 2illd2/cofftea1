using System;
using System.Collections.Generic;

namespace ApiCoffeeTea.Data;

public partial class coupon
{
    public int id { get; set; }

    public string code { get; set; } = null!;

    public string discount_type { get; set; } = null!;

    public decimal value { get; set; }

    public DateTime? valid_from { get; set; }

    public DateTime? valid_to { get; set; }

    public decimal? min_total { get; set; }

    public int? usage_limit { get; set; }

    public bool deleted { get; set; }

    public virtual ICollection<order> orders { get; set; } = new List<order>();
}
