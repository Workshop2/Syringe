﻿using System.Web.Mvc;
using Newtonsoft.Json;
using Syringe.Core.Security;
using Syringe.Core.Services;
using Syringe.Core.Tasks;
using Syringe.Core.Tests;

namespace Syringe.Web.Controllers
{
	public class JsonController : Controller
	{
		private readonly ITasksService _tasksClient;
		private readonly ITestService _testsClient;
		private readonly IUserContext _userContext;

		public JsonController(ITasksService tasksService, ITestService testsClient, IUserContext userContext)
		{
			_tasksClient = tasksService;
			_testsClient = testsClient;
			_userContext = userContext;
		}

		public ActionResult Run(string filename)
		{
			var taskRequest = new TaskRequest()
			{
				Filename = filename,
				Username = _userContext.FullName,
				BranchName = _userContext.DefaultBranchName,
			};

			int taskId = _tasksClient.Start(taskRequest);

			return Json(new { taskId = taskId });
	    }

		public ActionResult GetProgress(int taskId)
		{
			TaskDetails details = _tasksClient.GetRunningTaskDetails(taskId);

			// Don't use Json() as it fails for large objects.
			return Content(JsonConvert.SerializeObject(details), "application/json");
		}

		public ActionResult GetTests(string filename)
		{
			TestFile testFile = _testsClient.GetTestFile(filename, _userContext.DefaultBranchName);
			return Content(JsonConvert.SerializeObject(testFile), "application/json");
		}
	}
}