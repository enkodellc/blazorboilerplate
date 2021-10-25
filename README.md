# OMA-ML Frontend

Using the Blazorboilerplate template as a base. Documentation about the structure of the entire boilerplate can be found [HERE](https://blazor-boilerplate.readthedocs.io/en/latest/index.html).

# OMA-ML section

![](https://github.com/hochschule-darmstadt/MetaAutoML-Controller/blob/main/docs/images/frontend-overview.png)

# Worked on sections

Project sections which are relevant for OMA-ML
- MetaAutoML-Frontend/src/Server/BlazorBoilerplate.Server/
    - Controllers: containing the API controllers which are used by the frontend itself
    - Managers: called by the controllers to execute specific actions
    - Startup.cs: add new manager services here
- MetaAutoML-Frontend/src/Shared/Modules/
    - Demo: frontend pages and components can be found here
    - Admin: admin frontend pages and components can be found here
    - .: shared pages and components can be found here
- MetaAutoML-Frontend/src/Shared/BlazorBoilerplate.Infrastructure/Server/
    - .: Interface definitions for the managers are saved here

Blazorboilerplate suggest to use Dto (Data Transfer Objects) for any object moved between frontend and backend to avoid leaking any unnecessary information.
- MetaAutoML-Frontend/src/Shared/BlazorBoilerplate.Shared/
    - Dto: all Dto are saved here
    - Services: the ApiClient can be found here, requires expansion for new API calls

# Dockerfiles location

MetaAutoML-Frontend/src/Utils/Docker/

# Important notes

All pages require login using the default user, this is to provide a future login protection:
Login: user
PW: user123
