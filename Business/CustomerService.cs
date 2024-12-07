using DataAccess.Data;
using System;
using System.Collections.Generic;

namespace Business
{
    public class CustomerService
    {
        private readonly BaseService<Customer> _customerService;
        private readonly BaseService<Post> _postService;

        public CustomerService(BaseService<Customer> customerService, BaseService<Post> postService)
        {
            _customerService = customerService;
            _postService = postService;
        }

        public Customer DeleteCustomerWithPosts(int customerId)
        {
            Customer customer = _customerService.FindById(customerId) ?? throw new ArgumentException($"El Customer con ID {customerId} no existe.");

            IEnumerable<Post> posts = _postService.GetListByWhere(post => post.CustomerId == customerId);

           
            _postService.Delete(posts);  
            _customerService.Delete(customer);
            return customer;
        }
    }
}
