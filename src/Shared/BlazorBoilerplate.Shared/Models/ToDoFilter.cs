using AutoNotify;
using System;

namespace BlazorBoilerplate.Shared.Models
{
    public partial class ToDoFilter :QueryParameters
    {
        [AutoNotify]
        private DateTime? _from;

        [AutoNotify]
        private DateTime? _to;

        [AutoNotify]
        private Guid? _createdById;

        [AutoNotify]
        private Guid? _modifiedById;

        [AutoNotify]
        private bool? _isCompleted;
    }
}
