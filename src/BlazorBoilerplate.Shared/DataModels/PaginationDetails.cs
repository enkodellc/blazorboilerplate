using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.DataModels
{
    /// <summary>
    /// Standardized details including in paginated responses
    /// </summary>
    public class PaginationDetails
    {
        public int? PageIndex { get; set; }

        public int? PageSize { get; set; }

        /// <summary>
        /// Reference point that captures the relative state of the collection at the time of data generation.
        /// Usually the index id of the last item present in the collection (returned by client in future requests to enable delta requests)
        /// </summary>
        public int? SyncPointReference { get; set; }


        /// <summary>
        /// Total size of the backing collection referenced by the client request.
        /// </summary>
        public int? CollectionSize { get; set; }


        /// <summary>
        /// Returns the context of the original request (primarily for troubleshooting purposes but could also help to avoid race conditions)
        /// </summary>
        public int? DeltaRequestReference { get; set; } // may need to change to accomodate other reference object types (eg DateTime)


        /// <summary>
        /// Used in Delta Queries to indicate the net change in collection size since the given reference point (SyncPointReference including in the original client request)
        /// </summary>
        public int? CollectionSizeDelta { get; set; }
    }
}
