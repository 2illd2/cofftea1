using System;
using System.Collections.Generic;

namespace ApiCoffeeTea.Data;

public partial class article_category
{
    public int id { get; set; }

    public string name { get; set; } = null!;

    public bool deleted { get; set; }

    public virtual ICollection<article> articles { get; set; } = new List<article>();
}
