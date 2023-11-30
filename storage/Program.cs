using Microsoft.Data.Sqlite;
using System.Data;
using System.Drawing;
using System.IO.Pipes;
using System.Text;

internal class Program
{
    private static string ConnectionString = "Data Source=storage.sqlite;";

    private static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        using var connection = new SqliteConnection(ConnectionString);

        connection.Open();

        string[] CreateTablesCommands = {
            "CREATE TABLE types (id INTEGER PRIMARY KEY , name VARCHAR(255));",
            "CREATE TABLE suppliers (id INTEGER PRIMARY KEY , name VARCHAR(255));",
            "CREATE TABLE goods (id INTEGER PRIMARY KEY,name VARCHAR(255),type_id INTEGER, supplier_id INTEGER, quantity INTEGER, cost INTEGER, delivery_date DATETIME, FOREIGN KEY (type_id) REFERENCES types(id), FOREIGN KEY (supplier_id) REFERENCES suppliers(id));"
        };

        string[] types_items = { "Електроніка", "Одяг", "Їжа, напої", "Книги", "Побутові", "Спортивні", "Канцтовари", "Косметика", "Подарунки" };

        string[] suppliers_items = {"Зелений Луг", "БудТорг", "ЕкоСвіт", "Інтелект" , "Екофуд" , "Віташ" };

        CreateTables(connection, CreateTablesCommands);

        CreateItemForTypesAndSuppliers(connection, types_items, "types");

        CreateItemForTypesAndSuppliers(connection, suppliers_items, "suppliers");
        CreateItemForGoods(connection);

        string[] comands = {
            "SELECT g.name AS goods_name, s.name AS supplier_name, t.name AS type_name,quantity AS q, cost AS c, delivery_date AS d   FROM goods g JOIN suppliers s ON g.supplier_id = s.id JOIN types t ON g.type_id = t.id",
            "SELECT name FROM types",
            "SELECT name FROM suppliers",
            "SELECT name, MAX(quantity) AS m  FROM goods",
            "SELECT name, MIN(quantity) AS m  FROM goods",
            "SELECT name, MAX(cost) AS m  FROM goods",
            "SELECT name, MIN(cost) AS m  FROM goods",
            "SELECT g.name AS goods_name,t.name AS type_name FROM goods g JOIN types t ON g.type_id = t.id WHERE t.id=0",
            "SELECT g.name AS goods_name,s.name AS supplier_name FROM goods g JOIN suppliers s ON g.supplier_id = s.id WHERE s.id=0",
            "SELECT name AS n, MIN(delivery_date) AS m  FROM goods g",
            "SELECT t.name AS type_name,AVG(quantity) AS q  FROM goods g JOIN types t ON g.type_id = t.id GROUP BY t.id",
        };


        string[] texts = {
            "Вся інформація про оцінки студентів",
            "Всі типи товарів",
            "Всі постачальники",
            "Товар з максимальною кількістю",
            "Товар з мінімальною кількістю",
            "Товар з максимальною собівартістю",
            "Товар з мінімальною собівартістю",
            "Товари категорії \"Електроніка\"",
            "Товари постачальника \"Зелений Луг\"",
            "Товар, який знаходиться на складі найдовше з усіх",
            "Середня кількість товарів"
        };

        ReadAndDisplayAll(connection, comands, texts);
    }

    private static void CreateTables(SqliteConnection connection, string[] comands)
    {
        for(int i = 0; i < comands.Length; i++) 
        {
            string sql = comands[i];
            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
    }
    private static void CreateItemForTypesAndSuppliers(SqliteConnection connection, string[] items, string TableName)
    {
        using var transaction = connection.BeginTransaction();

        for (int i = 0; i < items.Length; i++)
        {
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = $"INSERT INTO {TableName} (id, name) VALUES ($id, $name)";

            string name = items[i];

            insertCommand.Parameters.AddWithValue("$id", i);
            insertCommand.Parameters.AddWithValue("$name", name);

            insertCommand.ExecuteNonQuery();
        }
        transaction.Commit();
    }


    private static void CreateItemForGoods(SqliteConnection connection)
    {

        using var transaction = connection.BeginTransaction();

        string[,] items = {
    {"Олівець", "6","1","1000","3","2023-11-29 14:30:00"},
    {"Шампунь", "7","3","250","50","2023-04-03 17:45:00"},
    {"Футболка", "1","4","50","250","2023-01-27 10:15:00"},
    {"Магніт", "8","2","100","30","2023-12-30 18:25:00"},
    {"Абетка", "3", "5", "150", "100", "2023-01-18 08:00:00"},
    {"Келихи", "4", "2", "50", "400", "2023-05-07 16:20:00"},
    {"Кавоварка","0", "0", "20", "5000", "2023-09-19 11:20:00"},
    {"Планшет", "0", "3", "10", "8000", "2023-12-05 18:25:00"},
    {"Гантелі", "5", "1", "75", "300", "2023-10-11 11:20:00"},
    {"Снігова куля", "8", "0", "200", "230", "2023-07-10 16:20:00"},
    {"Фанта", "2", "5", "500", "25", "2023-02-08  12:35:00"},
    {"Макарони", "2", "3", "34", "30", "2023-11-27  12:35:00"},
    {"Зошит", "6", "0", "100", "3", "2023-04-22 18:25:00"}
        };
        for (int i = 0; i < items.GetLength(0); i++)
        {
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = "INSERT INTO goods (id, name, type_id, supplier_id, quantity, cost, delivery_date) " +
                                        "VALUES ($id, $name, $type_id, $supplier_id, $quantity, $cost, $delivery_date)";


            string name = items[i, 0];
            int type_id = int.Parse(items[i, 1]);
            int supplier_id = int.Parse(items[i, 2]);
            int quantity = int.Parse(items[i, 3]);
            int cost = int.Parse(items[i, 4]);
            DateTime delivery_date = DateTime.Parse(items[i, 5]);

            insertCommand.Parameters.AddWithValue("$id", i);
            insertCommand.Parameters.AddWithValue("$name", name);
            insertCommand.Parameters.AddWithValue("$type_id", type_id);
            insertCommand.Parameters.AddWithValue("$supplier_id", supplier_id);
            insertCommand.Parameters.AddWithValue("$quantity", quantity);
            insertCommand.Parameters.AddWithValue("$cost", cost);
            insertCommand.Parameters.AddWithValue("$delivery_date", delivery_date);

            insertCommand.ExecuteNonQuery();
        }

        transaction.Commit();
    }


    private static void ReadAndDisplayAll(SqliteConnection connection, string[] commands, string[] displayMessages)
    {
        if (commands.Length != displayMessages.Length)
        {
            Console.WriteLine("Кількість команд та повідомлень для відображення не збігається.");
            return;
        }

        using var command = connection.CreateCommand();

        for (int batchIndex = 0; batchIndex < commands.Length; batchIndex++)
        {
            command.CommandText = commands[batchIndex];

            using var reader = command.ExecuteReader();
            Console.WriteLine();
            Console.WriteLine(displayMessages[batchIndex]);
            Console.WriteLine();

            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    Console.Write($"{reader.GetString(i),-15}\t");
                }
                Console.WriteLine();
            }
        }
    }


}