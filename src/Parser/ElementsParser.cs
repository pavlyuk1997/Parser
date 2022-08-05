using Atata;
using OpenQA.Selenium;
using System.Threading.Tasks;

namespace Parser
{
    public class ElementsParser
    {
        public async Task<IEnumerable<PageOblectElement>> Parse<TControl, TOwner>(TOwner pageObject)
            where TControl : Control<TOwner>
            where TOwner : Page<TOwner>
        {
            ControlList<TControl, TOwner> controls = pageObject.FindAll<TControl>();
            List<PageOblectElement> pageObjectElements = new List<PageOblectElement>();
            var getPageObjectTasks = new List<Task<PageOblectElement>>();
            foreach (var control in controls)
            {
                getPageObjectTasks.Add(Task.Run(() => GetPageObjectElement(pageObject, control)));
            }

            return await Task.WhenAll(getPageObjectTasks);
        }

        private PageOblectElement GetPageObjectElement<TControl, TOwner>(TOwner pageObject, TControl control)
            where TControl : Control<TOwner>
            where TOwner : Page<TOwner>
        {
            var findStrategy = GetFindStrategy(pageObject, control);
            if (findStrategy == String.Empty)
            {
                return null;
            }
            var propertyAsString = PropertyStrategy.GetElementPropertyInStringForm(pageObject, control);
            PageOblectElement pageOblectElement = new PageOblectElement
            {
                Strategy = findStrategy,
                Element = propertyAsString
            };
            return pageOblectElement;
        }

        private static string GetFindStrategy<TControl, TOwner>(TOwner page, TControl element)
            where TControl : Control<TOwner>
            where TOwner : Page<TOwner>
        {
            string findStrategy;

            if (!string.IsNullOrEmpty(element.Attributes.Id))
            {
                var elements = page.Scope.FindElements(By.XPath($"//a[@id = '{element.Attributes.Id}']"));

                if (elements.Count() == 1)
                {
                    return $" [FindById(\"{element.Attributes.Id})\"]";
                }
            }

            if (!string.IsNullOrEmpty(element.Attributes.TextContent.Value))
            {
                var elements = page.Scope.FindElements(By.XPath($"//a[normalize-space(.) = '{element.Attributes.TextContent.Value.Trim()}']"))
                    .Select(x => x.Displayed && x.Enabled);

                if (elements.Count() == 1)
                {
                    return $" [FindByContent(\"{element.Attributes.TextContent.Value}\")]";
                }
            }

            foreach (var classAttribute in element.Attributes.Class.Value)
            {
                if (!string.IsNullOrEmpty(classAttribute))
                {
                    var elements = page.Scope.FindElements(OpenQA.Selenium.By.ClassName(classAttribute));
                    if (elements.Count() == 1)
                    {
                        findStrategy = $" [FindByClass(\"{classAttribute})\"]";
                    }
                    else
                    {
                        continue;
                    }
                    return findStrategy;
                }
            }

            string xpath = GetXpathOfElement(page, element);

            var parentElement = (UIComponent<TOwner>)element;

            while (page.Scope.FindElements(By.XPath($"//{xpath}")).Count > 1)
            {
                parentElement = parentElement.Find<Control<TOwner>>(new FindFirstAttribute { OuterXPath = "parent::" });
                try
                {
                    xpath = GetXpathOfElement(page, parentElement) + $"/{xpath}";
                }
                catch
                {
                    return String.Empty;
                }
            }

            findStrategy = $"[FindByXPath(\"//{xpath}\")]";

            return findStrategy;
        }

        private static string GetXpathOfElement<TOwner>(TOwner page, UIComponent<TOwner> element)
            where TOwner : Page<TOwner>
        {
            string xpath = element.Scope.TagName;

            if (!string.IsNullOrEmpty(element.Attributes.Id))
            {
                xpath += $"[@id = '{element.Attributes.Id}']";
            }

            foreach (var classAttribute in element.Attributes.Class.Value)
            {
                if (!string.IsNullOrEmpty(classAttribute))
                {
                    var elements = page.Scope.FindElements(OpenQA.Selenium.By.ClassName(classAttribute));
                    if (elements.Count() == 1)
                    {
                        xpath += $"[contains(@class , \"{classAttribute}\")]";
                    }
                    else
                    {
                        continue;
                    }
                    return xpath;
                }
            }

            if (!string.IsNullOrEmpty(element.Attributes.TextContent) && element.Attributes.TextContent.Value.Trim().Length < 30)
            {
                xpath += $"[normalize-space(.) = '{element.Attributes.TextContent.Value.Trim()}']";
            }

            return xpath;
        }
    }
}