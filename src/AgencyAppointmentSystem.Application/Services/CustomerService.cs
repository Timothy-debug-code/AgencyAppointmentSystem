using AgencyAppointmentSystem.Application.DTOs;
using AgencyAppointmentSystem.Application.Interfaces.Repositories;
using AgencyAppointmentSystem.Application.Interfaces;
using AgencyAppointmentSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyAppointmentSystem.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<CustomerDto> GetCustomerByIdAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            return MapToDto(customer);
        }

        public async Task<CustomerDto> GetCustomerByEmailAsync(string email)
        {
            var customer = await _customerRepository.GetByEmailAsync(email);
            return MapToDto(customer);
        }

        public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto customerDto)
        {
            // Check if customer with same email already exists
            try
            {
                var existingCustomer = await _customerRepository.GetByEmailAsync(customerDto.Email);
                // If we reach here, customer exists
                throw new InvalidOperationException($"Customer with email {customerDto.Email} already exists.");
            }
            catch (KeyNotFoundException)
            {
                // This is expected, continue with creation
            }

            var customer = new Customer
            {
                Name = customerDto.Name,
                Email = customerDto.Email,
                PhoneNumber = customerDto.PhoneNumber
            };

            await _customerRepository.AddAsync(customer);
            return MapToDto(customer);
        }

        public async Task<CustomerDto> UpdateCustomerAsync(UpdateCustomerDto customerDto)
        {
            var customer = await _customerRepository.GetByIdAsync(customerDto.Id);

            // Check if the email is being changed and if so, that it's not already in use
            if (customer.Email != customerDto.Email)
            {
                try
                {
                    var existingCustomer = await _customerRepository.GetByEmailAsync(customerDto.Email);
                    if (existingCustomer.Id != customerDto.Id)
                    {
                        throw new InvalidOperationException($"Email {customerDto.Email} is already in use by another customer.");
                    }
                }
                catch (KeyNotFoundException)
                {
                    // This is expected, continue with update
                }
            }

            // Update properties
            customer.Name = customerDto.Name;
            customer.Email = customerDto.Email;
            customer.PhoneNumber = customerDto.PhoneNumber;

            await _customerRepository.UpdateAsync(customer);
            return MapToDto(customer);
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            await _customerRepository.DeleteAsync(id);
            return true;
        }

        private CustomerDto MapToDto(Customer customer)
        {
            return new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber
            };
        }
    }
}
