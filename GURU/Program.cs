using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

class Program
{
    static void ScrollElementIntoView(IWebDriver driver, IWebElement element)
    {
        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
        js.ExecuteScript("arguments[0].scrollIntoView({ behavior: 'auto', block: 'center', inline: 'center' }); window.scrollBy(0, -15);", element);

    }
    static bool CanScrollDown(IWebDriver driver)
    {
        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
        long documentHeight = (long)js.ExecuteScript("return document.documentElement.scrollHeight;");
        long windowHeight = (long)js.ExecuteScript("return window.innerHeight;");
        long scrollY = (long)js.ExecuteScript("return window.scrollY;");
        return scrollY + windowHeight < documentHeight;
    }
    static void DownloadImage(string imageUrl, string destinationPath)
    {
        using (WebClient client = new WebClient())
        {
            client.DownloadFile(imageUrl, destinationPath);
        }
    }
    public bool IsElementInView(IWebDriver driver, IWebElement element)
    {
        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
        long windowHeight = (long)js.ExecuteScript("return window.innerHeight;");
        long elementTop = (long)js.ExecuteScript("return arguments[0].getBoundingClientRect().top;", element);

        return elementTop >= 0 && elementTop <= windowHeight;
    }
    static void Main()
    {
        int pages = 10;//сколько страниц
        pages++;

        var chromeDriverService = ChromeDriverService.CreateDefaultService(@"C:\Users\Коля\source\repos\GURU\GURU\bin\Debug\net8.0");
        IWebDriver driver = new ChromeDriver(chromeDriverService);
        try
        {
            driver.Navigate().GoToUrl("https://elmir.ua/configurator/");
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            // Пропустить всплывающее окно
            IWebElement skipButton = driver.FindElement(By.XPath("//b[contains(@class, 'dg-btn dg-skip') and contains(text(), 'Пропустить')]"));
            skipButton.Click();


            IWebElement prodSelRowElement = driver.FindElement(By.CssSelector("div.prod-selrow[data-id='14000']"));
            IWebElement svgElement = prodSelRowElement.FindElement(By.CssSelector("svg.plus"));
            svgElement.Click();

            

            IWebElement headerElement, linkElement, tableElement, imgElement, priceElement;
            string headerText, resultString, tableHtml, imageUrl, tempImageName, price;
            int k = 0;
            List<string> headerTextList = new List<string>();
            List<string> tableHtmlList = new List<string>();
            List<string> prices = new List<string>();
            int a = 0;
            for (int j = 2; j < 12; j++)
            {
                // Найти все элементы внутри контейнера scrl-blu cat-products
                IList<IWebElement> productElements = driver.FindElements(By.CssSelector(".scrl-blu.cat-products > article[id^='pdtid']"));

                for (int i = 0; i < productElements.Count; i++)
                {
                    // Получить заголовок (имя продукта)
                    headerElement = productElements[i].FindElement(By.CssSelector("header a[target='_blank']"));
                    headerText = headerElement.Text;
                    headerText = Regex.Replace($"//a[contains(text(),'{headerText}')]", @" \([^)]*\) ", "");

                    if (headerText == "")
                        continue;

                    imgElement = productElements[i].FindElement(By.TagName("img"));
                    imageUrl = imgElement.GetAttribute("src");

                    //знаходження зображення та його завантаження
                    tempImageName = headerText.Replace("/", "");
                    DownloadImage(imageUrl, $"ImageMatherboard\\{k++}_{tempImageName}.jpg");


                    //знаходження компонента (процесора)
                    // Перевірка наявності елемента в списку headerTextList
                    if (headerTextList.Contains(headerText))
                    {
                        continue;
                    }

                     linkElement = productElements[i].FindElement(By.CssSelector("header > a"));

                    headerTextList.Add(headerText);
                    //linkElement = driver.FindElement(By.XPath(headerText));

                    // прокрутка до нужного елемнта
                    if (CanScrollDown(driver))
                    {
                        ScrollElementIntoView(driver, linkElement);
                        Thread.Sleep(250);
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
                if (j == pages)
                {
                    break;
                }
                IWebElement pageTwoElement = driver.FindElement(By.XPath($"//li[@page='{j}']"));
                Thread.Sleep(1000);
                ScrollElementIntoView(driver, pageTwoElement);
                pageTwoElement.Click();
                Thread.Sleep(1000);
            }

            for (int i = 0; i < headerTextList.Count; i++)
            {
                Console.WriteLine($"Заголовок: {headerTextList[i]}");
                Console.WriteLine($"Цена: {prices[i]}");
                Console.WriteLine($"Характеристики: {tableHtmlList[i]}");

                Console.WriteLine(new string('-', 100));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
        }
        finally
        {
            driver.Quit();
            Console.ReadKey();
        }
    }
}
/*try
        {
            driver.Navigate().GoToUrl("https://elmir.ua/configurator/");
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            // Нажать на svg элемент
            IWebElement svgElement = driver.FindElement(By.CssSelector("svg.plus"));
            svgElement.Click();

            // Пропустить всплывающее окно
            IWebElement skipButton = driver.FindElement(By.XPath("//b[contains(@class, 'dg-btn dg-skip') and contains(text(), 'Пропустить')]"));
            skipButton.Click();

            IWebElement headerElement, linkElement, tableElement, imgElement, priceElement;
            string headerText, resultString, tableHtml, imageUrl, tempImageName, price;
            int k = 0;
            List<string> headerTextList = new List<string>();
            List<string> tableHtmlList = new List<string>();
            List<string> prices = new List<string>();
            int a = 0;
            for (int j = 2; j < 6; j++)
            {
                // Найти все элементы внутри контейнера scrl-blu cat-products
                IList<IWebElement> productElements = driver.FindElements(By.CssSelector(".scrl-blu.cat-products > article[id^='pdtid']"));

                for (int i = 0; i < productElements.Count; i++)
                {
                    //if (a == 1 && i == 30)
                    //{
                    //}
                    //if (i == 46)
                    //{
                    //}


                    // Получить заголовок (имя продукта)
                    headerElement = productElements[i].FindElement(By.CssSelector("header a[target='_blank']"));
                    headerText = headerElement.Text;
                    headerText = Regex.Replace($"//a[contains(text(),'{headerText}')]", @" \([^)]*\) ", "");

                    if (headerText == "")
                        continue;

                    imgElement = productElements[i].FindElement(By.TagName("img"));
                    imageUrl = imgElement.GetAttribute("src");

                    //знаходження зображення та його завантаження
                    tempImageName = headerText.Replace("/", "");
                    DownloadImage(imageUrl, $"Image\\{k++}_{tempImageName}.jpg");


                    //знаходження компонента (процесора)
                    // Перевірка наявності елемента в списку headerTextList
                    if (headerTextList.Contains(headerText))
                    {
                        continue;
                    }
                    headerTextList.Add(headerText);
                    linkElement = driver.FindElement(By.XPath(headerText));

                    // прокрутка до нужного елемнта
                    if (CanScrollDown(driver))
                    {
                        ScrollElementIntoView(driver, linkElement);
                        Thread.Sleep(250);
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
                if (j == 5)
                {
                    break;
                }
                IWebElement pageTwoElement = driver.FindElement(By.XPath($"//li[@page='{j}']"));
                Thread.Sleep(1000);
                ScrollElementIntoView(driver, pageTwoElement);
                pageTwoElement.Click();
                Thread.Sleep(1000);

                a = 1;
            }

            for (int i = 0; i < headerTextList.Count; i++)
            {
                Console.WriteLine($"Заголовок: {headerTextList[i]}");
                Console.WriteLine($"Цена: {prices[i]}");
                Console.WriteLine($"Характеристики: {tableHtmlList[i]}");

                Console.WriteLine(new string('-', 100));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
        }
        finally
        {
            driver.Quit();
            Console.ReadKey();
        }*/