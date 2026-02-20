using System;
using System.Linq;
using SaasSystem.Customers;

Console.WriteLine(string.Join(",", typeof(CustomerAppService).BaseType!.GetProperties().Select(p => p.Name)));
