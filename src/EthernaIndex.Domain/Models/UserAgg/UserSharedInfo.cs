﻿using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Domain.Models.UserAgg
{
    /* This model will not be encapsulated with User until https://etherna.atlassian.net/browse/MODM-101 is solved.
     * After it, a full referenced inclusion can be implemented. */

    /// <summary>
    /// Shared with other Etherna services. Use in read-only mode.
    /// </summary>
    public class UserSharedInfo : EntityModelBase<string>
    {
        // Fields.
        private List<string> _etherPreviousAddresses = new();

        // Constructors.
        protected UserSharedInfo() { }

        // Properties.
        public virtual string EtherAddress { get; protected set; } = default!;
        public virtual IEnumerable<string> EtherPreviousAddresses
        {
            get => _etherPreviousAddresses;
            protected set => _etherPreviousAddresses = new List<string>(value ?? Array.Empty<string>());
        }
        public bool IsLockedOutNow => LockoutEnabled && LockoutEnd >= DateTimeOffset.UtcNow;
        public virtual bool LockoutEnabled { get; protected set; }
        public virtual DateTimeOffset? LockoutEnd { get; protected set; }
    }
}
