﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Syringe.Core.Security;
using Syringe.Core.Services;
using Syringe.Core.Tests;
using Syringe.Web.Models;

namespace Syringe.Web.Controllers
{
	[Authorize]
	public class TestFileController : Controller
    {
        private readonly ITestService _testsClient;
        private readonly IUserContext _userContext;

        public TestFileController(ITestService testsClient, IUserContext userContext)
        {
            _testsClient = testsClient;
            _userContext = userContext;
        }

        public ActionResult Add()
        {
            var model = new TestFileViewModel();
            return View("Add", model);
        }

        [HttpPost]
        public ActionResult Add(TestFileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var caseCollection = new TestFile
                {
                    Filename = model.Filename,
                    Variables = model.Variables != null ? model.Variables.Select(x => new Variable(x.Name, x.Value, x.Environment)).ToList() : new List<Variable>()
                };

                bool createdTestFile = _testsClient.CreateTestFile(caseCollection, _userContext.DefaultBranchName);
                if (createdTestFile)
                    return RedirectToAction("Index", "Home");
            }

            return View("Add", model);
        }

        public ActionResult Update(string fileName)
        {
            TestFile testTestFile = _testsClient.GetTestFile(fileName, _userContext.DefaultBranchName);

            TestFileViewModel model = new TestFileViewModel
            {
                Filename = fileName,
                Variables =
                    testTestFile.Variables.Select(x => new VariableViewModel { Name = x.Name, Value = x.Value, Environment = x.Environment.Name}).ToList()
            };

            return View("Update", model);
        }

        [HttpPost]
        public ActionResult Update(TestFileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var caseCollection = new TestFile
                {
                    Filename = model.Filename,
                    Variables = model.Variables != null ? model.Variables.Select(x => new Variable(x.Name, x.Value, x.Environment)).ToList() : new List<Variable>()
				};

                bool updateTestFile = _testsClient.UpdateTestFile(caseCollection, _userContext.DefaultBranchName);
                if (updateTestFile)
                    return RedirectToAction("Index", "Home");
            }

            return View("Update", model);
        }

        public ActionResult AddVariableItem()
        {
            return PartialView("EditorTemplates/VariableViewModel", new VariableViewModel());
        }
	}
}