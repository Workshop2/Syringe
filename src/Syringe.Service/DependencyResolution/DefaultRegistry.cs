// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultRegistry.cs" company="Web Advanced">
// Copyright 2012 Web Advanced (www.webadvanced.com)
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;
using Syringe.Core.Configuration;
using Syringe.Core.FileOperations;
using Syringe.Core.Repositories;
using Syringe.Core.Repositories.MongoDB;
using Syringe.Core.Xml.Reader;
using Syringe.Core.Xml.Writer;
using Syringe.Service.Api.Hubs;
using Syringe.Service.Parallel;
using WebApiContrib.IoC.StructureMap;

namespace Syringe.Service.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        public DefaultRegistry()
        {
            Scan(
                scan =>
                {
                    scan.TheCallingAssembly();
                    scan.WithDefaultConventions();
				});

            For<IDependencyResolver>().Use<StructureMapSignalRDependencyResolver>().Singleton();
            For<System.Web.Http.Dependencies.IDependencyResolver>().Use<StructureMapResolver>();

            For<SyringeService>().Use<SyringeService>().Singleton();

            For<TaskMonitorHub>().Use<TaskMonitorHub>();

			For<IConfigurationStore>().Use(new JsonConfigurationStore()).Singleton();
			For<IConfiguration>().Use(x => x.GetInstance<IConfigurationStore>().Load());

            For<ITestCaseSessionRepository>().Use<TestCaseSessionRepository>().Singleton();
            For<ITestSessionQueue>().Use<ParallelTestSessionQueue>().Singleton();
            Forward<ITestSessionQueue, ITaskObserver>();

            For<ITaskPublisher>().Use<TaskPublisher>().Singleton();
            For<ITaskGroupProvider>().Use<TaskGroupProvider>().Singleton();

            For<ITestCaseReader>().Use<TestCaseReader>();
            For<ITestCaseWriter>().Use<TestCaseWriter>();
            For<IFileHandler>().Use<FileHandler>();
            For<ICaseRepository>().Use<CaseRepository>();

            For<IHubConnectionContext<ITaskMonitorHubClient>>()
                .Use(context => context.GetInstance<IDependencyResolver>().Resolve<IConnectionManager>().GetHubContext<TaskMonitorHub, ITaskMonitorHubClient>().Clients);
        }
    }
}