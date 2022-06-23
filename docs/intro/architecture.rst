Architecture
============

.. image:: /images/blazorboilerplate-architecture.png
   :align: center

The diagram shows the dependencies of the main projects.
Every project with text to localize depends on **Localization** project.

**Client** project is used only with Blazor WebAssembly, so it runs on the browser. It initializes the WebAssembly startup. If you only use Blazor Server, remove this project from solution.

**Server** project manages the main services: authentication, authorization, API, etc.

**Storage** project manages the persistence of models in a database with `Entity Framework Core <https://docs.microsoft.com/en-us/ef/core/>`_.

**Infrastructure** project contains interfaces and models in common among projects running on server.

**Shared** project contains base interfaces and models in common among projects running on server or client.

**Module** projects contains UI, services, etc. that define your web application.