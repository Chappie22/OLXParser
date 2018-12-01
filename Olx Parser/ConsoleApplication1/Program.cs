using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using HtmlAgilityPack;
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

    internal static class Program
    {
        private static int _fieldCounter;
        private static string _currentUrl;
        private static int _pageCounter = 1;
        private static int _usualCounter = 2;
        private static readonly List<Info> Infos = new List<Info>();
        private static string _filePath;

        private static void Main()
        {                       
            Console.Write("Url: ");
            _currentUrl = Console.ReadLine();
            //
            string newUrl = String.Empty;
            int _counter = 0;
            
            
            if (_currentUrl.Contains("?page="))
            {
                for (int i = 0; i < _currentUrl.Length; i++)
                {
                    if (_currentUrl[i] == '?') break;
                    newUrl += _currentUrl[i];
                }
                
                for (int i = _currentUrl.Length-1; i > 0; i--)
                {
                    if (_currentUrl[i] == '=') break;
                    _counter++;
                }

                switch (_counter)
                {
                    case 1:
                    {
                        _pageCounter =(int)Char.GetNumericValue(_currentUrl[_currentUrl.Length-_counter]);
                        break;
                    }
                    default:
                    {
                        string number = String.Empty;
                        while (true)
                        {
                            number += _currentUrl[_currentUrl.Length - (_counter--)];
                            if (_counter == 0) break;
                        }
                        _pageCounter = Int32.Parse(number);
                        break;
                    }
                }

                Console.WriteLine("Num Page: " + _pageCounter);
                Console.WriteLine("New Url: " + newUrl);
                _currentUrl = newUrl;
            }
           
            //
            Console.Write("Type directory u wanna write data (file.txt) (disk C not always working): ");
            _filePath = Console.ReadLine();
            if (_filePath.Length <= 2) _filePath = String.Empty;
            
            var chromeOptions = new ChromeOptions();
            var proxy = new Proxy {HttpProxy = "160.119.153.206:13093"};
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

            if (_filePath != String.Empty)
            {
                File.AppendAllText(_filePath, "Made by Kirill Kryklyviy (sususik52@gmail.com)");
            }
            Console.WriteLine("Made by Kirill Kryklyviy (sususik52@gmail.com)");
            //InFile(_filePath);
        }

        private static void InListAllPageInfo(ChromeDriver driver)
        {
            for (var i = 0; i < Infos.Count; i++)
            {
                Console.Clear();
                Console.WriteLine($"Loading... {i+1} out of {Infos.Count-1}");
                driver.ResetInputState();
                if(!GetInfoFromPage(driver, Infos[i].Url, i))
                {
                    driver.Quit();
                    driver = new ChromeDriver();
                }
             }
        }

        private static bool GetInfoFromPage(ChromeDriver driver, object url, int index)
        {
            driver.Navigate().GoToUrl((string) url);
            try
            {
                driver.FindElementByXPath("//*[@id='contact_methods']/li[2]/div").Click();
            }
            catch (Exception)
            {
                Console.WriteLine("Find Element exception");
            }

            var htmlDoc = new HtmlDocument();
            Console.WriteLine(driver.PageSource.Normalize().Length);
            htmlDoc.LoadHtml(driver.PageSource.Normalize().Normalize().Normalize());
            try
            {
                try
                {
                    Infos[index].Title = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='offerdescription']/div[2]/h1")
                        .InnerText.Trim();
                }
                catch
                {
                    Console.WriteLine("Title exception");
                }

                try
                {
                    Infos[index].Name = htmlDoc.DocumentNode
                        .SelectSingleNode("//*[@id='offeractions']/div[3]/div[2]/h4/a/text()")
                        .InnerText.Trim();
                }
                catch
                {
                    try
                    {
                        string anotherTryName = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='similarads']/h2")
                            .InnerText.Trim();
                        for (int i = 22; i < anotherTryName.Length; i++)
                        {
                            Infos[index].Name += anotherTryName[i];
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("No way to get the exception");
                    }

                    Console.WriteLine("Name exception");
                }

                Infos[index].Phone = htmlDoc.DocumentNode
                    .SelectSingleNode("//*[@id='contact_methodsBigImage']/li/div[2]/strong").InnerText.Trim();

                int tryWhile = 0;
                while (tryWhile <= 10)
                {
                    tryWhile++;
                    if (Infos[index].Phone.Contains("x"))
                    {
                        try
                        {
                            driver.FindElementByXPath("//*[@id='contact_methods']/li[2]/div").Click();
                            driver.Navigate().GoToUrl((string) url);
                            Console.WriteLine(driver.PageSource.Normalize());
                            htmlDoc.LoadHtml(driver.PageSource.Normalize().Normalize().Normalize());
                            Infos[index].Phone = htmlDoc.DocumentNode
                                .SelectSingleNode("//*[@id='contact_methodsBigImage']/li/div[2]/strong").InnerText.Trim();
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Find Element exception");
                           
                        }
                        Console.Clear();
                        Console.WriteLine("Bad number");
                    }
                    else break;
                }

                if (tryWhile >= 10)
                {
                    return false;
                }

                //FileAdd
                if (_filePath != String.Empty)
                {
                    File.AppendAllText(_filePath,
                        $"       = = = = = = {index + 1} = = = = = = = \n" + Infos[index].Title + "\n" + Infos[index].Name +
                        "\n" + Infos[index].Phone + "\n" + Infos[index].Url + "\n\n");
                }
                
            }
            catch (Exception)
            {
                Console.WriteLine("Info exception");
            }

            return true;
        }
        
        private static void GetCurrentUrlsOnPage(ChromeDriver driver)
        {
            Thread.Sleep(100);
            while (true)
            {
                try
                {
                    Infos.Add(new Info(driver
                        .FindElementByXPath(
                            $"//*[@id='offers_table']/tbody/tr[{_usualCounter}]/td/div/table/tbody/tr[1]/td[2]/div/h3/a")
                        .GetAttribute("href")));
                    driver.FindElementByXPath(
                            $"//*[@id='offers_table']/tbody/tr[{_usualCounter}]/td/div/table/tbody/tr[1]/td[2]/div/h3/a")
                        .GetAttribute("href");
                    Console.WriteLine(_fieldCounter + ": " + Infos[_fieldCounter].Url);
                    _fieldCounter++;
                }
                catch (Exception)
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
                Console.WriteLine(_currentUrl + "?page=" + _pageCounter);
                driver.Navigate().GoToUrl(_currentUrl + "?page=" + _pageCounter);
                driver.FindElementByXPath($"//*[@id='body-container']/div[3]/div/div[5]/span[{_pageCounter}]/a");  
                return true;
            }
            catch
            {
                Console.WriteLine("Change page exception");
                return false;
            }
        }

//        private static void InFile(string filePath)
//        {
//            var counter = 1;
//            var inFile = string.Empty;
//            foreach (var info in Infos)
//                inFile += $"       = = = = = = {counter++} = = = = = = = \n" + info.Title + "\n" + info.Name + "\n" +
//                          info.Phone + "\n" + info.Url + "\n\n";
//            inFile += "Made by Kirill Kryklyviy (sususik52@gmail.com)";
//            File.WriteAllText(filePath, inFile);
//        }
    }
}