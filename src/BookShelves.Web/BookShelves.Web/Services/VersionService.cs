//using BookShelves.Shared.ServiceInterfaces;
//using BookShelves.Shared.ServiceModels;
//using System.Reflection;

//namespace BookShelves.Web.Services
//{
//    internal class VersionService : IVersionService
//    {
//        public VersionInfo GetVersion()
//        {
//            var version = Assembly.GetExecutingAssembly()
//                .GetCustomAttribute<AssemblyInformationalVersionAttribute>();

//            return new VersionInfo()
//            {
//                CurrentVersion = version?.InformationalVersion ?? "NA",
//                CurrentBuild = "0",
//            };
//        }
//    }
//}
