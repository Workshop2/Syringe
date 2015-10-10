﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Syringe.Core.FileOperations;
using Syringe.Core.TestCases;
using Syringe.Core.Xml.Reader;
using Syringe.Core.Xml.Writer;

namespace Syringe.Core.Repositories
{
    public class CaseRepository : ICaseRepository
    {
        private readonly ITestCaseReader _testCaseReader;
        private readonly ITestCaseWriter _testCaseWriter;
        private readonly IFileHandler _fileHandler;

        public CaseRepository() : this(new TestCaseReader(), new TestCaseWriter(), new FileHandler()) { }

        internal CaseRepository(ITestCaseReader testCaseReader, ITestCaseWriter testCaseWriter, IFileHandler fileHandler)
        {
            _testCaseReader = testCaseReader;
            _testCaseWriter = testCaseWriter;
            _fileHandler = fileHandler;
        }

        public Case GetTestCase(string filename, string teamName, int caseId)
        {
            var fullPath = _fileHandler.GetFileFullPath(filename, teamName);
            string xml = _fileHandler.ReadAllText(fullPath);

            using (var stringReader = new StringReader(xml))
            {
                CaseCollection collection = _testCaseReader.Read(stringReader);
                Case testCase = collection.TestCases.FirstOrDefault(x => x.Id == caseId);

                if (testCase == null)
                {
                    throw new NullReferenceException("Could not find specified Test Case:" + caseId);
                }

                testCase.ParentFilename = filename;

                return testCase;
            }
        }

        public bool SaveTestCase(Case testCase, string teamName)
        {
            if (testCase == null)
            {
                throw new ArgumentNullException("testCase");
            }

            var fullPath = _fileHandler.GetFileFullPath(testCase.ParentFilename, teamName);
            string xml = _fileHandler.ReadAllText(fullPath);

            CaseCollection collection;

            using (var stringReader = new StringReader(xml))
            {
                collection = _testCaseReader.Read(stringReader);

                Case item = collection.TestCases.First(x => x.Id == testCase.Id);

                item.Id = testCase.Id;
                item.ShortDescription = testCase.ShortDescription;
                item.ErrorMessage = testCase.ErrorMessage;
                item.Headers = testCase.Headers.Select(x => new HeaderItem(x.Key, x.Value)).ToList();
                item.LogRequest = testCase.LogRequest;
                item.LogResponse = testCase.LogResponse;
                item.LongDescription = testCase.LongDescription;
                item.Method = testCase.Method;
                item.ParentFilename = testCase.ParentFilename;
                item.ParseResponses = testCase.ParseResponses;
                item.PostBody = testCase.PostBody;
                item.VerifyPositives = testCase.VerifyPositives;
                item.VerifyNegatives = testCase.VerifyNegatives;
                item.ShortDescription = testCase.ShortDescription;
                item.Url = testCase.Url;
                item.Sleep = testCase.Sleep;
                item.PostType = testCase.PostType;
                item.VerifyResponseCode = testCase.VerifyResponseCode;
            }

            string contents = _testCaseWriter.Write(collection);

            return _fileHandler.WriteAllText(fullPath, contents);
        }

        public CaseCollection GetTestCaseCollection(string filename, string teamName)
        {
            var fullPath = _fileHandler.GetFileFullPath(filename, teamName);
            string xml = _fileHandler.ReadAllText(fullPath);

            using (var stringReader = new StringReader(xml))
            {
                return _testCaseReader.Read(stringReader);
            }
        }

        public IEnumerable<string> ListCasesForTeam(string teamName)
        {
            string fullPath = _fileHandler.GetFullPath(teamName);

            var fileNames = _fileHandler.GetFileNames(fullPath);

            return fileNames;
        }
    }
}