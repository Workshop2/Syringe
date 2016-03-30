﻿using System;
using System.Collections.Generic;
using Syringe.Core.Tests;

namespace Syringe.Core.Repositories
{
    public interface ITestRepository
    {
        IEnumerable<string> ListFilesForBranch(string branchName);
        TestFile GetTestFile(string filename, string branchName);
        Test GetTest(string filename, string branchName, int position);
        bool SaveTest(Test test, string branchName);
        bool CreateTest(Test test, string branchName);
        bool DeleteTest(int position, string fileName, string branchName);
        bool CreateTestFile(TestFile testFile, string branchName);
        bool UpdateTestFile(TestFile testFile, string branchName);
        string GetXml(string filename, string branchName);
    }
}