
/*
using Microsoft.Xrm.Sdk;

using Microsoft.Xrm.Sdk.Messages;

using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;

using System;

using System.Collections.Generic;

using System.Configuration;

using System.IO;

using System.Linq;



namespace z.Academy.Test2

{

    class Program

    {

        private const string APP_NAME = "z.Academy.Test2";



        private static readonly string clientId = ConfigurationManager.AppSettings["ClientId"];

        private static readonly string clientSecret = ConfigurationManager.AppSettings["ClientSecret"];

        private static readonly string organization = ConfigurationManager.AppSettings["Organization"];



        /*

          UTENTE ADMIN

          -----------------

          admin@CRM113374.onmicrosoft.com

         1i7bVKH;~#wR29+l

        



        // --- Main

        static void Maian(string[] args)

        {

            Console.WriteLine($"---------- | INIZIO | {APP_NAME} | ----------");

            try

            {

                if (false)

                {

                    //----- !!!!! FUNZIONE DI GENERAZONE RANDOM FILE DA DESERIALIZZARE !!!!!

                    // Cambiando i contenuti del file "MatriceProdotti.json", si possono generare file più o meno complessi

                    // !!! Prima di lanciare il programma, fare click destro e poi selezionare Proprietà nel file "MatriceProdotti.json"

                    // impostare (se non già impostato) la voce "Copia nella directory di output" a "Copia sempre"

                    var inputJsonFilePath = Matrice.GenerateRandomProdottiFiniti();



                    //----- !!!!! DA QUI !!!!! IMPLEMENTARE processProdottiFiniti() => DESCRIZIONE ALL'INTERNO

                    processProdottiFiniti(inputJsonFilePath);

                }



                var inputPath = @"C:\!-REPO\GIT\varconnecttemp\AcademyLab\DM_Dynamics365\z.Academy.Test2\bin\Debug\FileJsonOutput\ProdottiFiniti_2023-06-01_09-06-25.json";

                processProdottiFiniti(inputPath);

            }

            catch (Exception ex)

            {

                Console.WriteLine("##### ERRORE #####");

                Console.WriteLine(ex);

                Console.WriteLine("##########");

            }

            finally

            {

                Console.WriteLine($"---------- | FINE | {APP_NAME} | ----------");

            }

        }



        // --- Methods / CRM

        private static CrmServiceClient connectToCrm()

        {

            var CRM = new CrmServiceClient(new Uri($@"https://{organization}.crm4.dynamics.com"), clientId, clientSecret, true, null);

            if (!CRM.IsReady) throw new InvalidOperationException($"CRM Connection: {CRM.LastCrmError}");

            Console.WriteLine($"Connected to: [{CRM.ConnectedOrgFriendlyName}]");

            return CRM;

        }



        private static EntityReference upsertRecord(CrmServiceClient CRM, Entity record)

        {

            var upsertRequest = new UpsertRequest() { Target = record };

            var upsertResponse = CRM.Execute(upsertRequest) as UpsertResponse;

            return upsertResponse.Target;

        }



        // --- Methods --- !!!!! DA IMPLEMENTARE !!!!!

        private static void processProdottiFiniti(string inputJsonFilePath)

        {

            //LEGGERE IL FILE DA PATH PASSATO, DESERIALIZZARLO

            var jsonContent = File.ReadAllText(inputJsonFilePath);

            var prodottiFiniti = JsonConvert.DeserializeObject<List<ProdottoFinito>>(jsonContent);



            //-----



            var CRM = connectToCrm(); // - CrmServiceClient



            //POI CARICARE DATI IN CRM CON STRUTTURA ADEGUATA

            loadDataToCrm(CRM, prodottiFiniti);





            //-----



            //POI CONTARE USANDO DEI Dictionary<string, int> LE RIPETIZIONI DELLE VARIE FAMIGLIE/COMPONENTI (mostrare a video)



            var conteggioFamiglie = new Dictionary<string, int>();



            var queryProdotti = new QueryExpression("var_prodotto");

            queryProdotti.ColumnSet = new ColumnSet("var_famigliaid");



            var crmProdotti = CRM.RetrieveMultiple(queryProdotti).Entities;
            

            foreach (var prodottoRecord in crmProdotti)

            {

                var var_famigliaid = prodottoRecord.GetAttributeValue<EntityReference>("var_famigliaid");

                var famigliaRecord = CRM.Retrieve(var_famigliaid.LogicalName, var_famigliaid.Id, new ColumnSet("var_name"));

                var var_nameFamiglia = famigliaRecord.GetAttributeValue<string>("var_name");



                var checkFamiglia = conteggioFamiglie.TryGetValue(var_nameFamiglia, out var countFamiglia);

                conteggioFamiglie[var_nameFamiglia] = checkFamiglia ? countFamiglia + 1 : 1;

            }



            //-----



            var conteggioComponenti = new Dictionary<string, int>();



            var queryComponentiProdotti = new QueryExpression("var_prodotto_var_componente"); // var_prodotto_var_componente

            queryComponentiProdotti.ColumnSet = new ColumnSet(true); // "var_componenteid"



            var crmComponentiProdotti = CRM.RetrieveMultiple(queryComponentiProdotti).Entities;

            foreach (var relazioneRecord in crmComponentiProdotti)

            {

                var var_componenteid = relazioneRecord.GetAttributeValue<Guid>("var_componenteid");

                var componenteRecord = CRM.Retrieve("var_componente", var_componenteid, new ColumnSet("var_name"));

                var var_nameComponente = componenteRecord.GetAttributeValue<string>("var_name");



                var checkComponente = conteggioComponenti.TryGetValue(var_nameComponente, out var countComponente);

                conteggioComponenti[var_nameComponente] = checkComponente ? countComponente + 1 : 1;

            }



            //-----



            Console.WriteLine("FAMIGLIE");

            foreach (var countFamiglia in conteggioFamiglie) Console.WriteLine($"- [{countFamiglia.Key}]: {countFamiglia.Value}");



            Console.WriteLine("COMPONENTI");

            foreach (var countComponente in conteggioComponenti) Console.WriteLine($"- [{countComponente.Key}]: {countComponente.Value}");

        }



        private static void loadDataToCrm(CrmServiceClient CRM, List<ProdottoFinito> prodottiFiniti)

        {

            var famiglieProcessate = new List<string>();

            var componentiProcessati = new List<string>();



            var relazioneProdottoComponente = new Relationship("var_prodotto_var_componente_var_component");



            var indexProdotto = 0;

            foreach (var prodottoFinito in prodottiFiniti)

            {

                if (!famiglieProcessate.Contains(prodottoFinito.Famiglia))

                {

                    var famigliaRecord = new Entity("var_famiglia", "var_name", prodottoFinito.Famiglia);

                    famigliaRecord["var_name"] = prodottoFinito.Famiglia;

                    upsertRecord(CRM, famigliaRecord);



                    famiglieProcessate.Add(prodottoFinito.Famiglia);

                }



                var prodottoRecord = new Entity("var_prodotto", "var_name", prodottoFinito.Prodotto);

                prodottoRecord["var_name"] = prodottoFinito.Prodotto;

                //prodottoRecord["var_index"] = indexProdotto++;

                //prodottoRecord["var_date"] = DateTime.Now;



                prodottoRecord["var_famigliaid"] = new EntityReference("var_famiglia", "var_name", prodottoFinito.Famiglia);

                var prodottoReference = upsertRecord(CRM, prodottoRecord);



                var componentiReferenceCollection = new List<EntityReference>();

                foreach (var componente in prodottoFinito.Componenti)

                {

                    if (!componentiProcessati.Contains(componente))

                    {

                        var componenteRecord = new Entity("var_componente", "var_name", componente);

                        componenteRecord["var_name"] = prodottoFinito.Famiglia;

                        upsertRecord(CRM, componenteRecord);



                        componentiProcessati.Add(componente);

                    }



                    var componenteReference = new EntityReference("var_componente", "var_name", componente);

                    componentiReferenceCollection.Add(componenteReference);

                }



                CRM.Associate(prodottoReference.LogicalName, prodottoReference.Id, relazioneProdottoComponente, new EntityReferenceCollection(componentiReferenceCollection));

            }

        }

    }



    // --- Classes

    public class Matrice

    {

        public List<string> Famiglie;

        public List<string> Prodotti;

        public List<string> Componenti;



        public static string GenerateRandomProdottiFiniti()

        {

            var matricePath = Path.Combine(AppContext.BaseDirectory, @"FileJson\MatriceProdotti.json");

            var matriceContent = File.ReadAllText(matricePath);

            var matriceData = JsonConvert.DeserializeObject<Matrice>(matriceContent);



            //TODO: stampèare a video i dati input



            var randomGenerator = new Random();



            var prodottiFiniti = new List<ProdottoFinito>();

            foreach (var prodotto in matriceData.Prodotti)

            {

                var prodottoFinito = new ProdottoFinito();

                prodottoFinito.Prodotto = prodotto;

                prodottoFinito.Famiglia = matriceData.Famiglie[randomGenerator.Next(matriceData.Famiglie.Count)];



                var skip = randomGenerator.Next(matriceData.Componenti.Count);

                var take = randomGenerator.Next(1, matriceData.Componenti.Count + 1);

                prodottoFinito.Componenti = matriceData.Componenti.Skip(skip).Take(take).ToList();



                prodottiFiniti.Add(prodottoFinito);

            }



            var outputContent = JsonConvert.SerializeObject(prodottiFiniti);

            var baseDirectory = new DirectoryInfo(AppContext.BaseDirectory);

            baseDirectory.CreateSubdirectory("FileJsonOutput");

            var outputPath = Path.Combine(AppContext.BaseDirectory, $@"FileJsonOutput\ProdottiFiniti_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json");

            File.WriteAllText(outputPath, outputContent);



            return outputPath;

        }

    }



    public class ProdottoFinito

    {

        public string Prodotto;

        public string Famiglia;

        public List<string> Componenti;

    }

}
*/