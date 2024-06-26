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
        Thread thread1 = new Thread(() =>
        {
            string component = "div.prod-selrow[data-id='10000']";
            string path = "Image\\Processors1";
            new ExtractionDB(component, path).Parse();
        });

        // Материнские платы
        //Thread thread2 = new Thread(() =>
        //{
        //    string component = "div.prod-selrow[data-id='14000']";
        //    string path = "Image\\Motherboards";
        //    new ExtractionDB(component, path).Parse();
        //});

        // Оперативная память
        //Thread thread3 = new(() =>
        //{
        //    string component = "div.prod-selrow[data-id='16400']";
        //    string path = "Image\\RAM";
        //    new ExtractionDB(component, path).Parse();
        //});

        //// Видеокарты
        //Thread thread4 = new Thread(() =>
        //{
        //    string component = "div.prod-selrow[data-id='20000']";
        //    string path = "Image\\VideoCards";
        //    new ExtractionDB(component, path).Parse();
        //});

        // Корпуса
        //Thread thread5 = new Thread(() =>
        //{
        //    string component = "div.prod-selrow[data-id='24000']";
        //    string path = "Image\\Housing";
        //    new ExtractionDB(component, path).Parse();
        //});

        //// Блоки питания
        //Thread thread6 = new Thread(() =>
        //{
        //    string component = "div.prod-selrow[data-id='40000']";
        //    string path = "Image\\PowerSupplies";
        //    new ExtractionDB(component, path).Parse();
        //});

        // Запуск всех потоков
        thread1.Start();
        //thread2.Start();
        //thread3.Start();
        //thread4.Start();
        //thread5.Start();
        //thread6.Start();

        // Ожидание завершения всех потоков
        thread1.Join();
        //thread2.Join();
        //thread3.Join();
        //thread4.Join();
        //thread5.Join();
        //thread6.Join();
    }

}

//class ExtractionDB
//{
//    List<string> headerTextList = new List<string>();

//    List<string> tableHtmlList = new List<string>();

//    List<string> prices = new List<string>();

//    readonly IWebDriver chromeDriver;

//    readonly string filePath, component, pathImage;

//    int currentPage = 1, elementsCounter;

//    public List<string> Prices { get => prices; set => prices = value; }
//    public List<string> TableHtmlList { get => tableHtmlList; set => tableHtmlList = value; }
//    public List<string> HeaderTextList { get => headerTextList; set => headerTextList = value; }
//    private IWebDriver ChromeDriver => chromeDriver;
//    public string FilePath => filePath;
//    public string Component => component;
//    public string PathImage => pathImage;

//    public int CurrentPage { get => currentPage; set => currentPage = value; }
//    public int ElementsCounter { get => elementsCounter; set => elementsCounter = value; }

//    public ExtractionDB(string component, string pathImage)
//    {
//        this.pathImage = pathImage;
//        this.component = component;
//        filePath = Path.Combine(pathImage, "Characteristics.txt");
//        chromeDriver = OpenChromeDriver();
//    }

//    ChromeDriver OpenChromeDriver()
//    {
//        var chromeDriverService = ChromeDriverService.CreateDefaultService(@"C:\Users\Коля\source\repos\GURU\GURU\bin\Debug\net8.0");
//        return new ChromeDriver(chromeDriverService);
//    }

//    void ScrollElementIntoView(IWebElement element)
//    {
//        IJavaScriptExecutor js = (IJavaScriptExecutor)ChromeDriver;
//        js.ExecuteScript("arguments[0].scrollIntoView({ behavior: 'auto', block: 'center', inline: 'center' }); window.scrollBy(0, -15);", element);
//        Thread.Sleep(180);
//    }

//    void DownloadImage(string imageUrl, string destinationPath)
//    {
//        using (HttpClient client = new HttpClient())
//        {
//            byte[] fileBytes = client.GetByteArrayAsync(imageUrl).Result;
//            File.WriteAllBytes(destinationPath, fileBytes);
//        }
//    }

//    void CreateTXT()
//    {
//        using (StreamWriter sw = new StreamWriter(FilePath))
//        {
//            for (int i = 0; i < HeaderTextList.Count; i++)
//            {
//                sw.WriteLine($"Заголовок: {HeaderTextList[i]}");
//                sw.WriteLine($"Цена: {Prices[i]}");
//                sw.WriteLine($"Характеристики: {TableHtmlList[i]}");
//                sw.WriteLine(new string('-', 98));
//            }
//        }
//    }

//    string SanitizeFileName(string fileName)
//    {
//        string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
//        foreach (char c in invalidChars)
//        {
//            fileName = fileName.Replace(c.ToString(), "");
//        }
//        return fileName;
//    }

//    void FindingAndDownloadingImage(IList<IWebElement> productElements, int i, string headerText)
//    {
//        IWebElement imgElement = productElements[i].FindElement(By.TagName("img"));
//        string imageUrl = imgElement.GetAttribute("src");

//        string sanitizedHeaderText = SanitizeFileName(headerText);
//        string destinationPath = Path.Combine(PathImage, $"{ElementsCounter}_{sanitizedHeaderText}.jpg");

//        DownloadImage(imageUrl, destinationPath);
//        ElementsCounter++;
//    }

//    void NextPages(int j)
//    {
//        IWebElement pageTwoElement = ChromeDriver.FindElement(By.XPath($"//li[@page='{j}']"));
//        ScrollElementIntoView(pageTwoElement);
//        Thread.Sleep(600);
//        pageTwoElement.Click();
//        Thread.Sleep(500);
//    }

//    void GoToPage(int j)
//    {
//        for (int i = 2; i <= j; i++)
//            NextPages(i);
//        CurrentPage = j;//часть костиля
//    }

//    void ClickElement()
//    {
//        IWebElement prodSelRowElement = ChromeDriver.FindElement(By.CssSelector(Component));
//        IWebElement svgElement = prodSelRowElement.FindElement(By.CssSelector("svg.plus"));

//        ScrollElementIntoView(svgElement);
//        svgElement.Click();
//    }

//    void ClosePopup()
//    {
//        IWebElement skipButton = ChromeDriver.FindElement(By.XPath("//b[contains(@class, 'dg-btn dg-skip') and contains(text(), 'Пропустить')]"));
//        skipButton.Click();
//    }

//    int FindLastPages()
//    {
//        IWebElement pagination = ChromeDriver.FindElement(By.CssSelector("ul.pgnation.pager-products"));

//        var pageItems = pagination.FindElements(By.TagName("li"));
//        var lastPageItem = pageItems.Last(item => item.GetAttribute("page") != null);
//        string lastPageNumber = lastPageItem.GetAttribute("page");

//        return int.Parse(lastPageNumber);
//    }

//    public void ParsePages()
//    {
//        IWebElement headerElement, linkElement, tableElement;
//        string headerText, tableHtml;

//        int lastPages = FindLastPages();

//        ChromeDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1.75);

//        while (true)
//        {
//            IList<IWebElement> productElements = ChromeDriver.FindElements(By.CssSelector(".scrl-blu.cat-products > article[id^='pdtid']"));

//            for (int i = 0; i < productElements.Count; i++)
//            {
//                headerElement = productElements[i].FindElement(By.CssSelector("header a[target='_blank']"));
//                headerText = headerElement.Text;

//                if (string.IsNullOrEmpty(headerText))
//                    continue;

//                if (HeaderTextList.Contains(headerText))
//                    continue;

//                linkElement = productElements[i].FindElement(By.CssSelector("header > a"));

//                ScrollElementIntoView(linkElement);

//                if (i == 0)
//                    Thread.Sleep(1000);

//                try
//                {
//                    linkElement.Click();
//                    tableElement = productElements[i].FindElement(By.CssSelector(".ofr-main table"));
//                    tableHtml = tableElement.GetAttribute("innerHTML");
//                    linkElement.Click();

//                    TableHtmlList.Add(tableHtml);
//                }
//                catch (NoSuchElementException)
//                {
//                    continue;
//                }

//                HeaderTextList.Add($"{ElementsCounter}_{headerText}");

//                Prices.Add(productElements[i].FindElement(By.CssSelector("b.val")).GetAttribute("innerHTML"));

//                FindingAndDownloadingImage(productElements, i, headerText);
//            }

//            currentPage++; 

//            if (currentPage >= lastPages) 
//                break;
//            NextPages(currentPage);

//            Thread.Sleep(800);
//        }

//        CreateTXT();
//    }

//    public void Parse()
//    {
//        try
//        {
//            ChromeDriver.Navigate().GoToUrl("https://elmir.ua/configurator/");
//            ChromeDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5); //встановлено 5 секунд щоб дочекатись появи вспливашки і закрить її
//            ClosePopup();
//            ClickElement();

//            ParsePages();

//            Console.WriteLine("Программа успiшно завершила работу");
//            Console.WriteLine(Component);
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Произошла ошибка: {ex.Message}");
//        }
//        finally
//        {
//            ChromeDriver.Quit();
//            CreateTXT();
//        }
//    }

//    public void ParseFromThisPageAndThisElement(int page, int elementsCounter)
//    {
//        ElementsCounter = elementsCounter;
//        try
//        {
//            ChromeDriver.Navigate().GoToUrl("https://elmir.ua/configurator/");
//            ChromeDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);//встановлено 5 секунд щоб дочекатись появи вспливашки і закрить її
//            ClosePopup();
//            ClickElement();

//            GoToPage(page);//частично котильний
//            ParsePages();

//            Console.WriteLine("Программа успiшно завершила работу");
//            Console.WriteLine(Component);
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Произошла ошибка: {ex.Message}");
//        }
//        finally
//        {
//            ChromeDriver.Quit();
//            CreateTXT();
//        }
//    }
//}

public class ExtractionDB
{
    private readonly List<string> headerTextList = [];
    private readonly List<string> tableHtmlList = [];
    private readonly List<string> prices = [];
    private readonly ChromeDriver chromeDriver;
    private readonly string filePath;
    private readonly string component;
    private readonly string pathImage;
    private int currentPage = 1;
    private int elementsCounter;

    public List<string> Prices => prices;
    public List<string> TableHtmlList => tableHtmlList;
    public List<string> HeaderTextList => headerTextList;
    private int CurrentPage { get => currentPage; set => currentPage = value; }
    private int ElementsCounter { get => elementsCounter; set => elementsCounter = value; }

    public ExtractionDB(string component, string pathImage)
    {
        this.pathImage = pathImage;
        this.component = component;
        filePath = Path.Combine(pathImage, "Characteristics.txt");
        chromeDriver = InitializeChromeDriver();
    }

    private ChromeDriver InitializeChromeDriver()
    {
        var chromeDriverService = ChromeDriverService.CreateDefaultService(@"C:\Users\Коля\source\repos\GURU\GURU\bin\Debug\net8.0");
        return new ChromeDriver(chromeDriverService);
    }

    private void ScrollElementIntoView(IWebElement element)
    {
        IJavaScriptExecutor js = (IJavaScriptExecutor)chromeDriver;
        js.ExecuteScript("arguments[0].scrollIntoView({ behavior: 'auto', block: 'center', inline: 'center' }); window.scrollBy(0, -15);", element);
        Thread.Sleep(180);
    }

    private void DownloadImage(string imageUrl, string destinationPath)
    {
        using (HttpClient client = new())
        {
            byte[] fileBytes = client.GetByteArrayAsync(imageUrl).Result;
            File.WriteAllBytes(destinationPath, fileBytes);
        }
    }

    private void CreateTextFile()
    {
        using StreamWriter sw = new(filePath);
        for (int i = 0; i < headerTextList.Count; i++)
        {
            sw.WriteLine($"Заголовок: {headerTextList[i]}");
            sw.WriteLine($"Цена: {prices[i]}");
            sw.WriteLine($"Характеристики: {tableHtmlList[i]}");
            sw.WriteLine(new string('-', 98));
        }
    }

    private string SanitizeFileName(string fileName)
    {
        string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
        foreach (char c in invalidChars)
        {
            fileName = fileName.Replace(c.ToString(), "");
        }
        return fileName;
    }

    private void FindAndDownloadImage(IList<IWebElement> productElements, int index, string headerText)
    {
        IWebElement imgElement = productElements[index].FindElement(By.TagName("img"));
        string imageUrl = imgElement.GetAttribute("src");

        string sanitizedHeaderText = SanitizeFileName(headerText);
        string destinationPath = Path.Combine(pathImage, $"{ElementsCounter}_{sanitizedHeaderText}.jpg");

        DownloadImage(imageUrl, destinationPath);
    }

    private void NavigateToNextPage()
    {
        IWebElement pageElement = chromeDriver.FindElement(By.XPath($"//li[@page='{CurrentPage}']"));
        ScrollElementIntoView(pageElement);
        Thread.Sleep(600);
        pageElement.Click();
        Thread.Sleep(500);
    }

    private void NavigateToPage(int pageNumber)
    {
        for (int i = 2; i <= pageNumber; i++)
            NavigateToNextPage();
        CurrentPage = pageNumber;
    }

    private void ClickComponentElement()
    {
        /*
        придумать як зрообить откритиие елментов с разними путями
        (идея: добавить метод которий опредилит есть ли в видимости нужний елемент, если нет то он его откроет)
        откритие устройств хранение, охдаждение,доп елементи в сис блоке

        Переферия і їx дополнительні компоненти (пока без бази і інтерфейса для них)
        тіпа зробить шоб було, на будуще
        */

        IWebElement componentElement = chromeDriver.FindElement(By.CssSelector(component));
        IWebElement svgElement = componentElement.FindElement(By.CssSelector("svg.plus"));

        ScrollElementIntoView(svgElement);
        svgElement.Click();
    }

    private void ClosePopup()
    {
        IWebElement skipButton = chromeDriver.FindElement(By.XPath("//b[contains(@class, 'dg-btn dg-skip') and contains(text(), 'Пропустить')]"));
        skipButton.Click();
    }

    private int GetLastPageNumber()
    {
        IWebElement pagination = chromeDriver.FindElement(By.CssSelector("ul.pgnation.pager-products"));

        var pageItems = pagination.FindElements(By.TagName("li"));
        var lastPageItem = pageItems.Last(item => item.GetAttribute("page") != null);
        string lastPageNumber = lastPageItem.GetAttribute("page");

        return int.Parse(lastPageNumber);
    }

    public void ParsePages()
    {
        int lastPageNumber = GetLastPageNumber();

        chromeDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1.75);

        while (CurrentPage <= lastPageNumber)
        {
            IList<IWebElement> productElements = chromeDriver.FindElements(By.CssSelector(".scrl-blu.cat-products > article[id^='pdtid']"));

            for (int i = 0; i < productElements.Count; i++)
            {
                IWebElement headerElement = productElements[i].FindElement(By.CssSelector("header a[target='_blank']"));
                string headerText = headerElement.Text;

                if (string.IsNullOrEmpty(headerText) || headerTextList.Contains(headerText))
                    continue;

                IWebElement linkElement = productElements[i].FindElement(By.CssSelector("header > a"));
                ScrollElementIntoView(linkElement);

                if (i == 0)
                    Thread.Sleep(1000);

                try
                {
                    linkElement.Click();
                    IWebElement tableElement = productElements[i].FindElement(By.CssSelector(".ofr-main table"));
                    string tableHtml = tableElement.GetAttribute("innerHTML");
                    linkElement.Click();

                    tableHtmlList.Add(tableHtml);
                }
                catch (NoSuchElementException)
                {
                    continue;
                }

                headerTextList.Add($"{ElementsCounter}_{headerText}");

                prices.Add(productElements[i].FindElement(By.CssSelector("b.val")).GetAttribute("innerHTML"));

                FindAndDownloadImage(productElements, i, headerText);

                ElementsCounter++;
            }

            CurrentPage++;

            if (CurrentPage > lastPageNumber)
                break;
            NavigateToNextPage();

            Thread.Sleep(800);
        }

        CreateTextFile();
    }

    public void Parse()
    {
        try
        {
            chromeDriver.Navigate().GoToUrl("https://elmir.ua/configurator/");
            chromeDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            ClosePopup();
            ClickComponentElement();

            ParsePages();

            Console.WriteLine("Программа успiшно завершила работу");
            Console.WriteLine(component);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
        }
        finally
        {
            chromeDriver.Quit();
            CreateTextFile();
        }
    }

    public void ParseFromSpecificPage(int startPage, int initialElementCounter)//пока не смисла юзать
    {
        ElementsCounter = initialElementCounter;
        try
        {
            chromeDriver.Navigate().GoToUrl("https://elmir.ua/configurator/");
            chromeDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            ClosePopup();
            ClickComponentElement();

            NavigateToPage(startPage);
            ParsePages();

            Console.WriteLine("Программа успiшно завершила работу");
            Console.WriteLine(component);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
        }
        finally
        {
            chromeDriver.Quit();
            CreateTextFile();
        }
    }

}

