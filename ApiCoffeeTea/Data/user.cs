using System;
using System.Collections.Generic;

namespace ApiCoffeeTea.Data;

public partial class user
{
    public int id { get; set; }

    public int role_id { get; set; }

    public string last_name { get; set; } = null!;

    public string first_name { get; set; } = null!;

    public string? middle_name { get; set; }

    public string email { get; set; } = null!;

    public string? phone { get; set; }

    public string password_hash { get; set; } = null!;

    public DateTime created_at { get; set; }

    public bool deleted { get; set; }

    public virtual ICollection<address> addresses { get; set; } = new List<address>();

    public virtual ICollection<cart> carts { get; set; } = new List<cart>();

    public virtual ICollection<chat_message> chat_messages { get; set; } = new List<chat_message>();

    public virtual ICollection<chat_thread> chat_threadclients { get; set; } = new List<chat_thread>();

    public virtual ICollection<chat_thread> chat_threadconsultants { get; set; } = new List<chat_thread>();

    public virtual ICollection<order> orders { get; set; } = new List<order>();

    public virtual ICollection<review> reviews { get; set; } = new List<review>();

    public virtual role role { get; set; } = null!;
}
