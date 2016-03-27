﻿using System;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using Syringe.Core.Configuration;
using Syringe.Core.IO;

namespace Syringe.Tests.Unit.FileOperations
{
    [TestFixture]
    public class FileHandlerTests
    {
        private Mock<IConfiguration> _configurationMock;

        private readonly string _testFile = "test.xml";
        private readonly string _testTeamDirectory = TestContext.CurrentContext.TestDirectory + "\\test";
        private readonly string _testFileFullPath = TestContext.CurrentContext.TestDirectory + "\\test\\test.xml";
        private readonly string _testWriteFileFullPath = TestContext.CurrentContext.TestDirectory + "\\test\\testWrite.xml";
        private readonly string _teamName = "test";


        [TestFixtureSetUp]
        public void SetupFixture()
        {
            if (!Directory.Exists(_testTeamDirectory))
                Directory.CreateDirectory(_testTeamDirectory);

            if (!File.Exists(_testFileFullPath))
            {
                using (StreamWriter sw = File.CreateText(_testFileFullPath))
                {
                    sw.WriteLine("Test Data");
                }
            }
        }

        [SetUp]
        public void Setup()
        {
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(x => x.TestCasesBaseDirectory).Returns(TestContext.CurrentContext.TestDirectory);
        }

        [Test]
        public void GetFileFullPath_should_throw_FileNotFoundException_if_file_is_missing()
        {
            // given
            var fileHandler = new FileHandler(_configurationMock.Object);
            var fileName = "filedoesnotexist.xml";

            // when + then
            Assert.Throws<FileNotFoundException>(() => fileHandler.GetFileFullPath(_teamName, fileName));
        }
        [Test]
        public void GetFileFullPath_should_return_file_path_if_file_exists()
        {
            // given
            var fileHandler = new FileHandler(_configurationMock.Object);

            // when
            var fileFullPath = fileHandler.GetFileFullPath(_teamName, _testFile);

            // then
            Assert.AreEqual(_testFileFullPath, fileFullPath);
        }

        [Test]
        public void CreateFileFullPath_should_create_correct_path()
        {
            // given
            var fileHandler = new FileHandler(_configurationMock.Object);

            // when
            var fileFullPath = fileHandler.CreateFileFullPath(_teamName, _testFile);

            // then
            Assert.AreEqual(_testFileFullPath, fileFullPath);
        }

        [Test]
        public void FileExists_should_return_true_if_file_exists()
        {
            // given
            var fileHandler = new FileHandler(_configurationMock.Object);

            // when
            var fileExists = fileHandler.FileExists(_testFileFullPath);

            // then
            Assert.IsTrue(fileExists);
        }

        [Test]
        public void FileExists_should_return_false_if_does_not_exist()
        {
            // given
            var fileHandler = new FileHandler(_configurationMock.Object);

            // when
            var fileExists = fileHandler.FileExists("somefakepath/filedoesnotexist.xml");

            // then
            Assert.IsFalse(fileExists);
        }

        [Test]
        public void GetTeamDirectoryFullPath_should_throw_FileNotFoundException_if_file_is_missing()
        {
            // given
            var fileHandler = new FileHandler(_configurationMock.Object);
            var teamName = "teamdoesnotexist";

            // when + then
            Assert.Throws<DirectoryNotFoundException>(() => fileHandler.GetTeamDirectoryFullPath(teamName));
        }

        [Test]
        public void GetTeamDirectoryFullPath_should_return_file_path_if_file_exists()
        {
            // given
            var fileHandler = new FileHandler(_configurationMock.Object);

            // when
            var directoryFullPath = fileHandler.GetTeamDirectoryFullPath(_teamName);

            // then
            Assert.AreEqual(_testTeamDirectory, directoryFullPath);
        }

        [Test]
        public void ReadAllText_should_return_file_contents()
        {
            // given
            var fileHandler = new FileHandler(_configurationMock.Object);

            // when
            var allText = fileHandler.ReadAllText(_testFileFullPath);

            // then
            Assert.IsTrue(allText.Contains("Test Data"));
        }

        [Test]
        public void WriteAllText_should_return_true_when_contents_written()
        {
            // given
            var fileHandler = new FileHandler(_configurationMock.Object);

            // when
            var allText = fileHandler.WriteAllText(_testWriteFileFullPath, "test");

            // then
            Assert.IsTrue(allText);
        }

        [Test]
        public void GetFileNames_should_get_filenames_list()
        {
            // given

            var fileHandler = new FileHandler(_configurationMock.Object);

            // when
            var allText = fileHandler.GetFileNames(_testTeamDirectory);

            // then
            Assert.IsTrue(allText.Count() == 1);
            Assert.IsTrue(allText.First() == _testFile);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void CreateFilename_should_throw_argument_null_exception_when_filename_is_empty(string fileName)
        {
            // given + when + then
            Assert.Throws<ArgumentNullException>(() => new FileHandler(_configurationMock.Object).CreateFilename(fileName));
        }

        [TestCase("test")]
        [TestCase("cases")]
        public void CreateFilename_should_add_xml_extension_if_it_is_missing(string fileName)
        {
            // given
            var fileHandler = new FileHandler(_configurationMock.Object);

            // when
            var createdFileName = fileHandler.CreateFilename(fileName);

            // then
            Assert.AreEqual(string.Concat(fileName, ".xml"), createdFileName);
        }

        [TestCase("test.xml")]
        [TestCase("cases.xml")]
        public void CreateFilename_should__return_correct_name_if_passed_in_correctly(string fileName)
        {
            // given
            var fileHandler = new FileHandler(_configurationMock.Object);

            // when
            var createdFileName = fileHandler.CreateFilename(fileName);

            // then
            Assert.AreEqual(fileName, createdFileName);
        }

        [Test]
        public void WriteAllText_should_return_falseu_when_text_failed_to_write()
        {
            // given
            var fileHandler = new FileHandler(_configurationMock.Object);

            // when
            var allText = fileHandler.WriteAllText("invalidPath%*()", "test");

            // then
            Assert.IsFalse(allText);
        }

        [TestFixtureTearDown]
        public void TearDownFixture()
        {
            Directory.Delete(_testTeamDirectory, true);
        }
    }
}
