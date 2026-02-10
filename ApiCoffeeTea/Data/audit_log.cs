using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCoffeeTea.Data;

[Table("audit_log")]
public partial class audit_log
{
    [Key]
    public int id { get; set; }

    [Required]
    [MaxLength(100)]
    public string table_name { get; set; } = "";

    [Required]
    [MaxLength(10)]
    public string operation { get; set; } = ""; // INSERT|UPDATE|DELETE

    public int row_id { get; set; }

    public int? user_id { get; set; }

    [Column(TypeName = "jsonb")]
    public string? old_values { get; set; }

    [Column(TypeName = "jsonb")]
    public string? new_values { get; set; }

    public DateTime changed_at { get; set; }

    // Navigation
    public virtual user? user { get; set; }
}
