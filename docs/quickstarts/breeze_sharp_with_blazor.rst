Breeze Sharp with Blazor
========================

.. note:: What you will read here are my personal opinions.
   I have been using Breeze from years for both web and desktop applications and I have no connection with Breeze team.

   `Giovanni Quarella <https://github.com/GioviQ>`_

Recommended **CRUD API design** is too verborse and boring.

For each CRUD action Create, Read, Update and Delete you have to add a method to your API controller.
Also you have to repeat the same pattern for every entity of your application domain.

In real use cases you cannot perform also single actions on entities, because changes have to happen at the same time in a transaction.
So you have to write other code...

Do you know good coders are lazy? Do you know the DRY principle?

The solution is a `Breeze Controller`_.

As you can read in the official documentation: *“One controller to rule them all …”*.

Breeze Server
-------------
In this project the `Breeze Controller`_ is `ApplicationController`_ and the `EFPersistenceManager`_ is `ApplicationPersistenceManager`_.

The `ApplicationController`_ has no **Authorize** attribute, because to keep
policy management with Breeze, I implemented a simple custom policy
management in `ApplicationPersistenceManager`_.

First of all you can mark the `EF entities`_ with `Permissions
attribute`_ which takes `CRUD Actions flag enum`_ as parameter.
Examples:

::

   [Permissions(Actions.Delete)]
   public partial class Todo
   {
    ...
   }

   [Permissions(Actions.Create | Actions.Update)]
   public partial class Todo
   {
    ...
   }

   [Permissions(Actions.CRUD)]
   public partial class Todo
   {
    ...
   }

In BeforeSaveEntities method of `ApplicationPersistenceManager`_ the
actions of the entities to be saved are compared with the claims of
permission type of the current user. If a policy is violated an
EntityErrorsException is thrown.

To check **Actions.Read** in `ApplicationController`_ you have to access EF
DbSet with **GetEntities** method of `ApplicationPersistenceManager`_.

Breeze.Sharp (Breeze C# client)
-------------------------------

To access Breeze Controller you have to use `Breeze.Sharp`_.
In Blazor BlazorBoilerplate `ApiClient`_ is what you need to query `ApplicationController`_.
You can inject `IApiClient`_ in every Razor page where entities are requested.

The `Breeze.Sharp entities`_ are the so called DTO and you should create one for every entities of your Entity Framework context in `EF entities`_.
To avoid repetitive tasks, I created `EntityGenerator`_ based on `Source Generators`_.
Every time you update `EF entities`_, rebuild **BlazorBoilerplate.Shared** project to have Breeze entities automatically generated with namespace `BlazorBoilerplate.Shared.Dto.Db`_.

Notes
^^^^^

At the moment **Todo** is the only entity used to demonstrate Breeze.Sharp capabilities.
In fact you can extend the use to all other entities, but administrative entities (users, roles, etc.) are managed by others libraries like **ASP.NET Core Identity** and **IdentityServer4**,
so I preferred to keep the dedicated AdminController.

If you think that is all wonderful, here it comes the **drawback**.

The used of C# as a replacement of javascript is something very new: it is the Blazor revolution.
So till now Breeze.Sharp has been used a little only on desktop application.
In fact the most used Breeze client is the javascript client BreezeJS and Breeze itself is not .NET centric.

For this reason `breeze.sharp library`_ is rarely updated, so to make BlazorBoilerplate working with Breeze I used my fork `GioviQ/breeze.sharp`_ where I fixed some issues.
You can find the package in https://www.nuget.org/packages/Breeze.Sharp.Standard.Fork/.

.. _Breeze Controller: http://breeze.github.io/doc-net/webapi-controller-core.html
.. _ApplicationController: https://github.com/enkodellc/blazorboilerplate/blob/master/src/Server/BlazorBoilerplate.Server/Controllers/ApplicationController.cs
.. _EFPersistenceManager: http://breeze.github.io/doc-net/ef-efpersistencemanager-core.html
.. _ApplicationPersistenceManager: https://github.com/enkodellc/blazorboilerplate/blob/master/src/Server/BlazorBoilerplate.Storage/ApplicationPersistenceManager.cs
.. _EF entities: https://github.com/enkodellc/blazorboilerplate/tree/master/src/Shared/BlazorBoilerplate.Infrastructure.Storage/DataModels
.. _Permissions attribute: https://github.com/enkodellc/blazorboilerplate/blob/master/src/Shared/BlazorBoilerplate.Infrastructure/AuthorizationDefinitions/PermissionsAttribute.cs
.. _CRUD Actions flag enum: https://github.com/enkodellc/blazorboilerplate/blob/master/src/Shared/BlazorBoilerplate.Infrastructure/AuthorizationDefinitions/Actions.cs
.. _Breeze.Sharp: http://breeze.github.io/doc-cs/
.. _Breeze.Sharp entities: http://breeze.github.io/doc-cs/entities-and-complexobjects.html
.. _IApiClient: https://github.com/enkodellc/blazorboilerplate/blob/master/src/Shared/BlazorBoilerplate.Shared/Interfaces/IApiClient.cs
.. _ApiClient: https://github.com/enkodellc/blazorboilerplate/blob/master/src/Shared/BlazorBoilerplate.Shared/Services/ApiClient.cs
.. _BlazorBoilerplate.Shared.Dto.Db: https://github.com/enkodellc/blazorboilerplate/tree/master/src/Shared/BlazorBoilerplate.Shared/Dto/Db
.. _breeze.sharp library: https://github.com/Breeze/breeze.sharp
.. _GioviQ/breeze.sharp: https://github.com/GioviQ/breeze.sharp
.. _EntityGenerator: https://github.com/enkodellc/blazorboilerplate/blob/master/src/Utils/BlazorBoilerplate.SourceGenerator/EntityGenerator.cs
.. _Source Generators: https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/