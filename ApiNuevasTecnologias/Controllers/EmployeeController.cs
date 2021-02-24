﻿using ApiNuevasTecnologias.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ApiNuevasTecnologias.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        // GET: api/<EmployeeController>

        public NORTHWNDContext dbContext = new NORTHWNDContext();
        [HttpGet]
        public List<Employee> Get()
        {
            //select *from Employees;
            //Iquearyable
            return getAllEmployees().ToList();


        }

        // GET api/<EmployeeController>/5
        //[HttpGet("{id}")]
        [HttpGet("getEmployeeById")]
        public Employee Get(int id)
        {
            //select *from Employees where ID = 1
           return getAllEmployees().Where(w => w.EmployeeId == id).First();
        }

        [HttpGet("getEmployeeByName")]
        public Employee Get(string name)
        {
            //select *from Employees where ID = 1
            return getAllEmployees().Where(w => w.LastName.Contains(name)).First();
        }

        // POST api/<EmployeeController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<EmployeeController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<EmployeeController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        private DbSet<Employee> getAllEmployees()
        {
            var employeesQry = dbContext.Employees;
            return employeesQry;
        }
    }
}