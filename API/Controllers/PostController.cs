using Business;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using PostEntity = DataAccess.Data.Post;

namespace API.Controllers.Post
{
    /// <summary>
    /// Controlador para gestionar los posts.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly PostService _postService;

        /// <summary>
        /// Constructor para inyectar el servicio de posts.
        /// </summary>
        /// <param name="postService">Servicio para gestionar operaciones sobre los posts.</param>
        public PostController(PostService postService)
        {
            _postService = postService ?? throw new ArgumentNullException(nameof(postService));
        }

        /// <summary>
        /// Obtiene todos los posts.
        /// </summary>
        /// <returns>Una lista de todos los posts.</returns>
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                IQueryable<PostEntity> posts = _postService.GetAll();
                if (posts is null || !posts.Any())
                    return NoContent();                 

                return Ok(posts); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error al obtener los posts.", details = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo post.
        /// </summary>
        /// <param name="entity">El post a crear.</param>
        /// <returns>El post creado.</returns>
        [HttpPost]
        public ActionResult<PostEntity> Create([FromBody] PostEntity entity)
        {
            if (entity is null)
                return BadRequest("El post no puede ser nulo.");

            try
            {
                PostEntity createdPost = _postService.AddPost(entity);
                return CreatedAtAction(nameof(GetAll), new { id = createdPost.PostId }, createdPost); 
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new { message = "Entrada incorrecta", details = argEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error al crear el post.", details = ex.Message });
            }
        }

        /// <summary>
        /// Crea múltiples posts.
        /// </summary>
        /// <param name="request">Lista de posts a crear.</param>
        /// <returns>Mensaje de éxito.</returns>
        [HttpPost("AddMany")]
        public ActionResult CreateMultiplePosts([FromBody] List<PostEntity> request)
        {
            if (request is null || !request.Any())
                return BadRequest("Debe proporcionar al menos un post.");

            try
            {
                _postService.CreateMultiplePosts(request);
                return Ok(new { message = "Posts creados exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error al crear los posts.", details = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un post existente.
        /// </summary>
        /// <param name="entity">El post con la información actualizada.</param>
        /// <returns>El post actualizado.</returns>
        [HttpPut]
        public ActionResult<PostEntity> Update([FromBody] PostEntity entity)
        {
            if (entity is null || entity.PostId == 0)
                return BadRequest("El post debe tener un Id válido.");

            try
            {
                PostEntity updatedPost = _postService.Update(entity.PostId, entity, out bool changed);
                return Ok(updatedPost); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error al actualizar el post.", details = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un post existente.
        /// </summary>
        /// <param name="id">El Id del post a eliminar.</param>
        /// <returns>Mensaje de éxito o error.</returns>
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            if (id <= 0)
                return BadRequest("El Id proporcionado no es válido.");

            try
            {
                PostEntity entity = _postService.FindById(id);
                _postService.Delete(entity);
                return Ok(new { message = "Post eliminado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error al eliminar el post.", details = ex.Message });
            }
        }
    }
}
