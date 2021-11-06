﻿using ChapeauModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChapeauLogic;

namespace ChapeauUI
{
    public partial class LoginForm : Form
    {
        EmployeeService employeeService;
        public LoginForm()
        {
            InitializeComponent();
            employeeService = new EmployeeService();
            txtlogincode.PasswordChar = '*';
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            WinAPI.AnimateWindow(this.Handle, 2000, WinAPI.BLEND);
        }

        private void KeypadButton_Click(object sender, EventArgs e)
        {
            txtlogincode.AppendText(((Button)sender).Text);

            if (txtlogincode.TextLength == 4)
            {
                LogEmployeeIn();
            }
        }

        private void LogEmployeeIn()
        {
            Employee employee = employeeService.GetEmployeeByCode(int.Parse(txtlogincode.Text));

            // check if employee is null otherwise let the employee log into the application based on his role.
            if(employee == null)
            {
                txtlogincode.Text = "";
                MessageBox.Show("Entered pincode is not valid!");
            }

            else if (employee.Role == Role.Waiter)
            {
                this.Hide();
                TablePage page = new TablePage(employee);
                page.ShowDialog();
                
            }

            else if (employee.Role == Role.KitchenStaff || employee.Role == Role.Barman)
            {
                this.Hide();
                Kitchen_BarView kitchen_BarView = new Kitchen_BarView(employee);
                kitchen_BarView.Show();
            }

            else if (employee.Role == Role.Manager)
            {
                this.Hide();
                Stockview stockview = new Stockview();
                stockview.Show();
            }
        }

        private void exitbtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btndelete_Click(object sender, EventArgs e)
        {
            txtlogincode.Text = txtlogincode.Text.Substring(0, (txtlogincode.TextLength - 1));
        }
    }
}
