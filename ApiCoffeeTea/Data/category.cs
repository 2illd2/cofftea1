using System;
using System.Collections.Generic;

namespace ApiCoffeeTea.Data;

public partial class category
{
    public int id { get; set; }

    public string name { get; set; } = null!;

    public int? parent_id { get; set; }

    public bool deleted { get; set; }

    public virtual ICollection<category> Inverseparent { get; set; } = new List<category>();

    public virtual category? parent { get; set; }

    public virtual ICollection<product> products { get; set; } = new List<product>();
}
