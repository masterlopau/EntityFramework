// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.ModelConventions
{
    public class KeyDiscoveryConvention : IEntityTypeConvention
    {
        private const string KeySuffix = "Id";

        public virtual InternalEntityBuilder Apply(InternalEntityBuilder entityBuilder)
        {
            Check.NotNull(entityBuilder, "entityBuilder");
            var entityType = entityBuilder.Metadata;

            var keyProperties = DiscoverKeyProperties(entityType);
            if (keyProperties.Count != 0)
            {
                entityBuilder.PrimaryKey(keyProperties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);
            }

            return entityBuilder;
        }

        protected virtual IReadOnlyList<Property> DiscoverKeyProperties([NotNull] EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            // TODO: Honor [Key]
            // Issue #213
            var keyProperties = entityType.Properties
                .Where(p => string.Equals(p.Name, KeySuffix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (keyProperties.Count == 0)
            {
                keyProperties = entityType.Properties.Where(
                    p => string.Equals(p.Name, entityType.SimpleName + KeySuffix, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (keyProperties.Count > 1)
            {
                throw new InvalidOperationException(
                    Strings.MultiplePropertiesMatchedAsKeys(keyProperties.First().Name, entityType.Name));
            }

            return keyProperties;
        }
    }
}
