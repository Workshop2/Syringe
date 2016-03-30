﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Syringe.Core.Http;
using Syringe.Core.Http.Logging;
using Syringe.Core.Repositories;
using Syringe.Core.Runner;
using Syringe.Core.Tests;
using Syringe.Core.Tests.Results;
using Syringe.Tests.StubsMocks;

namespace Syringe.Tests.Unit.Runner
{
	public class TestSessionRunnerTests
	{
		private HttpClientMock _httpClientMock;
		private HttpResponse _httpResponse;

		[SetUp]
		public void Setup()
		{
			TestHelpers.EnableLogging();
		}

		private ITestCaseSessionRepository GetRepository()
		{
			return new TestCaseSessionRepositoryMock();
		}

		[Test]
		public async Task Run_should_repeat_testcases_from_repeat_property()
		{
			// Arrange
			TestFileRunner runner = CreateRunner();

			var caseCollection = CreateCaseCollection(new[]
			{
				new Test() { Url = "foo1" },
			});
			caseCollection.Repeat = 10;

			// Act
			TestFileResult session = await runner.RunAsync(caseCollection);

			// Assert
			Assert.That(session.TestResults.Count(), Is.EqualTo(10));
		}

		[Test]
		public async Task Run_should_set_MinResponseTime_and_MaxResponseTime_from_http_response_times()
		{
			// Arrange
			var response = new HttpResponse();
			response.ResponseTime = TimeSpan.FromSeconds(5);

			HttpClientMock httpClient = new HttpClientMock(response);
			httpClient.ResponseTimes = new List<TimeSpan>()
			{
				// Deliberately mixed up order
				TimeSpan.FromSeconds(5),
				TimeSpan.FromSeconds(88),
				TimeSpan.FromSeconds(3),
				TimeSpan.FromSeconds(10)
			};
			httpClient.Response = response;

			var runner = new TestFileRunner(httpClient, GetRepository());

			var caseCollection = CreateCaseCollection(new[]
			{
				new Test() { Url = "foo1" },
				new Test() { Url = "foo2" },
				new Test() { Url = "foo3" },
				new Test() { Url = "foo4" },
			});

			// Act
			TestFileResult session = await runner.RunAsync(caseCollection);

			// Assert
			Assert.That(session.MinResponseTime, Is.EqualTo(TimeSpan.FromSeconds(3)));
			Assert.That(session.MaxResponseTime, Is.EqualTo(TimeSpan.FromSeconds(88)));
		}

		[Test]
		public async Task Run_should_populate_StartTime_and_EndTime_and_TotalRunTime()
		{
			// Arrange
			var beforeStart = DateTime.UtcNow;

			var response = new HttpResponse();
			response.ResponseTime = TimeSpan.FromSeconds(5);

			HttpClientMock httpClient = new HttpClientMock(response);
			var runner = new TestFileRunner(httpClient, GetRepository());

			var caseCollection = CreateCaseCollection(new[]
			{
				new Test() { Url = "foo1" },
			});

			// Act
			TestFileResult session = await runner.RunAsync(caseCollection);

			// Assert
			Assert.That(session.StartTime, Is.GreaterThanOrEqualTo(beforeStart));
			Assert.That(session.EndTime, Is.GreaterThanOrEqualTo(session.StartTime));
			Assert.That(session.TotalRunTime, Is.EqualTo(session.EndTime - session.StartTime));
		}

		[Test]
		public async Task Run_should_set_parsed_variables()
		{
			// Arrange
			TestFileRunner runner = CreateRunner();
			_httpClientMock.Response.Content = "some content";
			_httpClientMock.Response.StatusCode = HttpStatusCode.OK;

			var caseCollection = CreateCaseCollection(new[]
			{
				new Test()
				{
					Url = "case1",
					VerifyResponseCode = HttpStatusCode.OK,
					CapturedVariables = new List<CapturedVariable>()
					{
						new CapturedVariable("1", "some content")
					},
					VerifyPositives = new List<Assertion>()
					{
						new Assertion("positive-1", "{capturedvariable1}", AssertionType.Positive)
					},
				}
			});

			// Act
			TestFileResult session = await runner.RunAsync(caseCollection);

			// Assert
			Assert.That(session.TestResults.Single().PositiveAssertionResults[0].Success, Is.True);
		}

		[Test]
		public async Task Run_should_set_parseresponsevariables_across_testcases()
		{
			// Arrange
			TestFileRunner runner = CreateRunner();
			_httpClientMock.Responses = new List<HttpResponse>()
			{
				new HttpResponse()
				{
					StatusCode = HttpStatusCode.OK,
					Content = "1st content SECRET_KEY"
				},
				new HttpResponse()
				{
					StatusCode = HttpStatusCode.OK,
					Content = "2nd content - SECRET_KEY in here to match"
				},
				new HttpResponse()
				{
					StatusCode = HttpStatusCode.OK,
					Content = "3rd content - SECRET_KEY in here to match"
				}
			};

			var caseCollection = CreateCaseCollection(new[]
			{
				new Test()
				{
					Url = "case1",
					VerifyResponseCode = HttpStatusCode.OK,
					CapturedVariables = new List<CapturedVariable>()
					{
						new CapturedVariable("1", @"(SECRET_KEY)")
					},
				},
				new Test()
				{
					Url = "case2",
					VerifyResponseCode = HttpStatusCode.OK,
					CapturedVariables = new List<CapturedVariable>()
					{
						new CapturedVariable("2", @"(SECRET_KEY)")
					},
					VerifyPositives = new List<Assertion>()
					{
						// Test the parsedresponse variable from the 1st case
						new Assertion("positive-for-case-2", "{capturedvariable1}", AssertionType.Positive)
					},
				},
				new Test()
				{
					Url = "case3",
					VerifyResponseCode = HttpStatusCode.OK,
					VerifyPositives = new List<Assertion>()
					{
						// Test the parseresponse variable from the 1st case
						new Assertion("positive-for-case-3", "{capturedvariable2}", AssertionType.Positive)
					},
				}
			});

			// Act
			TestFileResult session = await runner.RunAsync(caseCollection);

			// Assert
			Assert.That(session.TestResults.ElementAt(1).PositiveAssertionResults[0].Success, Is.True);
			Assert.That(session.TestResults.ElementAt(2).PositiveAssertionResults[0].Success, Is.True);
		}

		[Test]
		public async Task Run_should_set_testresult_success_and_response_when_httpcode_passes()
		{
			// Arrange
			TestFileRunner runner = CreateRunner();
			_httpClientMock.Response.StatusCode = HttpStatusCode.OK;

			var caseCollection = CreateCaseCollection(new[]
			{
				new Test()
				{
					Url = "foo1",
					VerifyResponseCode = HttpStatusCode.OK
				},
			});

			// Act
			TestFileResult session = await runner.RunAsync(caseCollection);

			// Assert
			Assert.That(session.TestResults.Single().Success, Is.True);
			Assert.That(session.TestResults.Single().HttpResponse, Is.EqualTo(_httpClientMock.Response));
		}

		[Test]
		public async Task Run_should_set_testresult_success_and_response_when_httpcode_fails()
		{
			// Arrange
			TestFileRunner runner = CreateRunner();
			_httpClientMock.Response.StatusCode = HttpStatusCode.OK;

			var caseCollection = CreateCaseCollection(new[]
			{
				new Test()
				{
					Url = "foo1",
					VerifyResponseCode = HttpStatusCode.Ambiguous
				},
			});

			// Act
			TestFileResult session = await runner.RunAsync(caseCollection);

			// Assert
			Assert.That(session.TestResults.Single().Success, Is.False);
			Assert.That(session.TestResults.Single().HttpResponse, Is.EqualTo(_httpClientMock.Response));
		}

		[Test]
		public async Task Run_should_set_message_from_case_errormessage_when_httpcode_fails()
		{
			// Arrange
			TestFileRunner runner = CreateRunner();

			var caseCollection = CreateCaseCollection(new[]
			{
				new Test() { Url = "foo1", ErrorMessage = "It broke", VerifyResponseCode = HttpStatusCode.Ambiguous},
			});

			// Act
			TestFileResult session = await runner.RunAsync(caseCollection);

			// Assert
			Assert.That(session.TestResults.Single().Message, Is.EqualTo("It broke"));
		}


		[Test]
		public async Task Run_should_save_testcasesession_to_repository()
		{
			// Arrange
			var repository = new TestCaseSessionRepositoryMock();

			TestFileRunner runner = CreateRunner();
			runner.Repository = repository;

			var caseCollection = CreateCaseCollection(new[]
			{
				new Test() { Url = "foo1"},
			});

			// Act
			await runner.RunAsync(caseCollection);

			// Assert
			Assert.That(repository.SavedSession, Is.Not.Null);
			Assert.That(repository.SavedSession.TestResults.Count(), Is.EqualTo(1));
		}

		[Test]
		public async Task Run_should_verify_positive_and_negative_items_when_httpcode_passes()
		{
			// Arrange
			TestFileRunner runner = CreateRunner();
			_httpClientMock.Response.StatusCode = HttpStatusCode.OK;
			_httpClientMock.Response.Content = "some content";

			var caseCollection = CreateCaseCollection(new[]
			{
				new Test()
				{
					Url = "foo1",
					VerifyResponseCode = HttpStatusCode.OK,
					VerifyPositives = new List<Assertion>()
					{
						new Assertion("positive-1", "some content", AssertionType.Positive)
					},
					VerifyNegatives = new List<Assertion>()
					{
						new Assertion("negative-1", "no text like this", AssertionType.Negative)
					}
				},
			});

			// Act
			TestFileResult session = await runner.RunAsync(caseCollection);

			// Assert
			var result = session.TestResults.Single();
			Assert.That(result.Success, Is.True);
			Assert.That(result.PositiveAssertionResults.Count, Is.EqualTo(1));
			Assert.That(result.PositiveAssertionResults[0].Success, Is.True);

			Assert.That(result.NegativeAssertionResults.Count, Is.EqualTo(1));
			Assert.That(result.NegativeAssertionResults[0].Success, Is.True);
		}

		[Test]
		public async Task Run_should_notify_observers_of_existing_results()
		{
			// Arrange
			var observedResults = new List<TestResult>();

			TestFileRunner runner = CreateRunner();

			var caseCollection = CreateCaseCollection(new[]
			{
				new Test() { Url = "foo1" },
				new Test() { Url = "foo2" },
				new Test() { Url = "foo3" }
			});

			await runner.RunAsync(caseCollection);

			// Act
			runner.Subscribe(r => { observedResults.Add(r); });

			// Assert
			Assert.That(observedResults.Select(r => r.ActualUrl), Is.EquivalentTo(new[] { "foo1", "foo2", "foo3" }), "Should have observed all of the results.");
		}

		[Test]
		public async Task Run_should_notify_observers_of_new_results()
		{
			// Arrange
			TestFileRunner runner = CreateRunner();

			var caseCollection = CreateCaseCollection(new[]
			{
				new Test() { Url = "foo1" },
				new Test() { Url = "foo2" },
				new Test() { Url = "foo3" }
			});

			var observedResults = new List<TestResult>();

			// Act
			runner.Subscribe(r => { observedResults.Add(r); });

			await runner.RunAsync(caseCollection);

			// Assert
			Assert.That(observedResults.Select(r => r.ActualUrl), Is.EquivalentTo(new[] { "foo1", "foo2", "foo3" }), "Should have observed all of the results.");
		}

		[Test]
		public async Task Run_should_not_notify_disposed_observers_of_new_results()
		{
			// Arrange
			var httpClientMock = new Mock<IHttpClient>();

			IDisposable subscription = null;

			httpClientMock
				.Setup(
					c =>
						c.ExecuteRequestAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
							It.IsAny<IEnumerable<HeaderItem>>(), It.IsAny<HttpLogWriter>()))
				.Returns(Task.FromResult(new HttpResponse()));

			// Dispose of the subscription before processing the third request.
			httpClientMock
				.Setup(c => c.ExecuteRequestAsync(It.IsAny<string>(), "foo3", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<HeaderItem>>(), It.IsAny<HttpLogWriter>()))
				.Callback(() => { if (subscription != null) subscription.Dispose(); })
				.Returns(Task.FromResult(new HttpResponse()));

			TestFileRunner runner = new TestFileRunner(httpClientMock.Object, GetRepository());

			var caseCollection = CreateCaseCollection(new[]
			{
				new Test() { Url = "foo1" },
				new Test() { Url = "foo2" },
				new Test() { Url = "foo3" }
			});

			var observedResults = new List<TestResult>();

			// Act
			subscription = runner.Subscribe(r => { observedResults.Add(r); });

			await runner.RunAsync(caseCollection);

			// Assert
			Assert.That(observedResults.Select(r => r.ActualUrl), Is.EquivalentTo(new[] { "foo1", "foo2" }), "Should not have included the result after having been disposed.");
		}

		[Test]
		public async Task Run_should_notify_subscribers_of_completion_when_test_case_session_ends()
		{
			// Arrange
			TestFileRunner runner = CreateRunner();

			var caseCollection = CreateCaseCollection(new[]
			{
				new Test() { Url = "foo1" },
				new Test() { Url = "foo2" },
				new Test() { Url = "foo3" }
			});

			var completed = false;

			runner.Subscribe(r => { }, onCompleted: () => completed = true);

			Assume.That(completed, Is.False);

			// Act
			await runner.RunAsync(caseCollection);

			// Assert
			Assert.That(completed, Is.True, "Should have notified of completion.");
		}

		[Test]
		public async Task Run_should_notify_subscribers_of_result_on_error()
		{
			// Arrange
			var httpClientMock = new Mock<IHttpClient>();

			// Throw an error.
			httpClientMock
				.Setup(c => c.ExecuteRequestAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<HeaderItem>>(), new HttpLogWriter()))
				.Throws(new InvalidOperationException("Bad"));

			TestFileRunner runner = new TestFileRunner(httpClientMock.Object, GetRepository());

			var caseCollection = CreateCaseCollection(new[]
			{
				new Test() { Url = "foo1" },
				new Test() { Url = "foo2" },
				new Test() { Url = "foo3" }
			});

			TestResult capturedResult = null;
			runner.Subscribe(r => capturedResult = r);

			// Act
			await runner.RunAsync(caseCollection);

			// Assert
			Assert.That(capturedResult, Is.Not.Null, "Should have notified of the result.");
			Assert.That(capturedResult.Success, Is.False, "Should not have succeeded.");
		}

		private TestFileRunner CreateRunner()
		{
			_httpResponse = new HttpResponse();
			_httpClientMock = new HttpClientMock(_httpResponse);

			return new TestFileRunner(_httpClientMock, GetRepository());
		}

		private TestFile CreateCaseCollection(Test[] tests)
		{
			var testCases = new List<Test>();
			testCases.AddRange(tests);

			var collection = new TestFile();
			collection.Tests = testCases;

			return collection;
		}
	}
}
