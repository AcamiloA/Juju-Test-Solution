using Business;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using CustomerEntity = DataAccess.Data.Customer;

namespace API.Controllers.Customer
{
    /// <summary>
    /// Controlador para gestionar los clientes.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerService _customerService;
        private readonly BaseService<CustomerEntity> _baseService;

        /// <summary>
        /// Constructor para inyectar los servicios necesarios.
        /// </summary>
        /// <param name="customerService">Servicio para gestionar operaciones relacionadas con clientes.</param>
        /// <param name="baseService">Servicio base genérico para operaciones CRUD.</param>
        public CustomerController(CustomerService customerService, BaseService<CustomerEntity> baseService)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _baseService = baseService ?? throw new ArgumentNullException(nameof(baseService));
        }

        /// <summary>
        /// Obtiene todos los clientes.
        /// </summary>
        /// <returns>Una lista de todos los clientes.</returns>
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                IQueryable<CustomerEntity> customers = _baseService.GetAll();
                if (customers == null || !customers.Any())  
                    return NoContent();                

                return Ok(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error al obtener los clientes.", details = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo cliente.
        /// </summary>
        /// <param name="entity">El cliente a crear.</param>
        /// <returns>El cliente creado.</returns>
        [HttpPost]
        public ActionResult<CustomerEntity> Create([FromBody] CustomerEntity entity)
        {
            if (entity == null)            
                return BadRequest("Los datos del cliente no pueden ser nulos.");            

            if (_baseService.GetByWhere(_ => _.Name == entity.Name) != null)            
                return BadRequest("Datos duplicados, ya existe un cliente con ese nombre.");            

            try
            {
                var createdCustomer = _baseService.Create(entity);
                return Ok(createdCustomer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error al crear el cliente.", details = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un cliente existente.
        /// </summary>
        /// <param name="entity">El cliente con la información actualizada.</param>
        /// <returns>El cliente actualizado.</returns>
        [HttpPut]
        public ActionResult<CustomerEntity> Update([FromBody] CustomerEntity entity)
        {
            if (entity is null || entity.CustomerId == 0)
                return BadRequest("El cliente debe tener un Id válido.");            

            if (_baseService.GetByWhere(_ => _.Name == entity.Name && _.CustomerId != entity.CustomerId) != null)            
                return BadRequest("Datos duplicados, ya existe un cliente con ese nombre.");            

            try
            {
                CustomerEntity updatedCustomer = _baseService.Update(entity.CustomerId, entity, out bool changed);
                if (updatedCustomer is null || !changed)                
                    return NotFound("Cliente no encontrado o no se realizaron cambios.");                

                return Ok(updatedCustomer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error al actualizar el cliente.", details = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un cliente por su Id.
        /// </summary>
        /// <param name="id">El Id del cliente a eliminar.</param>
        /// <returns>Mensaje de éxito o error.</returns>
        [HttpDelete("{id}")]
        public ActionResult<CustomerEntity> Delete(int id)
        {
            if (id <= 0)
             return BadRequest("El Id proporcionado no es válido.");

            try
            {
                CustomerEntity result = _customerService.DeleteCustomerWithPosts(id);
                if (result is null) 
                    return NotFound("Cliente no encontrado.");

                return Ok(new { message = "Cliente eliminado exitosamente." });
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new { message = "Entrada incorrecta", details = argEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error al eliminar el cliente.", details = ex.Message });
            }
        }
    }
}
