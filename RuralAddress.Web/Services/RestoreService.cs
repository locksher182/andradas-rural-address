using Microsoft.EntityFrameworkCore;
using RuralAddress.Infrastructure.Data;
using System.Diagnostics;
using System.IO;

namespace RuralAddress.Web.Services
{
    public class RestoreService
    {
        private readonly ILogger<RestoreService> _logger;
        private readonly AppDbContext _dbContext;
        private const string DbHost = "147.93.68.244";
        private const string DbUser = "postgres";
        private const string DbName = "RuralAddressDb";

        public RestoreService(ILogger<RestoreService> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private void LogDebug(string message)
        {
            try
            {
                File.AppendAllText(@"C:\Users\Prefeitura\.gemini\antigravity\scratch\RuralAddress\restore_debug.txt", $"{DateTime.Now}: {message}\n");
            }
            catch { }
        }

        private string FindPgRestore()
        {
            // Linux Support
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                // Common locations on Ubuntu/Debian/CentOS
                var linuxPaths = new List<string> { "/usr/bin/pg_restore", "/usr/local/bin/pg_restore" };
                for (int v = 17; v >= 10; v--)
                {
                    linuxPaths.Add($"/usr/lib/postgresql/{v}/bin/pg_restore");
                }

                foreach (var path in linuxPaths)
                {
                    if (File.Exists(path))
                    {
                        LogDebug($"Found pg_restore (Linux) at: {path}");
                        return path;
                    }
                }
                
                // If not found in standard paths, return generic command and hope it's in PATH
                LogDebug("pg_restore not found in standard Linux paths. Trying default command 'pg_restore'.");
                return "pg_restore";
            }

            // Windows Support
            var possiblePaths = new List<string>();

            // 1. Try common versions in Program Files
            for (int v = 17; v >= 10; v--)
            {
                possiblePaths.Add($@"C:\Program Files\PostgreSQL\{v}\bin\pg_restore.exe");
                possiblePaths.Add($@"C:\Program Files (x86)\PostgreSQL\{v}\bin\pg_restore.exe");
            }

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    LogDebug($"Found pg_restore (Windows) at: {path}");
                    return path;
                }
            }

            return null;
        }

        public async Task RestoreFromStreamAsync(Stream backupStream)
        {
            LogDebug("Service: Restore requested.");

            var pgRestorePath = FindPgRestore();
            if (pgRestorePath == null)
            {
                LogDebug("Service: pg_restore not found in any common location.");
                throw new Exception("pg_restore.exe não encontrado (Versões 10-17 verificadas em 'Program Files').");
            }

            var tempPath = Path.GetTempFileName();
            LogDebug($"Service: Temp path {tempPath}");

            try
            {
                LogDebug("Service: Keeping connections alive check skipped (logic moved).");
                // Note: In a real service, we might still want to kill connections.
                // Re-implementing connection killing logic here.
                
                var killSql = $@"
                    SELECT pg_terminate_backend(pg_stat_activity.pid)
                    FROM pg_stat_activity
                    WHERE pg_stat_activity.datname = '{DbName}'
                      AND pid <> pg_backend_pid();";

                LogDebug("Service: Killing connections...");
                await _dbContext.Database.ExecuteSqlRawAsync(killSql);
                LogDebug("Service: Connections killed.");

                using (var fileStream = new FileStream(tempPath, FileMode.Create))
                {
                    await backupStream.CopyToAsync(fileStream);
                }
                LogDebug("Service: File saved to disk.");

                var startInfo = new ProcessStartInfo
                {
                    FileName = pgRestorePath,
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

                    LogDebug("Service: Starting pg_restore process...");
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    await process.WaitForExitAsync();
                    LogDebug($"Service: Process finished. ExitCode: {process.ExitCode}");

                    if (process.ExitCode != 0)
                    {
                        LogDebug($"Service: Failed. Error: {error}");
                        throw new Exception($"Erro no pg_restore (Code {process.ExitCode}): {error}");
                    }
                    LogDebug("Service: Success.");
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Service: Exception {ex.Message}");
                _logger.LogError(ex, "Service: Fatal error during restore.");
                throw;
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
