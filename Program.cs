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
        // Change UserName / Password
        static string UserName = System.Environment.GetEnvironmentVariable("cosmos_name", EnvironmentVariableTarget.User); 
        static string Password = System.Environment.GetEnvironmentVariable("cosmos_password", EnvironmentVariableTarget.User);

        public static void Main(string[] args)
        {
            if (string.IsNullOrEmpty(UserName))
            {
                Console.WriteLine("Invalid User Name\n\nSet cosmos_name environment value");
                return;
            }

            if (string.IsNullOrEmpty(Password))
            {
                Console.WriteLine("Invalid Password\n\nSet cosmos_password environment value");
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


            session.Execute("DROP TABLE IF EXISTS user;");
            // session.Execute("DROP KEYSPACE IF EXISTS myapp;");

            // Create table
            // session.Execute("CREATE KEYSPACE IF NOT EXISTS myapp WITH REPLICATION = { 'class' : 'SimpleStrategy', 'replication_factor' : 3 } ;");
            session.Execute("CREATE TABLE IF NOT EXISTS user (userid int, name text, PRIMARY KEY (userid)) WITH cosmosdb_provisioned_throughput=400");

            const string sql = "insert into user (userid, name) values ({0}, '{1}')";

            session.Execute(string.Format(sql, 1, "Bart"));

            int max = 0;
            int i = 0;
            var rs = session.Execute("select userid from user");
            foreach (var r in rs)
            {
                i = r.GetValue<int>(0);
                max = i > max ? i:max;
            }
            Console.WriteLine(max);

            // insert via execute
            session.Execute(string.Format(sql, ++max, "LyubovK"));
            session.Execute(string.Format(sql, ++max, "JiriK"));
            session.Execute(string.Format(sql, ++max, "IvanH"));

            IMapper mapper = new Mapper(session);

            // insert via mapper
            mapper.Insert<User>(new User(++max, "LiliyaB"));
            mapper.Insert<User>(new User(++max, "JindrichH"));

            Console.WriteLine("Select ALL");
            Console.WriteLine("-------------------------------");
            foreach (User user in mapper.Fetch<User>("Select * from user"))
            {
                Console.WriteLine(user);
            }

            i = 1;
            Console.WriteLine("\nGetting by id {0}", i);
            Console.WriteLine("-------------------------------");
            User userId3 = mapper.FirstOrDefault<User>("Select * from user where userid = ?", i);
            Console.WriteLine(userId3);

        }

        public static bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
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
