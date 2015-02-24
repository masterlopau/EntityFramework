// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata.ModelConventions
{
    public class KeyDiscoveryConventionTest
    {
        private class EntityWithNoId
        {
            public string Name { get; set; }
            public DateTime ModifiedDate { get; set; }
        }

        [Fact]
        public void Primary_key_is_not_set_when_zero_key_properties()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithNoId>();

            Assert.Same(entityBuilder, new KeyDiscoveryConvention().Apply(entityBuilder));

            var key = entityBuilder.Metadata.TryGetPrimaryKey();
            Assert.Null(key);
        }

        [Fact]
        public void Composite_primary_key_is_set_when_multiple_key_properties()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithNoId>();
            var convention = new Mock<KeyDiscoveryConvention> { CallBase = true };
            convention.Protected().Setup<IEnumerable<Property>>("DiscoverKeyProperties", ItExpr.IsAny<EntityType>()).Returns<EntityType>(t => t.Properties.ToList());
            
            Assert.Same(entityBuilder, convention.Object.Apply(entityBuilder));

            var key = entityBuilder.Metadata.TryGetPrimaryKey();
            Assert.NotNull(key);
            Assert.Equal(new[] { "ModifiedDate", "Name" }, key.Properties.Select(p => p.Name));
        }

        private class EntityWithId
        {
            public int Id { get; set; }
        }

        [Fact]
        public void DiscoverKeyProperties_discovers_id()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithId>();

            Assert.Same(entityBuilder, new KeyDiscoveryConvention().Apply(entityBuilder));

            var key = entityBuilder.Metadata.TryGetPrimaryKey();
            Assert.NotNull(key);
            Assert.Equal(new[] { "Id" }, key.Properties.Select(p => p.Name));
        }

        private class EntityWithTypeId
        {
            public int EntityWithTypeIdId { get; set; }
        }

        [Fact]
        public void DiscoverKeyProperties_discovers_type_id()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithTypeId>();

            Assert.Same(entityBuilder, new KeyDiscoveryConvention().Apply(entityBuilder));

            var key = entityBuilder.Metadata.TryGetPrimaryKey();
            Assert.NotNull(key);
            Assert.Equal(new[] { "EntityWithTypeIdId" }, key.Properties.Select(p => p.Name));
        }

        private class EntityWithIdAndTypeId
        {
            public int Id { get; set; }
            public int EntityWithIdAndTypeIdId { get; set; }
        }

        [Fact]
        public void DiscoverKeyProperties_prefers_id_over_type_id()
        {
            var entityBuilder = CreateInternalEntityBuilder<EntityWithIdAndTypeId>();

            Assert.Same(entityBuilder, new KeyDiscoveryConvention().Apply(entityBuilder));

            var key = entityBuilder.Metadata.TryGetPrimaryKey();
            Assert.NotNull(key);
            Assert.Equal(new[] { "Id" }, key.Properties.Select(p => p.Name));
        }

        private class EntityWithMultipleIds
        {
            public int ID { get; set; }
            public int Id { get; set; }
        }

        [Fact]
        public void DiscoverKeyProperties_throws_when_multiple_ids()
        {
            var entityType = CreateInternalEntityBuilder<EntityWithMultipleIds>();
            var convention = new KeyDiscoveryConvention();

            var ex = Assert.Throws<InvalidOperationException>(() => convention.Apply(entityType));

            Assert.Equal(
                Strings.MultiplePropertiesMatchedAsKeys("ID", typeof(EntityWithMultipleIds).FullName),
                ex.Message);
        }

        private static InternalEntityBuilder CreateInternalEntityBuilder<T>()
        {
            var modelBuilder = new InternalModelBuilder(new Model());
            var entityBuilder = modelBuilder.Entity(typeof(T), ConfigurationSource.Convention);

            new PropertiesConvention().Apply(entityBuilder);

            return entityBuilder;
        }
    }
}
