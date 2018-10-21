﻿using NUnit.Engine;
using NUnit.Engine.Extensibility;
using System.Xml;
using System;
using System.IO;
using Difido;
using Difido.Model;
using Difido.Model.Test;
using System.Web.Script.Serialization;

namespace AddIn
{
    [Extension(Description = "Difido Addin")]
    public class NUnitDifidoAddin : ITestEventListener
    {

        IReportDispatcher dispatcher = ReportManager.Instance;

        public void OnTestEvent(string xmlEventString)
        {            
            {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xmlEventString);
                    if (xmlDoc.SelectSingleNode("/start-run") != null)
                    {
                        StartRun(xmlDoc);
                    }
                    else if (xmlDoc.SelectSingleNode("/test-run") != null)
                    {
                        EndRun(xmlDoc);
                    }
                    else if (xmlDoc.SelectSingleNode("/start-suite") != null)
                    {
                        StartSuite(xmlDoc);
                    }
                    else if (xmlDoc.SelectSingleNode("/test-suite") != null)
                    {
                        EndSuite(xmlDoc);
                    }
                    else if (xmlDoc.SelectSingleNode("/start-test") != null)
                    {
                        StartTest(xmlDoc);
                    }
                    else if (xmlDoc.SelectSingleNode("/test-case") != null)
                    {
                        EndTest(xmlDoc);
                        if (xmlDoc.SelectSingleNode("//output") != null)
                        {
                            TestOutput(xmlDoc);
                        }

                        if (xmlDoc.SelectSingleNode("//failure") != null)
                        {
                            TestFailures(xmlDoc);
                        }

                        if (xmlDoc.SelectSingleNode("//assertion") != null)
                        {
                            TestAssertions(xmlDoc);
                        }


                }
            }
        }

        private void EndTest(XmlDocument xmlDoc)
        {            
            EndTestInfo info = new EndTestInfo();
            info.Id = xmlDoc.SelectSingleNode("/*/@id").Value;
            info.FullName = xmlDoc.SelectSingleNode("/*/@fullname").Value;
            info.MethodName = xmlDoc.SelectSingleNode("/*/@methodname").Value;
            info.ClassName = xmlDoc.SelectSingleNode("/*/@classname").Value;
            info.RunState = xmlDoc.SelectSingleNode("/*/@runstate").Value;
            info.Seed = Convert.ToInt64(xmlDoc.SelectSingleNode("/*/@seed").Value);
            info.Result = ParseResult(xmlDoc.SelectSingleNode("/*/@result").Value);
            info.StartTime = xmlDoc.SelectSingleNode("/*/@start-time").Value;
            info.EndTime = xmlDoc.SelectSingleNode("/*/@end-time").Value;
            info.Duration = Convert.ToDouble(xmlDoc.SelectSingleNode("/*/@duration").Value);
            info.Asserts = Convert.ToInt32(xmlDoc.SelectSingleNode("/*/@asserts").Value);
            info.ParentId = xmlDoc.SelectSingleNode("/*/@parentId").Value;
            dispatcher.EndTest(info);

        }

        private TestStatus ParseResult(string value)
        {
            if ("Passed".Equals(value))
            {
                return TestStatus.success;
            }
            else if ("Failed".Equals(value))
            {
                return TestStatus.failure;
            }
            else if ("Inconclusive".Equals(value))
            {
                return TestStatus.warning;
            }
            else
            {
                return TestStatus.success;
            }
        }

        private void StartTest(XmlDocument xmlDoc)
        {            
            StartTestInfo info = new StartTestInfo();
            info.Id = xmlDoc.SelectSingleNode("/*/@id").Value;
            info.ParentId = xmlDoc.SelectSingleNode("/*/@parentId").Value;
            info.Name = xmlDoc.SelectSingleNode("/*/@name").Value;
            info.FullName = xmlDoc.SelectSingleNode("/*/@fullname").Value;
            info.Type = xmlDoc.SelectSingleNode("/*/@type").Value;
            dispatcher.StartTest(info);
        }

        private void EndSuite(XmlDocument xmlDoc)
        {            
            if (null == xmlDoc.SelectSingleNode("/*/@parentId"))
            {
                // From some reason NUnit is sending all the data twice, in two different 
                // formats. In in a single xml element for each event and the other one is 
                // a larger XML element that holds all the data.
                // Since we already collected the data in the first time, we don't need it
                // again and we can just ignore it from now on.
                return;
            }
            EndSuiteInfo info = new EndSuiteInfo();
            info.Type = xmlDoc.SelectSingleNode("/*/@type").Value;
            info.Id = xmlDoc.SelectSingleNode("/*/@id").Value;
            info.Name = xmlDoc.SelectSingleNode("/*/@name").Value;
            info.FullName= xmlDoc.SelectSingleNode("/*/@fullname").Value;
            XmlNode node = xmlDoc.SelectSingleNode("/*/@classname");
            if (node != null)
            {
                info.ClassName = node.Value;
            }
            
            info.RunState = xmlDoc.SelectSingleNode("/*/@runstate").Value;
            info.TestCaseCount = Convert.ToInt32(xmlDoc.SelectSingleNode("/*/@testcasecount").Value);
            info.Result = ParseResult(xmlDoc.SelectSingleNode("/*/@result").Value);
            info.StartTime = xmlDoc.SelectSingleNode("/*/@start-time").Value;
            info.EndTime = xmlDoc.SelectSingleNode("/*/@end-time").Value;
            info.Duration = Convert.ToDouble(xmlDoc.SelectSingleNode("/*/@duration").Value);
            info.Total = Convert.ToInt32(xmlDoc.SelectSingleNode("/*/@total").Value);
            info.Passed= Convert.ToInt32(xmlDoc.SelectSingleNode("/*/@passed").Value);
            info.Failed = Convert.ToInt32(xmlDoc.SelectSingleNode("/*/@failed").Value);
            info.Warnings = Convert.ToInt32(xmlDoc.SelectSingleNode("/*/@warnings").Value);
            info.Inconclusive = Convert.ToInt32(xmlDoc.SelectSingleNode("/*/@inconclusive").Value);
            info.Skipeed = Convert.ToInt32(xmlDoc.SelectSingleNode("/*/@skipped").Value);
            info.Asserts= Convert.ToInt32(xmlDoc.SelectSingleNode("/*/@asserts").Value);
            info.ParentId = xmlDoc.SelectSingleNode("/*/@parentId").Value;
            dispatcher.EndSuite(info);
        }

        private void StartSuite(XmlDocument xmlDoc)
        {            
            StartSuiteInfo info = new StartSuiteInfo(); 
            info.Id = xmlDoc.SelectSingleNode("/*/@id").Value;
            info.ParentId = xmlDoc.SelectSingleNode("/*/@parentId").Value;
            info.Name = xmlDoc.SelectSingleNode("/*/@name").Value;
            info.FullName = xmlDoc.SelectSingleNode("/*/@fullname").Value;
            info.Type = xmlDoc.SelectSingleNode("/*/@type").Value;
            dispatcher.StartSuite(info);
            
        }

        private void EndRun(XmlDocument xmlDoc)
        {            
            dispatcher.EndRun();
        }

        private void StartRun(XmlDocument xmlDoc)
        {            
         
            
        }

        private void TestFailures(XmlDocument xmlDoc)
        {
            ReportElement element = new ReportElement();
            element.time = CurrentTime();
            element.title = "Failures";
            element.type = ReportElementType.step.ToString();
            dispatcher.Report(element);

            element = new ReportElement();
            element.time = CurrentTime();
            element.title = xmlDoc.SelectSingleNode("/*/failure/message").InnerText;
            element.message = xmlDoc.SelectSingleNode("/*/failure/stack-trace").InnerText;
            element.status = TestStatus.failure.ToString();
            dispatcher.Report(element);

        }

        private void TestAssertions(XmlDocument xmlDoc)
        {
            ReportElement element = new ReportElement();
            element.time = CurrentTime();
            element.title = "Assertions";
            element.type = ReportElementType.step.ToString();
            dispatcher.Report(element);

            element = new ReportElement();
            element.time = CurrentTime();
            element.title = xmlDoc.SelectSingleNode("/*/assertions/assertion/message").InnerText;
            element.message = xmlDoc.SelectSingleNode("/*/assertions/assertion/stack-trace").InnerText;
            string resultString = xmlDoc.SelectSingleNode("/*/assertions/assertion/@result").InnerText;
            element.status = "Failed".Equals(resultString) ? TestStatus.failure.ToString() : TestStatus.success.ToString();
            dispatcher.Report(element);
        }

        private static string CurrentTime()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }

        private void TestOutput(XmlDocument xmlDoc)
        {
            using (StringReader sr = new StringReader(xmlDoc.SelectSingleNode("/*/output").InnerText))            
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    ReportElement element = null;
                    try
                    {
                        element = new JavaScriptSerializer().Deserialize<ReportElement>(line);
                    } catch
                    {
                        element = new ReportElement();
                        element.time = CurrentTime();
                        element.title = line;
                    }
                    dispatcher.Report(element);
                }
            }
        }

    }
}