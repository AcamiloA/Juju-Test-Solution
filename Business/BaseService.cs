using DataAccess;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using static Dapper.SqlMapper;

namespace Business
{
    /// <summary>
    /// Servicio base que maneja operaciones CRUD comunes para cualquier entidad con soporte para transacciones.
    /// </summary>
    /// <typeparam name="TEntity">Tipo de entidad que extiende la clase base.</typeparam>
    public class BaseService<TEntity> where TEntity : class, new()
    {
        private readonly BaseModel<TEntity> _baseModel;


        /// <summary>
        /// Constructor de la clase BaseService.
        /// </summary>
        /// <param name="baseModel">Instancia del modelo base que maneja operaciones sobre la entidad.</param>
        public BaseService(BaseModel<TEntity> baseModel)
        {
            _baseModel = baseModel ?? throw new ArgumentNullException(nameof(baseModel), "El modelo base no puede ser nulo.");
        }

        #region Repository

        /// <summary>
        /// Consulta una entidad que cumple con un predicado especificado.
        /// </summary>
        /// <param name="predicate">Expresión para filtrar las entidades.</param>
        /// <returns>Entidad que cumple con el predicado.</returns>
        public virtual TEntity GetByWhere(Expression<Func<TEntity, bool>> predicate)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate), "El predicado no puede ser nulo.");
            return _baseModel.GetByWhere(predicate);
        }

        /// <summary>
        /// Consulta una lista de entidades que cumplen con un predicado especificado.
        /// </summary>
        /// <param name="predicate">Expresión para filtrar las entidades.</param>
        /// <returns>Lista de entidades que cumplen con el predicado.</returns>
        public virtual IEnumerable<TEntity> GetListByWhere(Expression<Func<TEntity, bool>> predicate)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate), "El predicado no puede ser nulo.");
            return _baseModel.GetListByWhere(predicate);
        }

        /// <summary>
        /// Busca una entidad por su ID.
        /// </summary>
        /// <param name="id">ID de la entidad.</param>
        /// <returns>Entidad encontrada.</returns>
        public virtual TEntity FindById(object id)
        {
            if (id is null) throw new ArgumentNullException(nameof(id), "El ID no puede ser nulo.");
            return _baseModel.FindById(id);
        }

        /// <summary>
        /// Consulta todas las entidades.
        /// </summary>
        /// <returns>Colección de todas las entidades.</returns>
        public virtual IQueryable<TEntity> GetAll()
        {
            return _baseModel.GetAll;
        }

        /// <summary>
        /// Crea una nueva entidad.
        /// </summary>
        /// <param name="entity">Entidad a crear.</param>
        /// <returns>Entidad creada.</returns>
        public virtual TEntity Create(TEntity entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity), "La entidad no puede ser nula.");

            using (IDbContextTransaction transaction = _baseModel.BeginTransaction())
            {
                try
                {
                    TEntity createdEntity = _baseModel.Create(entity);
                    _baseModel.SaveChanges();
                    transaction.Commit();
                    return createdEntity;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw; 
                }
            }
        }

        /// <summary>
        /// Actualiza una entidad existente.
        /// </summary>
        /// <param name="id">ID de la entidad a actualizar.</param>
        /// <param name="editedEntity">Entidad editada.</param>
        /// <param name="changed">Indica si hubo un cambio en la entidad.</param>
        /// <returns>Entidad actualizada.</returns>
        public virtual TEntity Update(object id, TEntity editedEntity, out bool changed)
        {
            if (id is null) throw new ArgumentNullException(nameof(id), "El ID no puede ser nulo.");
            if (editedEntity is null) throw new ArgumentNullException(nameof(editedEntity), "La entidad editada no puede ser nula.");

            TEntity originalEntity = _baseModel.FindById(id) ?? throw new KeyNotFoundException($"La entidad con ID {id} no fue encontrada.");
            using (IDbContextTransaction transaction = _baseModel.BeginTransaction())
            {
                try
                {
                    TEntity updatedEntity = _baseModel.Update(editedEntity, originalEntity, out changed);
                    transaction.Commit();
                    return updatedEntity;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw; 
                }
            }
        }

        /// <summary>
        /// Elimina una entidad.
        /// </summary>
        /// <param name="entity">Entidad a eliminar.</param>
        /// <returns>Entidad eliminada.</returns>
        public virtual TEntity Delete(TEntity entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity), "La entidad no puede ser nula.");

            using (IDbContextTransaction transaction = _baseModel.BeginTransaction())
            {
                try
                {
                    TEntity deletedEntity = _baseModel.Delete(entity);
                    _baseModel.SaveChanges();
                    transaction.Commit();
                    return deletedEntity;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    transaction.Dispose();
                }
            }
        }

        /// <summary>
        /// Elimina varias entidades.
        /// </summary>
        /// <param name="entities">Entidad a eliminar.</param>
        /// <returns>Entidad eliminada.</returns>
        public virtual IEnumerable<TEntity> Delete(IEnumerable<TEntity> entities)
        {
            if (!entities.Any()) throw new ArgumentNullException(nameof(entities), "La entidad no puede ser nula.");

            using (IDbContextTransaction transaction = _baseModel.BeginTransaction())
            {
                try
                {
                    IEnumerable<TEntity> deletedEntities = _baseModel.DeleteMany(entities);
                    _baseModel.SaveChanges();
                    transaction.Commit();
                    return entities;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    transaction.Dispose();
                }
            }
        }

        public virtual IEnumerable<TEntity> AddMany(IEnumerable<TEntity> entities)
        {
            if (!entities.Any()) throw new ArgumentNullException(nameof(entities), "La entidad no puede ser nula.");

            using (IDbContextTransaction transaction = _baseModel.BeginTransaction())
            {
                try
                {
                    _baseModel.AddMany(entities);
                    _baseModel.SaveChanges();
                    transaction.Commit();
                    return entities;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    transaction.Dispose();
                }
            }
        }

        /// <summary>
        /// Guarda los cambios realizados en la base de datos.
        /// </summary>
        public virtual void SaveChanges()
        {
            _baseModel.SaveChanges();
        }

        #endregion
    }
}
