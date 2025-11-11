using System;
using System.Collections.Generic;

namespace ApiCoffeeTea.Data;

public partial class chat_thread
{
    public int id { get; set; }

    public int client_id { get; set; }

    public int? consultant_id { get; set; }

    public DateTime created_at { get; set; }

    public string status { get; set; } = null!;

    public virtual ICollection<chat_message> chat_messages { get; set; } = new List<chat_message>();

    public virtual user client { get; set; } = null!;

    public virtual user? consultant { get; set; }
}
