using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CattleFarm.Models;

namespace CattleFarm.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class BackupController : Controller
    {
        private readonly CattleFarmDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<BackupController> _logger;

        public BackupController(CattleFarmDbContext context, IWebHostEnvironment env, ILogger<BackupController> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var backupFolder = Path.Combine(_env.WebRootPath, "backups");
            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }

            var files = Directory.GetFiles(backupFolder, "*.bak")
                                 .Select(f => new FileInfo(f))
                                 .OrderByDescending(f => f.CreationTime)
                                 .ToList();

            return View(files);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBackup()
        {
            try
            {
                var backupFolder = Path.Combine(_env.WebRootPath, "backups");
                if (!Directory.Exists(backupFolder))
                {
                    Directory.CreateDirectory(backupFolder);
                }

                var dbName = _context.Database.GetDbConnection().Database;
                var fileName = $"{dbName}_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                var backupPath = Path.Combine(backupFolder, fileName);

                string sql = $"BACKUP DATABASE [{dbName}] TO DISK = @path WITH FORMAT, INIT";
                var connStr = _context.Database.GetDbConnection().ConnectionString;

                using (var connection = new Microsoft.Data.SqlClient.SqlConnection(connStr))
                {
                    await connection.OpenAsync();
                    using (var command = new Microsoft.Data.SqlClient.SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@path", backupPath);
                        await command.ExecuteNonQueryAsync();
                    }
                }

                TempData["SuccessMessage"] = $"Backup created successfully: {fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Direct database backup to webroot failed. Attempting fallback backup.");
                try
                {
                    var dbName = _context.Database.GetDbConnection().Database;
                    var tempBackupPath = Path.Combine(Path.GetTempPath(), $"{dbName}_temp_{Guid.NewGuid()}.bak");
                    var connStr = _context.Database.GetDbConnection().ConnectionString;

                    string sql = $"BACKUP DATABASE [{dbName}] TO DISK = @path WITH FORMAT, INIT";
                    using (var connection = new Microsoft.Data.SqlClient.SqlConnection(connStr))
                    {
                        await connection.OpenAsync();
                        using (var command = new Microsoft.Data.SqlClient.SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@path", tempBackupPath);
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    var backupFolder = Path.Combine(_env.WebRootPath, "backups");
                    if (!Directory.Exists(backupFolder))
                    {
                        Directory.CreateDirectory(backupFolder);
                    }

                    var destPath = Path.Combine(backupFolder, $"{dbName}_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak");
                    System.IO.File.Move(tempBackupPath, destPath);

                    TempData["SuccessMessage"] = $"Backup created successfully: {Path.GetFileName(destPath)}";
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "Fallback database backup also failed.");
                    TempData["ErrorMessage"] = $"Database backup failed: {ex.Message}. Fallback error: {fallbackEx.Message}";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreBackup(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                TempData["ErrorMessage"] = "Invalid backup file name.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var backupFolder = Path.Combine(_env.WebRootPath, "backups");
                var backupPath = Path.Combine(backupFolder, fileName);
                if (!System.IO.File.Exists(backupPath))
                {
                    TempData["ErrorMessage"] = "Backup file not found.";
                    return RedirectToAction(nameof(Index));
                }

                var dbConnection = _context.Database.GetDbConnection();
                var dbName = dbConnection.Database;
                var connStr = dbConnection.ConnectionString;

                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connStr)
                {
                    InitialCatalog = "master"
                };
                var masterConnStr = builder.ConnectionString;

                Microsoft.Data.SqlClient.SqlConnection.ClearAllPools();

                using (var connection = new Microsoft.Data.SqlClient.SqlConnection(masterConnStr))
                {
                    await connection.OpenAsync();

                    var singleUserSql = $"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
                    using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(singleUserSql, connection))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }

                    var restoreSql = $"RESTORE DATABASE [{dbName}] FROM DISK = @path WITH REPLACE;";
                    using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(restoreSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@path", backupPath);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    var multiUserSql = $"ALTER DATABASE [{dbName}] SET MULTI_USER;";
                    using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(multiUserSql, connection))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                TempData["SuccessMessage"] = "Database restored successfully. System connections refreshed.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restore database from backup.");
                TempData["ErrorMessage"] = $"Database restore failed: {ex.Message}";

                try
                {
                    var dbConnection = _context.Database.GetDbConnection();
                    var dbName = dbConnection.Database;
                    var connStr = dbConnection.ConnectionString;
                    var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connStr) { InitialCatalog = "master" };
                    using (var connection = new Microsoft.Data.SqlClient.SqlConnection(builder.ConnectionString))
                    {
                        await connection.OpenAsync();
                        var multiUserSql = $"ALTER DATABASE [{dbName}] SET MULTI_USER;";
                        using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(multiUserSql, connection))
                        {
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                catch { }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult DownloadBackup(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return BadRequest();
            var backupFolder = Path.Combine(_env.WebRootPath, "backups");
            var filePath = Path.Combine(backupFolder, fileName);
            if (!System.IO.File.Exists(filePath)) return NotFound();

            var bytes = System.IO.File.ReadAllBytes(filePath);
            return File(bytes, "application/octet-stream", fileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteBackup(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return BadRequest();
            var backupFolder = Path.Combine(_env.WebRootPath, "backups");
            var filePath = Path.Combine(backupFolder, fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                TempData["SuccessMessage"] = "Backup file deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Backup file not found.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
