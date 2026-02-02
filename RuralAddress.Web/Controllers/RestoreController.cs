using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RuralAddress.Infrastructure.Data;
using System.Diagnostics;

namespace RuralAddress.Web.Controllers
{
    [Route("api/restore")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RestoreController : ControllerBase
    {
        private readonly ILogger<RestoreController> _logger;
        private readonly AppDbContext _dbContext;
        private const string PgRestorePath = @"C:\Program Files\PostgreSQL\17\bin\pg_restore.exe";
        private const string DbHost = "147.93.68.244";
        private const string DbUser = "postgres";
        private const string DbName = "RuralAddressDb";

        public RestoreController(ILogger<RestoreController> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private void LogDebug(string message)
        {
            try
            {
                System.IO.File.AppendAllText(@"C:\Users\Prefeitura\.gemini\antigravity\scratch\RuralAddress\restore_debug.txt", $"{DateTime.Now}: {message}\n");
            }
            catch { }
        }

        [HttpPost]
        [RequestSizeLimit(100_000_000)] // 100 MB limit
        [RequestFormLimits(MultipartBodyLengthLimit = 100_000_000)]
        public async Task<IActionResult> RestoreSystem(IFormFile file)
        {
            LogDebug("Request received.");
            if (file == null || file.Length == 0)
            {
                LogDebug("File is empty.");
                return BadRequest("Arquivo inválido.");
            }

            if (!System.IO.File.Exists(PgRestorePath))
            {
                LogDebug("pg_restore not found.");
                return StatusCode(500, "pg_restore não encontrado no servidor.");
            }

            var tempPath = Path.GetTempFileName();
            LogDebug($"Temp path: {tempPath}");
            
            try
            {
                // 1. Force close other connections
                LogDebug("Killing connections...");
                var killSql = $@"
                    SELECT pg_terminate_backend(pg_stat_activity.pid)
                    FROM pg_stat_activity
                    WHERE pg_stat_activity.datname = '{DbName}'
                      AND pid <> pg_backend_pid();";

                await _dbContext.Database.ExecuteSqlRawAsync(killSql);
                LogDebug("Connections killed.");
                
                // 2. Save upload to temp file
                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                LogDebug($"File saved ({file.Length} bytes).");

                // 3. Prepare pg_restore process
                var startInfo = new ProcessStartInfo
                {
                    FileName = PgRestorePath,
                    Arguments = $"-h {DbHost} -U {DbUser} -d {DbName} -c --no-password --no-owner -v \"{tempPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                startInfo.EnvironmentVariables["PGPASSWORD"] = "Sushivivo182";

                using (var process = new Process { StartInfo = startInfo })
                {
                    var output = new System.Text.StringBuilder();
                    var error = new System.Text.StringBuilder();

                    process.OutputDataReceived += (s, e) => { if (e.Data != null) output.AppendLine(e.Data); };
                    process.ErrorDataReceived += (s, e) => { if (e.Data != null) error.AppendLine(e.Data); };

                    LogDebug("Starting pg_restore...");
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    await process.WaitForExitAsync();
                    LogDebug($"pg_restore finished. ExitCode: {process.ExitCode}");
                    
                    if (process.ExitCode == 0)
                    {
                        LogDebug("Success.");
                        return Ok(new { message = "Sistema restaurado com sucesso! Recarregue a página." });
                    }
                    else
                    {
                        LogDebug($"Failed. Stderr: {error}");
                        return StatusCode(500, new { message = "Erro (Code " + process.ExitCode + ").", details = error.ToString() });
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Exception: {ex.Message}");
                _logger.LogError(ex, "Erro fatal durante a restauração.");
                return StatusCode(500, new { message = ex.Message });
            }
            finally
            {
                if (System.IO.File.Exists(tempPath))
                    System.IO.File.Delete(tempPath);
            }
        }
    }
}
