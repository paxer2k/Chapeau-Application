﻿using ChapeauModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChapeauDAL
{
   public class OrderDAO : BaseDao
    {

        public List<Order> GetAllOrders(Place place)
        {
            //filling for the listview
            string query = "select m.menu_type, m.item_name,m.item_id,o.orderID,o.quantity,o.orderTime,o.itemStatus,o.comment,r.tableID from [ORDER_ITEM] as o " +
                " JOIN [MENU_ITEM] as m ON o.item_id = m.item_id " +
                " JOIN [ORDER] as r ON o.orderID = r.orderID " +
                " where placeID = @placeID AND itemStatus = 1 OR itemStatus = 2";
            SqlParameter[] sqlParameters = new SqlParameter[1];
            sqlParameters[0] = new SqlParameter("@placeID", place);
            return ReadKitchenBar(ExecuteSelectQuery(query, sqlParameters));
        }

        public Order GetOrderByID(int orderID, Place place)
        {
            //to get only the orders with the orderid
            string query = "select m.menu_type, m.item_name,m.item_id,o.orderID,o.quantity,o.orderTime,o.itemStatus,o.comment,r.tableID from [ORDER_ITEM] as o " +
                " JOIN [MENU_ITEM] as m ON o.item_id = m.item_id " +
                " JOIN [ORDER] as r ON o.orderID = r.orderID " +
                " where placeID = @placeID AND o.orderID = @orderID";
            SqlParameter[] sqlParameters = new SqlParameter[2];
            sqlParameters[0] = new SqlParameter("@placeID", place);
            sqlParameters[1] = new SqlParameter("@orderID", orderID);

            List<Order> orders = ReadKitchenBar(ExecuteSelectQuery(query, sqlParameters));
            return orders[0];
        }

        public void UpdateOrderItemStatus(OrderItem orderItem)
        {
            //update itemstatus in database depends on what has been given
            string query = "Update [ORDER_ITEM] Set itemStatus = @itemStatus" +
                " Where orderID = @orderID AND  item_id = @item_id";
            SqlParameter[] sqlParameters = new SqlParameter[3];
            sqlParameters[0] = new SqlParameter("@orderID", orderItem.OrderID);
            sqlParameters[1] = new SqlParameter("@itemStatus", orderItem.Status);
            sqlParameters[2] = new SqlParameter("@item_id", orderItem.menuItem.item_id);
            ExecuteEditQuery(query, sqlParameters);
        }

        public void UpdateOrderStatus(Order order)
        {
            //update orderstatus in database depends on what has been givens
            string query = "Update [ORDER] Set orderStatus = @orderStatus where orderID = @orderID";
            SqlParameter[] sqlParameters = new SqlParameter[2];
            sqlParameters[0] = new SqlParameter("@orderStatus", order.Status);
            sqlParameters[1] = new SqlParameter("@orderID", order.OrderID);
            ExecuteEditQuery(query, sqlParameters);
        }


        private List<Order> ReadKitchenBar(DataTable dataTable)
        {
            List<Order> Orders = new List<Order>();
            //reading from datbase the data
            foreach (DataRow dr in dataTable.Rows)
            {
                OrderItem orderItem = new OrderItem();
                orderItem.menuItem.menu_type = (MenuCategory)dr["menu_type"];
                orderItem.menuItem.item_name = (string)dr["item_name"];
                orderItem.menuItem.item_id = (int)dr["item_id"];
                orderItem.OrderID = (int)dr["orderID"];
                orderItem.Comment = (string)dr["comment"];
                orderItem.Quantity = (int)dr["quantity"];
                orderItem.Status = (OrderItemStatus)dr["itemStatus"];
                orderItem.OrderTime = (DateTime)dr["orderTime"];

                Order order = new Order();
                order.Table.TableID = (int)dr["tableID"];
                order.OrderItems.Add(orderItem);
                Orders.Add(order);
            }
            return Orders;
        }


        #region Alex's part

        // Alex's part


        // used joins so that i am able to read from both menuitem and orderitem (they are connected)
        private List<OrderItem> ReadOrderItems(DataTable dataTable)
        {
            List<OrderItem> orderItems = new List<OrderItem>();

            foreach (DataRow dr in dataTable.Rows)
            {
                // read from menu item
                // note to self: next time fill out all the fields in order to avoid errors with retrieving data
                MenuItem menu = new MenuItem()
                {
                    item_id = (int)dr["item_id"],
                    item_name = (string)dr["item_name"],
                    item_price = (double)dr["item_price"],
                    menu_type = (MenuCategory)dr["menu_type"],
                    item_type = (MenuSubCategory)dr["item_type"],
                };

                // read from order item
                OrderItem orderItem = new OrderItem();
                {
                    // store menu item in the order item object (menuItem)
                    orderItem.menuItem = menu;
                    orderItem.Quantity = (int)dr["quantity"];
                    if (dr["comment"] != System.DBNull.Value) orderItem.Comment = (string)dr["comment"]; // comment can be left null in database
                    orderItem.Status = (OrderItemStatus)dr["itemStatus"];
                };

                orderItems.Add(orderItem);
            }
            return orderItems;
        }

        // gets the right orderitem that is related to the orderID
        // this function read the right orderItems for the list in the listview
        public List<OrderItem> GetOrderItemsForOrderID(int orderID)
        {
            List<OrderItem> orderItems = new List<OrderItem>();

            string query =
                 "SELECT I.item_id, I.quantity, I.orderTime, I.itemStatus, I.comment, M.item_name, M.item_price, M.menu_type, M.item_type "
                + "FROM ORDER_ITEM AS I JOIN MENU_ITEM AS M on M.item_id = I.item_id WHERE I.orderID = @id";
            SqlParameter[] sqlParameters = new SqlParameter[1];
            sqlParameters[0] = new SqlParameter("@id", orderID);

            return ReadOrderItems(ExecuteSelectQuery(query, sqlParameters));
        }

        // this is for the rounding up for an order and paying with a method
        // this query inserts into the PAYMENT table after the paymnet has been completed
        // this information can be used for storage
        public bool SaveOrderPaymentInfo(Order order)
        {
            try
            {
                string query = "INSERT INTO [PAYMENT](orderID, paymentmethodID, paymentTotal, vatTotal, tip, employeeID, feedback, paymentDate) " +
                "VALUES(@id, @method, @payment, @vat, @tip, @empID, @feedback, @paymentDate)";
                SqlParameter[] sqlParameters = new SqlParameter[8];
                sqlParameters[0] = new SqlParameter("@id", order.OrderID);
                sqlParameters[1] = new SqlParameter("@method", order.paymentMethod);
                sqlParameters[2] = new SqlParameter("@payment", order.Total);
                sqlParameters[3] = new SqlParameter("@vat", order.VATTotal);
                sqlParameters[4] = new SqlParameter("@tip", order.Tip);
                sqlParameters[5] = new SqlParameter("@empID", order.Employee.EmployeeID);
                sqlParameters[6] = new SqlParameter("@feedback", order.Feedback);
                sqlParameters[7] = new SqlParameter("@paymentDate", DateTime.Now);


                ExecuteEditQuery(query, sqlParameters);

                return true;
            }
            catch (Exception exp)
            {
                return false;
            }
        }

        // gets the right order corresponding to the tableID
        public Order GetOrderForTableByTableID(int tableID)
        {
            string query =
                "SELECT O.orderID, O.paymentDate, O.totalPrice, O.tableID, O.employeeID, O.orderStatus, O.tip, O.vat, O.isPaid, " +
                "T.capacity, T.statusID, " +
                "E.employeeID, E.firstName, E.lastName, E.roleID, E.PIN " +
                "FROM [ORDER] AS O " +
                "JOIN [TABLE] AS T ON T.table_id = O.tableID " +
                "JOIN [EMPLOYEE] AS E ON E.employeeID = O.employeeID " +
                "WHERE O.tableID = @id AND O.isPaid = 0";
            SqlParameter[] sqlParameters = new SqlParameter[1];
            sqlParameters[0] = new SqlParameter("@id", tableID);

            return ReadFromOrder(ExecuteSelectQuery(query, sqlParameters));
        }

        // information that reads from the orders (used for GetOrderForTableByTableID method)
        private Order ReadFromOrder(DataTable dataTable)
        {
            Order order = null;
            foreach (DataRow dr in dataTable.Rows)
            {
                // used DBNull because otherwise it would give me an error regarding having nulls (specifically for VAT and TIP since they have to be empty)
                // due to the fact that they have to be filled out within the form and then updated to the database
                order = new Order();
                order.OrderID = (int)dr["orderID"];
                if (dr["isPaid"] != System.DBNull.Value) order.isPaid = (bool)dr["isPaid"];
                if (dr["orderStatus"] != System.DBNull.Value) order.Status = (OrderStatus)dr["orderStatus"];
                if (dr["totalPrice"] != System.DBNull.Value) order.Total = (double)dr["totalPrice"];
                if (dr["paymentDate"] != System.DBNull.Value) order.PaymentDate = (DateTime)dr["paymentDate"];
                if (dr["tip"] != System.DBNull.Value) order.Tip = (double)dr["tip"];
                if (dr["vat"] != System.DBNull.Value) order.VATTotal = (double)dr["vat"];


                // order.Table = GetTableByID(tmp_tableID);
                order.Table = new Table()
                {
                    TableID = (int)dr["tableID"]
                    // the rest of fields are UNINITIALIZED! But it is OK because we do not need the rest in this module.
                };

                // order.Employee = GetEmployeeByID(tmp_employeeID);
                order.Employee = new Employee();
                order.Employee.EmployeeID = (int)dr["employeeID"];
                order.Employee.FirstName = (string)dr["firstName"];
                order.Employee.LastName = (string)dr["lastName"];
                order.Employee.Role = (Role)dr["roleID"];
                order.Employee.LoginCode = (int)dr["PIN"];

                order.OrderItems = GetOrderItemsForOrderID(order.OrderID);
                order.paymentMethod = PaymentMethod.Cash; // PaymentMethod { get; set; } assign a default value bc paymentMethod doesnt exist in order table, but in payment table
            }
            return order;
        }

        // when the order has been paid for, the payment status changes to true in the database
        public void UpdateOrderDetails(Order order, bool newOrderIsPaidStatus)
        {
            string query = "UPDATE [ORDER] SET isPaid = @paid, VAT = @vat, tip = @tip, totalPrice = @totalPrice, paymentDate = @paymentDate  WHERE orderID = @ID";
            SqlParameter[] sqlParameters = new SqlParameter[6];
            sqlParameters[0] = new SqlParameter("@paid", newOrderIsPaidStatus);
            sqlParameters[1] = new SqlParameter("@ID", order.OrderID);
            sqlParameters[2] = new SqlParameter("@vat", order.VATTotal);
            sqlParameters[3] = new SqlParameter("@tip", order.Tip);
            sqlParameters[4] = new SqlParameter("@totalPrice", order.Total);
            sqlParameters[5] = new SqlParameter("@paymentDate", DateTime.Now);


            ExecuteEditQuery(query, sqlParameters);
            order.isPaid = newOrderIsPaidStatus;
        }

        #endregion

        // Tommy's DAO parts---------------------------------------------------------------------------
        public int GetNewestOrder() // Get newest order form the database
        {
            string query = "SELECT TOP 1 orderID FROM [ORDER] ORDER BY orderID DESC;";
            SqlParameter[] sqlParameters = new SqlParameter[0];
            return ReadOrderID(ExecuteSelectQuery(query, sqlParameters));
        }

        private int ReadOrderID(DataTable dataTable) // store orderID in a variable
        {
            int newestOrder = 0;
            foreach (DataRow dr in dataTable.Rows)
            {
                newestOrder = (int)dr["orderID"];
            }
            return newestOrder;
        }

        public int AddOrder(Order order)
        {
            int notPaid = 0; // means not paid
            int TempOrderStatus = 1; // Ordered status
            string query = $"INSERT INTO [ORDER](totalPrice, tableID, orderStatus, employeeID, isPaid) " +
                    $"VALUES(@totalPrice, @tableID, @orderStatus, @employeeID, @isPaid);";
            SqlParameter[] sqlParameters = new SqlParameter[5];
            sqlParameters[0] = new SqlParameter("@totalPrice", order.CalculateTotalOrderPriceByItems());
            sqlParameters[1] = new SqlParameter("@tableID", order.Table.TableID);
            sqlParameters[2] = new SqlParameter("@orderStatus", TempOrderStatus);
            sqlParameters[3] = new SqlParameter("@employeeID", order.Employee.EmployeeID);
            sqlParameters[4] = new SqlParameter("@isPaid", notPaid);
            ExecuteEditQuery(query, sqlParameters);
            return GetNewestOrder(); // gets newest orderID
        }

        public void AddOrderItem(Order order)
        {
            DateTime orderDate = DateTime.Now;          
            foreach (OrderItem orderItem in order.OrderItems)
            {
                int place = (int)orderItem.menuItem.place;
                int status = (int)orderItem.Status;
                // inserting each orderItem in the database
                string query = $"INSERT INTO [ORDER_ITEM](orderID, item_id, quantity, itemStatus, comment, orderTime, placeID) " +
                    $"VALUES(@orderID, @item_id, @quantity, @itemStatus, @comment, @orderTime, @placeID);";

                SqlParameter[] sqlParameters = new SqlParameter[7];
                sqlParameters[0] = new SqlParameter("@orderID", order.OrderID);
                sqlParameters[1] = new SqlParameter("@item_id", orderItem.menuItem.item_id);
                sqlParameters[2] = new SqlParameter("@quantity", orderItem.Quantity);
                sqlParameters[3] = new SqlParameter("@itemStatus", status);
                sqlParameters[4] = new SqlParameter("@comment", orderItem.Comment);
                sqlParameters[5] = new SqlParameter("@orderTime", orderDate);
                sqlParameters[6] = new SqlParameter("@placeID", place);
                ExecuteEditQuery(query, sqlParameters);
                /////////////////////// decreasing the stock
                int decreaseAmount = orderItem.menuItem.item_id;
                string query2 = "Update [MENU_ITEM] Set stock = stock - @stock where item_id = @item_id";
                SqlParameter[] sqlParameters2 = new SqlParameter[2];
                sqlParameters2[0] = new SqlParameter("@stock", orderItem.Quantity);
                sqlParameters2[1] = new SqlParameter("@item_id", decreaseAmount);
                ExecuteEditQuery(query2, sqlParameters2);
            }
        }
    }
}