using Cassandra;
using Cassandra.Mapping;
using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace CassandraQuickStartSample
{
    public class Program
    {
        // Get UserName / Password from environment
        static string UserName = System.Environment.GetEnvironmentVariable("cname"); 
        static string Password = System.Environment.GetEnvironmentVariable("cpass");

        public static void Main(string[] args)
        {
            // validate UserName
            if (string.IsNullOrEmpty(UserName))
            {
                Console.WriteLine("Invalid User Name\n\nExport cname environment value");
                return;
            }

            // validate password
            if (string.IsNullOrEmpty(Password))
            {
                Console.WriteLine("Invalid Password\n\nExport cpass environment value");
                return;
            }

            string CassandraContactPoint = UserName + ".cassandra.cosmosdb.azure.com";  
            int CassandraPort = 10350;

            // Connect to cassandra cluster  (Cassandra API on Azure Cosmos DB supports only TLSv1.2)
            var options = new Cassandra.SSLOptions(SslProtocols.Tls12, true, ValidateServerCertificate);
            options.SetHostNameResolver((ipAddress) => CassandraContactPoint);
            Cluster cluster = Cluster.Builder().WithCredentials(UserName, Password).WithPort(CassandraPort).AddContactPoint(CassandraContactPoint).WithSSL(options).Build();
            ISession session = cluster.Connect("myapp");
            Console.WriteLine("connected to myapp");

            // create a new table each time the app runs
            session.Execute("DROP TABLE IF EXISTS user;");

            // Create table
            // Set CosmosDB RUs to 400 (lowest possible)
            session.Execute("CREATE TABLE IF NOT EXISTS user (userid int, name text, PRIMARY KEY (userid)) WITH cosmosdb_provisioned_throughput=400");

            const string sql = "insert into user (userid, name) values ({0}, '{1}')";
            int id = 0;

            // insert via execute
            session.Execute(string.Format(sql, ++id, "Bart"));
            session.Execute(string.Format(sql, ++id, "Carla"));
            session.Execute(string.Format(sql, ++id, "Joshua"));
            session.Execute(string.Format(sql, ++id, "Sasha"));

            IMapper mapper = new Mapper(session);

            // insert via mapper (just another way to insert)
            mapper.Insert<User>(new User(++id, "Matthew"));

            // query via mapper
            Console.WriteLine("Select ALL");
            Console.WriteLine("-------------------------------");
            foreach (User user in mapper.Fetch<User>("Select * from user"))
            {
                Console.WriteLine(user);
            }

            Console.WriteLine("\n\nSelect one user");
            Console.WriteLine("-------------------------------");

            // query via RowSet
            var rs = session.Execute("select userid, name from user where userid = 1");
            foreach (var r in rs)
            {
                int userid = r.GetValue<int>(0);
                string name = r.GetValue<string>(1);
                Console.WriteLine(string.Format(" {0} | {1} ", userid, name));
            }
        }

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // callback that verifies certificate is setup correctly
            if (sslPolicyErrors == SslPolicyErrors.None) {
                return true;
            }

             Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Fail
            return false;
        }
    }


    public class User
    {
        // used by Mapper to query / insert
        public int userid { get; set; }
        public string name { get; set; }

        public User(int userid, string name)
        {
            this.userid = userid;
            this.name = name;
        }

        public override string ToString()
        {
            return string.Format(" {0} | {1} ", userid, name);
        }
    }
}

