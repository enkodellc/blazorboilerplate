//Autogenerated by BlazorBoilerplate.EntityGenerator
using BlazorBoilerplate.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BlazorBoilerplate.Shared.DataInterfaces
{
    public interface IWikiPage
    {
        Int32 Id { get; set; }

        int pageid { get; set; }
        string ns { get; set; }

        string Title { get; set; }

        string Extract { get; set; }

    }
}
