using System;
using System.Collections.Generic;

namespace ApiCoffeeTea.Data;

public partial class address
{
    public int id { get; set; }

    public int user_id { get; set; }

    public string line1 { get; set; } = null!;

    public string city { get; set; } = null!;

    public string postal_code { get; set; } = null!;

    public string country { get; set; } = null!;

    public bool is_default { get; set; }

    public bool deleted { get; set; }

    public virtual ICollection<order> orders { get; set; } = new List<order>();

    public virtual user user { get; set; } = null!;
}
