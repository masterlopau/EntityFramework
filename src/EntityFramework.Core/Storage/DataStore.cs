// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStore
    {
        private readonly StateManager _stateManager;
        private readonly DbContextService<IModel> _model;
        private readonly ClrCollectionAccessorSource _collectionAccessorSource;
        private readonly ClrPropertySetterSource _propertySetterSource;
        private readonly LazyRef<ILogger> _logger;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected DataStore()
        {
        }

        protected DataStore(
            [NotNull] StateManager stateManager,
            [NotNull] DbContextService<IModel> model,
            [NotNull] EntityKeyFactorySource entityKeyFactorySource,
            [NotNull] EntityMaterializerSource entityMaterializerSource,
            [NotNull] ClrCollectionAccessorSource collectionAccessorSource,
            [NotNull] ClrPropertySetterSource propertySetterSource,
            [NotNull] ILoggerFactory loggerFactory)
        {
            Check.NotNull(stateManager, nameof(stateManager));
            Check.NotNull(model, nameof(model));
            Check.NotNull(entityKeyFactorySource, nameof(entityKeyFactorySource));
            Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource));
            Check.NotNull(collectionAccessorSource, nameof(collectionAccessorSource));
            Check.NotNull(propertySetterSource, nameof(propertySetterSource));
            Check.NotNull(loggerFactory, nameof(loggerFactory));

            _stateManager = stateManager;
            _model = model;

            EntityKeyFactorySource = entityKeyFactorySource;
            EntityMaterializerSource = entityMaterializerSource;

            _collectionAccessorSource = collectionAccessorSource;
            _propertySetterSource = propertySetterSource;

            _logger = new LazyRef<ILogger>(loggerFactory.Create<DataStore>);
        }

        public virtual ILogger Logger => _logger.Value;

        public virtual IModel Model => _model.Service;

        public virtual EntityKeyFactorySource EntityKeyFactorySource { get; }

        public virtual EntityMaterializerSource EntityMaterializerSource { get; }

        protected virtual IQueryBuffer CreateQueryBuffer()
        {
            return new QueryBuffer(
                _stateManager,
                EntityKeyFactorySource,
                _collectionAccessorSource,
                _propertySetterSource);
        }

        public abstract int SaveChanges(
            [NotNull] IReadOnlyList<InternalEntityEntry> entries);

        public abstract Task<int> SaveChangesAsync(
            [NotNull] IReadOnlyList<InternalEntityEntry> entries,
            CancellationToken cancellationToken = default(CancellationToken));

        public static readonly MethodInfo CompileQueryMethod
            = typeof(DataStore).GetTypeInfo().GetDeclaredMethod("CompileQuery");

        public abstract Func<QueryContext, IEnumerable<TResult>> CompileQuery<TResult>([NotNull] QueryModel queryModel);

        public static readonly MethodInfo CompileAsyncQueryMethod
            = typeof(DataStore).GetTypeInfo().GetDeclaredMethod("CompileAsyncQuery");

        public virtual Func<QueryContext, IAsyncEnumerable<TResult>> CompileAsyncQuery<TResult>([NotNull] QueryModel queryModel)
        {
            throw new NotImplementedException();
        }

        public abstract QueryContext CreateQueryContext();
    }
}
