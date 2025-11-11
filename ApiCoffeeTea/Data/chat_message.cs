using System;
using System.Collections.Generic;

namespace ApiCoffeeTea.Data;

public partial class chat_message
{
    public int id { get; set; }

    public int thread_id { get; set; }

    public int sender_id { get; set; }

    public string text { get; set; } = null!;

    public DateTime created_at { get; set; }

    public virtual user sender { get; set; } = null!;

    public virtual chat_thread thread { get; set; } = null!;
}
