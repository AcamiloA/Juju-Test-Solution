using DataAccess;
using DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business
{
    /// <summary>
    /// Servicio encargado de la gestión de los posts.
    /// </summary>
    public class PostService : BaseService<Post>
    {
        private readonly BaseModel<Customer> _customerModel;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="PostService"/>.
        /// </summary>
        /// <param name="baseModel">Modelo base para la entidad Post.</param>
        /// <param name="customerModel">Modelo para la entidad Customer.</param>
        public PostService(BaseModel<Post> baseModel, BaseModel<Customer> customerModel) : base(baseModel)
        {
            _customerModel = customerModel ?? throw new ArgumentNullException(nameof(customerModel), "El modelo de cliente no puede ser nulo.");
        }

        /// <summary>
        /// Agrega un nuevo post a la base de datos, asociándolo con un cliente.
        /// </summary>
        /// <param name="entity">Entidad de tipo Post a agregar.</param>
        /// <returns>El post agregado.</returns>
        /// <exception cref="ArgumentException">Lanzado si el cliente no existe o si los datos del post son inválidos.</exception>
        public Post AddPost(Post entity)
        {
            Customer customer = _customerModel.GetByWhere(c => c.CustomerId == entity.CustomerId)
                                              ?? throw new ArgumentException($"El cliente con ID {entity.CustomerId} no existe.");

            entity.Body = TruncateBody(entity.Body);

            entity.Category = GetCategoryByType(entity.Type) ?? entity.Category;

            base.Create(entity);

            return entity;
        }

        /// <summary>
        /// Crea múltiples posts en la base de datos.
        /// </summary>
        /// <param name="posts">Lista de posts a crear.</param>
        public void CreateMultiplePosts(List<Post> posts)
        {
            if (posts is null || !posts.Any())
                throw new ArgumentException("La lista de posts no puede estar vacía.", nameof(posts));

            try
            {
                foreach (Post post in posts)
                {
                    Customer customer = _customerModel.GetByWhere(c => c.CustomerId == post.CustomerId)
                                              ?? throw new ArgumentException($"El cliente con ID {post.CustomerId} no existe.");

                    post.Body = TruncateBody(post.Body);

                    post.Category = GetCategoryByType(post.Type) ?? post.Category;
                }

                base.AddMany(posts);
            }
            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>
        /// Trunca el cuerpo del post si excede el tamaño máximo permitido.
        /// </summary>
        /// <param name="body">Cuerpo del post.</param>
        /// <returns>El cuerpo del post truncado si es necesario.</returns>
        private string TruncateBody(string body)
        {
            if (!string.IsNullOrWhiteSpace(body) && body.Length > 20)
                return body.Length > 97 ? body.Substring(0, 97) + "..." : body;

            return body;
        }

        /// <summary>
        /// Obtiene la categoría del post según su tipo.
        /// </summary>
        /// <param name="type">Tipo de post.</param>
        /// <returns>La categoría correspondiente al tipo.</returns>
        private string GetCategoryByType(int type)
        {
            switch (type)
            {
                case 1:
                    return "Farándula";
                case 2:
                    return "Política";
                case 3:
                    return "Futbol";
                default:
                    return null;
            }
        }
    }
}
