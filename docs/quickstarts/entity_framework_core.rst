Entity Framework Core
=====================

Getting started with EF Core (code first) reading this `tutorial <https://docs.microsoft.com/en-us/ef/core/get-started/overview/first-app>`_.

Migrations
----------
You can perform Entity Framework Core migrations directly from Visual Studio with `Package Manager Console <https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/powershell>`_
or with `command-line interface <https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dotnet>`_.

Here some tips to manage migrations in Blazor Boilerplate with command-line interface (CLI).

1. Make sure you have installed CLI; from the command line execute:
::

 dotnet tool install --global dotnet-ef

or to update
::

 dotnet tool update --global dotnet-ef

2. Keep in mind that every DbContext has its own migrations.

   The main DbContext is **ApplicationDbContext**.
   **PersistedGrantDbContext** and **ConfigurationDbContext** depend on
   `IdentityServer4 <https://identityserver4.readthedocs.io/en/latest/quickstarts/5_entityframework.html#database-schema-changes-and-using-ef-migrations>`_; if new versions have changed one or both db
   contexts, you have to add migrations. The migrations are in **BlazorBoilerplate.Storage**, so you have
   to open your shell in this path. Then you have to specify the project
   where the db contexts are added to the services, because **dotnet
   ef** has to know how to find and instantiate them. In our case the so
   called startup project is **BlazorBoilerplate.Server**. So the
   commands are:

::

 dotnet ef --startup-project ../BlazorBoilerplate.Server/ migrations add 4Preview3 -c PersistedGrantDbContext --verbose --no-build --configuration Debug

::

 dotnet ef --startup-project ../BlazorBoilerplate.Server/ migrations add 4Preview3 -c ConfigurationDbContext --verbose --no-build --configuration Debug

Without the **--no-build** parameter, **dotnet** rebuilds the startup
project, but if you have just built it with Visual Studio, it is just a
waste of time. Also the **dotnet** build is not the Visual Studio build,
so to avoid issue, use this procedure.

The name of migration, in this case *4Preview3*, is only descriptive and
it does not need to be unique, because automatically the name is
prepended with date time.

If the command is successful, **[datetime]_[migration-name].cs** is
added to the solution. Also the **[db-context-name]ModelSnapshot.cs** is
updated to reflect the db changes in case a new db has to be created.

When you run the project, the migrations are applied to the database (in
our case we have only one db). The db table **\__EFMigrationsHistory**
keeps track of applied migrations without information about the related
db context. To get this information use the following command; for
example for ConfigurationDbContext:
::

 dotnet ef --startup-project ../BlazorBoilerplate.Server/ migrations list -c ConfigurationDbContext --verbose --no-build --configuration Debug

You can also update the database, without running the project with the
following command:
::

 dotnet ef --startup-project ../BlazorBoilerplate.Server/ database update 4Preview3 -c ConfigurationDbContext --verbose --no-build --configuration Debug

If you specify a previous migration, you can revert the db changes to
that migration.

**Warning**

The migrations are not smart, when you have an existing db with
populated tables. If migrations add keys and/or unique indexes or
something else violating referential integrity, you have to manually
modify **[datetime]_[migration-name].cs** for example to add some SQL to
fix the errors. E.g. **migrationBuilder.Sql("UPDATE AspNetUserLogins SET
Id=NEWID() WHERE Id=''");** to add unique values to a new field before
setting as a new primary key.

Shadow Properties vs Source Generator
-------------------------------------

Always keeping in mind the DRY principle, it is boring implementing audit information and adding the same properties
**CreatedOn**, **ModifiedOn**, **CreatedBy** and **ModifiedBy** to all entities.

Some articles teach you to use `Shadow Properties`_ to add audit information,
but this is not the right solution, if you want expose these properties on the mapped entity types and use them e.g. in UI.

A solution is using `Source Generator`_.
`AuditableGenerator`_ generates for every class implementing **IAuditable** the above properties.
Remember all classes to be extended by Source Generator have to be **partial**.



.. _Shadow Properties: https://docs.microsoft.com/en-us/ef/core/modeling/shadow-properties
.. _Source Generator: https://devblogs.microsoft.com/dotnet/introducing-c-source-generators
.. _AuditableGenerator: https://github.com/enkodellc/blazorboilerplate/blob/master/src/Utils/BlazorBoilerplate.SourceGenerator/AuditableGenerator.cs