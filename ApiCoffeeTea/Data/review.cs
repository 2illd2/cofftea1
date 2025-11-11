using System;
using System.Collections.Generic;

namespace ApiCoffeeTea.Data;

public partial class review
{
    public int id { get; set; }

    public int product_id { get; set; }

    public int user_id { get; set; }

    public int rating { get; set; }

    public string? text { get; set; }

    public DateTime created_at { get; set; }

    public bool is_moderated { get; set; }

    public bool deleted { get; set; }

    public virtual product product { get; set; } = null!;

    public virtual user user { get; set; } = null!;
}
