using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Net.Http;
using System.Threading;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        //Thread thread1 = new Thread(() =>
        //{
        //    string component = "div.prod-selrow[data-id='10000']";
        //    string path = "Image\\Processors";
        //    new ExtractionDB().Parse(component, path);
        //});

        // Материнские платы
        //Thread thread2 = new Thread(() =>
        //{
        //    string component = "div.prod-selrow[data-id='14000']";
        //    string path = "Image\\Motherboards";
        //    new ExtractionDB().Parse(component, path);
        //});

        // Оперативная память
        Thread thread3 = new Thread(() =>
        {
            string component = "div.prod-selrow[data-id='16400']";
            string path = "Image\\RAM";
            new ExtractionDB().Parse(component, path);
        });

        //// Видеокарты
        //Thread thread4 = new Thread(() =>
        //{
        //    string component = "div.prod-selrow[data-id='20000']";
        //    string path = "Image\\VideoCards";
        //    new ExtractionDB().Parse(component, path);
        //});

        // Корпуса
        //Thread thread5 = new Thread(() =>
        //{
        //    string component = "div.prod-selrow[data-id='24000']";
        //    string path = "Image\\Housing";
        //    new ExtractionDB().Parse(component, path);
        //});

        //// Блоки питания
        //Thread thread6 = new Thread(() =>
        //{
        //    string component = "div.prod-selrow[data-id='40000']";
        //    string path = "Image\\PowerSupplies";
        //    new ExtractionDB().Parse(component, path);
        //});

        // Запуск всех потоков
        //thread1.Start();
        //thread2.Start();
        thread3.Start();
        //thread4.Start();
        //thread5.Start();
        //thread6.Start();

        // Ожидание завершения всех потоков
        //thread1.Join();
        //thread2.Join();
        thread3.Join();
        //thread4.Join();
        //thread5.Join();
        //thread6.Join();
    }

}

class ExtractionDB
{
    List<string> headerTextList = new List<string>();
    List<string> tableHtmlList = new List<string>();
    List<string> prices = new List<string>();
    int j = 2;
    string filePath, component, pathImage;
    IWebDriver driver;

    int k = 0;

    public ExtractionDB() { }

    void OpenChromeDriver()
    {
        var chromeDriverService = ChromeDriverService.CreateDefaultService(@"C:\Users\Коля\source\repos\GURU\GURU\bin\Debug\net8.0");
        driver = new ChromeDriver(chromeDriverService);
    }

    void ScrollElementIntoView(IWebElement element)
    {
        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
        js.ExecuteScript("arguments[0].scrollIntoView({ behavior: 'auto', block: 'center', inline: 'center' }); window.scrollBy(0, -15);", element);
        Thread.Sleep(180);
    }

    void DownloadImage(string imageUrl, string destinationPath)
    {
        using (HttpClient client = new HttpClient())
        {
            byte[] fileBytes = client.GetByteArrayAsync(imageUrl).Result;
            File.WriteAllBytes(destinationPath, fileBytes);
        }
    }

    bool IsElementInView(IWebDriver driver, IWebElement element)
    {
        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
        long windowHeight = (long)js.ExecuteScript("return window.innerHeight;");
        long elementTop = (long)js.ExecuteScript("return arguments[0].getBoundingClientRect().top;", element);

        return elementTop >= 0 && elementTop <= windowHeight;
    }

    void CreateTXT()
    {
        using (StreamWriter sw = new StreamWriter(filePath))
        {
            for (int i = 0; i < headerTextList.Count; i++)
            {
                sw.WriteLine($"Заголовок: {headerTextList[i]}");
                sw.WriteLine($"Цена: {prices[i]}");
                sw.WriteLine($"Характеристики: {tableHtmlList[i]}");
                sw.WriteLine(new string('-', 98));
            }
        }
    }

    string SanitizeFileName(string fileName)
    {
        string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
        foreach (char c in invalidChars)
        {
            fileName = fileName.Replace(c.ToString(), "");
        }
        return fileName;
    }

    void FindingAndDownloadingImage(IList<IWebElement> productElements, int i, string headerText)
    {
        IWebElement imgElement = productElements[i].FindElement(By.TagName("img"));
        string imageUrl = imgElement.GetAttribute("src");

        string sanitizedHeaderText = SanitizeFileName(headerText);
        string destinationPath = Path.Combine(pathImage, $"{k}_{sanitizedHeaderText}.jpg");

        DownloadImage(imageUrl, destinationPath);
        k++;
    }

    void NextPages(int j)
    {
        IWebElement pageTwoElement = driver.FindElement(By.XPath($"//li[@page='{j}']"));
        ScrollElementIntoView(pageTwoElement);
        Thread.Sleep(600);
        pageTwoElement.Click();
        Thread.Sleep(500);
    }

    void GoToPage(int j)
    {
        for (int i = 2; i <= j; i++)
            NextPages(i);
        this.j = j + 1;
    }

    void ClickElement(string component)
    {
        IWebElement prodSelRowElement = driver.FindElement(By.CssSelector(component));
        IWebElement svgElement = prodSelRowElement.FindElement(By.CssSelector("svg.plus"));

        ScrollElementIntoView(svgElement);
        svgElement.Click();
    }

    void ClosePopup()
    {
        IWebElement skipButton = driver.FindElement(By.XPath("//b[contains(@class, 'dg-btn dg-skip') and contains(text(), 'Пропустить')]"));
        skipButton.Click();
    }

    int FindLastPages()
    {
        IWebElement pagination = driver.FindElement(By.CssSelector("ul.pgnation.pager-products"));

        var pageItems = pagination.FindElements(By.TagName("li"));
        var lastPageItem = pageItems.Last(item => item.GetAttribute("page") != null);
        string lastPageNumber = lastPageItem.GetAttribute("page");

        return int.Parse(lastPageNumber);
    }

    void ParsePages()
    {
        IWebElement headerElement, linkElement, tableElement;
        string headerText, tableHtml;

        int pages = FindLastPages();
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1.75);
        for (; ; j++)
        {
            IList<IWebElement> productElements = driver.FindElements(By.CssSelector(".scrl-blu.cat-products > article[id^='pdtid']"));

            for (int i = 0; i < productElements.Count; i++)
            {
                headerElement = productElements[i].FindElement(By.CssSelector("header a[target='_blank']"));
                headerText = headerElement.Text;

                if (string.IsNullOrEmpty(headerText))
                    continue;

                if (headerTextList.Contains(headerText))
                    continue;

                linkElement = productElements[i].FindElement(By.CssSelector("header > a"));

                ScrollElementIntoView(linkElement);
                if (i == 0)
                    Thread.Sleep(1000);

                try
                {
                    linkElement.Click();
                    tableElement = productElements[i].FindElement(By.CssSelector(".ofr-main table"));
                    tableHtml = tableElement.GetAttribute("innerHTML");
                    linkElement.Click();

                    tableHtmlList.Add(tableHtml);
                }
                catch (NoSuchElementException)
                {
                    continue;
                }

                headerTextList.Add($"{k}_{headerText}");
                prices.Add(productElements[i].FindElement(By.CssSelector("b.val")).GetAttribute("innerHTML"));

                FindingAndDownloadingImage(productElements, i, headerText);
            }
            if (j == pages + 1)
                break;

            NextPages(j);
            Thread.Sleep(800);
        }

        CreateTXT();
    }

    public void Parse(string component, string pathImage)
    {
        this.pathImage = pathImage;
        this.component = component;
        this.filePath = Path.Combine(pathImage, "Characteristics.txt");

        OpenChromeDriver();
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        try
        {
            driver.Navigate().GoToUrl("https://elmir.ua/configurator/");
            ClosePopup();
            ClickElement(component);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1.75);

            ParsePages();

            Console.WriteLine("Программа успешно завершила работу");
            Console.WriteLine(component);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
        }
        finally
        {
            driver.Quit();
            CreateTXT();
        }
    }

    public void Parse(string component, string pathImage, int page, int k)
    {
        this.pathImage = pathImage;
        this.component = component;
        this.filePath = Path.Combine(pathImage, "Characteristics.txt");
        this.k = k;

        OpenChromeDriver();
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        try
        {
            driver.Navigate().GoToUrl("https://elmir.ua/configurator/");
            ClosePopup();
            ClickElement(component);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);

            GoToPage(page);
            ParsePages();
            Console.WriteLine("Программа успешно завершила работу");
            Console.WriteLine(component);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
        }
        finally
        {
            driver.Quit();
            CreateTXT();
        }
    }
}
