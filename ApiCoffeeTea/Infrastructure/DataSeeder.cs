using ApiCoffeeTea.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiCoffeeTea.Infrastructure
{
    public class DataSeeder
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _cfg;

        public DataSeeder(AppDbContext db, IConfiguration cfg)
        {
            _db = db;
            _cfg = cfg;
        }

        public async Task EnsureSystemSetupAsync()
        {
            await _db.Database.MigrateAsync();

            //  Роли
            await EnsureRoleAsync(RoleNames.Admin);
            await EnsureRoleAsync(RoleNames.Consultant);
            await EnsureRoleAsync(RoleNames.User);

            //  Системные пользователи
            await EnsureSystemUserAsync(section: "Admin", roleName: RoleNames.Admin);
            await EnsureSystemUserAsync(section: "Consultant", roleName: RoleNames.Consultant);

            //  Статусы заказов
            await EnsureOrderStatusesAsync();
        }

        private async Task EnsureRoleAsync(string roleName)
        {
            var role = await _db.roles.FirstOrDefaultAsync(r => r.name == roleName && !r.deleted);
            if (role is null)
            {
                _db.roles.Add(new role { name = roleName, deleted = false });
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Создаёт пользователя из секции конфигурации:
        /// { Email, Password, FirstName, LastName, MiddleName?, Phone? }
        /// </summary>
        private async Task EnsureSystemUserAsync(string section, string roleName)
        {
            var s = _cfg.GetSection(section);
            var email = s["Email"]?.Trim().ToLowerInvariant();
            var pass = s["Password"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pass))
                return; 

            var exists = await _db.users.AnyAsync(u => u.email == email && !u.deleted);
            if (exists) return;

            var roleId = await _db.roles
                .Where(r => r.name == roleName && !r.deleted)
                .Select(r => r.id)
                .FirstAsync();

            var u = new user
            {
                email = email,
                password_hash = BCrypt.Net.BCrypt.HashPassword(pass),
                first_name = s["FirstName"] ?? roleName,
                last_name = s["LastName"] ?? "User",
                middle_name = s["MiddleName"],
                phone = s["Phone"],
                role_id = roleId,
                created_at = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                deleted = false
            };

            _db.users.Add(u);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Создаёт базовые статусы заказов, если их нет.
        /// </summary>
        private async Task EnsureOrderStatusesAsync()
        {
            if (await _db.order_statuses.AnyAsync())
                return; 

            var statuses = new[]
            {
                new order_status { name = "Новый" },
                new order_status { name = "В обработке" },
                new order_status { name = "Отправлен" },
                new order_status { name = "Завершён" },
                new order_status { name = "Отменён" }
            };

            _db.order_statuses.AddRange(statuses);
            await _db.SaveChangesAsync();
        }
    }
}
