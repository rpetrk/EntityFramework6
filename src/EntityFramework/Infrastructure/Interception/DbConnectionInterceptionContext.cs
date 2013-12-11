// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Common;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Represents contextual information associated with calls to <see cref="DbConnection"/> that don't return any results.
    /// </summary>
    public class DbConnectionInterceptionContext : MutableInterceptionContext
    {
        /// <summary>
        /// Constructs a new <see cref="DbConnectionInterceptionContext" /> with no state.
        /// </summary>
        public DbConnectionInterceptionContext()
        {
        }

        /// <summary>
        /// Creates a new <see cref="DbConnectionInterceptionContext" /> by copying immutable state from the given
        /// interception context. Also see <see cref="Clone" />
        /// </summary>
        /// <param name="copyFrom">The context from which to copy state.</param>
        public DbConnectionInterceptionContext(DbInterceptionContext copyFrom)
            : base(copyFrom)
        {
            Check.NotNull(copyFrom, "copyFrom");
        }

        /// <summary>
        /// Creates a new <see cref="DbConnectionInterceptionContext" /> that contains all the contextual information in this
        /// interception context together with the <see cref="DbInterceptionContext.IsAsync" /> flag set to true.
        /// </summary>
        /// <returns>A new interception context associated with the async flag set.</returns>
        public new DbConnectionInterceptionContext AsAsync()
        {
            return (DbConnectionInterceptionContext)base.AsAsync();
        }

        /// <summary>
        /// Creates a new <see cref="DbConnectionInterceptionContext" /> that contains all the contextual information in this
        /// interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new DbConnectionInterceptionContext WithDbContext(DbContext context)
        {
            Check.NotNull(context, "context");

            return (DbConnectionInterceptionContext)base.WithDbContext(context);
        }

        /// <summary>
        /// Creates a new <see cref="DbConnectionInterceptionContext" /> that contains all the contextual information in this
        /// interception context with the addition of the given <see cref="ObjectContext" />.
        /// </summary>
        /// <param name="context">The context to associate.</param>
        /// <returns>A new interception context associated with the given context.</returns>
        public new DbConnectionInterceptionContext WithObjectContext(ObjectContext context)
        {
            Check.NotNull(context, "context");

            return (DbConnectionInterceptionContext)base.WithObjectContext(context);
        }

        /// <inheritdoc />
        protected override DbInterceptionContext Clone()
        {
            return new DbConnectionInterceptionContext(this);
        }
    }
}