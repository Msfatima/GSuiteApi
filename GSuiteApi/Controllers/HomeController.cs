using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GSuiteApi.Models;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Auth.OAuth2;
using System.IO;
using System.Threading;
using Google.Apis.Util.Store;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.Http;

namespace GSuiteApi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        static string[] Scopes = { DirectoryService.Scope.AdminDirectoryUserReadonly };
          
        public const string SessionKeyName = "";
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        
        public IActionResult Index()
        {
            GetService();                   
            DirectoryService service = GetService();
            UsersResource.ListRequest request = service.Users.List();
            request.Customer = "my_customer";
            request.MaxResults = 100;
            request.OrderBy = UsersResource.ListRequest.OrderByEnum.Email;
            //get all Users from G Suite.    
            IList<User> users = request.Execute().UsersValue;
            // model view 
            List<Employees> employeeslist = new List<Employees>();           
            if (users != null && users.Count > 0)
            {             
                foreach (var userItem in users)
                {  
                    Employees employees = new Employees
                    {
                        Id = userItem.CustomerId,
                        Email = userItem.PrimaryEmail,
                        Orgnization = userItem.OrgUnitPath,
                        Phone = userItem.RecoveryPhone,
                    };
                    employeeslist.Add(employees);
                    
                }              
            }     
            return View(employeeslist);
        }
        public static DirectoryService GetService()
        {
            UserCredential credential;
            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;             
            }

            // Create Directory API service.
            var service = new DirectoryService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Directory API " ,
            });
            
            return service;
         }
      
      
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel 
            { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
