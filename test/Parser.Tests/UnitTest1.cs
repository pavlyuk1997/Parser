using Atata;
using NUnit.Framework;

namespace Parser.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            AtataContext.GlobalConfiguration
                .UseChrome()
                    .WithArguments("start-maximized")
                .UseBaseUrl("https://atata.io")
                .UseCulture("en-US")
                .UseAllNUnitFeatures();

            AtataContext.GlobalConfiguration.AutoSetUpDriverToUse();
        }

        [Test]
        public void Test1()
        {
            var currentContext = AtataContext.Configure().Build();
            var pageObject = Go.To<OrdinaryPage>(url: "https://atata.io/getting-started/");
            var parcer = new ElementsParser();
            var elements = parcer.Parse<Link<OrdinaryPage>, OrdinaryPage>(pageObject).Result;
        }
    }
}