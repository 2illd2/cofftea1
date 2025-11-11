using System;
using System.Collections.Generic;

namespace ApiCoffeeTea.Data;

public partial class order
{
    public int id { get; set; }

    public int user_id { get; set; }

    public int address_id { get; set; }

    public int status_id { get; set; }

    public decimal total { get; set; }

    public string payment_status { get; set; } = null!;

    public DateTime created_at { get; set; }

    public bool deleted { get; set; }

    public virtual address address { get; set; } = null!;

    public virtual ICollection<order_item> order_items { get; set; } = new List<order_item>();

    public virtual ICollection<payment> payments { get; set; } = new List<payment>();

    public virtual shipment? shipment { get; set; }

    public virtual order_status status { get; set; } = null!;

    public virtual user user { get; set; } = null!;

    public virtual ICollection<coupon> coupons { get; set; } = new List<coupon>();
}
