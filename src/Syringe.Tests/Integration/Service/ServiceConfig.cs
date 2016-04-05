using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Web.Http.Dependencies;
using Microsoft.Owin.Hosting;
using Syringe.Core.Configuration;
using Syringe.Service;
using Syringe.Service.DependencyResolution;

namespace Syringe.Tests.Integration.Service
{
	public class ServiceConfig
	{
		private static string _baseUrl;
		private static string _xmlDirectoryPath;

		public static string MongodbDatabaseName => "Syringe-Tests";
		public static string BranchName => "master";
		public static IDisposable OwinServer;

		public static string BaseUrl
		{
			get
			{
				if (string.IsNullOrEmpty(_baseUrl))
				{
					// Find a free port. Using port 0 gives you the next free port.
					var listener = new TcpListener(IPAddress.Loopback, 0);
					listener.Start();
					int port = ((IPEndPoint)listener.LocalEndpoint).Port;
					listener.Stop();

					_baseUrl = $"http://localhost:{port}";
				}

				return _baseUrl;
			}
		}

		public static string XmlDirectoryPath
		{
			get
			{
				if (string.IsNullOrEmpty(_xmlDirectoryPath))
				{
					string integrationFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Integration");
					_xmlDirectoryPath = Path.Combine(integrationFolder, BranchName);
				}

				return _xmlDirectoryPath;
			}
		}

		public static void StartSelfHostedOwin()
		{
			string integrationFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Integration");

			var jsonConfiguration = new JsonConfiguration()
			{
				MongoDbDatabaseName = MongodbDatabaseName,
				TestFilesBaseDirectory = integrationFolder,
				ServiceUrl = BaseUrl
			};

			// Use the service's IoC container
			var container = IoC.Initialize();
			container.Configure(x => x.For<IConfiguration>().Use(jsonConfiguration));

			// Inject instances into it
			var service = new Startup(container.GetInstance<IDependencyResolver>(), jsonConfiguration, container.GetInstance<ITestFileQueue>(), container.GetInstance<Microsoft.AspNet.SignalR.IDependencyResolver>());

			// Start it up
			OwinServer = WebApp.Start(BaseUrl, service.Configuration);
		}

		public static void CreateXmlDirectory()
		{
			if (!Directory.Exists(XmlDirectoryPath))
				Directory.CreateDirectory(XmlDirectoryPath);
		}
	}
}