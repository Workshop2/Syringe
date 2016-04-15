﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Syringe.Core.Environment;
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
        private readonly IEnvironmentsService _environmentsService;

        public TestFileController(ITestService testsClient, IUserContext userContext, IEnvironmentsService environmentsService)
        {
            _testsClient = testsClient;
            _userContext = userContext;
            _environmentsService = environmentsService;
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
                var testFile = new TestFile
                {
                    Filename = model.Filename,
                    Variables = model.Variables != null ? model.Variables.Select(x => new Variable(x.Name, x.Value, x.Environment)).ToList() : new List<Variable>()
                };

                bool createdTestFile = _testsClient.CreateTestFile(testFile, _userContext.DefaultBranchName);
                if (createdTestFile)
                    return RedirectToAction("Index", "Home");
            }

            return View("Add", model);
        }

        public ActionResult Update(string fileName)
        {
            TestFile testFile = _testsClient.GetTestFile(fileName, _userContext.DefaultBranchName);
            SelectListItem[] environments = GetEnvironmentsDropDown();

            var variables = testFile.Variables
                .Select(x => new VariableViewModel
                {
                    Name = x.Name,
                    Value = x.Value,
                    Environment = x.Environment.Name,
                    AvailableEnvironments = environments
                })
                .ToList();

            var model = new TestFileViewModel
            {
                Filename = fileName,
                Variables = variables
            };

            return View("Update", model);
        }

        [HttpPost]
        public ActionResult Delete(string fileName)
        {
            _testsClient.DeleteFile(fileName, _userContext.DefaultBranchName);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Update(TestFileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var testFile = new TestFile
                {
                    Filename = model.Filename,
                    Variables = model.Variables != null ? model.Variables.Select(x => new Variable(x.Name, x.Value, x.Environment)).ToList() : new List<Variable>()
                };

                bool updateTestFile = _testsClient.UpdateTestVariables(testFile, _userContext.DefaultBranchName);
                if (updateTestFile)
                    return RedirectToAction("Index", "Home");
            }

            return View("Update", model);
        }

        public ActionResult AddVariableItem()
        {
            var model = new VariableViewModel
            {
                AvailableEnvironments = GetEnvironmentsDropDown()
            };

            return PartialView("EditorTemplates/VariableViewModel", model);
        }

        private SelectListItem[] GetEnvironmentsDropDown()
        {
            var environments = _environmentsService.List();

            return environments
                .OrderBy(x => x.Order)
                .Select(x => new SelectListItem { Value = x.Name, Text = x.Name })
                .ToArray();
        }
    }
}