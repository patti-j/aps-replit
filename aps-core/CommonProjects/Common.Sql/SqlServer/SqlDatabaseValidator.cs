namespace PT.Common.Sql.SqlServer;

///// <summary>
///// Updates a SQL Database to match the structure of a DataSet
///// </summary>
//internal class SqlDatabaseValidator
//{
//public void ValidateAndRepairDatabase(string a_dbConnectionString, string a_databaseName, DataSet a_ds)
//{
//    string masterConnString = a_dbConnectionString.Replace(a_databaseName, "MASTER");
//    using (SqlConnection conn = new SqlConnection(masterConnString))
//    {
//        //See if database exists
//        SqlCommand testDbCommand = new SqlCommand(String.Format("SELECT dbid FROM sysdatabases WHERE [name]='{0}'", a_databaseName), conn);
//        conn.Open();
//        int databaseId = (int)testDbCommand.ExecuteScalar();
//        if (databaseId <= 0) //Database doesn't exist so create it
//        {
//            SqlCommand sqlCommand = new SqlCommand("CREATE DATABASE " + a_databaseName, conn);
//        }

//        conn.Close();
//    }
//}

//As far as I am aware, there is no method in ADO.NET to check for the existense of a database. You could however use ADO.NET to query the Master databases sysdatabases table in SQL Server 2000 (not sure about this on SQL Server 2005, but I assume it has similar) to determine if a database exists. The code would be something like:

//Dim connection As New SqlConnection("Data Source=DAVE;Initial Catalog=Master;Integrated Security=SSPI")

//Dim command As New SqlCommand("SELECT dbid FROM sysdatabases WHERE [name]='northwind'", connection)

//Dim databaseID As Int32 = -1

//connection.Open()

//databaseID = CType(command.ExecuteScalar, Int32)

//connection.Dispose()

//command.Dispose()

//If databaseID > 0 Then

//    MessageBox.Show("Database exists")

//Else

//    MessageBox.Show("Database does not exist")

//End If

//        After adding controls, add the following variables in the beginning of the form class. 

//private string ConnectionString ="Integrated Security=SSPI;" +
//"Initial Catalog=;" +
//"Data Source=localhost;";
//private SqlDataReader reader = null; 
//private SqlConnection conn = null; 
//private SqlCommand cmd = null;
//private DevExpress.XtraEditors.SimpleButton AlterTableBtn;
//private string sql = null;
//private DevExpress.XtraEditors.SimpleButton CreateOthersBtn;
//private DevExpress.XtraEditors.SimpleButton button1; 

//First thing I'm going to do is create ExecuteSQLStmt method. This method executes a SQL statement against the SQL Sever database (mydb which I will create from my program) using Sql data providers using ExecuteNonQuery method. The ExecuteSQLStmt method is listed in Listing 1. 

//Listing 1. The ExecuteSQLStmt method.  

//private void ExecuteSQLStmt(string sql)
//{
//if( conn.State == ConnectionState.Open)
//conn.Close(); 
//ConnectionString ="Integrated Security=SSPI;" +
//"Initial Catalog=mydb;" +
//"Data Source=localhost;"; 
//conn.ConnectionString = ConnectionString;
//conn.Open(); 
//cmd = new SqlCommand(sql, conn);
//try
//{
//cmd.ExecuteNonQuery();
//}
//catch(SqlException ae)
//{
//MessageBox.Show(ae.Message.ToString());
//}
//}

//After this I'm going to create a new SQL Server database. The CREATE DATABASE SQL statement creates a database. The syntax of CREATE DATABASE depends on the database you create. Depending on the database type, you can also set values of database size, growth and file name. Listing 2 creates a SQL Server database mydb and data files are stored in the C:\\mysql directory. 

//Listing 2. Creating a SQL Server database.  

//// This method creates a new SQL Server database
//private void CreateDBBtn_Click(object sender, System.EventArgs e)
//{ 
//// Create a connection
//conn = new SqlConnection(ConnectionString); 
//// Open the connection
//if( conn.State != ConnectionState.Open)
//conn.Open(); 
//string sql = "CREATE DATABASE mydb ON PRIMARY"
//+"(Name=test_data, filename = 'C:\\mysql\\mydb_data.mdf', size=3,"
//+"maxsize=5, filegrowth=10%)log on"
//+"(name=mydbb_log, filename='C:\\mysql\\mydb_log.ldf',size=3,"
//+"maxsize=20,filegrowth=1)" ; 
//ExecuteSQLStmt(sql);
//}

//Now next step is to create a table. You use CREATE TABLE SQL statement to create a table. In this statement you define the table and schema (table columns and their data types). Listing 3 creates a table myTable with four column listed in Table 1. 

//Table 1. New table myTable schema. 

//Column Name Type Size Property 
//myId integer 4 Primary Key 
//myName char  50  Allow Null  
//myAddress char 255 Allow Null 
//myBalance float 8 Allow Null  

//Listing 4. Creating a database table. 

//private void CreateTableBtn_Click(object sender, System.EventArgs e)
//{ 
//// Open the connection
//if( conn.State == ConnectionState.Open)
//conn.Close(); 
//ConnectionString ="Integrated Security=SSPI;" +
//"Initial Catalog=mydb;" +
//"Data Source=localhost;"; 
//conn.ConnectionString = ConnectionString;
//conn.Open(); 
//sql = "CREATE TABLE myTable"+
//"(myId INTEGER CONSTRAINT PKeyMyId PRIMARY KEY,"+
//"myName CHAR(50), myAddress CHAR(255), myBalance FLOAT)" ; 
//cmd = new SqlCommand(sql, conn);
//try
//{
//cmd.ExecuteNonQuery(); 
//// Adding records the table
//sql = "INSERT INTO myTable(myId, myName, myAddress, myBalance) "+
//"VALUES (1001, 'Puneet Nehra', 'A 449 Sect 19, DELHI', 23.98 ) " ;
//cmd = new SqlCommand(sql, conn);
//cmd.ExecuteNonQuery(); 
//sql = "INSERT INTO myTable(myId, myName, myAddress, myBalance) "+
//"VALUES (1002, 'Anoop Singh', 'Lodi Road, DELHI', 353.64) " ;
//cmd = new SqlCommand(sql, conn);
//cmd.ExecuteNonQuery(); 
//sql = "INSERT INTO myTable(myId, myName, myAddress, myBalance) "+
//"VALUES (1003, 'Rakesh M', 'Nag Chowk, Jabalpur M.P.', 43.43) " ;
//cmd = new SqlCommand(sql, conn);
//cmd.ExecuteNonQuery(); 
//sql = "INSERT INTO myTable(myId, myName, myAddress, myBalance) "+
//"VALUES (1004, 'Madan Kesh', '4th Street, Lane 3, DELHI', 23.00) " ;
//cmd = new SqlCommand(sql, conn);
//cmd.ExecuteNonQuery(); 
//}
//catch(SqlException ae)
//{
//MessageBox.Show(ae.Message.ToString());
//}
//}

//As you can see from Listing 5, I also add data to the table using INSERT INTO SQL statement. 

//The CREATE PROCEDURE statement creates a stored procedure as you can see in Listing 10-18, I create a stored procedure myPoc which returs data result of SELECT myName and myAddress column. 

//Listing 5. Creating a stored procedure programmatically.  

//Private void CreateSPBtn_Click(object sender, System.EventArgs e)
//{
//sql = "CREATE PROCEDURE myProc AS"+
//" SELECT myName, myAddress FROM myTable GO";
//ExecuteSQLStmt(sql); 
//}

//Now I show you how to create views programmatically using CREATE VIEW SQL statement. As you can see from Listing 6, I create a view myView which is result of myName column rows from myTable. 

//Listing 6. Creating a view using CREATE VIEW 

//private void CreateViewBtn_Click(object sender, System.EventArgs e)
//{
//sql = "CREATE VIEW myView AS SELECT myName FROM myTable"; 
//ExecuteSQLStmt(sql); 
//}

//The ALTER TABLE is a useful SQL statement if you need to change your database schema programmatically. The ALTER TABLE statement can be used to add and remove new columns to a table, changing column properties, data types and constraints. The Listing 7 show that I change the database schema of myTable by first change column data type range from 50 to 100 characters and by adding a new column newCol of TIMESTAMP type.   

//Listing 7. Using ALTER TABLE to change a database schema programmatically.  

//Private void AlterTableBtn_Click(object sender, System.EventArgs e)
//{
//sql = "ALTER TABLE MyTable ALTER COLUMN"+
//"myName CHAR(100) NOT NULL"; 
//ExecuteSQLStmt(sql); 
//}

//The new table schema looks like Table 2.  

//Table 2. MyTable after ALTER TABLE 

//Column Name Type                 Size                           Property  
//myId                      integer 4 Primary Key  
//myName char  50 Allow Null 
//myAddress char 255 Allow Null 
//myBalance  float 8 Allow Null 
//newCol timestamp  8 Allow Null 

//You can also create other database object such as index, rule, and users. The code listed in Listing 8 creates one rule and index on myTable. 

//Note: Create Index can only create an index if you don't have an index on a table. Otherwise you will get an error message.  

//Listing 8. Creating rules and indexes using SQL statement. 

//private void CreateOthersBtn_Click(object sender, System.EventArgs e)
//{
//sql = "CREATE UNIQUE CLUSTERED INDEX "+
//"myIdx ON myTable(myName)"; 
//ExecuteSQLStmt(sql); 
//sql = "CREATE RULE myRule "+
//"AS @myBalance >= 32 AND @myBalance < 60";
//ExecuteSQLStmt(sql); 
//}

//The DROP TABLE command can be used to delete a table and its data permanently. The code listed in Listing 9 deletes myTable.  

//Listing 9. Deleting table using DROP TABLE. 

//Private void DropTableBtn_Click(object sender, System.EventArgs e)
//{
//string sql = "DROP TABLE MyTable "; 
//ExecuteSQLStmt(sql); 
//}

//Now next step is to view data from the table, view and stored procedure. The ViewDataBtn_Click method listed in Listing 10 shows the entire data from the table. The ViewSPBtn_Click and ViewViewBtn_Click methods view stored procedure and view data we have created earlier. As you can see using views and stored procedures work same as you use a SQL Statement. We have discussed working with Views and stored procedures in the beginning of this chapter. As you can see from Listing 10, 11, and 12, I view data from stored procedure and view.   

//Listing 10. Viewing data from a database table.   

//private void ViewDataBtn_Click(object sender, System.EventArgs e)
//{
///// Open the connection
//if( conn.State == ConnectionState.Open)
//conn.Close();
//ConnectionString ="Integrated Security=SSPI;" +
//"Initial Catalog=mydb;" +
//"Data Source=localhost;"; 
//conn.ConnectionString = ConnectionString;
//conn.Open(); 
//// Create a data adapter
//SqlDataAdapter da = new SqlDataAdapter
//("SELECT * FROM myTable", conn); 
//// Create DataSet, fill it and view in data grid
//DataSet ds = new DataSet("myTable");
//da.Fill(ds, "myTable");
//dataGrid1.DataSource = ds.Tables["myTable"].DefaultView;
//} 

//Listing 11.Using a stored procedure to view data from a table.   

//private void ViewSPBtn_Click(object sender, System.EventArgs e)
//{ 
///// Open the connection
//if( conn.State == ConnectionState.Open)
//conn.Close(); 
//ConnectionString ="Integrated Security=SSPI;" +
//"Initial Catalog=mydb;" +"Data Source=localhost;"; 
//conn.ConnectionString = ConnectionString;
//conn.Open(); 
//// Create a data adapter
//SqlDataAdapter da = new SqlDataAdapter("myProc", conn); 
//// Create DataSet, fill it and view in data grid
//DataSet ds = new DataSet("SP");
//da.Fill(ds, "SP");
//dataGrid1.DataSource = ds.DefaultViewManager; 
//}

//Listing 12.Using a view to view data from a table.   

//private void ViewViewBtn_Click(object sender, System.EventArgs e)
//{ 
///// Open the connection
//if( conn.State == ConnectionState.Open)
//conn.Close(); 
//ConnectionString ="Integrated Security=SSPI;" +
//"Initial Catalog=mydb;" +
//"Data Source=localhost;"; 
//conn.ConnectionString = ConnectionString;
//conn.Open(); 
//// Create a data adapter
//SqlDataAdapter da = new SqlDataAdapter
//("SELECT * FROM myView", conn); 
//// Create DataSet, fill it and view in data grid
//DataSet ds = new DataSet();
//da.Fill(ds);
//dataGrid1.DataSource = ds.DefaultViewManager; 
//}

//Finally, I create AppExit method which releases the connection and reader objects and I call them from the Dispose method as you can see in Listing 13.  

//Listing 13. AppExit method  

//protected override void Dispose( bool disposing )
//{
//AppExit();
//if( disposing )
//{
//if (components != null) 
//{
//components.Dispose();
//}
//}
//base.Dispose( disposing );
//}

//// Called when you are done with the applicaton
//// Or from Close button
//private void AppExit()
//{
//if (reader != null)
//reader.Close();
//if (conn.State == ConnectionState.Open)
//conn.Close();
//} 
//}