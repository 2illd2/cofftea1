using System;
using System.Collections.Generic;

namespace ApiCoffeeTea.Data;

public partial class order_status
{
    public int id { get; set; }

    public string name { get; set; } = null!;

    public virtual ICollection<order> orders { get; set; } = new List<order>();
}
