﻿using ChapeauDAL;
using ChapeauModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChapeauLogic
{
    public class EmployeeService
    {
        private EmployeeDAO employeeDAO;

        public EmployeeService()
        {
            employeeDAO = new EmployeeDAO();
        }

        public Employee GetEmployeeByCode(int logincode)
        {
            return employeeDAO.GetEmployeeByCode(logincode);
        }
        public Employee GetEmployeeById(int employeeId)
        {
            return employeeDAO.GetEmployeeId(employeeId);
        }
    }
}
