using System;

namespace MySolution.DomainModels
{
    public class Employee
    {
        public string EmployeeId { get; set; }
        public string FullName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Department { get; set; }

        public Employee(){ }

        public Employee(string employeeId, string fullName)
        {
            EmployeeId = employeeId;
            FullName = fullName;
        }

        public override string ToString()
        {
            return $"{EmployeeId} - {FullName} - {Department}";
        }
    }
}
