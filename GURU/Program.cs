using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Net;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        ExtractionDB extractionDB = new ExtractionDB();

        string component, pathImage;
        // component = "div.prod-selrow[data-id='10000']"; // проци
        // pathImage = "Image\\Processors";
        //extractionDB.Parse(component, pathImage, pathImage + $"\\Characteristics.txt");

        //component = "div.prod-selrow[data-id='14000']"; // метеринки
        //pathImage = "Image\\Motherboards";
        //extractionDB.Parse(component, pathImage, pathImage + $"\\Characteristics.txt");

        //component = "div.prod-selrow[data-id='16400']"; //оперативка
        //pathImage = "Image\\RAM";
        //extractionDB.Parse(component, pathImage, pathImage + $"\\Characteristics.txt");

        component = "div.prod-selrow[data-id='20000']"; //видеокарта
        pathImage = "Image\\VideoCards";
        extractionDB.Parse(component, pathImage, pathImage + $"\\Characteristics.txt");

        //component = "div.prod-selrow[data-id='24000']"; //корпус
        //pathImage = "Image\\Housing";
        //extractionDB.Parse(component, pathImage, pathImage + $"\\Characteristics.txt");

        //component = "div.prod-selrow[data-id='40000']"; //БП
        //pathImage = "Image\\PowerSupplies";
        //extractionDB.Parse(component, pathImage, pathImage + $"\\Characteristics.txt");

    }
}

class ExtractionDB
{
    //string component, pathImage, nameTXT;

    void ScrollElementIntoView(IWebDriver driver, IWebElement element)
    {
        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
        js.ExecuteScript("arguments[0].scrollIntoView({ behavior: 'auto', block: 'center', inline: 'center' }); window.scrollBy(0, -15);", element);

    }
    bool CanScrollDown(IWebDriver driver)
    {
        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
        long documentHeight = (long)js.ExecuteScript("return document.documentElement.scrollHeight;");
        long windowHeight = (long)js.ExecuteScript("return window.innerHeight;");
        long scrollY = (long)js.ExecuteScript("return window.scrollY;");
        return scrollY + windowHeight < documentHeight;
    }
    void DownloadImage(string imageUrl, string destinationPath)
    {
        using (WebClient client = new WebClient())
        {
            client.DownloadFile(imageUrl, destinationPath);
        }
    }
    bool IsElementInView(IWebDriver driver, IWebElement element)
    {
        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
        long windowHeight = (long)js.ExecuteScript("return window.innerHeight;");
        long elementTop = (long)js.ExecuteScript("return arguments[0].getBoundingClientRect().top;", element);

        return elementTop >= 0 && elementTop <= windowHeight;
    }


    public void Parse(string component, string pathImage, string filePath)
    {
        //може винести це в конструктор
        var chromeDriverService = ChromeDriverService.CreateDefaultService(@"C:\Users\Коля\source\repos\GURU\GURU\bin\Debug\net8.0");
        IWebDriver driver = new ChromeDriver(chromeDriverService);

        try
        {
            driver.Navigate().GoToUrl("https://elmir.ua/configurator/");
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            // Пропустить всплывающее окно
            IWebElement skipButton = driver.FindElement(By.XPath("//b[contains(@class, 'dg-btn dg-skip') and contains(text(), 'Пропустить')]"));
            skipButton.Click();

            //натискання на потрібний тип компонента
            IWebElement prodSelRowElement = driver.FindElement(By.CssSelector(component));
            IWebElement svgElement = prodSelRowElement.FindElement(By.CssSelector("svg.plus"));

            ScrollElementIntoView(driver, svgElement);
            Thread.Sleep(250);

            svgElement.Click();

            //знаходження останньої сторінки
            IWebElement pagination = driver.FindElement(By.CssSelector("ul.pgnation.pager-products"));
            var pageItems = pagination.FindElements(By.TagName("li"));
            var lastPageItem = pageItems.Last(item => item.GetAttribute("page") != null);
            string lastPageNumber = lastPageItem.GetAttribute("page");

            int pages = int.Parse(lastPageNumber);
            //pages++;

            IWebElement headerElement, linkElement, tableElement, imgElement, priceElement;
            string headerText, tableHtml, imageUrl, tempImageName;
            int k = 0;
            List<string> headerTextList = new List<string>();
            List<string> tableHtmlList = new List<string>();
            List<string> prices = new List<string>();

            for (int j = 2; ; j++)
            {
                // Найти все элементы внутри контейнера scrl-blu cat-products
                IList<IWebElement> productElements = driver.FindElements(By.CssSelector(".scrl-blu.cat-products > article[id^='pdtid']"));

                for (int i = 0; i < productElements.Count; i++)
                {
                    // Получить заголовок (имя продукта)
                    headerElement = productElements[i].FindElement(By.CssSelector("header a[target='_blank']"));
                    headerText = headerElement.Text;
                    //headerText = Regex.Replace($"//a[contains(text(),'{headerText}')]", @" \([^)]*\) ", "");
                    headerText = $"//a[contains(text(),'{headerText}')]";

                    if (headerText == "")
                        continue;

                    imgElement = productElements[i].FindElement(By.TagName("img"));
                    imageUrl = imgElement.GetAttribute("src");

                    //знаходження зображення та його завантаження указану папку
                    tempImageName = headerText.Replace("/", "");
                    DownloadImage(imageUrl, @$"{pathImage}\{k++}_{tempImageName}.jpg");


                    //знаходження компонента (процесора)
                    // Перевірка наявності елемента в списку headerTextList
                    if (headerTextList.Contains(headerText))
                    {
                        continue;
                    }

                    linkElement = productElements[i].FindElement(By.CssSelector("header > a"));
                    headerTextList.Add($"{k}_headerText");

                    // прокрутка до нужного елемнта
                    if (CanScrollDown(driver))
                    {
                        ScrollElementIntoView(driver, linkElement);
                        Thread.Sleep(200);
                    }
                    //откритие характеристик
                    linkElement.Click();

                    tableElement = driver.FindElement(By.CssSelector(".ofr-main table"));
                    tableHtml = tableElement.GetAttribute("innerHTML");
                    tableHtmlList.Add(tableHtml);

                    //отримання ціни
                    priceElement = productElements[i].FindElement(By.CssSelector("b.val"));
                    prices.Add(priceElement.GetAttribute("innerHTML"));

                    //закритие характеристик
                    linkElement.Click();
                }
                if (j == pages + 1)
                {
                    break;
                }
                IWebElement pageTwoElement = driver.FindElement(By.XPath($"//li[@page='{j}']"));
                ScrollElementIntoView(driver, pageTwoElement);
                pageTwoElement.Click();
                Thread.Sleep(1000);
            }

            //for (int i = 0; i < headerTextList.Count; i++)
            //{
            //    Console.WriteLine($"Заголовок: {headerTextList[i]}");
            //    Console.WriteLine($"Цена: {prices[i]}");
            //    Console.WriteLine($"Характеристики: {tableHtmlList[i]}");

            //    Console.WriteLine(new string('-', 100));
            //}

            using (StreamWriter sw = new StreamWriter(filePath))
            {
                for (int i = 0; i < headerTextList.Count; i++)
                {
                    sw.WriteLine($"Заголовок: {headerTextList[i]}");
                    sw.WriteLine($"Цена: {prices[i]}");
                    sw.WriteLine($"Характеристики: {tableHtmlList[i]}");
                    sw.WriteLine(new string('-', 100));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
        }
        finally
        {
            driver.Quit();
            Console.WriteLine("Програма успішно завеошила роботу");
            Console.WriteLine(component);
        }
    }

}