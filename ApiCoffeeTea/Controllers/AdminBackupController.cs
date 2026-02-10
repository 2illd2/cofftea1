using ApiCoffeeTea.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace ApiCoffeeTea.Controllers;

[ApiController]
[Route("api/admin/backup")]
[Authorize(Roles = "admin")]
public class AdminBackupController : ControllerBase
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<AdminBackupController> _logger;

    public AdminBackupController(IConfiguration cfg, ILogger<AdminBackupController> logger)
    {
        _cfg = cfg;
        _logger = logger;
    }

    // Создать бэкап
    [HttpPost("create")]
    public async Task<ActionResult<object>> CreateBackup()
    {
        try
        {
            _logger.LogInformation("Starting backup creation...");

            var connString = _cfg.GetConnectionString("DefaultConnection");
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            _logger.LogInformation("Database connection opened");

            await using var cmd = new NpgsqlCommand("SELECT sp_createbackup()", conn);
            cmd.CommandTimeout = 300; // 5 минут на создание бэкапа

            var result = (string?)await cmd.ExecuteScalarAsync();

            _logger.LogInformation("Backup created successfully: {Result}", result);

            return Ok(new { success = true, message = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating backup");
            return BadRequest(new { success = false, error = ex.Message, details = ex.ToString() });
        }
    }

    // Получить список бэкапов
    [HttpGet("list")]
    public async Task<ActionResult<List<BackupDto>>> ListBackups()
    {
        try
        {
            _logger.LogInformation("Fetching backup list...");

            var connString = _cfg.GetConnectionString("DefaultConnection");
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand("SELECT * FROM sp_listbackups()", conn);

            var backups = new List<BackupDto>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var backupName = reader.GetString(0);

                // Извлекаем дату из имени бэкапа
                DateTime? createdAt = null;
                if (backupName.StartsWith("coffeetea_backup_"))
                {
                    var dateStr = backupName.Replace("coffeetea_backup_", "");
                    if (DateTime.TryParseExact(dateStr, "yyyy_MM_dd_HH_mm_ss", null,
                        System.Globalization.DateTimeStyles.None, out var dt))
                    {
                        createdAt = dt;
                    }
                }

                backups.Add(new BackupDto(backupName, createdAt));
            }

            _logger.LogInformation("Found {Count} backups", backups.Count);

            return Ok(backups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching backup list");
            return BadRequest(new { error = ex.Message, details = ex.ToString() });
        }
    }

    // Восстановить из бэкапа
    [HttpPost("restore")]
    public async Task<ActionResult<object>> RestoreBackup([FromBody] RestoreBackupRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BackupName))
        {
            return BadRequest(new { success = false, error = "Не указано имя бэкапа" });
        }

        try
        {
            var connString = _cfg.GetConnectionString("DefaultConnection");
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand("SELECT sp_restorefrombackup(@backup_name)", conn);
            cmd.Parameters.AddWithValue("backup_name", request.BackupName);
            cmd.CommandTimeout = 300; // 5 минут на восстановление

            _logger.LogInformation("Restoring from backup: {BackupName}", request.BackupName);

            var result = (string?)await cmd.ExecuteScalarAsync();

            _logger.LogInformation("Restore completed: {Result}", result);

            return Ok(new { success = true, message = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring backup");
            return BadRequest(new { success = false, error = ex.Message, details = ex.ToString() });
        }
    }

    // Удалить бэкап
    [HttpDelete("delete")]
    public async Task<ActionResult<object>> DeleteBackup([FromQuery] string backupName)
    {
        if (string.IsNullOrWhiteSpace(backupName))
        {
            return BadRequest(new { success = false, error = "Не указано имя бэкапа" });
        }

        try
        {
            _logger.LogInformation("Deleting backup: {BackupName}", backupName);

            var connString = _cfg.GetConnectionString("DefaultConnection");
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand("SELECT sp_deletebackup(@backup_name)", conn);
            cmd.Parameters.AddWithValue("backup_name", backupName);

            var result = (string?)await cmd.ExecuteScalarAsync();

            _logger.LogInformation("Backup deleted: {Result}", result);

            return Ok(new { success = true, message = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting backup");
            return BadRequest(new { success = false, error = ex.Message, details = ex.ToString() });
        }
    }
}

public class RestoreBackupRequest
{
    public string BackupName { get; set; } = "";
}
