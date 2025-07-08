using Gremlin.Net.Driver;
using Gremlin.Net.Process.Traversal;
using Gremlin.Net.Structure;
using static Gremlin.Net.Process.Traversal.AnonymousTraversalSource;
using Mathema.Bytecode4CosmosDB;

namespace Mathema.DiningByFriendsCosmosDB
{
    public class Program
    {
        private const string partitionKey = "partitionKey";

        private const string PERSON = "person";
        private const string FIRST_NAME = "first_name";
        private const string HOBBY = "hobby";
        private const string FRIENDS = "friends";

        static void Main(string[] args)
        {
            /*to be changed to your CosmosDB Gremlin endpoint and auth key*/
             string cosmosHostname = "YOUR HOST NAME";
            int cosmosPort = 443;
            string cosmosAuthKey = "YOUR AUTH KEY";
            string cosmosDatabase = "YOUR DATABASE";
            string cosmosCollection = "YOUR COLLECTION";


            try
            {
                var connectionPoolSettings = new ConnectionPoolSettings
                {
                    MaxInProcessPerConnection = 32,
                    PoolSize = 4,
                    ReconnectionAttempts = 3,
                    ReconnectionBaseDelay = TimeSpan.FromSeconds(1),
                };
                CosmosDbGremlinClientBuilder builder =CosmosDbGremlinClientBuilder.BuildClientForServer(cosmosHostname, cosmosPort, cosmosDatabase, cosmosAuthKey, cosmosCollection);
                using (var gremlinClient = builder.WithConnectionPoolSettings(connectionPoolSettings).Create())
                {

                    Console.WriteLine($"Person-Vertices: {GetVertexCountScriptBasedAsync(gremlinClient).WaitAsync(new TimeSpan(0, 0, 30)).Result}");

                    RunApplication(gremlinClient);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Error: {ex.StackTrace}");
            }
        }

        public static int GetUserChoice()
        {

            Console.WriteLine();
            Console.WriteLine("Menu:");
            Console.WriteLine("--------------");
            Console.WriteLine("1) Add person");
            Console.WriteLine("2) Read person");
            Console.WriteLine("3) Update person");
            Console.WriteLine("4) Delete person");
            Console.WriteLine("5) Get person count");
            Console.WriteLine("6) Add friendship");
            Console.WriteLine("7) Show friendship");
            Console.WriteLine("8) Show friendship of second degree");
            Console.WriteLine("9) Show path");
            Console.WriteLine("0) Exit");
            Console.WriteLine("--------------");
            Console.Write("Your choice: ");

            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            while (!char.IsDigit(keyInfo.KeyChar))
            {
                keyInfo = Console.ReadKey(true);
            }
            Console.WriteLine(keyInfo.KeyChar);

            return int.Parse(keyInfo.KeyChar.ToString());
        }

        public static void RunApplication(IGremlinClient gremlinClient)
        {
            var g = GetTraversal(gremlinClient);
           
            int option = -1;
            while (option != 0)
            {
                option = GetUserChoice();
                switch (option)
                {
                    case 0:
                        break;
                    case 1:
                        //Add Person
                        Console.WriteLine("New person: " + AddPerson(g));
                        
                        break;
                    case 2:
                        //Read Person
                        Console.WriteLine("Read person: " + GetPerson(g));
                        break;
                    case 3:
                        //Update Person
                        Console.WriteLine("Update person: \n" + UpdatePerson(g));
                        break;
                    case 4:
                        //Delete person
                        Console.WriteLine("Delete person: \n" + DeletePersonWithDeleteCount(g));
                        break;
                    case 5:
                        //person count
                        Console.WriteLine("Person count: \n" + GetVertexCountScriptBasedAsync(gremlinClient).WaitAsync(new TimeSpan(0, 0, 30)).Result);
                        break;
                    case 6:
                        Console.WriteLine("Add friendship: \n" + AddFriendsEdge(g));
                        break;
                    case 7:
                        Console.WriteLine("Show friendship: \n" + GetFriends(g));
                        break;
                    case 8:
                        Console.WriteLine("Show friendship (2nd degree)" + GetFriendsOfFriends(g));
                        break;
                    case 9:
                        Console.WriteLine("Show path" + FindPathBetweenPeople(g));
                        break;
                    default:
                        Console.WriteLine("wrong choice");
                        break;
                }
            }

            Console.WriteLine("Exiting DiningByFriends, Bye!");
        }


        public static string AddPerson(GraphTraversalSource g)
        {
            Console.WriteLine("--- Add Person ---");

            string name = GetInput("First name of new person: ");
            string hobby = GetInput("Hobby of new person: ");

            var newVertex = g.AddV(PERSON).Property(partitionKey, "").Property(FIRST_NAME, name).Property(HOBBY, hobby).Next();

            return newVertex.ToString();
        }


        public static async Task<long> GetVertexCountScriptBasedAsync(IGremlinClient gremlinClient)
        {
            var count = await gremlinClient.SubmitWithSingleResultAsync<long>($"g.V().count()");

            return count;
            
        }    

        public static string GetPerson(GraphTraversalSource g)
        {
            string name = GetInput("First name of person: ");

            var traverser = g.V().
                    Has(PERSON, FIRST_NAME, name).ValueMap<string, object>();
            

            IList<IDictionary<string, object>> properties = traverser.ToList();
            
            string result = "";
            foreach (var p in properties)
            {
                foreach (var item in p)
                {
                    result = result + item.Key + " = "+ string.Join(",", item.Value as IEnumerable<object>) + "\n";
                }
            }
            
            return result;
        }

        private static string UpdatePerson(GraphTraversalSource g)
        {
            Console.WriteLine("--- Update person ---");

            string nameOld = GetInput("First name of person: ");
            string nameNew = GetInput("(New) first name of person: ");
            string hobby = GetInput("(New) hobby of person: ");

            Vertex vertex = g.V().Has(PERSON, FIRST_NAME, nameOld)
                                 .Property(FIRST_NAME, nameNew)
                                 .Property(HOBBY, hobby).Next();

            return vertex.ToString();
        }


        private static long DeletePersonWithDeleteCount(GraphTraversalSource g)
        {
            Console.WriteLine("--- Delete Person ---");

            string name = GetInput("First name of person: ");

            long vertexCount = g.V().Has(PERSON, FIRST_NAME, name)
                                    .SideEffect(__.Drop())
                                    .Count().Next();

            return vertexCount;
        }



        public static String AddFriendsEdge(GraphTraversalSource g)
        {
            string fromName = GetInput("First name of person (out vertex): ");
            string toName = GetInput("First name of person (in vertex): ");


            Edge newEdge = 
            g.V().Has(PERSON, FIRST_NAME, fromName)
                    .AddE(FRIENDS).To(__.V().Has(PERSON, FIRST_NAME, toName))
                    .Next();

            return newEdge.ToString();
        }

        public static string GetFriends(GraphTraversalSource g)
        {
            string name = GetInput("First name of person: ");


            IList<object> friends = g.V().Has(PERSON, FIRST_NAME, name)
                                     .Both(FRIENDS).Dedup()
                                     .Values<object>(FIRST_NAME)
                                     .ToList();

            return String.Join(", \n", friends);
        }


        public static string GetFriendsOfFriends(GraphTraversalSource g)
        {
            string name = GetInput("First name of person: ");


            IList<string> foff = g.V().Has(PERSON, FIRST_NAME, name)
                                  .Repeat(__.Out(FRIENDS)).Times(2).Dedup()
                                  .Values<string>(FIRST_NAME).ToList();

            return String.Join(", \n", foff);
        }

        public static String FindPathBetweenPeople(GraphTraversalSource g)
        {
            string fromName = GetInput("First name of person (first person): ");
            string toName = GetInput("First name of person: (second person)");

            IList<Gremlin.Net.Structure.Path> friends = g.V().Has(PERSON, FIRST_NAME, fromName)
                                                         .Until(__.Has(PERSON, FIRST_NAME, toName))
                                                         .Repeat(
                                                                 __.Both(FRIENDS).SimplePath()
                                                          ).Path()
                                                         .ToList();

            return String.Join(", \n", friends);
        }


        /// <summary>
        /// Get non-empty input from keyboard
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static string GetInput(string message)
        {
            Console.Write(message);
            string input = Console.ReadLine();
            while (input == null)
            {
                Console.Write(message);
                input = Console.ReadLine();
            }

            return input;
        }

        private static GraphTraversalSource GetTraversal(IGremlinClient gremlinClient)
        {
            GraphTraversalSource g = Traversal().WithRemote(new
                    CosmosDbRemoteConnection(gremlinClient));

            return g;
        }
    }
}
