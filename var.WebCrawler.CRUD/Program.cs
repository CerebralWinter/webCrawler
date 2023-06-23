using HtmlAgilityPack; //HAP is an HTML parser written in C# to read/write DOM and supports plain XPATH or XSLT.
//https://html-agility-pack.net/online-examples
using System.Text.RegularExpressions; //namespace delle regex
using System.Text.Json; //namespace per serializzare e deserializzare proprietà degli oggetti in json
using Microsoft.Extensions.Logging; //serve per tenere traccia del comportamento del programma e  degli errori
using NLog.Extensions.Logging; //https://nlog-project.org/
//using Var_WebCrawler_CRUD;
//using Microsoft.Extensions.Configuration; //

namespace Var_WebCrawler_CRUD
{
    public class Program
    {
        static void Main(string[] args)
        {

            var log = LoggerFactory.Create(builder => builder.AddNLog()).CreateLogger<Program>();
            //var builder = new ConfigurationBuilder().AddJsonFile(".\\appsettings.json", optional: false, reloadOnChange: true);
            //IConfigurationRoot configuration = builder.Build();
            //gestire l'errore?
            try //
            {
                log.LogTrace("Program has started.");
                //richiamiamo la pagina che permette di ottenere i "codici alimento" di ciascun alimento, che utilizzeremo per ricreare l'url completo di ciascun alimento
                string url = "https://www.alimentinutrizione.it/tabelle-nutrizionali/ricerca-per-ordine-alfabetico";
                var web = new HtmlWeb(); //istanziazione di un oggetto di classe HtmlWeb, chiamato web: permette di utilizzare gli html elements come oggetti
                var htmlDoc = web.Load(url); //
                log.LogInformation("The Html document loaded Perfectly."); //se il browser restituisce 200, se va a 404 "Exception e" riga 150
                
                //NELLA PAGINA WEB C'è UNA TABLE CON ID "cercatabella:"
                var HtmlInfo = htmlDoc.DocumentNode.SelectNodes("//table[@id = 'cercatabella']//li");
                List<FoodGeneralInfo> listOfFoods = new List<FoodGeneralInfo>();

                //we add the name and the url of foods to a list
                foreach (var item in HtmlInfo)
                {
                    var foodUrl = "https://www.alimentinutrizione.it" + item.ChildNodes[1].ChildNodes[1].Attributes[0].Value;
                    var FoodName = item.InnerText.Trim();
                    listOfFoods.Add(new FoodGeneralInfo { ItalianName = FoodName, Url = foodUrl });

                }
                var stop = 30;
                int count = 1;
                log.LogInformation($"There are 900 foods and I want to get just {stop}");
                Parallel.ForEach<FoodGeneralInfo>(listOfFoods, food =>
                {
                    string foodUrl = food.Url;
                    var htmlFood = web.Load(foodUrl);
                    #region the first table
                    string GeneralTableXPath = "//*[@id=\"conttableft\"]/div[1]/table";
                    var foodGeneraltable = htmlFood.DocumentNode.SelectNodes(GeneralTableXPath);
                    foreach (HtmlNode node in foodGeneraltable)
                    {
                        if (node.ChildNodes.Count() > 1)
                        {
                            for (int j = 3; j < foodGeneraltable[0].ChildNodes.Count() - 1; j++)
                            {
                                switch (node.ChildNodes[j].ChildNodes[0].InnerText)
                                {
                                    case "Categoria": food.Category = node.ChildNodes[j].ChildNodes[1].InnerText.Trim(); break;
                                    case "Codice Alimento": food.FoodCode = node.ChildNodes[j].ChildNodes[1].InnerText.Trim(); break;
                                    case "Nome Scientifico": food.ScientificName = node.ChildNodes[j].ChildNodes[1].InnerText.Trim(); break;
                                    case "English Name": food.EnglishName = node.ChildNodes[j].ChildNodes[1].InnerText.Trim(); break;
                                    case "Parte Edibile": food.EatablePartpercentage = node.ChildNodes[j].ChildNodes[1].InnerText.Trim(); break;
                                    case "Porzione": food.Portion = node.ChildNodes[j].ChildNodes[1].InnerText.Trim(); break;
                                    case "Informazioni": food.Information = node.ChildNodes[j].ChildNodes[1].InnerText.Trim(); break;
                                    case "Numero Campioni": food.NumberOfSamples = node.ChildNodes[j].ChildNodes[1].InnerText.Trim(); break;
                                }
                            }
                        }
                    }

                    #endregion
                    #region the second table
                    string NutTableXPath = "//*[@id=\"t3-content\"]/div[2]/article/section/table/tbody/tr";
                    var foodNuttable = htmlFood.DocumentNode.SelectNodes(NutTableXPath);
                    food.Nutritions = new List<Nutrition>();
                    string curentCategory = string.Empty;
                    foreach (var item in foodNuttable)
                    {
                        if (item.Attributes["class"].Value.Contains("title"))
                        {
                            curentCategory = item.InnerText.Trim();
                        }
                        if (item.Attributes["class"].Value.Contains("corpo"))
                        {
                            food.Nutritions.Add(new Nutrition
                            {
                                Category = curentCategory,
                                Description = item.ChildNodes[0].InnerText.Trim(),
                                ValueFor100g = item.ChildNodes[2].InnerText.Replace("\u0026nbsp;", "").Trim(),
                                Procedures = item.ChildNodes[7].InnerText.Trim(),
                                DataSource = item.ChildNodes[6].InnerText.Trim(),
                                Reference = item != null
                                && item.HasChildNodes
                                && item.ChildNodes[8] != null
                                && item.ChildNodes[8].HasChildNodes
                                && item.ChildNodes[8].ChildNodes[0].Attributes.Contains("data-content")
                                ? item.ChildNodes[8].ChildNodes[0].Attributes["data-content"].Value
                                : ""
                            });
                        }
                    }
                    #endregion
                    #region Codice Langual Table
                    food.LangualCodes = new List<Langual>();
                    string langualXPath = "//*[@id=\"t3-content\"]/div[2]/article/section/div[2]/div[1]/div";
                    var langualtable = htmlFood.DocumentNode.SelectNodes(langualXPath);
                    string info = string.Empty;
                    foreach (var element in langualtable)
                    {

                        for (int i = 1; i < element.ChildNodes.Count; i++)
                            food.LangualCodes.Add(new Langual
                            {
                                Id = element.ChildNodes[i].InnerText.Replace('|', ' ').Trim(),
                                Info = element.ChildNodes[i].Attributes["data-content"].Value.Trim()
                            }
                            );
                    }
                    #endregion
                    #region  Chart
                    food.ChartData = new Chart();
                    string html = htmlFood.DocumentNode.InnerHtml;
                    int startingIndex = html.IndexOf("['Proteine', ");
                    string chartData = html.Substring(startingIndex, 100);
                    string regex = @"\d+";
                    var matches = Regex.Matches(chartData, regex);
                    food.ChartData = new Chart
                    {
                        Protein = matches[0].Value.Trim(),
                        Fat = matches[1].Value.Trim(),
                        Carbohydrate = matches[2].Value.Trim(),
                        Fiber = matches[3].Value.Trim(),
                        Alcohol = matches[4].Value.Trim()
                    };
                    #endregion
                    log.LogInformation($"The alimento numero -->{count}<-- loaded completely");
                    count++;
                    if (count > stop)
                    {
                        //break;
                    }
                });
                var option = new JsonSerializerOptions { WriteIndented = true, AllowTrailingCommas = true };
                string jsonString = JsonSerializer.Serialize(listOfFoods, option);
                string jsonOutpath = ".\\result.json";
                File.AppendAllText(jsonOutpath, jsonString);
                log.LogInformation($"\nthe information saved in a json file with the address {jsonOutpath}");

            }
            catch (Exception e)
            {
                log.LogError(e.Message);
            }
        }
    }
}
