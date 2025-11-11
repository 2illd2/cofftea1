using System;
using System.Collections.Generic;

namespace ApiCoffeeTea.Data;

public partial class article
{
    public int id { get; set; }

    public int? category_id { get; set; }

    public string title { get; set; } = null!;

    public string slug { get; set; } = null!;

    public string? cover_image_url { get; set; }

    public string? summary { get; set; }

    public string content { get; set; } = null!;

    public DateTime? published_at { get; set; }

    public bool is_published { get; set; }

    public bool deleted { get; set; }

    public virtual article_category? category { get; set; }

    public virtual ICollection<product> products { get; set; } = new List<product>();
}
