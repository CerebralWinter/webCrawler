using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client; // NuGet
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Var_WebCrawler_CRUD;
// with static it turns to an enum ====> intressting
using static System.ConsoleColor;

namespace var.WebCrawler.CRUD
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ServiceClient D365Client = ConfigServiceClient();
                List<FoodGeneralInfo> listOfFood = GetJsonData(GetJsonPath());
                Console.WriteLine("\nThe numbr of food loaded: " + listOfFood.Count());
                var listofCategory = listOfFood.Select(x => x.Category).Distinct().ToList();
                List<LangualInfo> listOfAllLangual = GetLanguals(listOfFood);
                List<LangualInfo> listOfDistinctLanguals = listOfAllLangual.DistinctBy(x => x.Id).ToList();
                List<NutrientsInfo> listOfAllnutrients = GetNutrients(listOfFood);
                List<string> listOfnutCategory = listOfAllnutrients.Select(X => X.Category).Distinct().ToList();
                PushFoodData(listOfFood, D365Client);
                //PushFoodCategory(listofCategory, D365Client);
                //PushLangual(listOfDistinctLanguals, D365Client);
                //PushNutrient(listOfAllnutrients, D365Client);
                //PushNutCategory(listOfnutCategory, D365Client);
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

        }
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
        #region Connection to the powerApps
        public static ServiceClient ConfigServiceClient()
        {
            const string D365_ORG = "varprime-academy-2023";
            var D365_URI = new Uri($"https://{D365_ORG}.crm4.dynamics.com");
            const string D365_ClientId = "9e84e477-fa60-40bf-8bf2-a5f6234c52a0";
            const string D365_ClientSecret = "j2O8Q~wcZx-qIXtQtHTFDQ16ll.ZXLf.4e-IHaM2";
            Console.WriteLine($"Connessione a Dynamics 365 (Url: {D365_URI.AbsoluteUri})");
            var D365Client = new ServiceClient(D365_URI, D365_ClientId, D365_ClientSecret, true);
            var d365WhoAmI = D365Client.Execute(new WhoAmIRequest()) as WhoAmIResponse; // richiedo al CRM "chi sono?", mi ritorna i dati dell'utente usato per collegarsi
            var d365UserName = D365Client.Retrieve("systemuser", d365WhoAmI?.UserId ?? Guid.Empty, new("fullname"));
            Console.WriteLine($"Connesso come utente '{d365WhoAmI?.UserId}' ( Id utente: {d365UserName["fullname"]} )");
            return D365Client;
        }
        #endregion
        #region PushFoodData(List<string> list, ServiceClient D365Client) function void
        public static void PushFoodData(List<FoodGeneralInfo> list, ServiceClient D365Client)
        {
            string foodTable = "aa_food";
            Console.WriteLine("Total Items of the list: " + list.Count);
            int count = 1;
            // retrive the collection entity of the category food
            QueryExpression query = new QueryExpression()
            {
                EntityName = "aa_foodcategory",
                ColumnSet = new ColumnSet("aa_foodcategoryid", "aa_name")
            };
            var categoryEntities = D365Client.RetrieveMultiple(query).Entities;
            foreach (var item in list)
            {
                List<EntityReference> lagualEntityReferenceforOneFodd = new List<EntityReference>();
                int count1 = 1;
                Console.WriteLine($"Total Langual item of the food  {item.ItalianName} : " + item.LangualCodes.Count());
                foreach (var langual in item.LangualCodes)
                {
                    var rowLangual = new Entity("aa_langual");
                    new EntityReference("aa_langualid", Guid.Empty);
                    rowLangual.KeyAttributes.Add("aa_langualidkey", langual.Id);
                    rowLangual.Attributes.Add("aa_name", langual.Info);
                    var upsertRequest1 = new UpsertRequest { Target = rowLangual };
                    var upsertResponse = (UpsertResponse)D365Client.Execute(upsertRequest1);
                    var riferimentoNuovaRiga = upsertResponse.Target;
                    lagualEntityReferenceforOneFodd.Add(riferimentoNuovaRiga);
                    Console.WriteLine($"{count1} Langual added to CRM");
                    count1++;
                }
                var row = new Entity(foodTable);
                new EntityReference("aa_foodid", Guid.Empty);
                row.KeyAttributes.Add("aa_foodidkey", item.FoodCode);
                row.Attributes.Add("aa_alcohol", item.ChartData.Alcohol);
                row.Attributes.Add("aa_fat", item.ChartData.Fat);
                row.Attributes.Add("aa_protein", item.ChartData.Protein);
                row.Attributes.Add("aa_carbohydrate", item.ChartData.Carbohydrate);
                row.Attributes.Add("aa_fiber", item.ChartData.Fiber);
                row.Attributes.Add("aa_eatablepart", item.EatablePartpercentage);
                row.Attributes.Add("aa_englishname", item.EnglishName);
                row.Attributes.Add("aa_foodname", item.ItalianName);
                row.Attributes.Add("aa_information", item.Information);
                row.Attributes.Add("aa_numberofsample", item.NumberOfSamples);
                row.Attributes.Add("aa_portion", item.Portion);
                row.Attributes.Add("aa_scientificname", item.ScientificName);
                foreach (var entity in categoryEntities)
                {
                    if (item.Category.Equals(entity.GetAttributeValue<string>("aa_name")))
                    {
                        row.Attributes.Add("aa_foodcategoryid", entity.ToEntityReference());
                        break;
                    }
                }
                var upsertRequest = new UpsertRequest { Target = row };
                var upsertAnswer = (UpsertResponse)D365Client.Execute(upsertRequest);
                var RowReference = upsertAnswer.Target;
                Relationship relationship = new Relationship("aa_food_aa_langual_aa_langual");
                D365Client.Associate(RowReference.LogicalName, RowReference.Id, relationship, new EntityReferenceCollection(lagualEntityReferenceforOneFodd));
                Console.WriteLine($"{count} item added to CRM");
                count++;
            }
        }
        #endregion
        #region PushFoodCategory(List<FoodCategory> list, ServiceClient D365Client) --> returns void
        public static void PushFoodCategory(List<string> list, ServiceClient D365Client)
        {
            string FoodCategoryTable = "aa_foodcategory";
            int count = 1;
            Console.WriteLine("Total Items of the list: " + list.Count);
            foreach (var item in list)
            {
                var row = new Entity(FoodCategoryTable);
                new EntityReference("aa_foodcategoryid", Guid.Empty);
                row.KeyAttributes.Add("aa_name", item);
                var upsertRequest = new UpsertRequest { Target = row };
                var upsertResponse = (UpsertResponse)D365Client.Execute(upsertRequest);
                var riferimentoNuovaRiga = upsertResponse.Target;
                Console.WriteLine($"{count} item added to CRM");
                count++;
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
        #region PushLangual(List<LangualInfo> list, ServiceClient D365Client) function void
        public static void PushLangual(List<LangualInfo> list, ServiceClient D365Client)
        {
            string languaTable = "aa_langual";
            int count = 1;
            Console.WriteLine("Total Items of the list: " + list.Count);
            foreach (var item in list)
            {
                var row = new Entity(languaTable);
                new EntityReference("aa_langualid", Guid.Empty);
                row.KeyAttributes.Add("aa_langualidkey", item.Id);
                row.Attributes.Add("aa_name", item.Info);
                //row.Attributes.Add("aa_scientificname", item.Category); // 
                var upsertRequest = new UpsertRequest { Target = row };
                var upsertResponse = (UpsertResponse)D365Client.Execute(upsertRequest);
                var riferimentoNuovaRiga = upsertResponse.Target;

                Console.WriteLine($"{count} item added to CRM");
                count++;
            }
        }

        #endregion
        #region  PushNutrient(List<NutrientsInfo> list, ServiceClient D365Client) function void
        public static void PushNutrient(List<NutrientsInfo> list, ServiceClient D365Client)
        {
            QueryExpression query = new QueryExpression()
            {
                EntityName = "aa_nutrientcategory",
                ColumnSet = new ColumnSet("aa_name", "aa_nutrientcategoryid")
            };
            var nutCategoryCollectiont = D365Client.RetrieveMultiple(query).Entities;
            QueryExpression query2 = new QueryExpression()
            {
                EntityName = "aa_food",
                ColumnSet = new ColumnSet("aa_foodid", "aa_foodidkey", "aa_foodname")
            };
            var foodidCollectionEntity = D365Client.RetrieveMultiple(query2).Entities;

            string nutrientTable = "aa_nutrient";
            int count = 1;
            Console.WriteLine("Total Items of the list: " + list.Count);
            foreach (var item in list)
            {
                var row = new Entity(nutrientTable);
                new EntityReference("aa_nutrientid", Guid.Empty);
                foreach (var foodEntity in foodidCollectionEntity)
                {
                    if (item.FoodCode.Equals(foodEntity.GetAttributeValue<string>("aa_foodidkey")))
                    {
                        row.KeyAttributes.Add("aa_foodidkey", foodEntity.ToEntityReference());
                        break;
                    }
                }
                row.KeyAttributes.Add("aa_name", item.NutrientName);
                row.Attributes.Add("aa_datasource", item.DataSource);
                row.Attributes.Add("aa_procedure", item.Procedures);
                row.Attributes.Add("aa_reference", item.Reference);
                row.Attributes.Add("aa_valuefor100g", item.ValueFor100g);
                foreach (var categoryEntity in nutCategoryCollectiont)
                {
                    if (item.Category.Equals(categoryEntity.GetAttributeValue<string>("aa_name")))
                    {
                        row.Attributes.Add("aa_nutrienctategoryid", categoryEntity.ToEntityReference());
                    }
                }
                var upsertRequest = new UpsertRequest { Target = row };
                var upsertResponse = (UpsertResponse)D365Client.Execute(upsertRequest);
                var riferimentoNuovaRiga = upsertResponse.Target;

                Console.WriteLine($"{count} item added to CRM");
                count++;

            }
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
        #region PushNutCategory(List<NutrientCategory> list, ServiceClient D365Client) function void
        public static void PushNutCategory(List<string> list, ServiceClient D365Client)
        {
            string nutCategoryTable = "aa_nutrientcategory";
            int count = 1;
            Console.WriteLine("Total Items of the list: " + list.Count);
            foreach (var CategoryName in list)
            {
                var row = new Entity(nutCategoryTable);
                new EntityReference("aa_nutrientcategoryid", Guid.Empty);
                row.KeyAttributes.Add("aa_name", CategoryName);
                var upsertRequest = new UpsertRequest { Target = row };
                var upsertResponse = (UpsertResponse)D365Client.Execute(upsertRequest);
                var riferimentoNuovaRiga = upsertResponse.Target;
                Console.WriteLine($"{count} item added to CRM");
                count++;
            }
        }
        #endregion
    }
}
