using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace ConsoleApplication1
{
    internal class Info
    {
        public string Phone { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string Url { get; }

        public Info(string url)
        {
            Url = url;
        }
    }

    internal delegate void MyDelegate(string url);

    internal static class Program
    {
        private static int _fieldCounter = 0;
        
        private static string _currentUrl;
        private static int _pageCounter = 1;
        private static int _usualCounter = 2;
        private static readonly List<Info> Infos = new List<Info>();
        private static string _inFile;
        private static string _filePath;

        private static void Main(string[] args)
        {
           // @"https://www.olx.ua/vinnitsa/q-dell-24/";
            Console.Write("Url: ");
            _currentUrl = Console.ReadLine();  
            Console.Write("Type directory u wanna write data (file.txt) (disk C not always working): ");
            _filePath = Console.ReadLine();  
            var chromeOptions = new ChromeOptions();
            var proxy = new Proxy();
            proxy.HttpProxy = "160.119.153.206:13093";
            chromeOptions.Proxy = proxy;
            var driver = new ChromeDriver(chromeOptions);
            driver.Navigate().GoToUrl(_currentUrl);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(5);
            
            try
            {
                driver.FindElementByXPath("//*[@id='paramsListOpt']/div/div[3]/div[2]/div[2]/span").Click();
            }
            catch
            {
                Console.WriteLine("Special hrestik exception");
            }

            while (true)
            {
                GetCurrentUrlsOnPage(driver);
                if (!ChangePage(driver)) break;
            }

            InListAllPageInfo(driver);
            var counter = 1;
            foreach (var info in Infos)
            {
                Console.WriteLine($"       = = = = = = {counter++} = = = = = = = ");
                Console.WriteLine(info.Title + "\n" + info.Name + "\n" + info.Phone + "\n" + info.Url);
                Console.WriteLine();
            }
            
            InFile(_filePath);
        }

        private static void InListAllPageInfo(ChromeDriver driver)
        {
            for (var i = 0; i < Infos.Count; i++)
            {
                Thread.Sleep(2000);
                GetInfoFromPage(driver, Infos[i].Url, i);
            }
        }

        private static void GetInfoFromPage(ChromeDriver driver, object url, int index)
        {
            driver.Navigate().GoToUrl((string) url);
            try
            {
                driver.FindElementByXPath("//*[@id='contact_methods']/li[2]/div").Click();
                driver.FindElementByXPath("//*[@id='contact_methods']/li[2]/div").Click();
            }
            catch (Exception e)
            {
                Console.WriteLine("Find Element exception");
                throw;
            }
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            Console.WriteLine(driver.PageSource.Normalize());
            htmlDoc.LoadHtml(driver.PageSource.Normalize().Normalize().Normalize());
            try
            {
                Infos[index].Title = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='offerdescription']/div[2]/h1")
                    .InnerText.Trim();
                Infos[index].Phone = htmlDoc.DocumentNode
                    .SelectSingleNode("//*[@id='contact_methodsBigImage']/li/div[2]/strong").InnerText.Trim();
                Infos[index].Name = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='offeractions']/div[4]/div[2]/h4")
                    .InnerText.Trim();
            }
            catch (Exception e)
            {
                Console.WriteLine("Info exception");
            }
        }

        private static void GetCurrentUrlsOnPage(ChromeDriver driver)
        {
            Thread.Sleep(100);
            while (true)
            {
                try
                {
                    Infos.Add(new Info(driver.FindElementByXPath($"//*[@id='offers_table']/tbody/tr[{_usualCounter}]/td/div/table/tbody/tr[1]/td[2]/div/h3/a").GetAttribute("href")));
                    driver.FindElementByXPath($"//*[@id='offers_table']/tbody/tr[{_usualCounter}]/td/div/table/tbody/tr[1]/td[2]/div/h3/a").GetAttribute("href");
                    Console.WriteLine(_fieldCounter + ": " + Infos[_fieldCounter].Url);
                    _fieldCounter++;                    
                }
                catch (Exception e)
                {
                    if (_usualCounter >= 40)
                    {
                        _usualCounter = 2;
                        Console.WriteLine("Usual Page exception");
                        break;
                    }
                   
                }

                _usualCounter++;
            }
        }

        private static bool ChangePage(ChromeDriver driver)
        {
            try
            {
                _pageCounter++;
                Console.WriteLine(_currentUrl + "?page=" + _pageCounter.ToString());
                driver.Navigate().GoToUrl(_currentUrl + "?page=" + _pageCounter.ToString());
                driver.FindElementByXPath($"//*[@id='body-container']/div[3]/div/div[5]/span[{_pageCounter}]/a");
                return true;
            }
            catch
            {
                Console.WriteLine("Change page exception");
                return false;
            }
        }

        private static void InFile(string filePath)
        {
            var counter = 1;
            foreach (var info in Infos)
            {
                _inFile+=$"       = = = = = = {counter++} = = = = = = = \n" + info.Title + "\n" + info.Name + "\n" + info.Phone + "\n" + info.Url + "\n\n";
            }

            _inFile += "Made by Kirill Kryklyviy (sususik52@gmail.com)";
            File.WriteAllText(filePath, _inFile);
        }
    }
}

//*[@id="offers_table"]/tbody/tr[2]/td/div/table/tbody/tr[1]/td[2]/div/h3/a/strong
//*[@id="offers_table"]/tbody/tr[3]/td/div/table/tbody/tr[1]/td[2]/div/h3/a/strong
//*[@id="offers_table"]/tbody/tr[4]/td/div/table/tbody/tr[1]/td[2]/div/h3/a/strong

//*[@id="offers_table"]/tbody/tr[6]/td/div/table/tbody/tr[1]/td[2]/div/h3/a/strong

//*[@id="offers_table"]/tbody/tr[41]/td/div/table/tbody/tr[1]/td[2]/div/h3/a/strong