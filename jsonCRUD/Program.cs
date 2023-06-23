using Newtonsoft.Json;
using Var_WebCrawler_CRUD;
using System.Text.Json;
using ConsoleTables;
using static System.ConsoleColor;
using NLog.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace var.WebCrawler.CRUD
{
    public class Global
    {
        public static string OriginaljsonFile = "";
        public static Dictionary<string, List<FoodGeneralInfo>> Buffer = new Dictionary<string, List<FoodGeneralInfo>>();
        public static List<NutrientsInfo> ListofAllNutrients = new List<NutrientsInfo>();
        public static List<LangualInfo> ListOfAllLanguals = new List<LangualInfo>();
        public static List<ChartInfo> ListOfAllCharts = new List<ChartInfo>();
    }
    public class Program
    {
        static void Main(string[] args)
        {
            var log = LoggerFactory.Create(builder => builder.AddNLog()).CreateLogger<Program>();
            try
            {
                var builder = new ConfigurationBuilder().AddJsonFile("C:\\Users\\Admin\\Desktop\\webCrawlerCRUDjson\\jsonCRUD\\appsettings.json", optional: false, reloadOnChange: true);
                IConfigurationRoot configuration = builder.Build();
                ColoredPrint("Welcome to the Best Json browser in the world! :)))))))))))\n\n", Blue);
                Global.OriginaljsonFile = GetJsonPath();
                List<FoodGeneralInfo> listOfFood = GetJsonData(Global.OriginaljsonFile);
                Global.ListofAllNutrients = GetNutrients(listOfFood);
                Global.ListOfAllLanguals = GetLanguals(listOfFood);
                Global.ListOfAllCharts = GetCharts(listOfFood);
                MainMenu(listOfFood);
            }
            catch (Exception e)
            {
                /*
                log.LogTrace($"\nException Message : {e.Message}\n Exception Data: {e.Data}" +
                    $"\n Exception HelpLink: {e.HelpLink}\n Exception HResult: {e.HResult}" +
                    $"\n Exception Inner Exception: {e.InnerException}\n Exception Source: {e.Source}\n Exception StackTrace : {e.StackTrace}" +
                    $"\n Exception TargetSite: {e.TargetSite}");
                */
                ColoredPrint($"\nException Message : {e.Message}\n", Blue);
                ColoredPrint($"Exception Data: : {e.Data}\n", DarkCyan);
                ColoredPrint($"Exception HelpLink: {e.HelpLink}\n", DarkGray);
                ColoredPrint($"Exception HResult: {e.HResult}\n", DarkGreen);
                ColoredPrint($"Exception Inner Exception: {e.InnerException}\n", DarkMagenta);
                ColoredPrint($"Exception Source: {e.Source}\n", DarkRed);
                ColoredPrint($"\nException StackTrace : {e.StackTrace}\n", DarkYellow);
                ColoredPrint($"\nException TargetSite: {e.TargetSite}\n", Cyan);
            }

        }
        #region MainMenu(List<FoodGeneralInfo> list) function void
        public static void MainMenu(List<FoodGeneralInfo> list)
        {
            string operation = string.Empty;
            do
            {
                Console.WriteLine("Please Choose From the Menu Your Operation:\n\n\n 1-Select\n 2-Add\n 3-Delete\n 4-Update\n 5-DownLoad\n 6-Buffer \n 7-Exit\n");
                operation = Console.ReadLine();
                if (operation == "7") break;
                switch (operation)
                {
                    // if we manipolate the list like deletion, addition, update we must pass the list by reference to the relevante funcion
                    case "1": SelectElement(list); break;
                    case "2": AddElements(ref list); break;
                    case "3": DeleteElements(ref list); break;
                    case "4": UpdateElement(ref list); break;
                    case "5": MakeJsonFile(list, GetJsonPath()); break;
                    case "6": BufferOperation(); break;
                    default: Console.WriteLine("The operator should be between [1-7]"); break;
                }
            } while (Convert.ToInt32(operation) < 0 || Convert.ToInt32(operation) > 7 || operation != "7");
        }
        #endregion
        #region PrintData(List<FoodGeneralInfo> list) function void
        public static void PrintData(List<FoodGeneralInfo> list)
        {
            var briefInfoTable = new ConsoleTable("Italian Name", "Category", "URL of the source");
            string op = string.Empty;
            do
            {
                Console.WriteLine($"\n\n 1- Brief information of the food\n 2- Detailed Information of the food\n" +
                     $" 3- Print Nutritions of the food\n 4- Print Langual code of the food\n 5-Exit");
                op = Console.ReadLine();
                if (op == "5") break;
                foreach (var item in list)
                {
                    switch (op)
                    {
                        case "1":
                            briefInfoTable.AddRow(item.ItalianName, item.Category, item.Url);
                            break;
                        case "2":
                            Console.WriteLine($"Italian Name = {item.ItalianName}\nEnglish Name = {item.EnglishName}\nScientific Namee = {item.ScientificName}\n" +
                                $"Category = {item.Category}\nInformation = {item.Information}\nUrl = {item.Url}\nPortion ={item.Portion}\n" +
                                $"Number Of Samples = {item.NumberOfSamples}");
                            Console.WriteLine();
                            ColoredPrint("========================================================================================\n", Yellow);
                            break;
                        case "3":
                            foreach (var ele in item.Nutritions)
                            {
                                Console.WriteLine($" food Name: {item.ItalianName} ==>  \n Category: {ele.Category} \n Description: {ele.Description}" +
                                    $" \n ValueFor100g: {ele.ValueFor100g} \n Procedures: {ele.Procedures} \n DataSource: {ele.DataSource} \n Reference:  {ele.Reference}");
                                Console.WriteLine("----------------------------------------------------------------------------------------------------------");
                            }; break;
                        case "4":
                            foreach (var ele in item.LangualCodes)
                            {
                                Console.WriteLine($"\n food Name: {item.ItalianName} \nLangual Id: {ele.Id} \n Langual Info: {ele.Info}");
                            }; break;
                        default: ColoredPrint("You did not choose [1-5]", Red); break;
                    }
                }
                if (op == "1") briefInfoTable.Write(Format.Alternative);
            } while (op != "5" || (Convert.ToInt32(op) < 1 && Convert.ToInt32(op) > 5));

        }
        #endregion
        #region SelectElement(List<FoodGeneralInfo> list) function ---> returns a List<FoodGeneralInfo>
        public static List<FoodGeneralInfo> SelectElement(List<FoodGeneralInfo> list)
        {
            Console.WriteLine("\nPlease select from the menu the Argument you want to do seach on :\n\n 1-Italian Name\n 2-English Name\n 3-Scientific Name\n 4-Category\n 5-Food Code\n 6-All above Arguments\n");
            string command = string.Empty;
            do
            {
                Console.WriteLine("Please Choose from the Menu (1-6): ");
                command = Console.ReadLine().Trim();

            } while (Convert.ToInt32(command) < 0 || Convert.ToInt32(command) > 6);
            Console.WriteLine("Please Insert The searchKey: ");
            string searchKey = Console.ReadLine();
            List<FoodGeneralInfo> SelectedItems = new List<FoodGeneralInfo>();
            switch (command)
            {
                case "1": SelectedItems = list.Where(x => x.ItalianName.Contains(searchKey, StringComparison.CurrentCultureIgnoreCase)).ToList(); break;
                case "2": SelectedItems = list.Where(x => x.EnglishName.Contains(searchKey, StringComparison.CurrentCultureIgnoreCase)).ToList(); break;
                case "3": SelectedItems = list.Where(x => x.ScientificName.Contains(searchKey, StringComparison.CurrentCultureIgnoreCase)).ToList(); break;
                case "4": SelectedItems = list.Where(x => x.Category.Contains(searchKey, StringComparison.CurrentCultureIgnoreCase)).ToList(); break;
                case "5": SelectedItems = list.Where(x => x.FoodCode == searchKey).ToList(); break;
                case "6":
                    SelectedItems = list.Where(x => x.ItalianName.Contains(searchKey, StringComparison.CurrentCultureIgnoreCase)
                || x.ItalianName.Contains(searchKey, StringComparison.CurrentCultureIgnoreCase)
                || (!string.IsNullOrEmpty(x.ScientificName) && x.ScientificName.Contains(searchKey, StringComparison.CurrentCultureIgnoreCase))
                || x.Category.Contains(searchKey, StringComparison.CurrentCultureIgnoreCase)
                || x.FoodCode == searchKey).ToList(); break;
                default:
                    Console.WriteLine("Please insert a number between 1 - 6 ");
                    SelectedItems = new List<FoodGeneralInfo>(); break;
            }
            ColoredPrint("\nThe search result has " + SelectedItems.Count + " Elements out of " + list.Count + " Elements.\n", Green);
            if (SelectedItems.Count > 0)
            {
                Console.WriteLine($"\nDo you want to see the result? [y][n]\n");
                string op = string.Empty;
                do
                {
                    op = Console.ReadLine();

                } while (string.IsNullOrEmpty(op));
                if (op.Equals("y", StringComparison.CurrentCultureIgnoreCase))
                {
                    PrintData(SelectedItems);
                }
                Console.WriteLine("Do You like to Download the result of select search:  [y][n]?");
                if (Console.ReadLine().Equals("y", StringComparison.CurrentCultureIgnoreCase))
                {
                    Console.WriteLine("Please Insert the path of the json file you want to save (just the path!!): ");
                    string filePath = string.Empty;
                    do
                    {
                        filePath = Console.ReadLine();
                    } while (string.IsNullOrEmpty(filePath));
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }
                    Console.WriteLine("Please Insert the name of the file without .json: ");
                    string path = $"{filePath}\\{Console.ReadLine()}.json";
                    MakeJsonFile(SelectedItems, path);
                    ColoredPrint($"\nThe file Created successfully in the address : {path}\n", DarkGreen);
                }

                Console.WriteLine("Do You like to copy the result To the buffer: [y][n]?");
                if (Console.ReadLine().Equals("y", StringComparison.CurrentCultureIgnoreCase))
                {
                    Console.WriteLine("Please Insert The Name of The Buffer");
                    Global.Buffer.Add(Console.ReadLine(), SelectedItems);
                    ColoredPrint("\nThe selected Items is Added to the buffer You can always come Back to the main Menu and use it!\n", Green);
                }
            }
            return SelectedItems;
        }
        #endregion
        #region AddElement(ref List<FoodGeneralInfo> list) function void
        public static void AddElements(ref List<FoodGeneralInfo> list)
        {
            ColoredPrint("\nWelcome to the addition Section: \n\n", DarkMagenta);
            List<FoodGeneralInfo> listofNewitem = new List<FoodGeneralInfo>();
            listofNewitem.Add(AddOneElement(ref list));
            list.AddRange(listofNewitem);
            Console.WriteLine("\nPlease Choose the operation:\n 1-See the item Added: \n 2-Back To The Main Menu");
            string operation = string.Empty;
            do
            {
                operation = Console.ReadLine();

            } while (string.IsNullOrEmpty(operation));

            switch (operation)
            {
                case "1": PrintData(listofNewitem); break;
                case "2": MainMenu(list); break;
                case "3": break;
            }
        }
        #endregion
        #region UpdateElement(List<FoodGeneralInfo> list) function void
        public static void UpdateElement(ref List<FoodGeneralInfo> list)
        {
            List<NutrientsInfo> listofAllNutrients = Global.ListofAllNutrients;
            List<LangualInfo> listofAllLanguals = Global.ListOfAllLanguals;
            List<ChartInfo> listofAllCharts = Global.ListOfAllCharts;
            // I deleted the selected item list from the main list and after the modification I will add it again to the main list
            List<FoodGeneralInfo> selectedItems = SelectElement(list);
            list = list.Except(selectedItems).ToList();
            List<FoodGeneralInfo> orginalList = list;
            int numberOfFoodInSelectedList = selectedItems.Count();
            foreach (var item in selectedItems)
            {
                List<NutrientsInfo> listOfNutForSelectedItems = new List<NutrientsInfo>();
                List<LangualInfo> listOfLangualsForSelectedItems = new List<LangualInfo>();
                List<ChartInfo> listOfChartsForSelectedItems = new List<ChartInfo>();
                listOfNutForSelectedItems = listofAllNutrients.Where(x => x.FoodCode == item.FoodCode).ToList();
                listOfLangualsForSelectedItems = listofAllLanguals.Where(x => x.FoodCode == item.FoodCode).ToList();
                listOfChartsForSelectedItems = listofAllCharts.Where(x => x.FoodCode == item.FoodCode).ToList();
                List<string> AllCategories = listOfNutForSelectedItems.Select(x => x.Category).Distinct().ToList();

                //I jsut do this update for general section
                Console.WriteLine($"\nThe food: {item.ItalianName} selected: \n1-Update General section\n2-Update Nutritions section\n3-Update Langual code section\n4-Update chart Section\n5- Exit");
                string op = Console.ReadLine();
                switch (op)
                {
                    case "1":
                        Console.WriteLine($"\nPlease Choose your desired camp:");
                        Console.WriteLine($"1-Italian Name = {item.ItalianName}\n2-English Name = {item.EnglishName}\n3-Scientific Namee = {item.ScientificName}\n" +
                                $"4-Category = {item.Category}\n5-Information = {item.Information}\n6-Url = {item.Url}\n7-Portion ={item.Portion}\n" +
                                $"8-Number Of Samples = {item.NumberOfSamples}\n9-Exit");
                        string operation = Console.ReadLine();
                        switch (operation)
                        {
                            case "1":
                                Console.WriteLine("Please Insert the new Value:");
                                string value = Console.ReadLine();
                                ColoredPrint($"Italian Name = {item.ItalianName} ----> {value}\n", Green);
                                item.ItalianName = value;
                                break;
                            case "2":
                                Console.WriteLine("Please Insert the new Value:");
                                value = Console.ReadLine();
                                ColoredPrint($"Italian Name = {item.EnglishName} ----> {value}\n", Green);
                                item.EnglishName = value;
                                break;
                            case "3":
                                Console.WriteLine("Please Insert the new Value:");
                                value = Console.ReadLine();
                                ColoredPrint($"Italian Name = {item.ScientificName} ----> {value}\n", Green);
                                item.ScientificName = value;
                                break;
                            case "4":
                                Console.WriteLine("Please Insert the new Value:");
                                value = Console.ReadLine();
                                ColoredPrint($"Italian Name = {item.Category} ----> {value}\n", Green);
                                item.Category = value;
                                break;
                            case "5":
                                Console.WriteLine("Please Insert the new Value:");
                                value = Console.ReadLine();
                                ColoredPrint($"Italian Name = {item.Information} ----> {value}\n", Green);
                                item.Information = value;
                                break;
                            case "6":
                                Console.WriteLine("Please Insert the new Value:");
                                value = Console.ReadLine();
                                ColoredPrint($"Italian Name = {item.Url} ----> {value}\n", Green);
                                item.Url = value;
                                break;
                            case "7":
                                Console.WriteLine("Please Insert the new Value:");
                                value = Console.ReadLine();
                                ColoredPrint($"Italian Name = {item.Portion} ----> {value}\n", Green);
                                item.Portion = value;
                                break;
                            case "8":
                                Console.WriteLine("Please Insert the new Value:");
                                value = Console.ReadLine();
                                ColoredPrint($"Italian Name = {item.NumberOfSamples} ----> {value}\n", Green);
                                item.NumberOfSamples = value;
                                break;
                            case "9": break;
                            default:
                                break;
                        }
                        break;
                    case "2":
                        Console.WriteLine("The number of Categories: " + AllCategories.Count + "\n");
                        foreach (var category in AllCategories)
                        {
                            Console.WriteLine(category);
                        }
                        ColoredPrint("If you do not want to continue you can choose [exit]!\n", Red);
                        var listOfNutPerCategory = new List<NutrientsInfo>();
                        Console.WriteLine("Please insert the desiered Category by its exact name: \n");
                        string op1 = Console.ReadLine();
                        var nutTable = new ConsoleTable("Name", "Value For 100g", "DataSource");
                        Console.WriteLine($"This is Brief Information of the nutrients for the food [{item.ItalianName}]: ");

                        switch (op1)
                        {
                            case "MACRO NUTRIENTI":
                                var MACRONUTRIENTI = listOfNutForSelectedItems.Where(x => x.Category == "MACRO NUTRIENTI").ToList();
                                UpdateSelectednuts(ref MACRONUTRIENTI, nutTable);
                                break;
                            case "MINERALI":
                                var MINERALI = listOfNutForSelectedItems.Where(x => x.Category == "MMINERALI").ToList();
                                UpdateSelectednuts(ref MINERALI, nutTable);
                                break;
                            case "VITAMINE":
                                var VITAMINE = listOfNutForSelectedItems.Where(x => x.Category == "VITAMINE").ToList();
                                UpdateSelectednuts(ref VITAMINE, nutTable);
                                break;
                            case "ACIDI GRASSI":
                                var ACIDIGRASSI = listOfNutForSelectedItems.Where(x => x.Category == "CIDI GRASSI").ToList();
                                UpdateSelectednuts(ref ACIDIGRASSI, nutTable);
                                break;
                            case "AMINOACIDI (% di proteine)":
                                var AMINOACIDI = listOfNutForSelectedItems.Where(x => x.Category == "AMINOACIDI (% di proteine)").ToList();
                                UpdateSelectednuts(ref AMINOACIDI, nutTable);
                                break;
                            case "ALTRI COMPOSTI":
                                var ALTRI = listOfNutForSelectedItems.Where(x => x.Category == "ALTRI COMPOSTI").ToList();
                                UpdateSelectednuts(ref ALTRI, nutTable);
                                break;
                            case "SINGOLI ZUCCHERI":
                                var SINGOLI = listOfNutForSelectedItems.Where(x => x.Category == "INGOLI ZUCCHERI").ToList();
                                UpdateSelectednuts(ref SINGOLI, nutTable);
                                break;
                            case "exit": break;
                        }
                        break;
                    case "3":
                        var langualTable = new ConsoleTable("LangualID", "Langual Info");
                        foreach (var langual in listOfLangualsForSelectedItems)
                        {
                            langualTable.AddRow(langual.Id, langual.Info);
                        }
                        Console.WriteLine($"This is Brief Information of the langual for the food [{item.ItalianName}]: ");
                        langualTable.Write();
                        foreach (var langual in listOfLangualsForSelectedItems)
                        {
                            Console.WriteLine($"\nPlease insert  the new values for the colomn [Id] and [Info]: ");
                            Console.Write($"The value of [Id] is {langual.Id} changin to ---> ");
                            langual.Id = Console.ReadLine();
                            Console.Write($"The value of [Info] is {langual.Info} changin to ---> ");
                            langual.Info = Console.ReadLine();
                            ColoredPrint($"The new value is {langual.Id}\n", Green);
                            ColoredPrint($"The new value is {langual.Info}\n", Green);
                        }
                        break;
                    case "4":
                        var cahrtTable = new ConsoleTable("Protein", "Fat", "Carbohydrate", "Fiber", "Alcohol");
                        foreach (var chart in listOfChartsForSelectedItems)
                        {
                            cahrtTable.AddRow(chart.Protein, chart.Fat, chart.Carbohydrate, chart.Fiber, chart.Alcohol);
                        }
                        Console.WriteLine($"This is full Information of the chart for the food [{item.ItalianName}]: ");
                        cahrtTable.Write();
                        Console.WriteLine($"Please choose one of the following operation:\n 1-Protein\n 2-Fat\n 3-Carbohydrate\n 4-Fiber\n 5-Alcohol\n 6-Exit");
                        string operation1 = Console.ReadLine();
                        switch (operation1)
                        {
                            case "1":
                                Console.WriteLine("Please Insert the new value of Protein:\n");
                                Console.Write($"The value of [Protein] is {listOfChartsForSelectedItems[0].Protein} changin to ---> ");
                                listOfChartsForSelectedItems[0].Protein = Console.ReadLine();
                                ColoredPrint($"The new value is {listOfChartsForSelectedItems[0].Protein}\n", Green);
                                break;
                            case "2":
                                Console.WriteLine("Please Insert the new value of Fat:\n");
                                Console.Write($"The value of [Fat] is {listOfChartsForSelectedItems[0].Fat} changin to ---> ");
                                listOfChartsForSelectedItems[0].Fat = Console.ReadLine();
                                ColoredPrint($"The new value is {listOfChartsForSelectedItems[0].Fat}\n", Green);
                                break;
                            case "3":
                                Console.WriteLine("Please Insert the new value of Carbohydrate:\n");
                                Console.Write($"The value of [Carbohydrate] is {listOfChartsForSelectedItems[0].Carbohydrate} changin to ---> ");
                                listOfChartsForSelectedItems[0].Carbohydrate = Console.ReadLine();
                                ColoredPrint($"The new value is {listOfChartsForSelectedItems[0].Carbohydrate}\n", Green);
                                break;
                            case "4":
                                Console.WriteLine("Please Insert the new value of Fiber:\n");
                                Console.Write($"The value of [Fiber] is {listOfChartsForSelectedItems[0].Fiber} changin to ---> ");
                                listOfChartsForSelectedItems[0].Fiber = Console.ReadLine();
                                ColoredPrint($"The new value is {listOfChartsForSelectedItems[0].Fiber}\n", Green);
                                break;
                            case "5":
                                Console.WriteLine("Please Insert the new value of Alcohol:\n");
                                Console.Write($"The value of [Alcohol] is {listOfChartsForSelectedItems[0].Alcohol} changin to ---> ");
                                listOfChartsForSelectedItems[0].Alcohol = Console.ReadLine();
                                ColoredPrint($"The new value is {listOfChartsForSelectedItems[0].Alcohol}\n", Green);
                                break;
                            case "6": break;
                            default:
                                break;
                        }
                        break;
                    case "5": break;
                    default: ColoredPrint("Please choose a number between [1-5]", Red); break;
                }
                --numberOfFoodInSelectedList;
                if (numberOfFoodInSelectedList > 0)
                {
                    Console.WriteLine($"You updated your desiered camps for the food : {item.ItalianName}");
                    Console.WriteLine($"Your selected list has still {numberOfFoodInSelectedList} foods to update");
                    Console.WriteLine("Do you want to continue: [y][n]");
                    if (Console.ReadLine() != "y")
                    {
                        break;
                    }
                }
            }
            list.AddRange(selectedItems);
            list = list.OrderBy(x => x.ItalianName).ToList();
        }
        #endregion
        #region DeleteElements(ref List<FoodGeneralInfo> list) function void
        public static void DeleteElements(ref List<FoodGeneralInfo> list)
        {
            List<FoodGeneralInfo> desiredItemsToDelete = SelectElement(list);
            ColoredPrint("Warning: Do you confirm The deletion of selected items? [Y][N]\n", Yellow);
            string op = Console.ReadLine();
            if (op.Equals("Y", StringComparison.CurrentCultureIgnoreCase))
            {
                ColoredPrint($"\n----> You deleted {desiredItemsToDelete.Count} items successfully! <----\n", Red);
                list = list.Except(desiredItemsToDelete).ToList();
            }
        }
        #endregion
        #region string GetJsonPath() function ---> returns a string
        public static string GetJsonPath()
        {
            Console.WriteLine("Please Insert the path of the json file (Please insert complete path like C:/folder/filename.json): ");
            string filePath = string.Empty;
            do
            {
                filePath = Console.ReadLine();
            } while (string.IsNullOrEmpty(filePath));
            return filePath;
        }
        #endregion
        #region GetJsonData(string path)  ---> returns a List<FoodGeneralInfo>
        public static List<FoodGeneralInfo> GetJsonData(string path)
        {
            string jsonString = File.ReadAllText(path);
            if (!string.IsNullOrEmpty(jsonString))
            {
                return JsonConvert.DeserializeObject<List<FoodGeneralInfo>>(jsonString);
            }
            else
            {
                ColoredPrint("The file does not loaded correctly\n", Red);
                return new List<FoodGeneralInfo>();
            }
        }
        #endregion
        #region MakeJsonFileList<FoodGeneralInfo> list, string path) function void
        public static void MakeJsonFile(List<FoodGeneralInfo> list, string path)
        {

            var option = new JsonSerializerOptions { WriteIndented = true, AllowTrailingCommas = true };
            string jsonString = System.Text.Json.JsonSerializer.Serialize(list, option);
            File.WriteAllText(path, jsonString);
        }
        #endregion
        #region BufferOperation() function void
        public static void BufferOperation()
        {

            do
            {
                Console.WriteLine("\n inlevel - You are now in buffer section, you have the search result " +
                                    "that you saved in selection section\n");
                Console.WriteLine("1-See the buffers\n2-exit");
                string op = Console.ReadLine();
                if (op != "1") { break; }
                else if (Global.Buffer.Keys.Count == 0) { ColoredPrint("You do not have any saved buffer\n", Red); break; }
                Console.WriteLine($"you have this buffer names: ");
                foreach (var item in Global.Buffer)
                {
                    Console.WriteLine(item.Key);
                }
                Console.WriteLine("Please Choose a buffer by the name of it: ");
                string key = Console.ReadLine();
                foreach (var item in Global.Buffer)
                {
                    if (key == item.Key)
                    {
                        Console.WriteLine($"You are now in {item.Key} buffer \n");
                        Console.WriteLine($"\n The {item.Key} buffer has {item.Value.Count} Items. ");
                        MainMenu(item.Value);
                        Console.WriteLine($"\nYou Left {item.Key} buffer \n");
                    }
                }

            } while (true);

            Console.WriteLine("\n inlevel --> MainLevel Coming Back to the main Menu");
        }
        #endregion
        #region ColoredPrint(string text, ConsoleColor color) function void
        public static void ColoredPrint(string text, ConsoleColor color)
        {
            //       The background color is Black.
            //       The background color is DarkBlue.
            //       The background color is DarkGreen.
            //       The background color is DarkCyan.
            //       The background color is DarkRed.
            //       The background color is DarkMagenta.
            //       The background color is DarkYellow.
            //       The background color is Gray.
            //       The background color is DarkGray.
            //       The background color is Blue.
            //       The background color is Green.
            //       The background color is Cyan.
            //       The background color is Red.
            //       The background color is Magenta.
            //       The background color is Yellow
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();

        }
        #endregion
        #region GetNutrients(List<FoodGeneralInfo> list) funtion ---> returns a List<NutrientsInfo>
        public static List<NutrientsInfo> GetNutrients(List<FoodGeneralInfo> list)
        {
            List<NutrientsInfo> nutrients = new List<NutrientsInfo>();
            foreach (var food in list)
            {
                foreach (var nutrient in food.Nutritions)
                {
                    nutrients.Add(new NutrientsInfo
                    {
                        FoodCode = food.FoodCode,
                        Category = nutrient.Category,
                        NutrientName = nutrient.Description,
                        ValueFor100g = nutrient.ValueFor100g,
                        Procedures = nutrient.Procedures,
                        DataSource = nutrient.DataSource,
                        Reference = nutrient.Reference
                    });
                }
            }
            return nutrients;
        }
        #endregion
        #region UpdateSelectednuts(ref List<NutrientsInfo> list, ConsoleTable nutTable)  function void
        public static void UpdateSelectednuts(ref List<NutrientsInfo> list, ConsoleTable nutTable)
        {
            Console.WriteLine("The Nutrient has these colomns[ Name | Value For 100g | DataSource | Procedures] but you can not see some of them");
            foreach (var nutrient in list)
            {
                nutTable.AddRow(nutrient.NutrientName, nutrient.ValueFor100g, nutrient.DataSource);
            }
            nutTable.Write();
            foreach (var nutrient in list)
            {
                Console.WriteLine($"\nPlease insert  the new values for the colomn [Value For 100 g] of the nutrient {nutrient.NutrientName}: ");
                Console.Write($"The value of [Value for 100 g] is {nutrient.ValueFor100g} changin to ---> ");
                nutrient.ValueFor100g = Console.ReadLine();
                ColoredPrint($"The new value is {nutrient.ValueFor100g}\n", Green);
            }

            foreach (var nutrient in list)
            {
                Console.WriteLine($"\nPlease insert  the new values of [DataSource] of the nutrient {nutrient.NutrientName}: ");
                Console.Write($"The value of [DataSource] is {nutrient.DataSource} changin to ---> ");
                nutrient.DataSource = Console.ReadLine();
                ColoredPrint($"The new value is {nutrient.DataSource}\n", Green);
            }

            foreach (var nutrient in list)
            {
                Console.WriteLine($"\nPlease insert  the new values of [Procedures] of the nutrient {nutrient.NutrientName}: ");
                Console.Write($"The value of [Procedures] is {nutrient.Procedures} changin to ---> ");
                nutrient.Procedures = Console.ReadLine();
                ColoredPrint($"The new value is {nutrient.Procedures}\n", Green);
            }

            foreach (var nutrient in list)
            {
                Console.WriteLine($"\nPlease insert the new values of [Reference] of the nutrient {nutrient.NutrientName} : ");
                nutrient.Reference = "By Arash";
                ColoredPrint($"The new value is {nutrient.Reference}\n", Green);
            }
        }
        #endregion
        #region GetLanguals(List<FoodGeneralInfo> list) function ---> return List<LangualInfo>
        public static List<LangualInfo> GetLanguals(List<FoodGeneralInfo> list)
        {
            List<LangualInfo> Languals = new List<LangualInfo>();
            foreach (var food in list)
            {
                foreach (var langual in food.LangualCodes)
                {
                    Languals.Add(new LangualInfo
                    {
                        FoodCode = food.FoodCode,
                        Id = langual.Id,
                        Info = langual.Info
                    });
                }
            }
            return Languals;
        }
        #endregion
        #region GetCharts(List<FoodGeneralInfo> list) function ---> returns List<ChartInfo>
        public static List<ChartInfo> GetCharts(List<FoodGeneralInfo> list)
        {
            List<ChartInfo> chart = new List<ChartInfo>();
            foreach (var food in list)
            {
                chart.Add(new ChartInfo
                {
                    FoodCode = food.FoodCode,
                    Alcohol = food.ChartData.Alcohol,
                    Fat = food.ChartData.Fat,
                    Protein = food.ChartData.Protein,
                    Fiber = food.ChartData.Fiber,
                    Carbohydrate = food.ChartData.Carbohydrate
                });
            }
            return chart;
        }
        #endregion
        #region AddOneElement(ref List<FoodGeneralInfo> list) function --> return FoodGeneralInfo
        public static FoodGeneralInfo AddOneElement(ref List<FoodGeneralInfo> list)
        {

            FoodGeneralInfo food = new FoodGeneralInfo();
            List<string> FoodCodeProjectAttributeSTR = list.Where(x => x.FoodCode.All(Char.IsDigit)).Select(x => x.FoodCode).ToList();
            List<int> FoodCodeProjectAttributeINT = FoodCodeProjectAttributeSTR.Select(int.Parse).ToList();
            int maxFoodCode = FoodCodeProjectAttributeINT.Max();
            int newFoodCode = ++maxFoodCode;
            string foodCode = newFoodCode.ToString();
            Console.WriteLine("In this section you can add an Element to the list Of Elements."); 
            string op = string.Empty;
            do
            {
                Console.WriteLine("Please Choose a camp that you want to add Information:\n 1-General section\n 2-Nutrients section" +
                                    "\n 3-Langual Code section\n 4-Chart Data section\n 5-Exit");
                op = Console.ReadLine();
                switch (op)
                {
                    case "1":
                        do
                        {
                            Console.WriteLine("Please Insert the Italian name of the food:");
                            food.ItalianName = Console.ReadLine().Trim();
                        } while (string.IsNullOrEmpty(food.ItalianName));
                        Console.WriteLine("Please Insert the Category of the food:");
                        food.Category = Console.ReadLine().Trim();
                        Console.WriteLine("Please Insert the Scientific Name of the food:");
                        food.ScientificName = Console.ReadLine().Trim();
                        Console.WriteLine("Please Insert the English Name of the food:");
                        food.EnglishName = Console.ReadLine().Trim();
                        Console.WriteLine("Please Insert the Information of the food:");
                        food.Information = Console.ReadLine().Trim();
                        Console.WriteLine("Please Insert the Number Of Samples of the food:");
                        food.NumberOfSamples = Console.ReadLine().Trim();
                        Console.WriteLine("Please Insert the Eatable Partpercentage part of the food:");
                        food.EatablePartpercentage = Console.ReadLine().Trim();
                        Console.WriteLine("Please Insert the Portion of the food:");
                        food.Portion = Console.ReadLine().Trim();
                        ColoredPrint($"This is your new food code ---> {foodCode}\n\n", Green);
                        food.FoodCode = foodCode;
                        food.Url = "This Item is Added by the user!";
                        break;
                    case "2":
                        ColoredPrint("\nWelcome to the nutrients section\n\n", DarkMagenta);
                        string question = "";
                        do
                        {
                            Console.WriteLine("Please Insert the nutrient category:");
                            string category = Console.ReadLine().Trim();
                            Console.WriteLine("Please Insert the nutrient Name:");
                            string name = Console.ReadLine().Trim();
                            Console.WriteLine("Please Insert the nutrient energy for value for 100 g:");
                            string valueFor100g = Console.ReadLine().Trim();
                            Console.WriteLine("Please Insert the nutrient procedure to calculate the energy:");
                            string procedure = Console.ReadLine().Trim();
                            Console.WriteLine("Please Insert the nutrient data source:");
                            string dataSource = Console.ReadLine().Trim();
                            Console.WriteLine("Please Insert the nutrient energy calculation reference:");
                            string reference = Console.ReadLine().Trim();
                            food.Nutritions.Add(new Nutrition
                            {
                                Category = category,
                                Description = name,
                                ValueFor100g = valueFor100g,
                                Procedures = procedure,
                                DataSource = dataSource,
                                Reference = reference

                            });
                            Console.WriteLine("Do you want to add another nutrinet: [y][n]");
                            question = Console.ReadLine();

                        } while (question == "y");
                        Console.WriteLine($"You added {food.Nutritions.Count()} nutrients");
                        break;
                    case "3":
                        Console.WriteLine("Please insert how many langual codes you shoud enter\n");
                        int num = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("You can add a langual code by first add its id and then its info");
                        for (int i = 0; i < num; i++)
                        {
                            Console.Write("Id: "); string id = Console.ReadLine();
                            Console.Write("Info: "); string info = Console.ReadLine();
                            food.LangualCodes.Add(new Langual { Id = id, Info = info });
                        }
                        ColoredPrint("Thank you for adding the langual codes!\n\n", Green);
                        break;
                    case "4":
                        int percent = 0;
                        string protein, fat, carbohydrate, fiber, alcohol;
                        ColoredPrint("Welcome to the chart data section\n\n",DarkMagenta);

                        do
                        {

                            Console.WriteLine("Please Insert the Protein Percentage:");
                            protein = Console.ReadLine().Trim();
                            percent += Convert.ToInt32(protein);
                            Console.WriteLine("Please Insert the Fat Percentage:");
                            fat = Console.ReadLine().Trim();
                            percent += Convert.ToInt32(fat);
                            Console.WriteLine("Please Insert the Carbohydrate Percentage:");
                            carbohydrate = Console.ReadLine().Trim();
                            percent += Convert.ToInt32(carbohydrate);
                            Console.WriteLine("Please Insert the Fiber Percentage:");
                            fiber = Console.ReadLine().Trim();
                            percent += Convert.ToInt32(fiber);
                            Console.WriteLine("Please Insert the Alcohol Percentage:");
                            alcohol = Console.ReadLine().Trim();
                            percent += Convert.ToInt32(alcohol);

                        } while (percent > 100 || percent < 0);

                        food.ChartData.Protein = protein;
                        food.ChartData.Fat = fat;
                        food.ChartData.Carbohydrate = carbohydrate;
                        food.ChartData.Fiber = fiber;
                        food.ChartData.Alcohol = alcohol;

                        break;
                    case "5": break;
                    default:
                        ColoredPrint("Please Choose a number between [1-5]", Red);
                        break;
                }
            } while (op != "5");
            return food;
        }
        #endregion
    }
}