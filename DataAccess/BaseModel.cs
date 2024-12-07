using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DataAccess
{
    public class BaseModel<TEntity> where TEntity : class, new()
    {
        /// <summary>
        /// Contexto
        /// </summary>
        private readonly JujuTestContext _context;

        /// <summary>
        /// Entidad
        /// </summary>
        protected DbSet<TEntity> _dbSet;

        /// <summary>
        /// Transacción
        /// </summary>
        private IDbContextTransaction _transaction;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public BaseModel(JujuTestContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "El contexto no puede ser nulo.");
            _dbSet = _context.Set<TEntity>();
        }

        #region Transacciones

        /// <summary>
        /// Inicia una nueva transacción.
        /// </summary>
        public IDbContextTransaction BeginTransaction()
        {
            if (_transaction is null)
                _transaction = _context.Database.BeginTransaction();

            return _transaction; 
        }

        /// <summary>
        /// Confirma la transacción.
        /// </summary>
        public void CommitTransaction()
        {
            _transaction?.Commit();
            _transaction?.Dispose();
        }

        /// <summary>
        /// Revierte la transacción.
        /// </summary>
        public void RollbackTransaction()
        {
            _transaction?.Rollback();
            _transaction?.Dispose();
        }


        #endregion

        #region Repository

        /// <summary>
        /// Consulta la entidad por predicado
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual TEntity GetByWhere(Expression<Func<TEntity, bool>> predicate)
        {
            if (predicate is null) 
                throw new ArgumentNullException(nameof(predicate), "El predicado no puede ser nulo.");

            return _dbSet.Where(predicate).FirstOrDefault();
        }

        /// <summary>
        /// Consulta la entidad por predicado
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public virtual IEnumerable<TEntity> GetListByWhere(Expression<Func<TEntity, bool>> predicate)
        {
            if (predicate is null) 
                throw new ArgumentNullException(nameof(predicate), "El predicado no puede ser nulo.");

            return _dbSet.Where(predicate).ToList();
        }

        /// <summary>
        /// Consulta todas las entidades
        /// </summary>
        public virtual IQueryable<TEntity> GetAll
        {
            get { return _dbSet; }
        }

        /// <summary>
        /// Consulta una entidad por id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual TEntity FindById(object id)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id), "El ID no puede ser nulo.");

            return _dbSet.Find(id);
        }

        /// <summary>
        /// Crea una entidad (Guarda)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual TEntity Create(TEntity entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity), "La entidad no puede ser nula.");

            if (_transaction is null)
                BeginTransaction();

            try
            {
                _dbSet.Add(entity);
                _context.SaveChanges();
                return entity;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Agrega varias entidades (Guarda)
        /// </summary>
        /// <param name="entities">Listado de objetos a guardar</param>
        /// <returns></returns>
        public virtual IEnumerable<TEntity> AddMany(IEnumerable<TEntity> entities)
        {
            if (entities is null) throw new ArgumentNullException(nameof(entities), "La entidad no puede ser nula.");

            if (_transaction is null)
                BeginTransaction();

            try
            {
                _dbSet.AddRange(entities);
                _context.SaveChanges();
                return entities;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Actualiza la entidad (GUARDA)
        /// </summary>
        /// <param name="editedEntity">Entidad editada</param>
        /// <param name="originalEntity">Entidad Original sin cambios</param>
        /// <param name="changed">Indica si se modifico la entidad</param>
        /// <returns></returns>
        public virtual TEntity Update(TEntity editedEntity, TEntity originalEntity, out bool changed)
        {
            if (editedEntity is null) 
                throw new ArgumentNullException(nameof(editedEntity), "La entidad editada no puede ser nula.");

            if (originalEntity is null) 
                throw new ArgumentNullException(nameof(originalEntity), "La entidad original no puede ser nula.");

            if (_transaction is null)
                BeginTransaction();

            try
            {
                _context.Entry(originalEntity).CurrentValues.SetValues(editedEntity);
                changed = _context.Entry(originalEntity).State == EntityState.Modified;
                _context.SaveChanges();
                return originalEntity;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Elimina una entidad
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual TEntity Delete(TEntity entity)
        {
            if (entity is null) 
                throw new ArgumentNullException(nameof(entity), "La entidad no puede ser nula.");

            if (_transaction is null)
                BeginTransaction();

            try
            {
                _dbSet.Remove(entity);
                _context.SaveChanges();
                return entity;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Elimina varias entidades
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual IEnumerable<TEntity> DeleteMany(IEnumerable<TEntity> entities)
        {
            if (entities is null)
                throw new ArgumentNullException(nameof(entities), "La entidad no puede ser nula.");

            if (_transaction is null)
                BeginTransaction();

            try
            {
                _dbSet.RemoveRange(entities);
                _context.SaveChanges();
                return entities;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Guarda cambios
        /// </summary>
        public virtual void SaveChanges()
        {
            _context.SaveChanges();
        }

        #endregion
    }
}
