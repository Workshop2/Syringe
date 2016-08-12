﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Syringe.Core.Services;
using Syringe.Core.Tests;
using Syringe.Core.Tests.Repositories;
using Syringe.Core.Tests.Results;
using Syringe.Core.Tests.Results.Repositories;

namespace Syringe.Service.Controllers
{
    [UnitOfWork]
	public class TestsController : ApiController, ITestService
    {
        private readonly ITestRepository _testRepository;
        internal readonly ITestFileResultRepository TestFileResultRepository;

        public TestsController(ITestRepository testRepository, ITestFileResultRepository testFileResultRepository)
        {
            _testRepository = testRepository;
            TestFileResultRepository = testFileResultRepository;
        }

        [Route("api/tests/ListFiles")]
        [HttpGet]
        public IEnumerable<string> ListFiles()
        {
            return _testRepository.ListFiles();
        }

        [Route("api/tests/GetTest")]
        [HttpGet]
        public Test GetTest(string filename, int position)
        {
            return _testRepository.GetTest(filename, position);
        }

        [Route("api/tests/GetTestFile")]
        [HttpGet]
        public TestFile GetTestFile(string filename)
        {
            return _testRepository.GetTestFile(filename);
        }

        [Route("api/tests/GetRawFile")]
        [HttpGet]
        public string GetRawFile(string filename)
        {
            return _testRepository.GetRawFile(filename);
        }
        
        [Route("api/tests/EditTest")]
        [HttpPost]
        public bool EditTest(string filename, int position, [FromBody]Test test)
        {
            return _testRepository.SaveTest(filename, position, test);
        }
        
        [Route("api/tests/CreateTest")]
        [HttpPost]
        public bool CreateTest(string filename, [FromBody]Test test)
        {
            return _testRepository.CreateTest(filename, test);
        }

        [Route("api/tests/DeleteTest")]
        [HttpPost]
        public bool DeleteTest(int position, string fileName)
        {
            return _testRepository.DeleteTest(position, fileName);
        }

        [Route("api/tests/CopyTest")]
        [HttpPost]
        public bool CopyTest(int position, string fileName)
        {
            Test test = _testRepository.GetTest(fileName, position);
            test.Description = $"Copy of {test.Description}";
            return _testRepository.CreateTest(fileName, test);
        }

        [Route("api/tests/CreateTestFile")]
        [HttpPost]
        public bool CreateTestFile([FromBody]TestFile testFile)
        {
            return _testRepository.CreateTestFile(testFile);
        }

        [Route("api/tests/CopyTestFile")]
        [HttpPost]
        public bool CopyTestFile(string sourceFileName, string targetFileName)
        {
            TestFile testFile = _testRepository.GetTestFile(sourceFileName);
            testFile.Filename = targetFileName;

            bool result = _testRepository.CreateTestFile(testFile);
            return result;
        }

        [Route("api/tests/UpdateTestVariables")]
        [HttpPost]
        public bool UpdateTestVariables([FromBody]TestFile testFile)
        {
            return _testRepository.UpdateTestVariables(testFile);
        }

        [Route("api/tests/GetSummaries")]
        [HttpGet]
        public Task<TestFileResultSummaryCollection> GetSummaries(DateTime fromDateTime, int pageNumber = 1, int noOfResults = 20, string environment = "")
        {
            return TestFileResultRepository.GetSummaries(fromDateTime, pageNumber, noOfResults, environment);
        }

        [Route("api/tests/GetById")]
        [HttpGet]
        public TestFileResult GetResultById(Guid id)
        {
            return TestFileResultRepository.GetById(id);
        }

        [Route("api/tests/DeleteResultAsync")]
        [HttpPost]
        public Task DeleteResultAsync(Guid id)
        {
            return TestFileResultRepository.DeleteAsync(id);
        }

        [Route("api/tests/DeleteFile")]
        [HttpPost]
        public bool DeleteFile(string fileName)
        {
            return _testRepository.DeleteFile(fileName);
        }
    }
}