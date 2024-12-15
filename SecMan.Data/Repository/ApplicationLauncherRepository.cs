using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SecMan.Data.SQLCipher;
using SecMan.Model;


namespace SecMan.Data.Repository
{
    public class ApplicationLauncherRepository : IApplicationLauncherRepository
    {
        private Db _context { get; }
        private readonly IConfiguration _configuration;
        public ApplicationLauncherRepository(Db context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        /// <summary>
        /// Asynchronously retrieves the installed applications and their details from the database.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains an <see cref="Model.ApplicationLauncherResponse"/> 
        /// object, which includes the version and a list of installed applications.
        /// </returns>
        /// <remarks>
        /// This method reads the version number from appsettings.json, fetches the list of installed applications 
        /// from the database, and maps them to the model used by the business logic layer.
        /// </remarks>
        public async Task<ApplicationLauncherResponse> GetInstalledApplicationsAsync()
        {
            // Retrieve the version from appsettings.json
            float.TryParse(_configuration["ApplicationLauncherSettings:Version"], out float appVersion);

            // Fetch all applications from the database
            List<SQLCipher.DevDef> applications = await _context.DevDefs.Where(x => x.App && x.Name.ToLower() != "epm-suite").ToListAsync();

            // Map the fetched application data to the 'ApplicationLauncherResponse' model, including the version
            ApplicationLauncherResponse appLauncher = new ApplicationLauncherResponse
            {
                Version = appVersion, // Set the version from the configuration
                InstalledApps = applications.Select(app => app.Name).ToList()
            };

            // Return the constructed Applications object containing the version and installed apps
            return appLauncher;
        }
    }
}
