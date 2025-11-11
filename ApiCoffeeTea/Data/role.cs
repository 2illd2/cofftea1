using System;
using System.Collections.Generic;

namespace ApiCoffeeTea.Data;

public partial class role
{
    public int id { get; set; }

    public string name { get; set; } = null!;

    public bool deleted { get; set; }

    public virtual ICollection<user> users { get; set; } = new List<user>();
}
