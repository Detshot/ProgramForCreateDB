using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace GURU
{
    class Program
    {

        static void Main()
        {
            StartAllSegments(13,14,15,16,17,18);
            //StartSegmentRange();
        }
        static void StartAllSegments(params int[] segmentNumbers)
        {
            Thread[] threads = new Thread[segmentNumbers.Length];

            for (int i = 0; i < segmentNumbers.Length; i++)
            {
                int segmentNumber = segmentNumbers[i];
                threads[i] = new Thread(() => StartSegment(segmentNumber));
                threads[i].Start();
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }
        }

        static void StartSegmentRange(int start, int end)
        {
            if (start > end)
            {
                throw new ArgumentException("Start must be less than or equal to end");
            }

            for (int i = start; i <= end; i++)
            {
                StartSegment(i);
            }
        }

        static void StartSegment(int segmentNumber)
        {
            var segments = new Dictionary<int, (string component, string path)>
            {
                { 1, ("div.prod-selrow[data-id='10000']", "Image\\Processors1") },
                { 2, ("div.prod-selrow[data-id='14000']", "Image\\Motherboards") },
                { 3, ("div.prod-selrow[data-id='16400']", "Image\\RAM") },
                { 4, ("div.prod-selrow[data-id='20000']", "Image\\VideoCards") },
                { 5, ("div.prod-selrow[data-id='24000']", "Image\\Housing") },
                { 6, ("div.prod-selrow[data-id='40000']", "Image\\PowerSupplies1") },
                { 7, ("div.prod-selrow[data-id='108472_12B-4jn']", "Image\\Drives\\SSD2.5") },
                { 8, ("div.prod-selrow[data-id='108472_12B-r1I']", "Image\\Drives\\SSDM.2") },
                { 9, ("div.prod-selrow[data-id='92868_CcE-50jd']", "Image\\Drives\\HD3.5") },
                { 10, ("div.prod-selrow[data-id='92868_CcE-50je']", "Image\\Drives\\HD2.5") },
                { 11, ("div.prod-selrow[data-id='106242_1Bt-6jG']", "Image\\Сooling\\Сooler") },
                { 12, ("div.prod-selrow[data-id='821135']", "Image\\Сooling\\LiquidCooling") },
                { 13, ("div.prod-selrow[data-id='196161']", "Image\\ThermalPaste") },
                { 14, ("div.prod-selrow[data-id='106241']", "Image\\CaseFans") },
                { 15, ("div.prod-selrow[data-id='32000']", "Image\\SoundCard") },
                { 16, ("div.prod-selrow[data-id='902']", "Image\\OpticalDrive") },
                { 17, ("div.prod-selrow[data-id='106244']", "Image\\LightingAndAdapters") },
                { 18, ("div.prod-selrow[data-id='107622']", "Image\\NetworkCard") }
            };

            if (!segments.TryGetValue(segmentNumber, out var segment))
            {
                throw new ArgumentException("Invalid segment number");
            }

            new WebDataExtractor(segment.component, segment.path).ExtractAndSaveData();
        }

    }

    public class WebDataExtractor(string component, string pathImage)
    {
        private readonly List<string> headerTextList = [];
        private readonly List<string> tableHtmlList = [];
        private readonly List<string> prices = [];
        private readonly ChromeDriver chromeDriver = InitializeChromeDriver();
        private readonly string filePath = Path.Combine(pathImage, "Characteristics.txt");
        private readonly string component = component;
        private readonly string pathImage = pathImage;
        private int currentPage = 1;
        private int elementsCounter;


        public List<string> Prices => prices;
        public List<string> TableHtmlList => tableHtmlList;
        public List<string> HeaderTextList => headerTextList;
        private int CurrentPage { get => currentPage; set => currentPage = value; }
        private int ElementsCounter { get => elementsCounter; set => elementsCounter = value; }


        public void ExtractAndSaveData()
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

        public void ExtractAndSaveDataFromSpecificPage(int startPage, int initialElementCounter)//пока нема смисла юзать
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

        private static ChromeDriver InitializeChromeDriver()
        {
            var chromeDriverService = ChromeDriverService.CreateDefaultService(@"C:\Users\Коля\source\repos\GURU\GURU\bin\Debug\net8.0");
            return new ChromeDriver(chromeDriverService);
        }

        private static void DownloadImage(string imageUrl, string destinationPath)
        {
            using HttpClient client = new();
            byte[] fileBytes = client.GetByteArrayAsync(imageUrl).Result;
            File.WriteAllBytes(destinationPath, fileBytes);
        }

        private static string SanitizeFileName(string fileName)
        {
            string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalidChars)
            {
                fileName = fileName.Replace(c.ToString(), "");
            }
            return fileName;
        }

        private void ParsePages()
        {
            int lastPageNumber = 1;
            try
            {
                lastPageNumber = GetLastPageNumber();
            }
            catch (Exception)
            {

            }

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
                        TableHtmlList.Add(tableHtml);
                    }
                    catch (NoSuchElementException)
                    {
                        continue;
                    }

                    headerTextList.Add($"{ElementsCounter}_{headerText}");
                    Prices.Add(productElements[i].FindElement(By.CssSelector("b.val")).GetAttribute("innerHTML"));
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

        private void ScrollElementIntoView(IWebElement element)
        {
            chromeDriver.ExecuteScript("arguments[0].scrollIntoView({ behavior: 'auto', block: 'center', inline: 'center' }); window.scrollBy(0, -15);", element);
            Thread.Sleep(180);
        }

        private void CreateTextFile()
        {
            using StreamWriter sw = new(filePath);
            for (int i = 0; i < HeaderTextList.Count; i++)
            {
                sw.WriteLine($"Заголовок: {HeaderTextList[i]}");
                sw.WriteLine($"Цена: {Prices[i]}");
                sw.WriteLine($"Характеристики: {TableHtmlList[i]}");
                sw.WriteLine(new string('-', 98));
            }
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
            Переферия і їx дополнительні компоненти (пока без бази і інтерфейса для них в финальній прогі) 
            пока не робить
            */
            string[] drives =
            [
                "div.prod-selrow[data-id='108472_12B-4jn']",
                "div.prod-selrow[data-id='108472_12B-r1I']",
                "div.prod-selrow[data-id='92868_CcE-50jd']",
                "div.prod-selrow[data-id='92868_CcE-50je']"
            ];

            string[] cooling =
            [
                "div.prod-selrow[data-id='106242_1Bt-6jG']",
                "div.prod-selrow[data-id='821135']"
            ];

            string[] AdditionalComponentsSystemUnit =
            [
                "div.prod-selrow[data-id='196161']",
                "div.prod-selrow[data-id='106241']",
                "div.prod-selrow[data-id='32000']",
                "div.prod-selrow[data-id='902']",
                "div.prod-selrow[data-id='106244']",
                "div.prod-selrow[data-id='107622']"
            ];
            IWebElement componentElement0 = component switch
            {
                var _ when cooling.Contains(component) => chromeDriver.FindElement(By.CssSelector("div.prod-selrow[data-id='a1']")),
                var _ when drives.Contains(component) => chromeDriver.FindElement(By.CssSelector("div.prod-selrow[data-id='a2']")),
                var _ when AdditionalComponentsSystemUnit.Contains(component) => chromeDriver.FindElement(By.CssSelector(".pc-item-top.prod-selrow.load-add .title b")),
                _ => throw new NoSuchElementException("Component not found in any category."),
            };

            if (componentElement0 != null)
            {
                IWebElement svgElement0 = componentElement0.FindElement(By.CssSelector("svg"));
                ScrollElementIntoView(svgElement0);
                svgElement0.Click();
            }

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

    }
}

